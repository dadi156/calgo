using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class VWAP : Indicator
    {
        #region Parameters

        [Parameter("Source", DefaultValue = "Typical", Group = "VWAP")]
        public DataSeries Source { get; set; }

        [Parameter("Reset Period", DefaultValue = VwapResetPeriod.Weekly, Group = "VWAP")]
        public VwapResetPeriod ResetPeriod { get; set; }

        [Parameter("Anchor DateTime", DefaultValue = "01/01/2025 00:00", Group = "VWAP")]
        public string AnchorDateTime { get; set; }

        [Parameter("Band Type", DefaultValue = VwapBandType.HighLowRange, Group = "Band Calculation")]
        public VwapBandType BandType { get; set; }

        [Parameter("Pivot S/R Depth", DefaultValue = 1, MinValue = 1, MaxValue = 3, Group = "Band Calculation")]
        public int PivotDepth { get; set; }

        [Parameter("StdDev Multiplier", DefaultValue = 1.618, Group = "Band Calculation")]
        public double StdDevMultiplier { get; set; }

        [Parameter("Upper Band", DefaultValue = true, Group = "Band Visibility")]
        public bool ShowUpperBand { get; set; }

        [Parameter("Lower Band", DefaultValue = true, Group = "Band Visibility")]
        public bool ShowLowerBand { get; set; }

        [Parameter("Level 88.6%", DefaultValue = false, Group = "Fibonacci Levels")]
        public bool ShowFibo886 { get; set; }

        [Parameter("Level 76.4%", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowFibo764 { get; set; }

        [Parameter("Level 62.8%", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowFibo628 { get; set; }

        [Parameter("Level 38.2%", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowFibo382 { get; set; }

        [Parameter("Level 23.6%", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowFibo236 { get; set; }

        [Parameter("Level 11.4%", DefaultValue = false, Group = "Fibonacci Levels")]
        public bool ShowFibo114 { get; set; }

        [Parameter("Timezone UTC Offset", DefaultValue = 4, MinValue = -12, MaxValue = 14, Group = "Time Settings")]
        public int TimezoneOffset { get; set; }

        [Parameter("Asian Session Start", DefaultValue = 4, MinValue = 0, MaxValue = 23, Group = "Time Settings")]
        public int AsianSessionHour { get; set; }

        [Parameter("London Session Start", DefaultValue = 14, MinValue = 0, MaxValue = 23, Group = "Time Settings")]
        public int LondonSessionHour { get; set; }

        [Parameter("NY Session Start", DefaultValue = 19, MinValue = 0, MaxValue = 23, Group = "Time Settings")]
        public int NewYorkSessionHour { get; set; }

        #endregion
    }
}
