using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Configuration model for regression channel settings
    /// </summary>
    public class ChannelConfig
    {
        /// <summary>
        /// Number of bars to include in regression calculation
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Polynomial degree for polynomial regression (ignored for other types)
        /// </summary>
        public int Degree { get; set; }

        /// <summary>
        /// Width of the channel in standard deviations
        /// </summary>
        public double ChannelWidth { get; set; }

        /// <summary>
        /// Type of regression to use for calculations
        /// </summary>
        public RegressionType RegressionType { get; set; }

        /// <summary>
        /// Whether to use multi-timeframe calculations
        /// </summary>
        public bool UseMultiTimeframe { get; set; }

        /// <summary>
        /// Timeframe to use for calculations when UseMultiTimeframe is true
        /// </summary>
        public TimeFrame SelectedTimeFrame { get; set; }

        /// <summary>
        /// Bars data source for chart timeframe
        /// </summary>
        public Bars Bars { get; }

        /// <summary>
        /// Bars data source for selected timeframe (when UseMultiTimeframe is true)
        /// </summary>
        public Bars TimeframeBars { get; set; }

        /// <summary>
        /// Mode for regression calculation (Periods or DateRange)
        /// </summary>
        public RegressionMode RegressionMode { get; set; } = RegressionMode.Periods;

        /// <summary>
        /// Start date for date range mode
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// End date for date range mode
        /// </summary>
        public DateTime EndDate { get; set; } = DateTime.MaxValue;

        public ChannelConfig(Bars bars)
        {
            Bars = bars ?? throw new ArgumentNullException(nameof(bars));
            UseMultiTimeframe = false;
            SelectedTimeFrame = TimeFrame.Daily;
        }

        /// <summary>
        /// Sets the date range for custom regression range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        public void SetDateRange(DateTime startDate, DateTime endDate)
        {
            // Ensure start date is before end date
            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            StartDate = startDate;
            EndDate = endDate;
            RegressionMode = RegressionMode.DateRange;
        }

        /// <summary>
        /// Validates configuration parameters
        /// </summary>
        public void Validate()
        {
            if (Period <= 0) 
                throw new ArgumentException("Period must be positive");
            
            if (ChannelWidth <= 0) 
                throw new ArgumentException("Channel width must be positive");
            
            if (RegressionType == RegressionType.Polynomial && (Degree < 1 || Degree > 5))
                throw new ArgumentException("Polynomial degree must be between 1 and 5");
                
            // If using date range mode, ensure we have valid dates
            if (RegressionMode == RegressionMode.DateRange)
            {
                if (StartDate == DateTime.MinValue || EndDate == DateTime.MaxValue)
                {
                    // Fall back to period mode if dates are not set properly
                    RegressionMode = RegressionMode.Periods;
                }
            }
        }

        /// <summary>
        /// Initialize the timeframe bars if using multi-timeframe
        /// </summary>
        /// <param name="marketData">Market data provider</param>
        public void InitializeTimeframeBars(MarketData marketData)
        {
            if (UseMultiTimeframe)
            {
                // Get bars for the selected timeframe
                TimeframeBars = marketData.GetBars(SelectedTimeFrame);
            }
            else
            {
                // Use chart timeframe
                TimeframeBars = Bars;
            }
        }
    }
}
