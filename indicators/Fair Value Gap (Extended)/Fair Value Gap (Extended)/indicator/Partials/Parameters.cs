using cAlgo.API;

namespace cAlgo
{
    public partial class FairValueGap : Indicator
    {
        #region Settings

        [Parameter("Lookback", DefaultValue = 250, MinValue = 10, Group = "Settings")]
        public int LookbackPeriod { get; set; }

        [Parameter("Min. Pips Gap Size", DefaultValue = 10, MinValue = 0, Group = "Settings")]
        public double MinimumSizePips { get; set; }

        [Parameter("Partial Fill Threshold %", DefaultValue = 90, MinValue = 50, MaxValue = 99, Group = "Settings")]
        public int PartialFillThreshold { get; set; }

        #endregion

        #region Multi-Timeframe

        [Parameter("Enable", DefaultValue = false, Group = "Multi-Timeframe")]
        public bool EnableMultiTimeframe { get; set; }

        [Parameter("Timeframe", DefaultValue = "Daily", Group = "Multi-Timeframe")]
        public TimeFrame SelectedTimeframe { get; set; }

        #endregion

        #region FVG Display

        [Parameter("FVG Type", DefaultValue = FVGDisplay.Both, Group = "FVG Display")]
        public FVGDisplay DisplayMode { get; set; }

        [Parameter("FVG to Display", DefaultValue = -1, MinValue = -1, Group = "FVG Display")]
        public int MaxFVGsToDisplay { get; set; }

        [Parameter("Unfilled FVG", DefaultValue = true, Group = "FVG Display")]
        public bool ShowUnfilled { get; set; }

        [Parameter("Partial FVG", DefaultValue = true, Group = "FVG Display")]
        public bool ShowPartial { get; set; }

        [Parameter("Filled FVG", DefaultValue = false, Group = "FVG Display")]
        public bool ShowFilled { get; set; }

        [Parameter("Extend Filled FVGs", DefaultValue = false, Group = "FVG Display")]
        public bool ExtendFilledFVGs { get; set; }

        #endregion

        #region Fibonacci Levels

        [Parameter("Enable", DefaultValue = false, Group = "Fibonacci Levels (On Filled and Partial FVGs Only)")]
        public bool EnableFibonacciLines { get; set; }

        [Parameter("23.6% Level", DefaultValue = true, Group = "Fibonacci Levels (On Filled and Partial FVGs Only)")]
        public bool EnableFib236 { get; set; }

        [Parameter("38.2% Level", DefaultValue = false, Group = "Fibonacci Levels (On Filled and Partial FVGs Only)")]
        public bool EnableFib382 { get; set; }

        [Parameter("50% Level", DefaultValue = true, Group = "Fibonacci Levels (On Filled and Partial FVGs Only)")]
        public bool EnableFib500 { get; set; }

        [Parameter("61.8% Level", DefaultValue = false, Group = "Fibonacci Levels (On Filled and Partial FVGs Only)")]
        public bool EnableFib618 { get; set; }

        [Parameter("78.6% Level", DefaultValue = true, Group = "Fibonacci Levels (On Filled and Partial FVGs Only)")]
        public bool EnableFib786 { get; set; }

        #endregion

        #region Labels

        [Parameter("Enable", DefaultValue = false, Group = "Labels")]
        public bool EnableLabels { get; set; }

        [Parameter("Font Family", DefaultValue = "Consolas", Group = "Labels")]
        public string LabelFontFamily { get; set; }

        [Parameter("Font Size", DefaultValue = 10, MinValue = 6, MaxValue = 20, Group = "Labels")]
        public int LabelFontSize { get; set; }

        [Parameter("Color", DefaultValue = "White", Group = "Labels")]
        public Color LabelColor { get; set; }

        #endregion

        #region Colors

        [Parameter("Unfilled", DefaultValue = "#8801FF01", Group = "Bullish Colors")]
        public Color BullishUnfilledColor { get; set; }

        [Parameter("Partial", DefaultValue = "#889ACD32", Group = "Bullish Colors")]
        public Color BullishPartialColor { get; set; }

        [Parameter("Filled", DefaultValue = "#88005727", Group = "Bullish Colors")]
        public Color BullishFilledColor { get; set; }

        [Parameter("Unfilled", DefaultValue = "#88FF0000", Group = "Bearish Colors")]
        public Color BearishUnfilledColor { get; set; }

        [Parameter("Partial", DefaultValue = "#88F15923", Group = "Bearish Colors")]
        public Color BearishPartialColor { get; set; }

        [Parameter("Filled", DefaultValue = "#88800001", Group = "Bearish Colors")]
        public Color BearishFilledColor { get; set; }

        [Parameter("Lines", DefaultValue = LineStyle.DotsRare, Group = "Fibonacci Lines Style")]
        public LineStyle FibLinesStyle { get; set; }

        [Parameter("Color", DefaultValue = "88FFFFFF", Group = "Fibonacci Lines Style")]
        public Color FibLinesColor { get; set; }

        #endregion
    }
}
