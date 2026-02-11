using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Handle all date and time operations
    /// </summary>
    public class DateTimeHelper
    {
        /// <summary>
        /// Parse date string and return result
        /// </summary>
        public bool TryParseDateTime(string dateTimeString, out DateTime result)
        {
            result = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(dateTimeString))
                return false;

            try
            {
                string trimmed = dateTimeString.Trim();

                // Split by space to separate date and time
                string[] parts = trimmed.Split(' ');
                string datePart = parts[0];
                string timePart = parts.Length > 1 ? parts[1] : null;

                // Parse date (dd/MM/yyyy or d/M/yy formats)
                if (!TryParseDate(datePart, out DateTime date))
                    return false;

                // Parse time if provided
                if (!string.IsNullOrWhiteSpace(timePart))
                {
                    if (!TryParseTime(timePart, out int hours, out int minutes))
                        return false;

                    result = new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);
                }
                else
                {
                    // No time provided - use midnight
                    result = date.Date;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseDate(string datePart, out DateTime date)
        {
            date = DateTime.MinValue;

            string[] dateParts = datePart.Split('/');

            // Support both dd/MM and dd/MM/yyyy formats
            if (dateParts.Length < 2 || dateParts.Length > 3)
                return false;

            if (!int.TryParse(dateParts[0], out int day) || day < 1 || day > 31)
                return false;

            if (!int.TryParse(dateParts[1], out int month) || month < 1 || month > 12)
                return false;

            int year;

            if (dateParts.Length == 2)
            {
                // No year provided - use current year
                year = DateTime.Now.Year;
            }
            else
            {
                if (!int.TryParse(dateParts[2], out year))
                    return false;

                // Handle 2-digit year (e.g., 25 → 2025)
                if (year < 100)
                    year += 2000;
            }

            try
            {
                date = new DateTime(year, month, day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseTime(string timePart, out int hours, out int minutes)
        {
            hours = 0;
            minutes = 0;

            // Support single hour (e.g., "8") or HH:mm format
            if (!timePart.Contains(":"))
            {
                if (!int.TryParse(timePart, out hours) || hours < 0 || hours > 23)
                    return false;
                return true;
            }

            string[] timeParts = timePart.Split(':');
            if (timeParts.Length != 2)
                return false;

            if (!int.TryParse(timeParts[0], out hours) || hours < 0 || hours > 23)
                return false;

            if (!int.TryParse(timeParts[1], out minutes) || minutes < 0 || minutes > 59)
                return false;

            return true;
        }

        /// <summary>
        /// Find the first bar index that matches start date
        /// </summary>
        public int FindStartBarIndex(Bars bars, DateTime startDate, int currentIndex)
        {
            // Search in recent bars (max 1000 bars back)
            int searchStart = Math.Max(0, currentIndex - 1000);

            for (int i = searchStart; i <= currentIndex; i++)
            {
                if (i < bars.OpenTimes.Count && bars.OpenTimes[i] >= startDate)
                {
                    return i;
                }
            }

            return -1; // Not found
        }

        /// <summary>
        /// Check if current bar time is valid for calculation
        /// </summary>
        public bool IsBarTimeValid(DateTime barTime, DateTime startDate)
        {
            return barTime >= startDate;
        }
    }
}
