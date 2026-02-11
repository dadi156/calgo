using System;
using System.Globalization;
using cAlgo.API;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Simple mode manager for Regression Channel
    /// Handles StartPointMethod and Lock mechanism only
    /// </summary>
    public class RegressionModeManager
    {
        #region Parameters
        private StartPointMethod _startPointMethod;
        private bool _historicalBarsOnly;
        private bool _enableLock;
        private string _lockDateString;
        private DateTime _lockDateTime;
        private string _startDateString;
        private DateTime _startDateTime;
        private int _periods;
        #endregion

        #region Mode States
        private bool _isDateTimeMode;
        #endregion

        /// <summary>
        /// Simple constructor - only new system
        /// </summary>
        public RegressionModeManager(
            StartPointMethod startPointMethod,
            bool historicalBarsOnly,
            bool enableLock,
            string lockDateString,
            string startDateString,
            int periods)
        {
            // Store parameters
            _startPointMethod = startPointMethod;
            _historicalBarsOnly = historicalBarsOnly;
            _enableLock = enableLock;
            _lockDateString = lockDateString;
            _startDateString = startDateString;
            _periods = periods;

            // Determine mode - simple logic
            _isDateTimeMode = (_startPointMethod == StartPointMethod.DateTime);

            // Parse dates if needed
            if (_isDateTimeMode)
            {
                ParseStartDate();
            }

            if (_enableLock)
            {
                ParseLockDate();
            }
        }

        /// <summary>
        /// Parse start date - simple format support
        /// </summary>
        private void ParseStartDate()
        {
            try
            {
                // Try different formats
                string[] formats = {
                    "dd/MM/yyyy HH:mm",     // 13/04/2025 04:00
                    "dd/MM/yyyy",           // 13/04/2025
                    "dd/MM/yyyy HH:mm:ss",  // 13/04/2025 04:00:00
                    "d/M/yyyy HH:mm",       // 1/4/2025 04:00
                    "d/M/yyyy"              // 1/4/2025
                };

                foreach (string format in formats)
                {
                    if (DateTime.TryParseExact(_startDateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _startDateTime))
                    {
                        // If date-only format, add 4 hours (04:00:00)
                        if (format == "dd/MM/yyyy" || format == "d/M/yyyy")
                        {
                            _startDateTime = _startDateTime.AddHours(4);
                        }
                        return;
                    }
                }

                // If no format worked, try general parsing
                if (DateTime.TryParse(_startDateString, out _startDateTime))
                {
                    // If no time specified, add 4 hours
                    if (_startDateTime.TimeOfDay == TimeSpan.Zero)
                    {
                        _startDateTime = _startDateTime.AddHours(4);
                    }
                    return;
                }

                // Fallback - use 1 year ago + 4 hours
                _startDateTime = DateTime.Now.AddMonths(-12).Date.AddHours(4);
            }
            catch
            {
                // Fallback - use 1 year ago + 4 hours
                _startDateTime = DateTime.Now.AddMonths(-12).Date.AddHours(4);
            }
        }

        /// <summary>
        /// Parse lock date - simple format support
        /// </summary>
        private void ParseLockDate()
        {
            try
            {
                // Try different formats (same as start date)
                string[] formats = {
                    "dd/MM/yyyy HH:mm",     
                    "dd/MM/yyyy",           
                    "dd/MM/yyyy HH:mm:ss",  
                    "d/M/yyyy HH:mm",       
                    "d/M/yyyy"              
                };

                foreach (string format in formats)
                {
                    if (DateTime.TryParseExact(_lockDateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _lockDateTime))
                    {
                        // If date-only format, add 4 hours (04:00:00)
                        if (format == "dd/MM/yyyy" || format == "d/M/yyyy")
                        {
                            _lockDateTime = _lockDateTime.AddHours(4);
                        }
                        return;
                    }
                }

                // If no format worked, try general parsing
                if (DateTime.TryParse(_lockDateString, out _lockDateTime))
                {
                    // If no time specified, add 4 hours
                    if (_lockDateTime.TimeOfDay == TimeSpan.Zero)
                    {
                        _lockDateTime = _lockDateTime.AddHours(4);
                    }
                    return;
                }

                // Fallback - use yesterday + 4 hours
                _lockDateTime = DateTime.Now.AddDays(-1).Date.AddHours(4);
            }
            catch
            {
                // Fallback - use yesterday + 4 hours
                _lockDateTime = DateTime.Now.AddDays(-1).Date.AddHours(4);
            }
        }

        /// <summary>
        /// Get simple mode information for display
        /// </summary>
        public string GetModeInfo()
        {
            string modeInfo = "";

            if (_isDateTimeMode)
            {
                modeInfo = $"DateTime Mode (From: {_startDateTime:dd/MM/yyyy HH:mm})";
            }
            else
            {
                modeInfo = $"Period Mode ({_periods} periods)";
            }

            // Add historical bars setting
            if (!_historicalBarsOnly)
            {
                modeInfo += " [Including Current Bar]";
            }

            // Add lock information if enabled
            if (_enableLock)
            {
                modeInfo += $" | LOCKED at {_lockDateTime:dd/MM/yyyy HH:mm}";
            }

            return modeInfo;
        }

        #region Simple Properties

        public bool IsDateTimeMode => _isDateTimeMode;
        public StartPointMethod StartPointMethod => _startPointMethod;
        public bool HistoricalBarsOnly => _historicalBarsOnly;
        public bool EnableLock => _enableLock;
        public DateTime StartDateTime => _startDateTime;
        public DateTime LockDateTime => _lockDateTime;

        #endregion
    }
}
