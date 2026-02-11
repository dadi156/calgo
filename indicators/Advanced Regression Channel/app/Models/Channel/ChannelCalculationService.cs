using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Service for calculating regression channel data
    /// </summary>
    public class ChannelCalculationService
    {
        private readonly ChannelConfig _config;
        private readonly TimeframeDataProvider _dataProvider;
        private IRegressionCalculator _regression;
        private readonly ChannelLevelCalculator _levelCalculator;

        // Optimized LRU cache for channel data
        private readonly LRUCache<int, ChannelData> _channelCache;

        // Default cache size - adjust based on expected usage patterns
        private const int DEFAULT_CACHE_SIZE = 500;

        public ChannelCalculationService(ChannelConfig config, TimeframeDataProvider dataProvider)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _config.Validate();

            _regression = RegressionFactory.CreateRegression(
                _config.RegressionType,
                _config.Period,
                _config.Degree
            );

            _levelCalculator = new ChannelLevelCalculator();

            // Initialize the LRU cache with appropriate capacity
            // Larger capacity for higher timeframes, smaller for lower timeframes
            int cacheSize = DetermineCacheSizeFromConfig();
            _channelCache = new LRUCache<int, ChannelData>(cacheSize);
        }

        /// <summary>
        /// Determines appropriate cache size based on configuration
        /// </summary>
        private int DetermineCacheSizeFromConfig()
        {
            // If multi-timeframe is enabled, we need a larger cache
            if (_config.UseMultiTimeframe)
            {
                // Higher timeframes need less cache entries
                if (_config.SelectedTimeFrame == TimeFrame.Daily ||
                    _config.SelectedTimeFrame == TimeFrame.Weekly ||
                    _config.SelectedTimeFrame == TimeFrame.Monthly)
                {
                    return DEFAULT_CACHE_SIZE / 2;
                }
                // For hour-based timeframes
                else if (_config.SelectedTimeFrame == TimeFrame.Hour ||
                         _config.SelectedTimeFrame == TimeFrame.Hour2 ||
                         _config.SelectedTimeFrame == TimeFrame.Hour4)
                {
                    return DEFAULT_CACHE_SIZE;
                }
                // For minute-based timeframes (need more cache entries)
                else
                {
                    return DEFAULT_CACHE_SIZE * 2;
                }
            }

            // Default size for non-multi-timeframe mode
            return DEFAULT_CACHE_SIZE;
        }

        /// <summary>
        /// Calculates channel data for a specific bar index
        /// </summary>
        /// <param name="index">Bar index to calculate</param>
        /// <returns>Calculated channel data, or null if unable to calculate</returns>
        /// <summary>
        /// Calculates channel data for a specific bar index
        /// </summary>
        /// <param name="index">Bar index to calculate</param>
        /// <returns>Calculated channel data, or null if unable to calculate</returns>
        public ChannelData CalculateChannel(int index)
        {
            // Store total bars count
            int totalBars = _config.Bars.Count;

            // Skip calculation for forming bar
            if (index >= totalBars - 1)
                return null;

            // Special handling for date range mode
            if (_config.RegressionMode == RegressionMode.DateRange)
            {
                return CalculateChannelForDateRange();
            }

            // Check if we have cached data
            if (_channelCache.TryGetValue(index, out ChannelData cachedData))
                return cachedData;

            // Check if we have enough bars
            if (index < 0 || index >= totalBars)
                return null;

            // Get price data for the calculation
            var prices = _dataProvider.GetPriceData(index, _config.Period);

            if (prices.Length < _config.Period)
                return null;

            // Setup data arrays for regression calculation
            int windowSize = prices.Length;
            var x = new double[windowSize];
            var y = new double[windowSize];

            // Fill data arrays
            for (int i = 0; i < windowSize; i++)
            {
                // Normalize x values to prevent numerical instability
                x[i] = (double)i / windowSize;
                y[i] = prices[i];
            }

            // Calculate regression
            var (coefficients, standardDeviation) = _regression.Calculate(x, y);
            double channelOffset = standardDeviation * _config.ChannelWidth;

            // Calculate levels for the current bar (for statistics and other uses)
            double xValue = (double)(windowSize - 1) / windowSize; // Last bar in window
            double middleValue = _regression.EvaluateRegression(coefficients, xValue);
            double[] currentLevels = _levelCalculator.CalculateLevels(middleValue, channelOffset);

            // Calculate levels for each bar in the window
            var windowLevels = new Dictionary<int, double[]>();

            // Map the window to chart bars
            if (_config.UseMultiTimeframe)
            {
                int timeframeIndex = _dataProvider.MapChartIndexToTimeframeIndex(index);
                if (timeframeIndex >= 0)
                {
                    int timeframeStartIndex = Math.Max(0, timeframeIndex - _config.Period + 1);
                    DateTime startTime = _config.TimeframeBars.OpenTimes[timeframeStartIndex];

                    // Get the last historical timeframe bar index
                    int lastTimeframeIndex = _dataProvider.GetLastHistoricalTimeframeIndex();

                    // Get the chart index that corresponds to the last historical timeframe bar's open time
                    // If no exact match, this will return the closest bar with an earlier open time
                    int lastHistoricalChartIndex = _dataProvider.GetLastHistoricalTimeframeChartIndex();

                    // Get the open time of the last historical timeframe bar for reference
                    DateTime lastHistoricalTimeframeOpenTime = DateTime.MinValue;
                    if (lastTimeframeIndex >= 0 && lastTimeframeIndex < _config.TimeframeBars.Count)
                    {
                        lastHistoricalTimeframeOpenTime = _config.TimeframeBars.OpenTimes[lastTimeframeIndex];
                    }

                    // Process all valid chart bars within the timeframe window
                    for (int i = index; i >= 0; i--)
                    {
                        // Skip the forming bar
                        if (i >= _config.Bars.Count - 1)
                            continue;

                        // Skip any bars beyond the last historical chart index
                        if (lastHistoricalChartIndex >= 0 && i > lastHistoricalChartIndex)
                            continue;

                        // Skip any bars with open time beyond the last historical timeframe open time
                        if (lastHistoricalTimeframeOpenTime != DateTime.MinValue &&
                            _config.Bars.OpenTimes[i] > lastHistoricalTimeframeOpenTime)
                            continue;

                        DateTime barTime = _config.Bars.OpenTimes[i];
                        if (barTime < startTime)
                            break;

                        // Get the timeframe bar containing this chart bar time
                        int containingTimeframeIndex = _dataProvider.MapChartIndexToTimeframeIndex(i);

                        // Skip if invalid or beyond the last historical timeframe bar
                        if (containingTimeframeIndex < timeframeStartIndex ||
                            containingTimeframeIndex < 0 ||
                            containingTimeframeIndex > lastTimeframeIndex)
                            continue;

                        /// For interpolated channel display
                        TimeSpan timeframeSpan = _config.SelectedTimeFrame.ToTimeSpan();
                        double timeframeMs = timeframeSpan.TotalMilliseconds;
                        DateTime currentBarTime = _config.TimeframeBars.OpenTimes[containingTimeframeIndex];

                        // Calculate position within regression window with smooth interpolation
                        double timeRatio = (barTime - currentBarTime).TotalMilliseconds / timeframeMs;

                        // Get normalized position in regression window (0.0 to 1.0)
                        double tfPosition = (double)(timeframeIndex - containingTimeframeIndex) /
                                            (timeframeIndex - timeframeStartIndex + 1);

                        // Adjust position with time ratio for smoother interpolation
                        double position = 1.0 - (tfPosition - (timeRatio / (timeframeIndex - timeframeStartIndex + 1)));
                        position = Math.Max(0, Math.Min(1, position));

                        // Calculate values at this interpolated position
                        double barMiddleValue = _regression.EvaluateRegression(coefficients, position);
                        double[] barLevels = _levelCalculator.CalculateLevels(barMiddleValue, channelOffset);

                        // Only add if within valid range
                        if (lastHistoricalChartIndex < 0 || i <= lastHistoricalChartIndex)
                        {
                            windowLevels[i] = barLevels;
                        }
                    }

                    // Add extrapolation for forming bar if this is the last historical bar
                    // NOTE: We don't extrapolate in multi-timeframe mode to ensure lines end
                    // exactly at the last historical timeframe bar's open time
                }
            }
            else
            {
                // For current timeframe, direct mapping is simpler
                int startIndex = index - windowSize + 1;
                for (int i = 0; i < windowSize; i++)
                {
                    int plotIndex = startIndex + i;
                    if (plotIndex < 0 || plotIndex >= _config.Bars.Count - 1) // Skip forming bar
                        continue;

                    double xVal = (double)i / windowSize;
                    double barMiddleValue = _regression.EvaluateRegression(coefficients, xVal);
                    double[] barLevels = _levelCalculator.CalculateLevels(barMiddleValue, channelOffset);
                    windowLevels[plotIndex] = barLevels;
                }

                // Add extrapolation for the forming bar
                // FIXED EXTRAPOLATION: Use slope-based extrapolation for consistent direction
                int formingBarIndex = _config.Bars.Count - 1;

                // Check if this is the last historical bar
                if (index == _config.Bars.Count - 2 && formingBarIndex >= 0 && formingBarIndex < _config.Bars.Count)
                {
                    // Use the last two plotted points to ensure consistent direction
                    int lastIndex = index;
                    int secondLastIndex = index - 1;

                    if (secondLastIndex >= 0 && windowLevels.ContainsKey(lastIndex) && windowLevels.ContainsKey(secondLastIndex))
                    {
                        double[] lastLevels = windowLevels[lastIndex];
                        double[] secondLastLevels = windowLevels[secondLastIndex];

                        // Calculate extrapolated values using consistent slope
                        double[] formingBarLevels = new double[lastLevels.Length];
                        for (int i = 0; i < lastLevels.Length; i++)
                        {
                            // Calculate slope from last two points
                            double slope = (lastLevels[i] - secondLastLevels[i]);

                            // Extrapolate with the same slope
                            formingBarLevels[i] = lastLevels[i] + slope;
                        }

                        // Add to window levels
                        windowLevels[formingBarIndex] = formingBarLevels;
                    }
                }
            }

            // Create channel data with both current level and window levels
            var channelData = new ChannelData(
                index,
                currentLevels,
                windowLevels,
                channelOffset,
                coefficients,
                standardDeviation
            );

            // Cache the result using the optimized LRU cache
            _channelCache.Set(index, channelData);

            return channelData;
        }

        private ChannelData CalculateChannelForDateRange()
        {
            // Get the appropriate bars collection
            Bars bars = _config.UseMultiTimeframe ? _config.TimeframeBars : _config.Bars;

            if (bars == null || bars.Count == 0)
                return null;

            // Find the indices of bars within the date range
            List<int> barsInRange = new List<int>();
            Dictionary<int, int> localToGlobalIndex = new Dictionary<int, int>();

            for (int i = 0; i < bars.Count; i++)
            {
                DateTime barTime = bars.OpenTimes[i];

                if (barTime >= _config.StartDate && barTime <= _config.EndDate)
                {
                    localToGlobalIndex[barsInRange.Count] = i;
                    barsInRange.Add(i);
                }
            }

            // If we don't have enough data in the range (need at least 2 points for regression)
            if (barsInRange.Count < 2)
                return null;

            // Set up arrays for the regression calculation
            int windowSize = barsInRange.Count;
            var x = new double[windowSize];
            var y = new double[windowSize];

            // Fill data arrays with values from the specified date range
            for (int i = 0; i < windowSize; i++)
            {
                // Normalize x values from 0 to 1 based on position in the window
                x[i] = (double)i / windowSize;
                y[i] = bars.ClosePrices[barsInRange[i]];
            }

            // Calculate regression
            var (coefficients, standardDeviation) = _regression.Calculate(x, y);
            double channelOffset = standardDeviation * _config.ChannelWidth;

            // Calculate levels for the current bar (for statistics)
            double xValue = 1.0; // Last bar in window
            double middleValue = _regression.EvaluateRegression(coefficients, xValue);
            double[] currentLevels = _levelCalculator.CalculateLevels(middleValue, channelOffset);

            // Calculate levels for each bar in the date range window
            var windowLevels = new Dictionary<int, double[]>();

            // Calculate and store regression line values for each bar in the range
            for (int i = 0; i < windowSize; i++)
            {
                double position = (double)i / windowSize;
                double barMiddleValue = _regression.EvaluateRegression(coefficients, position);
                double[] barLevels = _levelCalculator.CalculateLevels(barMiddleValue, channelOffset);

                // Map back to the actual bar index
                int timeframeBarIndex = barsInRange[i];

                if (_config.UseMultiTimeframe)
                {
                    // Map timeframe bar index to chart bar indices
                    DateTime tfBarTime = _config.TimeframeBars.OpenTimes[timeframeBarIndex];
                    DateTime tfBarEndTime = tfBarTime.Add(_config.SelectedTimeFrame.ToTimeSpan());

                    // Find all chart bars within this timeframe bar
                    for (int j = 0; j < _config.Bars.Count - 1; j++)
                    {
                        DateTime chartBarTime = _config.Bars.OpenTimes[j];
                        if (chartBarTime >= tfBarTime && chartBarTime < tfBarEndTime)
                        {
                            windowLevels[j] = barLevels;
                        }
                    }
                }
                else
                {
                    windowLevels[timeframeBarIndex] = barLevels;
                }
            }

            // Create channel data with both current level and window levels
            var channelData = new ChannelData(
                barsInRange[windowSize - 1], // Use the last bar in range as reference
                currentLevels,
                windowLevels,
                channelOffset,
                coefficients,
                standardDeviation
            );

            // Cache the result
            _channelCache.Set(barsInRange[windowSize - 1], channelData);

            return channelData;
        }

        /// <summary>
        /// Clears the channel data cache
        /// </summary>
        public void ClearCache()
        {
            _channelCache.Clear();

            // Also clear TimeframeHelper caches
            TimeFrameHelper.ClearCaches();

            // And the timeframe provider cache
            _dataProvider.ClearMappingCache();
        }

        /// <summary>
        /// Updates the service configuration
        /// </summary>
        /// <param name="config">New configuration</param>
        public void UpdateConfig(ChannelConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // Only update if configuration has changed
            if (_config.Period != config.Period ||
                _config.RegressionType != config.RegressionType ||
                _config.Degree != config.Degree ||
                _config.ChannelWidth != config.ChannelWidth ||
                _config.UseMultiTimeframe != config.UseMultiTimeframe ||
                _config.SelectedTimeFrame != config.SelectedTimeFrame)
            {
                // Update the config
                _config.Period = config.Period;
                _config.RegressionType = config.RegressionType;
                _config.Degree = config.Degree;
                _config.ChannelWidth = config.ChannelWidth;
                _config.UseMultiTimeframe = config.UseMultiTimeframe;
                _config.SelectedTimeFrame = config.SelectedTimeFrame;

                // Recreate the regression calculator
                _regression = RegressionFactory.CreateRegression(
                    _config.RegressionType,
                    _config.Period,
                    _config.Degree
                );

                // Clear the cache
                ClearCache();
            }
        }
    }
}
