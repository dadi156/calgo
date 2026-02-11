using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class PriceTrackingModel
    {
        #region Core Properties
        private int _periods;
        private TimeFrame _timeFrame;
        private List<OHLC> _priceData;
        private List<OHLC> _cachedData;
        private Symbol _symbol;
        private Bars _bars;
        private MarketData _marketData;
        #endregion

        #region Enhanced Mode Management
        private RegressionModeManager _modeManager;
        #endregion

        #region Bar Change Detection
        // Track the last bar time for the selected timeframe
        private DateTime _lastKnownBarTime = DateTime.MinValue;
        private bool _isNewBarFormed = false;
        #endregion

        /// <summary>
        /// Simple constructor - only new system
        /// </summary>
        public PriceTrackingModel(
            int periods, 
            TimeFrame timeFrame, 
            Bars bars, 
            Symbol symbol,
            StartPointMethod startPointMethod,
            bool historicalBarsOnly,
            bool enableLock,
            string lockDate,
            string startDate)
        {
            _periods = periods;
            _timeFrame = timeFrame;
            _priceData = new List<OHLC>();
            _cachedData = new List<OHLC>();
            _bars = bars;
            _symbol = symbol;

            // Create enhanced mode manager
            _modeManager = new RegressionModeManager(
                startPointMethod,
                historicalBarsOnly,
                enableLock,
                lockDate,
                startDate,
                periods
            );
        }

        public void UpdateMarketData(MarketData marketData)
        {
            _marketData = marketData;
        }

        #region Update Detection

        /// <summary>
        /// Check if data should be refreshed
        /// </summary>
        public bool ShouldRefreshData()
        {
            // Check for new bar in selected timeframe
            bool newBarFormed = CheckForNewBar();

            // For period mode with HistoricalBarsOnly = true, only update on new completed bars
            if (!_modeManager.IsDateTimeMode && _modeManager.HistoricalBarsOnly)
            {
                return newBarFormed;
            }

            // For DateTime mode or HistoricalBarsOnly = false, update more frequently
            if (_modeManager.IsDateTimeMode || !_modeManager.HistoricalBarsOnly)
            {
                // Update if new bar formed OR if current bar is updating
                return newBarFormed || true; // Allow live updates
            }

            return newBarFormed;
        }

        #endregion

        #region Bar Change Detection

        /// <summary>
        /// Check if new bar formed in selected timeframe
        /// </summary>
        public bool CheckForNewBar()
        {
            _isNewBarFormed = false;

            // Get the bars for the selected timeframe
            Bars timeframeBars = GetTimeframeBars(_timeFrame);

            if (timeframeBars == null || timeframeBars.Count == 0)
                return false;

            // Get the most recent bar's open time
            DateTime latestBarTime = timeframeBars.OpenTimes[timeframeBars.Count - 1];

            // If this is the first check or a different bar time is detected
            if (_lastKnownBarTime == DateTime.MinValue || latestBarTime != _lastKnownBarTime)
            {
                // If not the first check, then a new bar has formed
                if (_lastKnownBarTime != DateTime.MinValue)
                {
                    _isNewBarFormed = true;
                }

                // Update the last known bar time
                _lastKnownBarTime = latestBarTime;
            }

            return _isNewBarFormed;
        }

        /// <summary>
        /// Returns true if the most recent check showed a new bar
        /// </summary>
        public bool IsNewBarFormed()
        {
            return _isNewBarFormed;
        }

        #endregion

        #region Data Collection

        /// <summary>
        /// Refresh data using enhanced system only
        /// </summary>
        public void RefreshData()
        {
            _priceData.Clear();

            // Get the bars for the selected timeframe
            Bars timeframeBars = GetTimeframeBars(_timeFrame);

            if (timeframeBars == null || timeframeBars.Count == 0)
                return;

            // Use enhanced data collection
            if (_modeManager.IsDateTimeMode)
            {
                CollectDateTimeData(timeframeBars);
            }
            else
            {
                CollectPeriodData(timeframeBars);
            }

            // Store a copy of the data for persistence
            _cachedData = new List<OHLC>(_priceData);
        }

        /// <summary>
        /// Collect data from specific DateTime start point with lock support
        /// </summary>
        private void CollectDateTimeData(Bars timeframeBars)
        {
            List<OHLC> tempData = new List<OHLC>();

            // Determine end index based on lock mechanism
            int endIndex = timeframeBars.Count;
            if (_modeManager.EnableLock)
            {
                endIndex = FindLockIndex(timeframeBars);
                if (endIndex == -1)
                {
                    // No data before lock date - use all available data
                    endIndex = timeframeBars.Count;
                }
            }

            // Collect data from start date to end index
            for (int i = 0; i < endIndex; i++)
            {
                DateTime barTime = timeframeBars.OpenTimes[i];

                // Only include bars from start date onwards
                if (barTime >= _modeManager.StartDateTime)
                {
                    OHLC dataPoint = new OHLC
                    {
                        Open = timeframeBars.OpenPrices[i],
                        High = timeframeBars.HighPrices[i],
                        Low = timeframeBars.LowPrices[i],
                        Close = timeframeBars.ClosePrices[i],
                        Time = barTime
                    };

                    tempData.Add(dataPoint);
                }
            }

            // Handle HistoricalBarsOnly setting (only if lock is not enabled)
            if (_modeManager.HistoricalBarsOnly && !_modeManager.EnableLock && tempData.Count > 0)
            {
                // Remove the last bar (current forming bar)
                tempData.RemoveAt(tempData.Count - 1);
            }

            // Sort by date (newest first) and store
            _priceData = tempData.OrderByDescending(bar => bar.Time).ToList();
        }

        /// <summary>
        /// FIXED: Collect period-based data with correct lock support
        /// Now keeps same start point when lock is enabled (like Price Average)
        /// </summary>
        private void CollectPeriodData(Bars timeframeBars)
        {
            List<OHLC> tempData = new List<OHLC>();

            if (_modeManager.EnableLock)
            {
                // FIXED: Period + Lock logic (same as Price Average)
                // Step 1: Calculate normal start point (same as without lock)
                int normalStartIndex = CalculateNormalStartIndex(timeframeBars);
                
                // Step 2: Find exact lock time or closest before
                int lockEndIndex = FindLockIndex(timeframeBars);
                
                // Step 3: Check if lock is valid
                if (lockEndIndex == -1 || normalStartIndex >= lockEndIndex)
                {
                    // Lock date is before start point - fallback to normal mode
                    CollectPeriodDataNormalMode(timeframeBars);
                    return;
                }
                
                // Step 4: Collect from normal start to lock end (keep same start point!)
                for (int i = normalStartIndex; i < lockEndIndex; i++)
                {
                    if (i >= 0 && i < timeframeBars.Count)
                    {
                        OHLC dataPoint = new OHLC
                        {
                            Open = timeframeBars.OpenPrices[i],
                            High = timeframeBars.HighPrices[i],
                            Low = timeframeBars.LowPrices[i],
                            Close = timeframeBars.ClosePrices[i],
                            Time = timeframeBars.OpenTimes[i]
                        };

                        tempData.Add(dataPoint);
                    }
                }
                
                // Sort by date (newest first) and store
                _priceData = tempData.OrderByDescending(bar => bar.Time).ToList();
            }
            else
            {
                // Normal mode (without lock)
                CollectPeriodDataNormalMode(timeframeBars);
            }
        }

        /// <summary>
        /// FIXED: Calculate normal start index (as if lock was not enabled)
        /// This keeps same start point for period-based + lock
        /// </summary>
        private int CalculateNormalStartIndex(Bars timeframeBars)
        {
            int periodsToCollect = _modeManager.HistoricalBarsOnly ? _periods - 1 : _periods;
            
            if (periodsToCollect <= 0)
                return 0;

            int totalBarsAvailable = timeframeBars.Count;
            if (_modeManager.HistoricalBarsOnly && totalBarsAvailable > 0)
            {
                totalBarsAvailable = totalBarsAvailable - 1;
            }

            if (totalBarsAvailable <= 0)
                return 0;

            periodsToCollect = Math.Min(periodsToCollect, totalBarsAvailable);
            return Math.Max(0, totalBarsAvailable - periodsToCollect);
        }

        /// <summary>
        /// FIXED: Normal mode data collection (extracted for reuse in fallback)
        /// Used when lock is disabled OR when lock date is before start point
        /// </summary>
        private void CollectPeriodDataNormalMode(Bars timeframeBars)
        {
            List<OHLC> tempData = new List<OHLC>();

            // Determine how many periods to collect
            int periodsToCollect = _modeManager.HistoricalBarsOnly ? _periods - 1 : _periods;
            if (periodsToCollect <= 0)
                return;

            // Calculate normal end index
            int totalBarsAvailable = timeframeBars.Count;
            int endIndex = totalBarsAvailable;

            // Normal mode: adjust for HistoricalBarsOnly
            if (_modeManager.HistoricalBarsOnly && totalBarsAvailable > 0)
            {
                totalBarsAvailable = totalBarsAvailable - 1;
                endIndex = endIndex - 1;
            }

            if (totalBarsAvailable <= 0)
                return;

            // Calculate start index
            periodsToCollect = Math.Min(periodsToCollect, totalBarsAvailable);
            int startIndex = Math.Max(0, totalBarsAvailable - periodsToCollect);

            // Collect data
            for (int i = startIndex; i < endIndex; i++)
            {
                if (i >= 0 && i < timeframeBars.Count)
                {
                    OHLC dataPoint = new OHLC
                    {
                        Open = timeframeBars.OpenPrices[i],
                        High = timeframeBars.HighPrices[i],
                        Low = timeframeBars.LowPrices[i],
                        Close = timeframeBars.ClosePrices[i],
                        Time = timeframeBars.OpenTimes[i]
                    };

                    tempData.Add(dataPoint);
                }
            }

            // Sort by date (newest first) and store
            _priceData = tempData.OrderByDescending(bar => bar.Time).ToList();
        }

        /// <summary>
        /// Find exact lock time or closest bar before lock time
        /// </summary>
        private int FindLockIndex(Bars timeframeBars)
        {
            DateTime lockTime = _modeManager.LockDateTime;
            int bestIndex = -1;

            // Look for exact match first, then closest before
            for (int i = 0; i < timeframeBars.Count; i++)
            {
                DateTime barTime = timeframeBars.OpenTimes[i];
                
                if (barTime == lockTime)
                {
                    // Found exact match!
                    return i + 1; // Return index + 1 (exclusive end)
                }
                else if (barTime < lockTime)
                {
                    // This bar is before lock time - keep it as best candidate
                    bestIndex = i + 1; // Store as exclusive end index
                }
                else
                {
                    // This bar is after lock time - stop searching
                    break;
                }
            }

            return bestIndex; // Return best index found (or -1 if none)
        }

        #endregion

        #region Utility Methods

        private Bars GetTimeframeBars(TimeFrame timeFrame)
        {
            // Always try to get the specific timeframe bars via MarketData
            if (_marketData != null)
                return _marketData.GetBars(timeFrame);

            // Fallback to current bars only if they match the requested timeframe
            if (_bars.TimeFrame == timeFrame)
                return _bars;

            return null;
        }

        #endregion

        #region Properties and Settings

        // Get the price data - return cached data if available
        public List<OHLC> GetPriceData()
        {
            return _cachedData.Count > 0 ? _cachedData : _priceData;
        }

        public TimeFrame GetTimeFrame()
        {
            return _timeFrame;
        }

        public Symbol GetSymbol()
        {
            return _symbol;
        }

        public int GetPeriods()
        {
            return _periods;
        }

        /// <summary>
        /// Get mode info for display
        /// </summary>
        public string GetModeInfo()
        {
            return _modeManager.GetModeInfo();
        }

        /// <summary>
        /// Check if using DateTime mode
        /// </summary>
        public bool IsDateTimeMode()
        {
            return _modeManager.IsDateTimeMode;
        }

        /// <summary>
        /// Check if lock is enabled
        /// </summary>
        public bool IsLockEnabled()
        {
            return _modeManager.EnableLock;
        }

        /// <summary>
        /// Get lock date
        /// </summary>
        public DateTime GetLockDateTime()
        {
            return _modeManager.LockDateTime;
        }

        /// <summary>
        /// Get historical bars setting
        /// </summary>
        public bool GetHistoricalBarsOnly()
        {
            return _modeManager.HistoricalBarsOnly;
        }

        #endregion
    }

    public class OHLC
    {
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public DateTime Time { get; set; }
    }
}
