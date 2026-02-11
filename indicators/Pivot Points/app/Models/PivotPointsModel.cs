using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Model for pivot points data processing
    /// </summary>
    public class PivotPointsModel
    {
        private TimeFrame _timeFrame;
        private MarketData _marketData;
        private PivotPointType _pivotType;
        private int _srLevelsToShow;
        private int _pivotToDraw;
        private List<PeriodPivotPointsModel> _pivotData;

        // Track the last bar time for the selected timeframe
        private DateTime _lastKnownBarTime = DateTime.MinValue;
        private bool _isNewBarFormed = false;

        public PivotPointsModel(TimeFrame timeFrame, MarketData marketData,
            PivotPointType pivotType, int srLevelsToShow, int pivotToDraw)
        {
            _timeFrame = timeFrame;
            _marketData = marketData;
            _pivotType = pivotType;
            _srLevelsToShow = srLevelsToShow;
            _pivotToDraw = pivotToDraw;
            _pivotData = new List<PeriodPivotPointsModel>();
        }

        /// <summary>
        /// Check if a new bar has formed in the selected timeframe
        /// </summary>
        public bool CheckForNewBar()
        {
            _isNewBarFormed = false;

            // Get the bars for the selected timeframe
            Bars timeframeBars = _marketData.GetBars(_timeFrame);

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
        /// Refreshes the pivot points data
        /// </summary>
        public void RefreshData()
        {
            _pivotData.Clear();

            // Get the bars for the selected timeframe
            Bars timeframeBars = _marketData.GetBars(_timeFrame);

            if (timeframeBars == null || timeframeBars.Count < 2)
                return;

            // Calculate pivot points for the necessary periods
            _pivotData = CalculatePivotPointsForPeriods(timeframeBars);
        }

        /// <summary>
        /// Gets the pivot points data
        /// </summary>
        public List<PeriodPivotPointsModel> GetPivotData()
        {
            return _pivotData;
        }

        /// <summary>
        /// Gets the periods to display based on the last periods to show setting
        /// </summary>
        public List<PeriodPivotPointsModel> GetPeriodsToDisplay()
        {
            if (_pivotData == null || _pivotData.Count == 0)
                return new List<PeriodPivotPointsModel>();

            // Show only the specified number of most recent periods
            return _pivotData
                .OrderByDescending(p => p.StartTime)
                .Take(_pivotToDraw)
                .OrderBy(p => p.StartTime)
                .ToList();
        }

        #region Private Methods

        /// <summary>
        /// Calculates pivot points for all periods
        /// </summary>
        private List<PeriodPivotPointsModel> CalculatePivotPointsForPeriods(Bars pivotBars)
        {
            if (pivotBars == null || pivotBars.Count < 2)
                return new List<PeriodPivotPointsModel>();

            List<PeriodPivotPointsModel> result = new List<PeriodPivotPointsModel>();

            // Get the current timeframe of the pivot bars
            TimeFrame currentTimeframe = pivotBars.TimeFrame;

            // Convert timeframe to timespan for consistent line length
            TimeSpan timeframeSpan = currentTimeframe.ToTimeSpan();

            // Get the maximum number of bars to process (limited by the pivotToDraw parameter)
            int maxBarCount = Math.Min(pivotBars.Count, _pivotToDraw + 1);

            // Get the appropriate calculator for the pivot type
            IPivotPointCalculator calculator = PivotPointCalculatorFactory.CreateCalculator(_pivotType);

            // Start from the most recent period and go back
            for (int i = pivotBars.Count - 1; i >= pivotBars.Count - maxBarCount; i--)
            {
                if (i <= 0) continue; // Skip if we don't have a previous bar

                DateTime periodStart = pivotBars.OpenTimes[i];

                // Calculate period end time based on exact timeframe length
                // This ensures consistent line lengths for all periods including the most recent
                DateTime periodEnd = periodStart.Add(timeframeSpan);

                // Get the previous bar's OHLC values for calculation
                double high = pivotBars.HighPrices[i - 1];
                double low = pivotBars.LowPrices[i - 1];
                double close = pivotBars.ClosePrices[i - 1];
                double open = pivotBars.OpenPrices[i - 1];

                // Calculate pivot points using the calculator
                PivotPointsData pivotData = calculator.Calculate(high, low, close, open, _srLevelsToShow);

                // Get period name based on timeframe
                string periodName = GetTimeframePeriodName(periodStart, timeframeSpan);

                // Create period pivot points data
                result.Add(new PeriodPivotPointsModel
                {
                    PivotData = pivotData,
                    StartTime = periodStart,
                    EndTime = periodEnd,
                    PeriodName = periodName
                });
            }

            // Sort by start time
            result = result.OrderBy(p => p.StartTime).ToList();

            return result;
        }

        private string GetTimeframePeriodName(DateTime time, TimeSpan timeSpan)
        {
            // Determine the appropriate format based on the time span
            if (timeSpan.TotalDays >= 28)
            {
                // Monthly
                return time.ToString("MMMM yyyy").ToUpper();
            }
            else if (timeSpan.TotalDays >= 7)
            {
                // Weekly
                return $"W{GetWeekNumber(time)} {time.Year}";
            }
            else if (timeSpan.TotalDays >= 1)
            {
                // Daily
                return time.ToString("dd/MM");
            }
            else if (timeSpan.TotalHours >= 1)
            {
                // Hourly
                return time.ToString("HH:mm dd/MM");
            }
            else
            {
                // Minutes
                return time.ToString("HH:mm:ss dd/MM");
            }
        }

        private int GetWeekNumber(DateTime time)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = culture.Calendar;
            var dateTimeFormat = culture.DateTimeFormat;

            return calendar.GetWeekOfYear(time, dateTimeFormat.CalendarWeekRule, dateTimeFormat.FirstDayOfWeek);
        }

        /// <summary>
        /// Gets the period for metrics calculation based on offset
        /// </summary>
        /// <param name="offset">Offset from most recent period (0 = most recent, -1 = one back, etc.)</param>
        public PeriodPivotPointsModel GetPeriodForMetrics(int offset)
        {
            if (_pivotData == null || _pivotData.Count == 0)
                return null;

            // Offset is negative, so we convert it to positive index from the end
            int index = _pivotData.Count - 1 + offset;

            // Ensure index is valid
            if (index < 0 || index >= _pivotData.Count)
                return null;

            return _pivotData
                .OrderBy(p => p.StartTime)
                .ElementAtOrDefault(index);
        }

        #endregion
    }

    /// <summary>
    /// Data structure for period-specific pivot points
    /// </summary>
    public class PeriodPivotPointsModel
    {
        public PivotPointsData PivotData { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string PeriodName { get; set; }
    }
}
