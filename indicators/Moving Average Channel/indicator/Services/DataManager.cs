using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class DataManager
    {
        private readonly MarketData _marketData;
        private readonly MAModel _model;

        private Bars _mtfBars;
        private bool _isMultiTimeframe;
        private int _lastMTFIndex = -1;
        private TimeFrame _selectedTimeframe;
        private TimeFrame _currentTimeframe;
        
        private bool _useTrendlines;

        private MAResult _previousMTFResult;
        private DateTime _previousMTFTime;
        private MAResult _currentMTFResult;  
        private DateTime _currentMTFTime;    

        private MAModel _mtfModel;
        private MAParameters _parameters;

        public bool IsMultiTimeframe => _isMultiTimeframe;
        public Bars MTFBars => _mtfBars;

        public DataManager(MarketData marketData, MAModel model)
        {
            _marketData = marketData;
            _model = model;
        }

        public bool Initialize(bool enableMultiTimeframe, TimeFrame selectedTimeframe,
                              TimeFrame currentTimeframe, bool useTrendlines)
        {
            _selectedTimeframe = selectedTimeframe;
            _currentTimeframe = currentTimeframe;
            _useTrendlines = useTrendlines;

            if (!IsValidTimeframeHierarchy(selectedTimeframe, currentTimeframe))
            {
                _isMultiTimeframe = false;
                return false;
            }

            _isMultiTimeframe = enableMultiTimeframe && selectedTimeframe != currentTimeframe;

            if (_isMultiTimeframe)
            {
                try
                {
                    _mtfBars = _marketData.GetBars(selectedTimeframe);

                    if (_mtfBars == null || _mtfBars.Count == 0)
                    {
                        _isMultiTimeframe = false;
                        return false;
                    }

                    EnsureSufficientHistory(Math.Max(50, _model.GetPeriod() * 3));
                    CalculateHistoricalMTFData();
                    return true;
                }
                catch (Exception)
                {
                    _isMultiTimeframe = false;
                    return false;
                }
            }

            return false;
        }

        public void SetParameters(MAParameters parameters)
        {
            _parameters = parameters;
            
            if (_isMultiTimeframe && _mtfBars != null)
            {
                int mtfArraySize = _mtfBars.Count + 1000;
                _mtfModel = new MAModel(mtfArraySize, parameters);
                
                if (_mtfBars.Count > 0)
                {
                    double firstHigh = _mtfBars.HighPrices[0];
                    double firstLow = _mtfBars.LowPrices[0];
                    double firstClose = _mtfBars.ClosePrices[0];
                    double firstOpen = _mtfBars.OpenPrices[0];
                    
                    _mtfModel.InitializeFirstValues(firstHigh, firstLow, firstClose, firstOpen);
                }
            }
        }

        public MAResult GetAllValues(DateTime currentTime, Bars currentBars, int currentIndex)
        {
            if (!_isMultiTimeframe)
            {
                return _model.Calculate(currentIndex, currentBars);
            }

            return GetMultiTimeframeValues(currentTime, currentIndex);
        }

        private MAResult GetMultiTimeframeValues(DateTime currentTime, int currentIndex)
        {
            if (_mtfBars == null || _mtfBars.Count == 0)
                return new MAResult(0, 0, 0, 0, 0, 0, 0);

            int mtfIndex = FindMTFIndex(currentTime);

            if (mtfIndex >= 0)
            {
                if (mtfIndex != _lastMTFIndex)
                {
                    for (int i = Math.Max(0, _lastMTFIndex); i <= mtfIndex; i++)
                    {
                        if (_mtfModel != null)
                        {
                            _mtfModel.Calculate(i, _mtfBars);
                        }
                        else
                        {
                            _model.Calculate(i, _mtfBars);
                        }
                    }
                    
                    MAResult newMTFResult;
                    if (_mtfModel != null)
                    {
                        newMTFResult = _mtfModel.Calculate(mtfIndex, _mtfBars);
                    }
                    else
                    {
                        newMTFResult = _model.Calculate(mtfIndex, _mtfBars);
                    }
                    
                    if (ShouldUseTrendlines() && ValidationHelper.IsValidResult(newMTFResult))
                    {
                        _previousMTFResult = _currentMTFResult; 
                        _previousMTFTime = _currentMTFTime;     
                        
                        _currentMTFResult = newMTFResult;       
                        _currentMTFTime = _mtfBars.OpenTimes[mtfIndex]; 
                    }
                    
                    _lastMTFIndex = mtfIndex;
                    
                    return new MAResult(newMTFResult.HighMA, newMTFResult.LowMA, 
                                       newMTFResult.CloseMA, newMTFResult.OpenMA, 
                                       newMTFResult.MedianMA,
                                       newMTFResult.Fib618MA, newMTFResult.Fib382MA, true);
                }
                else
                {
                    MAResult currentMTFResult;
                    if (_mtfModel != null)
                    {
                        currentMTFResult = _mtfModel.Calculate(mtfIndex, _mtfBars);
                    }
                    else
                    {
                        currentMTFResult = _model.Calculate(mtfIndex, _mtfBars);
                    }

                    return new MAResult(currentMTFResult.HighMA, currentMTFResult.LowMA, 
                                       currentMTFResult.CloseMA, currentMTFResult.OpenMA,
                                       currentMTFResult.MedianMA,
                                       currentMTFResult.Fib618MA, currentMTFResult.Fib382MA, false);
                }
            }

            return new MAResult(0, 0, 0, 0, 0, 0, 0, false);
        }

        public bool ShouldUseTrendlines()
        {
            return _useTrendlines && 
                   _isMultiTimeframe && 
                   IsCurrentTimeframeLower();
        }

        public MAResult GetCurrentMTFResult()
        {
            return _currentMTFResult;
        }

        public DateTime GetCurrentMTFTime()
        {
            return _currentMTFTime;
        }

        public DateTime GetNextMTFTime()
        {
            if (_currentMTFTime == default(DateTime))
                return default(DateTime);

            var mtfTimeSpan = _selectedTimeframe.ToTimeSpan();
            return _currentMTFTime.Add(mtfTimeSpan);
        }

        // Calculate projection values for OHLC + Median + 2 Fibonacci
        public MAResult CalculateProjection()
        {
            if (_currentMTFResult == null || _previousMTFResult == null)
                return null;

            if (!ValidationHelper.IsValidResult(_currentMTFResult) || 
                !ValidationHelper.IsValidResult(_previousMTFResult))
                return null;

            try
            {
                // Calculate change rates for OHLC lines
                double highChange = _currentMTFResult.HighMA - _previousMTFResult.HighMA;
                double lowChange = _currentMTFResult.LowMA - _previousMTFResult.LowMA;
                double closeChange = _currentMTFResult.CloseMA - _previousMTFResult.CloseMA;
                double openChange = _currentMTFResult.OpenMA - _previousMTFResult.OpenMA;

                // Calculate OHLC projections: current + change
                double projectedHigh = _currentMTFResult.HighMA + highChange;
                double projectedLow = _currentMTFResult.LowMA + lowChange;
                double projectedClose = _currentMTFResult.CloseMA + closeChange;
                double projectedOpen = _currentMTFResult.OpenMA + openChange;

                // NEW: Calculate projected median from projected High and Low
                double projectedMedian = CalculationHelper.CalculateMedian(projectedHigh, projectedLow);

                // Calculate 2 Fibonacci levels from projected High and Low using helper
                var (projectedFib618, projectedFib382) = 
                    CalculationHelper.CalculateFibonacciLevels(projectedHigh, projectedLow);

                return new MAResult(projectedHigh, projectedLow, projectedClose, projectedOpen, 
                                   projectedMedian, projectedFib618, projectedFib382, false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool IsCurrentTimeframeLower()
        {
            var selectedMinutes = _selectedTimeframe.ToTimeSpan().TotalMinutes;
            var currentMinutes = _currentTimeframe.ToTimeSpan().TotalMinutes;
            
            return currentMinutes < selectedMinutes;
        }

        private int FindMTFIndex(DateTime currentTime)
        {
            int mtfIndex = -1;

            for (int i = 0; i < _mtfBars.Count; i++)
            {
                if (_mtfBars.OpenTimes[i] <= currentTime)
                {
                    mtfIndex = i;
                }
                else
                {
                    break;
                }
            }

            return mtfIndex;
        }

        private void CalculateHistoricalMTFData()
        {
            int startIndex = Math.Max(0, _model.GetPeriod());

            if (_mtfModel != null)
            {
                for (int i = startIndex; i < _mtfBars.Count; i++)
                {
                    _mtfModel.Calculate(i, _mtfBars);
                }

                for (int i = 0; i < startIndex && i < _mtfBars.Count; i++)
                {
                    _mtfModel.Calculate(i, _mtfBars);
                }
            }
            else
            {
                for (int i = startIndex; i < _mtfBars.Count; i++)
                {
                    _model.Calculate(i, _mtfBars);
                }

                for (int i = 0; i < startIndex && i < _mtfBars.Count; i++)
                {
                    _model.Calculate(i, _mtfBars);
                }
            }
        }

        public void EnsureSufficientHistory(int requiredBars)
        {
            if (_mtfBars != null && _mtfBars.Count < requiredBars)
            {
                try
                {
                    int attempts = 0;
                    int maxAttempts = 10;

                    while (_mtfBars.Count < requiredBars && attempts < maxAttempts)
                    {
                        int loadedCount = _mtfBars.LoadMoreHistory();

                        if (loadedCount == 0)
                        {
                            break;
                        }

                        attempts++;
                    }

                    if (attempts > 0)
                    {
                        CalculateHistoricalMTFData();
                    }
                }
                catch (Exception)
                {
                    // Continue with available data
                }
            }
        }

        private bool IsValidTimeframeHierarchy(TimeFrame selected, TimeFrame current)
        {
            var selectedMinutes = selected.ToTimeSpan().TotalMinutes;
            var currentMinutes = current.ToTimeSpan().TotalMinutes;

            return selectedMinutes >= currentMinutes;
        }
    }
}
