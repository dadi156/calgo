using cAlgo.API;

namespace cAlgo
{
    public partial class TrendChannelMovingAverage : Indicator
    {
        #region Parameters

        // Moving Average parameters
        [Parameter("Type", Group = "Moving Average", DefaultValue = MAType.SuperSmootherMA)]
        public MAType MovingAverageType { get; set; }

        [Parameter("Source", Group = "Moving Average")]
        public DataSeries Source { get; set; }

        // Calculation
        [Parameter("Method", DefaultValue = PeriodCalculationType.Period, Group = "Calculation")]
        public PeriodCalculationType PeriodCalculationType { get; set; }

        [Parameter("Period", DefaultValue = 10, MinValue = 1, Group = "Calculation")]
        public int Period { get; set; }

        [Parameter("Anchor Date", DefaultValue = "01/07/2025 04:00", Group = "Calculation")]
        public string AnchorDateTime { get; set; }

        [Parameter("Trend Factor Period", DefaultValue = 4, MinValue = 1, Group = "Calculation")]
        public int TrendAveragingPeriod { get; set; }

        // Multi-timeframe parameters
        [Parameter("Multi-timeframe", Group = "Multi-timeframe", DefaultValue = false)]
        public bool MultiTimeframeMode { get; set; }

        [Parameter("Base Timeframe", Group = "Multi-timeframe", DefaultValue = "Monthly")]
        public TimeFrame BaseTimeframe { get; set; }

        // Display parameters
        [Parameter("Line Display", Group = "Display", DefaultValue = LineDisplayMode.TrendBased)]
        public LineDisplayMode LineDisplayMode { get; set; }

        [Parameter("Fibonacci Lines", Group = "Display", DefaultValue = FibonacciDisplayMode.None)]
        public FibonacciDisplayMode FibonacciDisplayMode { get; set; }

        [Parameter("Lines Start Point", DefaultValue = "01/07/2025 04:00, true", Group = "Display")]
        public string LinesStartPoint { get; set; }

        // Bar Colors parameters
        [Parameter("Trend-based Bar", Group = "Bar Colors", DefaultValue = false)]
        public bool EnableBarColors { get; set; }

        [Parameter("Uptrend", Group = "Bar Colors", DefaultValue = "FF00843B")]
        public Color UptrendBarColor { get; set; }

        [Parameter("Downtrend", Group = "Bar Colors", DefaultValue = "FFF15923")]
        public Color DowntrendBarColor { get; set; }

        [Parameter("Neutral", Group = "Bar Colors", DefaultValue = "Gray")]
        public Color NeutralBarColor { get; set; }

        #endregion
    }
}
