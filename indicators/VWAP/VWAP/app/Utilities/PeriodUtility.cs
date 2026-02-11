using System;
using System.Collections.Generic;
using System.Globalization;
using cAlgo.API;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Utility class for period-related calculations
    /// </summary>
    public static class PeriodUtility
    {
        // Cache for ISO week numbers - using limited size, self-managing cache
        private static readonly Dictionary<DateTime, int> WeekNumberCache = new Dictionary<DateTime, int>(365);

        // Cache for period start times - using limited size, self-managing cache
        private static readonly Dictionary<string, DateTime> PeriodStartCache = new Dictionary<string, DateTime>(200);

        // Cache for period comparison results - using limited size, self-managing cache
        private static readonly Dictionary<string, bool> PeriodComparisonCache = new Dictionary<string, bool>(200);

        // Default market session start times in UTC (hour, minute)
        private static readonly Dictionary<VwapResetPeriod, Tuple<int, int>> DefaultSessionStartTimes =
            new Dictionary<VwapResetPeriod, Tuple<int, int>>
            {
                { VwapResetPeriod.AsianSession, new Tuple<int, int>(0, 0) },      // 00:00 UTC
                { VwapResetPeriod.LondonSession, new Tuple<int, int>(7, 0) },     // 07:00 UTC
                { VwapResetPeriod.NewYorkSession, new Tuple<int, int>(12, 0) }    // 12:00 UTC
            };

        // Cache management constants
        private const int MAX_CACHE_SIZE_WEEK = 100;
        private const int MAX_CACHE_SIZE_PERIOD = 50;
        private static int _cacheHitCount = 0;
        private static readonly int CLEANUP_INTERVAL = 1000;

        // Session hours configuration
        private static int _asianSessionHour = 0;
        private static int _londonSessionHour = 7;
        private static int _newYorkSessionHour = 12;
        private static int _timezoneOffset = 0;

        /// <summary>
        /// Update the session hours configuration
        /// </summary>
        public static void UpdateSessionHours(int asianHour, int londonHour, int newYorkHour, int timezoneOffset)
        {
            _asianSessionHour = asianHour;
            _londonSessionHour = londonHour;
            _newYorkSessionHour = newYorkHour;
            _timezoneOffset = timezoneOffset;

            // Clear caches when session hours change to ensure correct calculations
            ClearCaches();
        }

        /// <summary>
        /// Determines if the current time is in a different period than the period start time
        /// </summary>
        public static bool IsDifferentPeriod(DateTime currentTime, DateTime periodStartTime,
            VwapResetPeriod resetPeriod, DateTime? anchorPoint = null, Bars bars = null)
        {
            // For simple cases, use direct comparison without caching
            if (resetPeriod == VwapResetPeriod.Daily)
                return currentTime.Date != periodStartTime.Date;

            if (resetPeriod == VwapResetPeriod.Monthly)
                return currentTime.Month != periodStartTime.Month || currentTime.Year != periodStartTime.Year;

            if (resetPeriod == VwapResetPeriod.Yearly)
                return currentTime.Year != periodStartTime.Year;

            if (resetPeriod == VwapResetPeriod.Never)
                return false;

            // For more complex cases, use caching
            string cacheKey = $"{currentTime.Ticks}_{periodStartTime.Ticks}_{resetPeriod}_{(anchorPoint.HasValue ? anchorPoint.Value.Ticks : 0)}_{_asianSessionHour}_{_londonSessionHour}_{_newYorkSessionHour}_{_timezoneOffset}";

            // Check if result is in cache
            if (PeriodComparisonCache.TryGetValue(cacheKey, out bool result))
            {
                IncrementCacheHit();
                return result;
            }

            // Calculate the result
            result = CalculateIsDifferentPeriod(currentTime, periodStartTime, resetPeriod, anchorPoint, bars);

            // Store in cache (with size management)
            if (PeriodComparisonCache.Count >= MAX_CACHE_SIZE_PERIOD)
            {
                PeriodComparisonCache.Clear();
            }
            PeriodComparisonCache[cacheKey] = result;

            return result;
        }

        /// <summary>
        /// Internal calculation for period difference
        /// </summary>
        private static bool CalculateIsDifferentPeriod(DateTime currentTime, DateTime periodStartTime,
            VwapResetPeriod resetPeriod, DateTime? anchorPoint = null, Bars bars = null)
        {
            switch (resetPeriod)
            {
                case VwapResetPeriod.Daily:
                    return currentTime.Date != periodStartTime.Date;

                case VwapResetPeriod.Weekly:
                    // Enhanced check for weekly reset
                    if (bars != null && bars.TimeFrame >= TimeFrame.Weekly)
                    {
                        // Simple check: different week of year
                        return currentTime.Year != periodStartTime.Year ||
                               GetWeekNumberBasic(currentTime) != GetWeekNumberBasic(periodStartTime);
                    }
                    // Otherwise use the original ISO week calculation for lower timeframes
                    int currentWeek = GetCachedISOWeekNumber(currentTime);
                    int periodWeek = GetCachedISOWeekNumber(periodStartTime);
                    return currentWeek != periodWeek || currentTime.Year != periodStartTime.Year;

                case VwapResetPeriod.Monthly:
                    return currentTime.Month != periodStartTime.Month || currentTime.Year != periodStartTime.Year;

                case VwapResetPeriod.Yearly:
                    return currentTime.Year != periodStartTime.Year;

                case VwapResetPeriod.OneHour:
                    // Convert times to timezone-adjusted times
                    DateTime current1HAdjustedTime = currentTime.AddHours(_timezoneOffset);
                    DateTime period1HAdjustedTime = periodStartTime.AddHours(_timezoneOffset);

                    // Compare day and hour
                    return current1HAdjustedTime.Date != period1HAdjustedTime.Date ||
                           current1HAdjustedTime.Hour != period1HAdjustedTime.Hour;

                case VwapResetPeriod.TwoHour:
                    // Convert times to timezone-adjusted times
                    DateTime current2HAdjustedTime = currentTime.AddHours(_timezoneOffset);
                    DateTime period2HAdjustedTime = periodStartTime.AddHours(_timezoneOffset);

                    // Get adjusted hour groups
                    int current2HAdjustedGroup = current2HAdjustedTime.Hour / 2;
                    int period2HAdjustedGroup = period2HAdjustedTime.Hour / 2;

                    // Compare day and group
                    return current2HAdjustedTime.Date != period2HAdjustedTime.Date ||
                           current2HAdjustedGroup != period2HAdjustedGroup;

                case VwapResetPeriod.ThreeHour:
                    // Convert times to timezone-adjusted times
                    DateTime current3HAdjustedTime = currentTime.AddHours(_timezoneOffset);
                    DateTime period3HAdjustedTime = periodStartTime.AddHours(_timezoneOffset);

                    // Get adjusted hour groups
                    int current3HAdjustedGroup = current3HAdjustedTime.Hour / 3;
                    int period3HAdjustedGroup = period3HAdjustedTime.Hour / 3;

                    // Compare day and group
                    return current3HAdjustedTime.Date != period3HAdjustedTime.Date ||
                           current3HAdjustedGroup != period3HAdjustedGroup;

                case VwapResetPeriod.FourHour:
                    // Convert times to timezone-adjusted times
                    DateTime current4HAdjustedTime = currentTime.AddHours(_timezoneOffset);
                    DateTime period4HAdjustedTime = periodStartTime.AddHours(_timezoneOffset);

                    // Get adjusted hour groups
                    int current4HAdjustedGroup = current4HAdjustedTime.Hour / 4;
                    int period4HAdjustedGroup = period4HAdjustedTime.Hour / 4;

                    // Compare day and group
                    return current4HAdjustedTime.Date != period4HAdjustedTime.Date ||
                           current4HAdjustedGroup != period4HAdjustedGroup;

                case VwapResetPeriod.SixHour:
                    // Convert times to timezone-adjusted times
                    DateTime current6HAdjustedTime = currentTime.AddHours(_timezoneOffset);
                    DateTime period6HAdjustedTime = periodStartTime.AddHours(_timezoneOffset);

                    // Get adjusted hour groups
                    int current6HAdjustedGroup = current6HAdjustedTime.Hour / 6;
                    int period6HAdjustedGroup = period6HAdjustedTime.Hour / 6;

                    // Compare day and group
                    return current6HAdjustedTime.Date != period6HAdjustedTime.Date ||
                           current6HAdjustedGroup != period6HAdjustedGroup;

                case VwapResetPeriod.EightHour:
                    // Convert times to timezone-adjusted times
                    DateTime currentAdjustedTime = currentTime.AddHours(_timezoneOffset);
                    DateTime periodAdjustedTime = periodStartTime.AddHours(_timezoneOffset);

                    // Get adjusted hour groups
                    int currentAdjustedGroup = currentAdjustedTime.Hour / 8;
                    int periodAdjustedGroup = periodAdjustedTime.Hour / 8;

                    // Compare day and group
                    return currentAdjustedTime.Date != periodAdjustedTime.Date ||
                           currentAdjustedGroup != periodAdjustedGroup;

                case VwapResetPeriod.TwelveHour:
                    // Convert times to timezone-adjusted times
                    DateTime current12HAdjustedTime = currentTime.AddHours(_timezoneOffset);
                    DateTime period12HAdjustedTime = periodStartTime.AddHours(_timezoneOffset);

                    // Get adjusted hour groups
                    int current12HAdjustedGroup = current12HAdjustedTime.Hour / 12;
                    int period12HAdjustedGroup = period12HAdjustedTime.Hour / 12;

                    // Compare day and group
                    return current12HAdjustedTime.Date != period12HAdjustedTime.Date ||
                           current12HAdjustedGroup != period12HAdjustedGroup;

                case VwapResetPeriod.AsianSession:
                case VwapResetPeriod.LondonSession:
                case VwapResetPeriod.NewYorkSession:
                    // Get the session hour
                    int sessionHour = GetSessionHour(resetPeriod);
                    DateTime nextMarketSessionStart;

                    // Calculate session start time
                    if (periodStartTime.Hour < sessionHour)
                    {
                        nextMarketSessionStart = periodStartTime.Date.AddHours(sessionHour);
                    }
                    else
                    {
                        nextMarketSessionStart = periodStartTime.Date.AddDays(1).AddHours(sessionHour);
                    }

                    // If current time is past the next session start, we've reset
                    return currentTime >= nextMarketSessionStart;

                case VwapResetPeriod.AnchorPoint:
                    if (anchorPoint == null)
                        return false;

                    // Reset if we've reached the anchor point but haven't reset yet
                    return currentTime >= anchorPoint.Value && periodStartTime < anchorPoint.Value;

                case VwapResetPeriod.Never:
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the start time of the period containing the specified time
        /// </summary>
        public static DateTime GetPeriodStartTime(DateTime time, VwapResetPeriod resetPeriod,
            DateTime? anchorPoint = null, Bars bars = null)
        {
            // For simple cases, calculate directly without caching for better performance
            if (resetPeriod == VwapResetPeriod.Daily)
                return time.Date;

            if (resetPeriod == VwapResetPeriod.Monthly)
                return new DateTime(time.Year, time.Month, 1);

            if (resetPeriod == VwapResetPeriod.Yearly)
                return new DateTime(time.Year, 1, 1);

            // For more complex cases, use caching
            string cacheKey = $"{time.Ticks}_{resetPeriod}_{(anchorPoint.HasValue ? anchorPoint.Value.Ticks : 0)}_{(bars != null ? bars.OpenTimes[0].Ticks : 0)}_{_asianSessionHour}_{_londonSessionHour}_{_newYorkSessionHour}_{_timezoneOffset}";

            // Check if result is in cache
            if (PeriodStartCache.TryGetValue(cacheKey, out DateTime result))
            {
                IncrementCacheHit();
                return result;
            }

            // Calculate the result
            result = CalculatePeriodStartTime(time, resetPeriod, anchorPoint, bars);

            // Store in cache (with size management)
            if (PeriodStartCache.Count >= MAX_CACHE_SIZE_PERIOD)
            {
                PeriodStartCache.Clear();
            }
            PeriodStartCache[cacheKey] = result;

            return result;
        }

        /// <summary>
        /// Internal calculation for period start time
        /// </summary>
        private static DateTime CalculatePeriodStartTime(DateTime time, VwapResetPeriod resetPeriod,
            DateTime? anchorPoint = null, Bars bars = null)
        {
            switch (resetPeriod)
            {
                case VwapResetPeriod.Daily:
                    return time.Date;

                case VwapResetPeriod.Weekly:
                    // Get the Monday of the current week
                    int dayOfWeek = (int)time.DayOfWeek;
                    int daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // Sunday is 0, so handle specially
                    return time.Date.AddDays(-daysToSubtract);

                case VwapResetPeriod.Monthly:
                    return new DateTime(time.Year, time.Month, 1);

                case VwapResetPeriod.Yearly:
                    return new DateTime(time.Year, 1, 1);

                case VwapResetPeriod.OneHour:
                    // Convert local time to timezone-adjusted time
                    DateTime adjusted1HTime = time.AddHours(_timezoneOffset);

                    // Create datetime for hour start in adjusted timezone
                    DateTime adjusted1HPeriodStart = new DateTime(
                        adjusted1HTime.Year,
                        adjusted1HTime.Month,
                        adjusted1HTime.Day,
                        adjusted1HTime.Hour,
                        0,
                        0);

                    // Convert back to local timezone
                    return adjusted1HPeriodStart.AddHours(-_timezoneOffset);

                case VwapResetPeriod.TwoHour:
                    // Convert local time to timezone-adjusted time
                    DateTime adjusted2HTime = time.AddHours(_timezoneOffset);

                    // Find the 2-hour period start in the adjusted timezone
                    int adjusted2HGroup = adjusted2HTime.Hour / 2;
                    int adjusted2HStart = adjusted2HGroup * 2;

                    // Create datetime for period start in adjusted timezone
                    DateTime adjusted2HPeriodStart = new DateTime(
                        adjusted2HTime.Year,
                        adjusted2HTime.Month,
                        adjusted2HTime.Day,
                        adjusted2HStart,
                        0,
                        0);

                    // Convert back to local timezone
                    return adjusted2HPeriodStart.AddHours(-_timezoneOffset);

                case VwapResetPeriod.ThreeHour:
                    // Convert local time to timezone-adjusted time
                    DateTime adjusted3HTime = time.AddHours(_timezoneOffset);

                    // Find the 3-hour period start in the adjusted timezone
                    int adjusted3HGroup = adjusted3HTime.Hour / 3;
                    int adjusted3HStart = adjusted3HGroup * 3;

                    // Create datetime for period start in adjusted timezone
                    DateTime adjusted3HPeriodStart = new DateTime(
                        adjusted3HTime.Year,
                        adjusted3HTime.Month,
                        adjusted3HTime.Day,
                        adjusted3HStart,
                        0,
                        0);

                    // Convert back to local timezone
                    return adjusted3HPeriodStart.AddHours(-_timezoneOffset);

                case VwapResetPeriod.FourHour:
                    // Convert local time to timezone-adjusted time
                    DateTime adjusted4HTime = time.AddHours(_timezoneOffset);

                    // Find the 4-hour period start in the adjusted timezone
                    int adjusted4HGroup = adjusted4HTime.Hour / 4;
                    int adjusted4HStart = adjusted4HGroup * 4;

                    // Create datetime for period start in adjusted timezone
                    DateTime adjusted4HPeriodStart = new DateTime(
                        adjusted4HTime.Year,
                        adjusted4HTime.Month,
                        adjusted4HTime.Day,
                        adjusted4HStart,
                        0,
                        0);

                    // Convert back to local timezone
                    return adjusted4HPeriodStart.AddHours(-_timezoneOffset);

                case VwapResetPeriod.SixHour:
                    // Convert local time to timezone-adjusted time
                    DateTime adjusted6HTime = time.AddHours(_timezoneOffset);

                    // Find the 6-hour period start in the adjusted timezone
                    int adjusted6HGroup = adjusted6HTime.Hour / 6;
                    int adjusted6HStart = adjusted6HGroup * 6;

                    // Create datetime for period start in adjusted timezone
                    DateTime adjusted6HPeriodStart = new DateTime(
                        adjusted6HTime.Year,
                        adjusted6HTime.Month,
                        adjusted6HTime.Day,
                        adjusted6HStart,
                        0,
                        0);

                    // Convert back to local timezone
                    return adjusted6HPeriodStart.AddHours(-_timezoneOffset);

                case VwapResetPeriod.EightHour:
                    // Convert local time to timezone-adjusted time
                    DateTime adjustedTime = time.AddHours(_timezoneOffset);

                    // Find the 8-hour period start in the adjusted timezone
                    int adjustedHourGroup = adjustedTime.Hour / 8;
                    int adjustedStartHour = adjustedHourGroup * 8;

                    // Create datetime for period start in adjusted timezone
                    DateTime adjustedPeriodStart = new DateTime(
                        adjustedTime.Year,
                        adjustedTime.Month,
                        adjustedTime.Day,
                        adjustedStartHour,
                        0,
                        0);

                    // Convert back to local timezone
                    return adjustedPeriodStart.AddHours(-_timezoneOffset);

                case VwapResetPeriod.TwelveHour:
                    // Convert local time to timezone-adjusted time
                    DateTime adjusted12HTime = time.AddHours(_timezoneOffset);

                    // Find the 12-hour period start in the adjusted timezone
                    int adjusted12HGroup = adjusted12HTime.Hour / 12;
                    int adjusted12HStart = adjusted12HGroup * 12;

                    // Create datetime for period start in adjusted timezone
                    DateTime adjusted12HPeriodStart = new DateTime(
                        adjusted12HTime.Year,
                        adjusted12HTime.Month,
                        adjusted12HTime.Day,
                        adjusted12HStart,
                        0,
                        0);

                    // Convert back to local timezone
                    return adjusted12HPeriodStart.AddHours(-_timezoneOffset);

                case VwapResetPeriod.AsianSession:
                case VwapResetPeriod.LondonSession:
                case VwapResetPeriod.NewYorkSession:
                    // Get the session hour
                    int sessionHour = GetSessionHour(resetPeriod);

                    // Calculate the current session start
                    if (time.Hour < sessionHour)
                    {
                        // Before today's session - use yesterday's session
                        return time.Date.AddDays(-1).AddHours(sessionHour);
                    }
                    else
                    {
                        // After session start - use today's session
                        return time.Date.AddHours(sessionHour);
                    }

                case VwapResetPeriod.AnchorPoint:
                    if (anchorPoint != null && time >= anchorPoint.Value)
                        return anchorPoint.Value;
                    else
                        return bars != null ? bars.OpenTimes[0] : DateTime.MinValue;

                case VwapResetPeriod.Never:
                    return bars != null ? bars.OpenTimes[0] : DateTime.MinValue;

                default:
                    return time.Date;
            }
        }

        /// <summary>
        /// Gets the hour for the specified market session
        /// </summary>
        private static int GetSessionHour(VwapResetPeriod sessionType)
        {
            switch (sessionType)
            {
                case VwapResetPeriod.AsianSession:
                    return _asianSessionHour;

                case VwapResetPeriod.LondonSession:
                    return _londonSessionHour;

                case VwapResetPeriod.NewYorkSession:
                    return _newYorkSessionHour;

                default:
                    // Default case - lookup in the defaults table
                    if (DefaultSessionStartTimes.TryGetValue(sessionType, out var sessionTime))
                    {
                        return sessionTime.Item1; // Return the hour component
                    }
                    return 0; // Fallback to midnight
            }
        }

        /// <summary>
        /// Gets the ISO week number for a date with caching for performance
        /// </summary>
        public static int GetCachedISOWeekNumber(DateTime time)
        {
            // Use just the date part for caching
            DateTime dateKey = time.Date;

            // Check if in cache
            if (WeekNumberCache.TryGetValue(dateKey, out int weekNumber))
            {
                IncrementCacheHit();
                return weekNumber;
            }

            // Calculate week number
            weekNumber = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                time,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            // Store in cache (with size management)
            if (WeekNumberCache.Count >= MAX_CACHE_SIZE_WEEK)
            {
                WeekNumberCache.Clear();
            }
            WeekNumberCache[dateKey] = weekNumber;

            return weekNumber;
        }

        /// <summary>
        /// Gets a basic week number using standard calendar rules
        /// </summary>
        private static int GetWeekNumberBasic(DateTime time)
        {
            return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                time,
                System.Globalization.CalendarWeekRule.FirstDay,
                DayOfWeek.Monday);
        }

        /// <summary>
        /// Increment the cache hit counter and clean caches if threshold is reached
        /// </summary>
        private static void IncrementCacheHit()
        {
            _cacheHitCount++;

            // Periodically check and clean up caches to prevent memory buildup
            if (_cacheHitCount > CLEANUP_INTERVAL)
            {
                ClearCaches();
                _cacheHitCount = 0;
            }
        }

        /// <summary>
        /// Clear all caches - called automatically after CLEANUP_INTERVAL cache hits
        /// </summary>
        public static void ClearCaches()
        {
            WeekNumberCache.Clear();
            PeriodStartCache.Clear();
            PeriodComparisonCache.Clear();
        }
    }
}
