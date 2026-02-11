using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Helper methods for working with timeframes
    /// </summary>
    public static class TimeFrameHelper
    {
        // Cache timespan values to avoid recalculation
        private static readonly Dictionary<TimeFrame, TimeSpan> _timeframeSpans = new Dictionary<TimeFrame, TimeSpan>();

        // Optimize caching with more efficient key structure instead of Tuple
        private class TimeframeCacheKey : IEquatable<TimeframeCacheKey>
        {
            public DateTime Time { get; }
            public int BarsHash { get; }
            public TimeFrame Timeframe { get; }

            public TimeframeCacheKey(DateTime time, int barsHash, TimeFrame timeframe)
            {
                // Normalize time to minute precision to reduce cache size
                Time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
                BarsHash = barsHash;
                Timeframe = timeframe;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TimeframeCacheKey);
            }

            public bool Equals(TimeframeCacheKey other)
            {
                if (other == null) return false;
                return Time.Equals(other.Time) &&
                       BarsHash == other.BarsHash &&
                       Timeframe.Equals(other.Timeframe);
            }

            public override int GetHashCode()
            {
                return Time.GetHashCode() ^ BarsHash ^ Timeframe.GetHashCode();
            }
        }

        // Use a Dictionary with custom key instead of Tuple for better performance
        private static readonly Dictionary<TimeframeCacheKey, int> _timeframeIndexCache =
            new Dictionary<TimeframeCacheKey, int>(100);

        // Cache interpolation calculations with efficient key
        private static readonly Dictionary<TimeframeCacheKey, double> _interpolationCache =
            new Dictionary<TimeframeCacheKey, double>(100);

        // Limit cache size
        private const int MAX_CACHE_SIZE = 1000;
        private static DateTime _lastCachePurge = DateTime.MinValue;

        /// <summary>
        /// Converts a TimeFrame enum to a TimeSpan
        /// </summary>
        public static TimeSpan ToTimeSpan(this TimeFrame timeFrame)
        {
            // Check cache first
            if (_timeframeSpans.TryGetValue(timeFrame, out TimeSpan cachedSpan))
                return cachedSpan;

            // Calculate and cache
            TimeSpan span;

            if (timeFrame == TimeFrame.Minute)
                span = TimeSpan.FromMinutes(1);
            else if (timeFrame == TimeFrame.Minute2)
                span = TimeSpan.FromMinutes(2);
            else if (timeFrame == TimeFrame.Minute3)
                span = TimeSpan.FromMinutes(3);
            else if (timeFrame == TimeFrame.Minute4)
                span = TimeSpan.FromMinutes(4);
            else if (timeFrame == TimeFrame.Minute5)
                span = TimeSpan.FromMinutes(5);
            else if (timeFrame == TimeFrame.Minute6)
                span = TimeSpan.FromMinutes(6);
            else if (timeFrame == TimeFrame.Minute10)
                span = TimeSpan.FromMinutes(10);
            else if (timeFrame == TimeFrame.Minute15)
                span = TimeSpan.FromMinutes(15);
            else if (timeFrame == TimeFrame.Minute30)
                span = TimeSpan.FromMinutes(30);
            else if (timeFrame == TimeFrame.Hour)
                span = TimeSpan.FromHours(1);
            else if (timeFrame == TimeFrame.Hour2)
                span = TimeSpan.FromHours(2);
            else if (timeFrame == TimeFrame.Hour3)
                span = TimeSpan.FromHours(3);
            else if (timeFrame == TimeFrame.Hour4)
                span = TimeSpan.FromHours(4);
            else if (timeFrame == TimeFrame.Hour6)
                span = TimeSpan.FromHours(6);
            else if (timeFrame == TimeFrame.Hour8)
                span = TimeSpan.FromHours(8);
            else if (timeFrame == TimeFrame.Hour12)
                span = TimeSpan.FromHours(12);
            else if (timeFrame == TimeFrame.Daily)
                span = TimeSpan.FromDays(1);
            else if (timeFrame == TimeFrame.Day2)
                span = TimeSpan.FromDays(2);
            else if (timeFrame == TimeFrame.Day3)
                span = TimeSpan.FromDays(3);
            else if (timeFrame == TimeFrame.Weekly)
                span = TimeSpan.FromDays(7);
            else if (timeFrame == TimeFrame.Monthly)
                span = TimeSpan.FromDays(30);
            else
                span = TimeSpan.FromMinutes(1); // Default to 1 minute if unknown

            // Cache the result
            _timeframeSpans[timeFrame] = span;

            return span;
        }

        /// <summary>
        /// Gets the index in the source timeframe bars that corresponds to the target timeframe bar time
        /// </summary>
        /// <param name="targetTime">Target bar datetime</param>
        /// <param name="timeframeBars">Source timeframe bars</param>
        /// <param name="selectedTimeFrame">Selected timeframe</param>
        /// <returns>Index in the source timeframe bars, or -1 if not found</returns>
        public static int GetTimeframeIndex(DateTime targetTime, Bars timeframeBars, TimeFrame selectedTimeFrame)
        {
            // Create an efficient cache key
            var cacheKey = new TimeframeCacheKey(targetTime, timeframeBars.GetHashCode(), selectedTimeFrame);

            // Check cache
            if (_timeframeIndexCache.TryGetValue(cacheKey, out int cachedIndex))
                return cachedIndex;

            // Calculate timespan once
            TimeSpan timeframeSpan = selectedTimeFrame.ToTimeSpan();

            // Use binary search for better performance with large datasets
            int lo = 0;
            int hi = timeframeBars.Count - 1;
            int resultIndex = -1;

            while (lo <= hi)
            {
                int mid = lo + (hi - lo) / 2;
                DateTime barOpenTime = timeframeBars.OpenTimes[mid];
                DateTime barCloseTime = barOpenTime.Add(timeframeSpan);

                if (targetTime >= barOpenTime && targetTime < barCloseTime)
                {
                    resultIndex = mid;
                    break;
                }
                else if (targetTime < barOpenTime)
                {
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            // NEW CODE: If no exact match was found, find the closest bar backward
            if (resultIndex < 0)
            {
                // Binary search already narrowed position - use hi directly
                // hi points to largest index where OpenTime < targetTime
                if (hi >= 0 && timeframeBars.OpenTimes[hi] <= targetTime)
                {
                    resultIndex = hi;
                }

                // Cache the result if found
                if (resultIndex >= 0)
                {
                    _timeframeIndexCache[cacheKey] = resultIndex;

                    // Manage cache size
                    if (_timeframeIndexCache.Count > MAX_CACHE_SIZE)
                    {
                        PurgeOldCache();
                    }
                }
            }
            else
            {
                // Cache successful binary search result
                _timeframeIndexCache[cacheKey] = resultIndex;

                if (_timeframeIndexCache.Count > MAX_CACHE_SIZE)
                {
                    PurgeOldCache();
                }
            }

            return resultIndex;
        }

        /// <summary>
        /// Performs linear interpolation between two timeframe bar values
        /// </summary>
        /// <param name="time">Target time for interpolation</param>
        /// <param name="timeframeBars">Source timeframe bars</param>
        /// <param name="values">Values to interpolate</param>
        /// <param name="selectedTimeFrame">Selected timeframe</param>
        /// <returns>Interpolated value</returns>
        public static double Interpolate(DateTime time, Bars timeframeBars, double[] values, TimeFrame selectedTimeFrame)
        {
            // Find the current and next timeframe bar indices
            int currentTimeframeIndex = -1;
            int nextTimeframeIndex = -1;
            TimeSpan timeframeSpan = selectedTimeFrame.ToTimeSpan();

            // Check interpolation cache with efficient key
            var valuesHash = values.GetHashCode();
            var cacheKey = new TimeframeCacheKey(time, valuesHash, selectedTimeFrame);

            if (_interpolationCache.TryGetValue(cacheKey, out double cachedValue))
                return cachedValue;

            // Fast path: check if we're looking in a common timeframe bar
            currentTimeframeIndex = GetTimeframeIndex(time, timeframeBars, selectedTimeFrame);

            if (currentTimeframeIndex < 0 || currentTimeframeIndex >= values.Length)
                return double.NaN;

            nextTimeframeIndex = (currentTimeframeIndex < timeframeBars.Count - 1) ? currentTimeframeIndex + 1 : -1;

            // If there's no next bar, just return the current value
            if (nextTimeframeIndex < 0 || nextTimeframeIndex >= values.Length)
            {
                double singleBarValue = values[currentTimeframeIndex];
                _interpolationCache[cacheKey] = singleBarValue;
                return singleBarValue;
            }

            // Perform linear interpolation
            double currentValue = values[currentTimeframeIndex];
            double nextValue = values[nextTimeframeIndex];

            // Calculate how far we are between the current and next timeframe bars (0.0 to 1.0)
            DateTime currentBarOpenTime = timeframeBars.OpenTimes[currentTimeframeIndex];
            DateTime nextBarOpenTime = timeframeBars.OpenTimes[nextTimeframeIndex];
            double totalTimeDiff = (nextBarOpenTime - currentBarOpenTime).TotalMilliseconds;
            double currentTimeDiff = (time - currentBarOpenTime).TotalMilliseconds;

            // Avoid division by zero
            if (totalTimeDiff <= 0)
            {
                _interpolationCache[cacheKey] = currentValue;
                return currentValue;
            }

            double ratio = currentTimeDiff / totalTimeDiff;

            // Linear interpolation formula: currentValue + ratio * (nextValue - currentValue)
            double interpolatedValue = currentValue + ratio * (nextValue - currentValue);

            // Cache the result
            _interpolationCache[cacheKey] = interpolatedValue;

            // Manage cache size
            if (_interpolationCache.Count > MAX_CACHE_SIZE)
            {
                PurgeOldCache();
            }

            return interpolatedValue;
        }

        /// <summary>
        /// Purge old cache entries to prevent memory leaks
        /// </summary>
        private static void PurgeOldCache()
        {
            DateTime now = DateTime.UtcNow;

            // Only purge every 5 minutes
            if ((now - _lastCachePurge).TotalMinutes < 5)
                return;

            if (_timeframeIndexCache.Count > MAX_CACHE_SIZE)
            {
                // Remove approximately 20% of the oldest items
                int removeCount = MAX_CACHE_SIZE / 5;
                int removed = 0;

                // More efficient approach than creating a list of all keys
                var keysToRemove = new List<TimeframeCacheKey>();
                foreach (var key in _timeframeIndexCache.Keys)
                {
                    keysToRemove.Add(key);
                    removed++;

                    if (removed >= removeCount)
                        break;
                }

                foreach (var key in keysToRemove)
                {
                    _timeframeIndexCache.Remove(key);
                }
            }

            if (_interpolationCache.Count > MAX_CACHE_SIZE)
            {
                // Similar approach for interpolation cache
                int removeCount = MAX_CACHE_SIZE / 5;
                int removed = 0;

                var keysToRemove = new List<TimeframeCacheKey>();
                foreach (var key in _interpolationCache.Keys)
                {
                    keysToRemove.Add(key);
                    removed++;

                    if (removed >= removeCount)
                        break;
                }

                foreach (var key in keysToRemove)
                {
                    _interpolationCache.Remove(key);
                }
            }

            _lastCachePurge = now;
        }

        /// <summary>
        /// Clears all timeframe caches
        /// </summary>
        public static void ClearCaches()
        {
            _timeframeIndexCache.Clear();
            _interpolationCache.Clear();
        }

        /// <summary>
        /// Performs binary search to find the bar index by time
        /// </summary>
        public static int BinarySearchBarIndex(Bars bars, DateTime time, int low, int high)
        {
            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                DateTime midTime = bars.OpenTimes[mid];

                if (midTime == time)
                    return mid;
                else if (midTime < time)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return ~low; // Return bitwise complement of insertion point if not found
        }
    }
}
