using System;
using cAlgo.API;
using cAlgo.Indicators.App.Models;
using cAlgo.Indicators.App.Views;

namespace cAlgo.Indicators.App.Controllers
{
    public class VolumeProfileController
    {
        private readonly VolumeProfileModel _model;
        private readonly VolumeProfileView _view;
        private readonly Bars _bars;
        private readonly int _lookbackPeriods;

        // New fields for DateTime profiles
        private readonly bool _useDateTimeProfiles;
        private readonly string _startDateTimeProfiles;
        private readonly string _endDateTimeProfiles;
        private readonly int _timezoneOffsetHours;

        private readonly bool _enableTPO;

        public VolumeProfileController(
            VolumeProfileModel model,
            VolumeProfileView view,
            Bars bars,
            int lookbackPeriods,
            bool useDateTimeProfiles,
            string startDateTimeProfiles,
            string endDateTimeProfiles,
            int timezoneOffsetHours,
            bool enableTPO)
        {
            _model = model;
            _view = view;
            _bars = bars;
            _lookbackPeriods = lookbackPeriods;
            _useDateTimeProfiles = useDateTimeProfiles;
            _startDateTimeProfiles = startDateTimeProfiles;
            _endDateTimeProfiles = endDateTimeProfiles;
            _timezoneOffsetHours = timezoneOffsetHours;
            _enableTPO = enableTPO;
        }

        private bool TryGetDateTimeIndices(out int startIndex, out int endIndex)
        {
            int barsCount = _bars.Count;
            startIndex = 0;
            endIndex = barsCount - 1;

            if (barsCount == 0)
                return false;

            // If DateTime profiles are disabled or start datetime is empty, return false
            if (!_useDateTimeProfiles || string.IsNullOrEmpty(_startDateTimeProfiles))
                return false;

            try
            {
                // Parse start date-time
                DateTime startDateTime = DateTime.ParseExact(_startDateTimeProfiles,
                    "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                startDateTime = startDateTime.AddHours(-_timezoneOffsetHours); // Convert to UTC

                // Validate start is within data range
                if (startDateTime > _bars.OpenTimes[_bars.Count - 1])
                    return false; // Start is after all available data

                // Find nearest bar index for start time
                startIndex = FindNearestBarIndex(startDateTime);

                // For end time, either parse the provided value or find the most recent bar
                if (string.IsNullOrEmpty(_endDateTimeProfiles))
                {
                    // Use the most recent bar if end date is empty
                    endIndex = barsCount - 1;
                }
                else
                {
                    // Parse provided end date-time and find nearest bar
                    DateTime endDateTime = DateTime.ParseExact(_endDateTimeProfiles,
                        "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    endDateTime = endDateTime.AddHours(-_timezoneOffsetHours); // Convert to UTC

                    // Validate end is within data range
                    if (endDateTime < _bars.OpenTimes[0])
                        return false; // End is before all available data

                    endIndex = FindNearestBarIndex(endDateTime);
                }

                // Ensure indices are in correct order
                if (startIndex > endIndex)
                {
                    int temp = startIndex;
                    startIndex = endIndex;
                    endIndex = temp;
                }

                return true;
            }
            catch
            {
                // On any error, fallback to standard lookback period
                return false;
            }
        }

        private int FindNearestBarIndex(DateTime targetTime)
        {
            int left = 0;
            int right = _bars.Count - 1;

            // Binary search for closest bar
            while (left < right)
            {
                int mid = (left + right) / 2;
                if (_bars.OpenTimes[mid] < targetTime)
                    left = mid + 1;
                else
                    right = mid;
            }

            // Check if previous bar is closer
            if (left > 0)
            {
                TimeSpan diffLeft = targetTime > _bars.OpenTimes[left - 1]
                    ? targetTime - _bars.OpenTimes[left - 1]
                    : _bars.OpenTimes[left - 1] - targetTime;

                TimeSpan diffRight = targetTime > _bars.OpenTimes[left]
                    ? targetTime - _bars.OpenTimes[left]
                    : _bars.OpenTimes[left] - targetTime;

                if (diffLeft < diffRight)
                    return left - 1;
            }

            return left;
        }

        public void Calculate(int index)
        {
            // Clear previous drawings
            _view.ClearChart();

            int barsCount = _bars.Count;

            int startIndex, endIndex;
            bool isDateTimeMode = false;

            // Try to get indices from date-time range
            if (TryGetDateTimeIndices(out startIndex, out endIndex))
            {
                isDateTimeMode = true;
            }
            else
            {
                // Fallback to lookback periods
                endIndex = index;
                startIndex = Math.Max(0, index - _lookbackPeriods);
            }

            // Make sure indices are within bounds
            startIndex = Math.Max(0, startIndex);
            endIndex = Math.Min(barsCount - 1, endIndex);

            // Calculate date span for view width
            int dateTimeSpan = endIndex - startIndex;

            // Determine price range
            _model.CalculatePriceRange(_bars, startIndex, endIndex);

            // Create price levels
            _model.CreatePriceLevels();

            // Process candles to collect volume at each price level
            _model.ProcessCandles(_bars, startIndex, endIndex);

            // Process TPO data if enabled
            if (_enableTPO)
            {
                _model.ProcessTPO(_bars, startIndex, endIndex);
            }

            // Find point of control
            _model.IdentifyPointOfControl();

            // Calculate value area
            _model.CalculateValueArea();

            // Draw the volume profile - pass both start and end indices
            _view.DrawVolumeProfile(_model, endIndex, startIndex, _bars, isDateTimeMode, dateTimeSpan, _enableTPO);
        }
    }
}
