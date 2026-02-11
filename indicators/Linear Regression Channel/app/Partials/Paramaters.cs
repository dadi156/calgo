using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class LinearRegressionChannel : Indicator
    {
        #region Data Collection Parameters

        [Parameter("Timeframe", DefaultValue = "Monthly", Group = "Data Collection")]
        public TimeFrame SelectedTimeFrame { get; set; }

        [Parameter("Start Point Method", DefaultValue = StartPointMethod.Period, Group = "Data Collection")]
        public StartPointMethod StartPointMethod { get; set; }

        [Parameter("Periods", DefaultValue = 4, MinValue = 1, Group = "Data Collection")]
        public int Periods { get; set; }

        [Parameter("Start Date", DefaultValue = "01/11/2025 04:00", Group = "Data Collection")]
        public string StartDate { get; set; }

        [Parameter("Historical Price Only", DefaultValue = true, Group = "Data Collection")]
        public bool HistoricalBarsOnly { get; set; }

        #endregion

        #region Lock Mechanism Parameters

        [Parameter("Enable Lock", DefaultValue = false, Group = "Data Lock")]
        public bool EnableLock { get; set; }

        [Parameter("Lock Date", DefaultValue = "01/11/2025 04:00", Group = "Data Lock")]
        public string LockDate { get; set; }

        #endregion

        #region Regression Channel Parameters

        [Parameter("Price Type", DefaultValue = PriceType.Median, Group = "Regression Channel")]
        public PriceType SelectedPriceType { get; set; }

        [Parameter("Deviation Method", DefaultValue = DeviationMethod.Independent, Group = "Regression Channel")]
        public DeviationMethod SelectedDeviationMethod { get; set; }

        [Parameter("StdDev Multiplier", DefaultValue = 3.0, MinValue = 0.1, MaxValue = 5.0, Step = 0.1, Group = "Regression Channel")]
        public double StdDevMultiplier { get; set; }

        [Parameter("ATR Multiplier", DefaultValue = 1, MinValue = 0.1, MaxValue = 5.0, Step = 0.1, Group = "Regression Channel")]
        public double ATRMultiplier { get; set; }

        [Parameter("Extend to Infinity", DefaultValue = false, Group = "Regression Channel")]
        public bool ExtendRegressionToInfinity { get; set; }

        #endregion

        #region Fibonacci Level Parameters

        [Parameter("88.6% Level", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowLevel886 { get; set; }

        [Parameter("78.6% Level", DefaultValue = false, Group = "Fibonacci Levels")]
        public bool ShowLevel786 { get; set; }

        [Parameter("61.8% Level", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowLevel618 { get; set; }

        [Parameter("38.2% Level", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowLevel382 { get; set; }

        [Parameter("23.6% Level", DefaultValue = false, Group = "Fibonacci Levels")]
        public bool ShowLevel236 { get; set; }

        [Parameter("11.4% Level", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowLevel114 { get; set; }

        #endregion

        #region Color Parameters

        [Parameter("Regression Line", DefaultValue = "FFB8860B", Group = "Line Colors")]
        public Color RegressionLineColor { get; set; }

        [Parameter("Upper Line", DefaultValue = "FFDC143C", Group = "Line Colors")]
        public Color UpperLineColor { get; set; }

        [Parameter("Lower Line", DefaultValue = "FF8FBC8F", Group = "Line Colors")]
        public Color LowerLineColor { get; set; }

        [Parameter("Fibonacci Lines", DefaultValue = "FF778899", Group = "Line Colors")]
        public Color FibonacciLinesColor { get; set; }

        #endregion
    }
}
