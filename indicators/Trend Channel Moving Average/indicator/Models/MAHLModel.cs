using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Main Model for TrendChannelMovingAverage indicator
    /// </summary>
    public class MAHLModel
    {
        // Helper classes
        private readonly ConfigurationManager _configManager;
        private readonly CacheManager _cacheManager;
        private readonly ArrayManager _arrayManager;
        private readonly TimeframeCalculator _timeframeCalculator;
        private readonly MultiTimeframeCalculator _multiTimeframeCalculator;
        private readonly TrendAnalyzer _trendAnalyzer;

        // Reference to main indicator for anchor date info
        private readonly TrendChannelMovingAverage _indicator;

        // CHANGED: Constructor now takes DataSeries source instead of SourcePrice
        public MAHLModel(Bars bars, Indicator indicator, int period, MAType maType,
                         DataSeries source, bool multiTimeframeMode, TimeFrame baseTimeframe, int trendAveragingPeriod = 5)
        {
            _indicator = indicator as TrendChannelMovingAverage;

            // CHANGED: Pass source directly instead of sourcePrice
            _configManager = new ConfigurationManager(bars, indicator, period, maType,
                                                     source, multiTimeframeMode, baseTimeframe);

            _cacheManager = new CacheManager();
            _arrayManager = new ArrayManager(bars.Count);
            _timeframeCalculator = new TimeframeCalculator(_configManager);
            _trendAnalyzer = new TrendAnalyzer(trendAveragingPeriod);

            if (_configManager.HasMultiTimeframeData())
            {
                _multiTimeframeCalculator = new MultiTimeframeCalculator(_configManager, _cacheManager, _indicator);
            }
        }

        /// <summary>
        /// Calculate effective period based on anchor date settings
        /// </summary>
        private int CalculateEffectivePeriod(int currentIndex)
        {
            if (_indicator == null)
                return _configManager.Period;

            if (_indicator.GetPeriodCalculationType() != PeriodCalculationType.AnchorDate)
                return _configManager.Period;

            int firstValidIndex = _indicator.GetFirstValidBarIndex();

            if (currentIndex < firstValidIndex)
                return 1;

            int expandingPeriod = currentIndex - firstValidIndex + 1;
            return Math.Max(1, expandingPeriod);
        }

        /// <summary>
        /// Main calculation method
        /// </summary>
        public void CalculateForIndex(int index)
        {
            if (index < 0 || index >= _configManager.CurrentBars.Count || !_arrayManager.IsValidIndex(index))
                return;

            var (startIndex, endIndex) = _cacheManager.GetCalculationRange(index);

            if (endIndex - startIndex > 1)
            {
                CalculateBatch(startIndex, endIndex);
            }
            else
            {
                CalculateOneBar(index);
            }

            _cacheManager.UpdateLastCalculatedIndex(index, _configManager.CurrentBars.Count);
        }

        /// <summary>
        /// Calculate multiple bars together
        /// </summary>
        private void CalculateBatch(int fromIndex, int toIndex)
        {
            for (int i = fromIndex; i <= toIndex; i++)
            {
                CalculateOneBar(i);
            }
        }

        /// <summary>
        /// Calculate one bar
        /// </summary>
        private void CalculateOneBar(int index)
        {
            int effectivePeriod = CalculateEffectivePeriod(index);

            CachedValues values;

            if (_configManager.HasMultiTimeframeData())
            {
                values = _multiTimeframeCalculator.Calculate(index, effectivePeriod);
            }
            else
            {
                values = _timeframeCalculator.Calculate(index, effectivePeriod);
            }

            if (values.IsValid())
            {
                TrendDirection trend;

                // Different logic for multi-timeframe vs normal mode
                if (_configManager.HasMultiTimeframeData())
                {
                    // MULTI-TIMEFRAME MODE: Use base timeframe index for trend calculation
                    int baseIndex = FindBaseTimeframeIndex(index);

                    if (baseIndex >= 0)
                    {
                        // Store and calculate trend using BASE timeframe index
                        _trendAnalyzer.StoreCloseMA(baseIndex, values.Close);
                        trend = _trendAnalyzer.CalculateTrend(baseIndex, values.Close);
                    }
                    else
                    {
                        trend = TrendDirection.Neutral;
                    }
                }
                else
                {
                    // NORMAL MODE: Use current timeframe index
                    _trendAnalyzer.StoreCloseMA(index, values.Close);
                    trend = _trendAnalyzer.CalculateTrend(index, values.Close);
                }

                var valuesWithTrend = values.WithTrend(trend);
                _arrayManager.StoreValues(index, valuesWithTrend);
            }
        }

        /// <summary>
        /// Helper method to find base timeframe index
        /// </summary>
        private int FindBaseTimeframeIndex(int currentIndex)
        {
            try
            {
                if (!_configManager.HasMultiTimeframeData())
                    return -1;

                if (currentIndex < 0 || currentIndex >= _configManager.CurrentBars.Count)
                    return -1;

                DateTime currentTime = _configManager.CurrentBars.OpenTimes[currentIndex];

                // Find the base timeframe bar that contains this current time
                for (int i = _configManager.BaseTimeframeBars.Count - 1; i >= 0; i--)
                {
                    DateTime baseBarTime = _configManager.BaseTimeframeBars.OpenTimes[i];

                    if (currentTime >= baseBarTime)
                    {
                        return i;
                    }
                }

                return -1;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Reset all optimizations
        /// </summary>
        public void ResetOptimizations()
        {
            _cacheManager.ClearCache();
            _trendAnalyzer.Clear();
        }

        // Fast get methods for MA lines
        public double GetOpenLine(int index) => _arrayManager.GetOpenLine(index);
        public double GetCloseLine(int index) => _arrayManager.GetCloseLine(index);
        public double GetMedianLine(int index) => _arrayManager.GetMedianLine(index);

        // Get methods for High/Low lines
        public double GetHighLine(int index)
        {
            var values = _arrayManager.GetValues(index);
            if (!values.IsValid()) return double.NaN;
            return values.High;
        }

        public double GetLowLine(int index)
        {
            var values = _arrayManager.GetValues(index);
            if (!values.IsValid()) return double.NaN;
            return values.Low;
        }

        // Get methods for specific trend lines
        public double GetHighLineUptrend(int index)
        {
            var values = _arrayManager.GetValues(index);
            return (values.IsValid() && values.Trend == TrendDirection.Uptrend) ? values.High : double.NaN;
        }

        public double GetHighLineDowntrend(int index)
        {
            var values = _arrayManager.GetValues(index);
            return (values.IsValid() && values.Trend == TrendDirection.Downtrend) ? values.High : double.NaN;
        }

        public double GetHighLineNeutral(int index)
        {
            var values = _arrayManager.GetValues(index);
            return (values.IsValid() && values.Trend == TrendDirection.Neutral) ? values.High : double.NaN;
        }

        public double GetLowLineUptrend(int index)
        {
            var values = _arrayManager.GetValues(index);
            return (values.IsValid() && values.Trend == TrendDirection.Uptrend) ? values.Low : double.NaN;
        }

        public double GetLowLineDowntrend(int index)
        {
            var values = _arrayManager.GetValues(index);
            return (values.IsValid() && values.Trend == TrendDirection.Downtrend) ? values.Low : double.NaN;
        }

        public double GetLowLineNeutral(int index)
        {
            var values = _arrayManager.GetValues(index);
            return (values.IsValid() && values.Trend == TrendDirection.Neutral) ? values.Low : double.NaN;
        }

        // Get current trend direction as string
        public string GetTrendDirection(int index)
        {
            var values = _arrayManager.GetValues(index);
            if (!values.IsValid()) return "Invalid";
            return _trendAnalyzer.GetTrendDirectionAsString(values.Trend);
        }

        // Settings get methods - CHANGED: Remove GetSourcePrice
        public MAType GetMAType() => _configManager.MAType;
        public DataSeries GetSource() => _configManager.Source;  // NEW: Get source directly
        public int GetPeriod() => _configManager.Period;
        public bool IsMultiTimeframeEnabled() => _configManager.IsMultiTimeframeEnabled;

        /// <summary>
        /// Get all values for debugging or other indicators
        /// </summary>
        public CachedValues GetAllValues(int index)
        {
            return _arrayManager.GetValues(index);
        }

        /// <summary>
        /// Get effective period for current index
        /// </summary>
        public int GetEffectivePeriod(int index)
        {
            return CalculateEffectivePeriod(index);
        }

        /// <summary>
        /// Get cache performance info
        /// </summary>
        public double GetCacheHitRatio()
        {
            return _cacheManager.GetCacheHitRatio();
        }
    }
}
