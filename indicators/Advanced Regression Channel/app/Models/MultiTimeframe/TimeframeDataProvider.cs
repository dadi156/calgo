using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Provides data access for multi-timeframe functionality
    /// </summary>
    public class TimeframeDataProvider
    {
        private readonly ChannelConfig _config;

        // Cache for chart index to timeframe index mappings
        private readonly Dictionary<int, int> _chartToTimeframeIndexCache;

        // Cache size management
        private const int MAX_CACHE_SIZE = 1000;
        private DateTime _lastCachePurgeTime = DateTime.MinValue;

        public TimeframeDataProvider(ChannelConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _chartToTimeframeIndexCache = new Dictionary<int, int>();
        }

        /// <summary>
        /// Gets price data for the calculation window based on chart index
        /// </summary>
        /// <param name="chartIndex">Chart index to calculate for</param>
        /// <param name="period">Period to include in calculation</param>
        /// <returns>Array of price data for the calculation window</returns>
        public double[] GetPriceData(int chartIndex, int period)
        {
            if (_config.RegressionMode == RegressionMode.DateRange)
            {
                return GetDateRangePriceData();
            }
            else if (_config.UseMultiTimeframe)
            {
                return GetMultiTimeframePriceData(chartIndex, period);
            }
            else
            {
                return GetCurrentTimeframePriceData(chartIndex, period);
            }
        }

        /// <summary>
        /// Gets price data for date range mode
        /// </summary>
        private double[] GetDateRangePriceData()
        {
            // Get the bars for the appropriate timeframe
            Bars bars = _config.UseMultiTimeframe ? _config.TimeframeBars : _config.Bars;

            if (bars == null || bars.Count == 0)
                return new double[0];

            // Find all bars within the date range
            List<double> prices = new List<double>();

            for (int i = 0; i < bars.Count; i++)
            {
                DateTime barTime = bars.OpenTimes[i];

                if (barTime >= _config.StartDate && barTime <= _config.EndDate)
                {
                    prices.Add(bars.ClosePrices[i]);
                }
            }

            // If we don't have enough data, fall back to period-based data
            if (prices.Count < 2)
            {
                int fallbackPeriod = Math.Max(2, _config.Period);
                if (_config.UseMultiTimeframe)
                {
                    return GetMultiTimeframePriceData(bars.Count - 2, fallbackPeriod);
                }
                else
                {
                    return GetCurrentTimeframePriceData(bars.Count - 2, fallbackPeriod);
                }
            }

            return prices.ToArray();
        }

        /// <summary>
        /// Gets the bar indices within the specified date range
        /// </summary>
        /// <returns>List of bar indices within the date range</returns>
        public List<int> GetBarsInDateRange()
        {
            List<int> result = new List<int>();

            // Get the appropriate bars collection
            Bars bars = _config.UseMultiTimeframe ? _config.TimeframeBars : _config.Bars;

            if (bars == null || bars.Count == 0)
                return result;

            // Binary search to find start index
            int startIdx = BinarySearchStartIndex(bars, _config.StartDate);
            if (startIdx < 0) return result;

            // Iterate only the relevant range
            for (int i = startIdx; i < bars.Count; i++)
            {
                DateTime barTime = bars.OpenTimes[i];
                if (barTime > _config.EndDate)
                    break;

                result.Add(i);
            }

            return result;
        }

        /// <summary>
        /// Binary search to find first bar index >= targetTime
        /// </summary>
        private int BinarySearchStartIndex(Bars bars, DateTime targetTime)
        {
            int lo = 0;
            int hi = bars.Count - 1;
            int result = -1;

            while (lo <= hi)
            {
                int mid = lo + (hi - lo) / 2;
                DateTime midTime = bars.OpenTimes[mid];

                if (midTime >= targetTime)
                {
                    result = mid;
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets price data from the current timeframe
        /// </summary>
        private double[] GetCurrentTimeframePriceData(int chartIndex, int period)
        {
            if (chartIndex < period - 1 || chartIndex >= _config.Bars.Count)
                return new double[0];

            int startIndex = chartIndex - period + 1;
            if (startIndex < 0)
                return new double[0];

            double[] prices = new double[period];
            for (int i = 0; i < period; i++)
            {
                prices[i] = _config.Bars.ClosePrices[startIndex + i];
            }

            return prices;
        }

        /// <summary>
        /// Gets price data from the selected higher timeframe
        /// </summary>
        private double[] GetMultiTimeframePriceData(int chartIndex, int period)
        {
            if (chartIndex >= _config.Bars.Count)
                return new double[0];

            // Find the timeframe bar that contains this chart bar using cached mapping
            int timeframeIndex = MapChartIndexToTimeframeIndex(chartIndex);

            // Check if the timeframe index is valid
            if (timeframeIndex < 0)
                return new double[0];

            // Check if we have enough bars for the period
            if (timeframeIndex < period - 1)
                return new double[0];

            int startIndex = timeframeIndex - period + 1;
            if (startIndex < 0)
                return new double[0];

            double[] prices = new double[period];
            for (int i = 0; i < period; i++)
            {
                prices[i] = _config.TimeframeBars.ClosePrices[startIndex + i];
            }

            return prices;
        }

        /// <summary>
        /// Maps a chart index to a timeframe index with caching
        /// </summary>
        /// <param name="chartIndex">Chart bar index</param>
        /// <returns>Corresponding timeframe bar index, or -1 if not found</returns>
        public int MapChartIndexToTimeframeIndex(int chartIndex)
        {
            // Check if chart index is valid
            if (chartIndex < 0 || chartIndex >= _config.Bars.Count)
                return -1;

            // If not using multi-timeframe, chart index = timeframe index
            if (!_config.UseMultiTimeframe)
                return chartIndex;

            // Check cache first for improved performance
            if (_chartToTimeframeIndexCache.TryGetValue(chartIndex, out int cachedIndex))
                return cachedIndex;

            // Perform the original calculation if not found in cache
            DateTime barTime = _config.Bars.OpenTimes[chartIndex];
            int result = TimeFrameHelper.GetTimeframeIndex(
                barTime,
                _config.TimeframeBars,
                _config.SelectedTimeFrame);

            // Cache the result if valid
            if (result >= 0)
            {
                _chartToTimeframeIndexCache[chartIndex] = result;

                // Manage cache size to prevent excessive memory usage
                MaintainCacheSize();
            }

            return result;
        }

        /// <summary>
        /// Maintains the cache size by periodically purging oldest entries
        /// </summary>
        private void MaintainCacheSize()
        {
            // Only check cache size periodically to avoid performance impact
            if (_chartToTimeframeIndexCache.Count > MAX_CACHE_SIZE)
            {
                DateTime now = DateTime.UtcNow;
                if ((now - _lastCachePurgeTime).TotalSeconds > 5) // Only purge every 5 seconds at most
                {
                    // Remove about 20% of the cached entries (oldest chart indices)
                    int removeCount = MAX_CACHE_SIZE / 5;
                    int[] keysToRemove = _chartToTimeframeIndexCache.Keys.OrderBy(k => k).Take(removeCount).ToArray();

                    foreach (int key in keysToRemove)
                    {
                        _chartToTimeframeIndexCache.Remove(key);
                    }

                    _lastCachePurgeTime = now;
                }
            }
        }

        /// <summary>
        /// Clears the mapping cache
        /// </summary>
        public void ClearMappingCache()
        {
            _chartToTimeframeIndexCache.Clear();
        }

        /// <summary>
        /// Gets the last historical timeframe bar index
        /// </summary>
        /// <returns>Index of the last historical timeframe bar</returns>
        public int GetLastHistoricalTimeframeIndex()
        {
            if (!_config.UseMultiTimeframe)
                return _config.Bars.Count - 2; // Last completed bar in current timeframe

            // Find the last historical bar index of the higher timeframe
            // This ensures we don't include the current forming bar of the higher timeframe
            return _config.TimeframeBars.Count - 2;
        }

        /// <summary>
        /// Gets the chart index that corresponds to the last historical timeframe bar
        /// </summary>
        /// <returns>Chart index corresponding to last historical timeframe bar, or -1 if not found</returns>
        public int GetLastHistoricalTimeframeChartIndex()
        {
            if (!_config.UseMultiTimeframe)
                return _config.Bars.Count - 2;

            int lastTimeframeIndex = GetLastHistoricalTimeframeIndex();
            if (lastTimeframeIndex < 0)
                return -1;

            // Get the open time of the last historical timeframe bar
            DateTime lastTimeframeOpenTime = _config.TimeframeBars.OpenTimes[lastTimeframeIndex];

            // First, try to find an exact match
            for (int i = _config.Bars.Count - 2; i >= 0; i--)
            {
                DateTime chartBarTime = _config.Bars.OpenTimes[i];
                if (chartBarTime == lastTimeframeOpenTime)
                {
                    return i;
                }
            }

            // If no exact match found, find the closest bar backwards
            int closestIndex = -1;
            TimeSpan closestDiff = TimeSpan.MaxValue;

            for (int i = _config.Bars.Count - 2; i >= 0; i--)
            {
                DateTime chartBarTime = _config.Bars.OpenTimes[i];

                // Only consider bars that are before or equal to the last timeframe bar's open time
                if (chartBarTime <= lastTimeframeOpenTime)
                {
                    TimeSpan diff = lastTimeframeOpenTime - chartBarTime;
                    if (diff < closestDiff)
                    {
                        closestDiff = diff;
                        closestIndex = i;
                    }
                }
            }

            return closestIndex;
        }

        /// <summary>
        /// Gets a interpolated value from the timeframe data, if interpolation is enabled
        /// </summary>
        /// <param name="chartIndex">Chart bar index</param>
        /// <param name="values">Values array to interpolate from</param>
        /// <returns>Interpolated value or direct mapping value</returns>
        public double GetTimeframeValue(int chartIndex, double[] values)
        {
            if (!_config.UseMultiTimeframe)
            {
                if (chartIndex < 0 || chartIndex >= values.Length)
                    return double.NaN;
                return values[chartIndex];
            }

            if (chartIndex < 0 || chartIndex >= _config.Bars.Count)
                return double.NaN;

            DateTime barTime = _config.Bars.OpenTimes[chartIndex];

            // Always use interpolation
            return TimeFrameHelper.Interpolate(
                barTime,
                _config.TimeframeBars,
                values,
                _config.SelectedTimeFrame);
        }
    }
}
