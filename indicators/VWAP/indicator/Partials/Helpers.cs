using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class VWAP : Indicator
    {
        #region Helpers

        /// <summary>
        /// Update the session configuration in PeriodUtility
        /// </summary>
        private void UpdateSessionConfiguration()
        {
            int asianHour = AsianSessionHour;
            int londonHour = LondonSessionHour;
            int nyHour = NewYorkSessionHour;

            // Update the PeriodUtility with the current session configuration
            PeriodUtility.UpdateSessionHours(asianHour, londonHour, nyHour, TimezoneOffset);
        }

        /// <summary>
        /// Update the session/period information display
        /// </summary>
        private void UpdateSessionInfoDisplay()
        {
            // Clear existing text
            _sessionInfoText.Text = "";

            // Handle market sessions
            if (ResetPeriod == VwapResetPeriod.AsianSession ||
                ResetPeriod == VwapResetPeriod.LondonSession ||
                ResetPeriod == VwapResetPeriod.NewYorkSession)
            {
                // Get the session hour based on type
                int sessionHour;
                string sessionName;

                switch (ResetPeriod)
                {
                    case VwapResetPeriod.AsianSession:
                        sessionHour = AsianSessionHour;
                        sessionName = "Asian";
                        break;
                    case VwapResetPeriod.LondonSession:
                        sessionHour = LondonSessionHour;
                        sessionName = "London";
                        break;
                    case VwapResetPeriod.NewYorkSession:
                        sessionHour = NewYorkSessionHour;
                        sessionName = "New York";
                        break;
                    default:
                        sessionHour = 0;
                        sessionName = "Unknown";
                        break;
                }

                // Format the message
                string infoText = $"{sessionName} Session VWAP\nStart Time: {sessionHour:D2}:00";

                // Update the display
                _sessionInfoText.Text = infoText;
            }
            // Handle hourly periods
            else if (ResetPeriod == VwapResetPeriod.OneHour ||
                ResetPeriod == VwapResetPeriod.TwoHour ||
                ResetPeriod == VwapResetPeriod.ThreeHour ||
                ResetPeriod == VwapResetPeriod.FourHour ||
                ResetPeriod == VwapResetPeriod.SixHour ||
                ResetPeriod == VwapResetPeriod.EightHour ||
                ResetPeriod == VwapResetPeriod.TwelveHour)
            {
                string periodName;
                string resetInfo;

                switch (ResetPeriod)
                {
                    case VwapResetPeriod.OneHour:
                        periodName = "1-Hour";
                        resetInfo = "Reset every hour";
                        break;
                    case VwapResetPeriod.TwoHour:
                        periodName = "2-Hour";
                        // Show times with timezone adjustment
                        if (TimezoneOffset != 0)
                        {
                            int[] hours = { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22 };
                            string[] adjustedHours = new string[hours.Length];
                            for (int i = 0; i < hours.Length; i++)
                            {
                                int adjusted = (hours[i] + TimezoneOffset) % 24;
                                if (adjusted < 0) adjusted += 24;
                                adjustedHours[i] = $"{adjusted:D2}:00";
                            }

                            // Create first line with first 6 hours
                            string firstLine = $"{adjustedHours[0]}, {adjustedHours[1]}, {adjustedHours[2]}, {adjustedHours[3]}, {adjustedHours[4]}, {adjustedHours[5]}";
                            // Create second line with remaining 6 hours
                            string secondLine = $"{adjustedHours[6]}, {adjustedHours[7]}, {adjustedHours[8]}, {adjustedHours[9]}, {adjustedHours[10]}, {adjustedHours[11]}";

                            resetInfo = $"Reset at: {firstLine},\n{secondLine}";
                        }
                        else
                        {
                            resetInfo = "Reset at: 00:00, 02:00, 04:00, 06:00, 08:00, 10:00,\n12:00, 14:00, 16:00, 18:00, 20:00, 22:00";
                        }
                        break;
                    case VwapResetPeriod.ThreeHour:
                        periodName = "3-Hour";
                        // Show times with timezone adjustment
                        if (TimezoneOffset != 0)
                        {
                            int[] hours = { 0, 3, 6, 9, 12, 15, 18, 21 };
                            string[] adjustedHours = new string[hours.Length];
                            for (int i = 0; i < hours.Length; i++)
                            {
                                int adjusted = (hours[i] + TimezoneOffset) % 24;
                                if (adjusted < 0) adjusted += 24;
                                adjustedHours[i] = $"{adjusted:D2}:00";
                            }

                            // Create first line with first 4 hours
                            string firstLine = $"{adjustedHours[0]}, {adjustedHours[1]}, {adjustedHours[2]}, {adjustedHours[3]}";
                            // Create second line with remaining 4 hours
                            string secondLine = $"{adjustedHours[4]}, {adjustedHours[5]}, {adjustedHours[6]}, {adjustedHours[7]}";

                            resetInfo = $"Reset at: {firstLine},\n{secondLine}";
                        }
                        else
                        {
                            resetInfo = "Reset at: 00:00, 03:00, 06:00, 09:00,\n12:00, 15:00, 18:00, 21:00";
                        }
                        break;
                    case VwapResetPeriod.FourHour:
                        periodName = "4-Hour";
                        // Show times with timezone adjustment
                        if (TimezoneOffset != 0)
                        {
                            int[] hours = { 0, 4, 8, 12, 16, 20 };
                            string[] adjustedHours = new string[hours.Length];
                            for (int i = 0; i < hours.Length; i++)
                            {
                                int adjusted = (hours[i] + TimezoneOffset) % 24;
                                if (adjusted < 0) adjusted += 24;
                                adjustedHours[i] = $"{adjusted:D2}:00";
                            }
                            resetInfo = $"Reset at: {adjustedHours[0]}, {adjustedHours[1]}, {adjustedHours[2]},\n{adjustedHours[3]}, {adjustedHours[4]}, {adjustedHours[5]}";
                        }
                        else
                        {
                            resetInfo = "Reset at: 00:00, 04:00, 08:00,\n12:00, 16:00, 20:00";
                        }
                        break;
                    case VwapResetPeriod.SixHour:
                        periodName = "6-Hour";
                        // Show times with timezone adjustment
                        if (TimezoneOffset != 0)
                        {
                            int[] hours = { 0, 6, 12, 18 };
                            string[] adjustedHours = new string[hours.Length];
                            for (int i = 0; i < hours.Length; i++)
                            {
                                int adjusted = (hours[i] + TimezoneOffset) % 24;
                                if (adjusted < 0) adjusted += 24;
                                adjustedHours[i] = $"{adjusted:D2}:00";
                            }
                            resetInfo = $"Reset at: {adjustedHours[0]}, {adjustedHours[1]}, {adjustedHours[2]}, {adjustedHours[3]}";
                        }
                        else
                        {
                            resetInfo = "Reset at: 00:00, 06:00, 12:00, 18:00";
                        }
                        break;
                    case VwapResetPeriod.EightHour:
                        periodName = "8-Hour";
                        // Show times with timezone adjustment
                        if (TimezoneOffset != 0)
                        {
                            int[] hours = { 0, 8, 16 };
                            string[] adjustedHours = new string[hours.Length];
                            for (int i = 0; i < hours.Length; i++)
                            {
                                int adjusted = (hours[i] + TimezoneOffset) % 24;
                                if (adjusted < 0) adjusted += 24;
                                adjustedHours[i] = $"{adjusted:D2}:00";
                            }
                            resetInfo = $"Reset at: {adjustedHours[0]}, {adjustedHours[1]}, {adjustedHours[2]}";
                        }
                        else
                        {
                            resetInfo = "Reset at: 00:00, 08:00, 16:00";
                        }
                        break;
                    case VwapResetPeriod.TwelveHour:
                        periodName = "12-Hour";
                        // Show times with timezone adjustment
                        if (TimezoneOffset != 0)
                        {
                            int[] hours = { 0, 12 };
                            string[] adjustedHours = new string[hours.Length];
                            for (int i = 0; i < hours.Length; i++)
                            {
                                int adjusted = (hours[i] + TimezoneOffset) % 24;
                                if (adjusted < 0) adjusted += 24;
                                adjustedHours[i] = $"{adjusted:D2}:00";
                            }
                            resetInfo = $"Reset at: {adjustedHours[0]}, {adjustedHours[1]}";
                        }
                        else
                        {
                            resetInfo = "Reset at: 00:00, 12:00";
                        }
                        break;
                    default:
                        periodName = "Hourly";
                        resetInfo = "";
                        break;
                }

                // Apply timezone info for hourly periods
                if (TimezoneOffset != 0)
                {
                    string tzSign = TimezoneOffset > 0 ? "+" : "";
                    resetInfo += $"\n(UTC{tzSign}{TimezoneOffset})";
                }

                // Format the message
                string infoText = $"{periodName} VWAP\n{resetInfo}";

                // Update the display
                _sessionInfoText.Text = infoText;
            }
        }

        /// <summary>
        /// Display an error message on the chart
        /// </summary>
        private void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.Text = message;
            }
        }

        /// <summary>
        /// Clear any displayed error message
        /// </summary>
        private void ClearError()
        {
            if (_errorText != null)
            {
                _errorText.Text = "";
            }
        }

        /// <summary>
        /// Parse anchor point from combined date and time string
        /// </summary>
        private DateTime? ParseAnchorDateTime(string dateTimeStr)
        {
            try
            {
                // Clear any previous error message
                ClearError();

                // Split the combined string into date and time parts
                string[] parts = dateTimeStr.Trim().Split(' ');
                if (parts.Length != 2)
                {
                    ShowError("VWAP Error: Invalid anchor datetime format.\nExpected: dd/mm/yyyy hh:mm");
                    return null;
                }

                string dateStr = parts[0];
                string timeStr = parts[1];

                // Parse the dd/mm/yyyy format
                string[] dateParts = dateStr.Split('/');
                if (dateParts.Length != 3)
                {
                    ShowError("VWAP Error: Invalid date format.\nExpected: dd/mm/yyyy");
                    return null;
                }

                // Parse the hh:mm format
                string[] timeParts = timeStr.Split(':');
                if (timeParts.Length != 2)
                {
                    ShowError("VWAP Error: Invalid time format.\nExpected: hh:mm");
                    return null;
                }

                int day = int.Parse(dateParts[0]);
                int month = int.Parse(dateParts[1]);
                int year = int.Parse(dateParts[2]);
                int hour = int.Parse(timeParts[0]);
                int minute = int.Parse(timeParts[1]);

                DateTime parsedDateTime = new DateTime(year, month, day, hour, minute, 0);

                // Validate the date is not in the future
                if (parsedDateTime > DateTime.Now)
                {
                    ShowError("VWAP Error: Anchor datetime cannot be in the future");
                    return null;
                }

                return parsedDateTime;
            }
            catch (Exception ex)
            {
                ShowError($"VWAP Error: {ex.Message}.\nExpected format: dd/mm/yyyy hh:mm");
                return null;
            }
        }

        #endregion
    }
}
