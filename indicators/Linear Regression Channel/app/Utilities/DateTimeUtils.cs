using System;
using System.Globalization;

namespace cAlgo.Indicators
{
    public static class DateTimeUtils
    {
        // Standard date format used by the indicator
        private const string DateFormat = "dd/MM/yyyy HH:mm";
        
        // Culture info for consistent parsing
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        
        /// <summary>
        /// Parse a date string in the standard format (dd/MM/yyyy HH:mm)
        /// </summary>
        /// <param name="dateStr">Date string to parse</param>
        /// <returns>DateTime object or DateTime.MinValue if parsing fails</returns>
        public static DateTime ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return DateTime.MinValue;
                
            try
            {
                return DateTime.ParseExact(dateStr, DateFormat, Culture);
            }
            catch (Exception)
            {
                // Try alternate formats if the standard format fails
                try
                {
                    return DateTime.Parse(dateStr, Culture);
                }
                catch (Exception)
                {
                    return DateTime.MinValue;
                }
            }
        }
        
        /// <summary>
        /// Format a DateTime as a string in the standard format
        /// </summary>
        /// <param name="date">DateTime to format</param>
        /// <returns>Formatted date string</returns>
        public static string FormatDate(DateTime date)
        {
            return date.ToString(DateFormat, Culture);
        }
        
        /// <summary>
        /// Check if a date is valid (not min or max value)
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if the date is valid</returns>
        public static bool IsValidDate(DateTime date)
        {
            return date != DateTime.MinValue && date != DateTime.MaxValue;
        }
        
        /// <summary>
        /// Get a human-readable description of a time span
        /// </summary>
        /// <param name="timeSpan">TimeSpan to describe</param>
        /// <returns>Human-readable description</returns>
        public static string GetTimeSpanDescription(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 365)
            {
                double years = Math.Round(timeSpan.TotalDays / 365, 1);
                return $"{years} {(years == 1 ? "year" : "years")}";
            }
            if (timeSpan.TotalDays >= 30)
            {
                double months = Math.Round(timeSpan.TotalDays / 30, 1);
                return $"{months} {(months == 1 ? "month" : "months")}";
            }
            if (timeSpan.TotalDays >= 1)
            {
                return $"{timeSpan.Days} {(timeSpan.Days == 1 ? "day" : "days")}";
            }
            if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours} {(timeSpan.Hours == 1 ? "hour" : "hours")}";
            }
            
            return $"{timeSpan.Minutes} {(timeSpan.Minutes == 1 ? "minute" : "minutes")}";
        }
    }
}
