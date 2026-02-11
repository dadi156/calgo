using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Parameters

        [Parameter("Lookback Period", Group = "Calculation", DefaultValue = 20, MinValue = 10, MaxValue = 100)]
        public int LookbackPeriod { get; set; }

        [Parameter("Trim Percent", Group = "Calculation", DefaultValue = 10, MinValue = 5, MaxValue = 25)]
        public int TrimPercent { get; set; }

        [Parameter("Wide Threshold", Group = "Spread", DefaultValue = 70, MinValue = 60, MaxValue = 90)]
        public int SpreadWideThreshold { get; set; }

        [Parameter("Narrow Threshold", Group = "Spread", DefaultValue = 30, MinValue = 10, MaxValue = 40)]
        public int SpreadNarrowThreshold { get; set; }

        [Parameter("High Ratio", Group = "Volume", DefaultValue = 1.5, MinValue = 1.2, MaxValue = 2.0)]
        public double VolumeHighRatio { get; set; }

        [Parameter("Ultra Ratio", Group = "Volume", DefaultValue = 2.0, MinValue = 1.5, MaxValue = 3.0)]
        public double VolumeUltraRatio { get; set; }

        [Parameter("Low Ratio", Group = "Volume", DefaultValue = 0.5, MinValue = 0.3, MaxValue = 0.8)]
        public double VolumeLowRatio { get; set; }

        [Parameter("Trend Bars", Group = "Context", DefaultValue = 5, MinValue = 3, MaxValue = 20)]
        public int TrendBars { get; set; }

        [Parameter("Efficiency Threshold", Group = "Context", DefaultValue = 0.3, MinValue = 0.1, MaxValue = 0.5)]
        public double EfficiencyThreshold { get; set; }

        [Parameter("Color Chart Bars", Group = "Display", DefaultValue = false)]
        public bool ColorChartBars { get; set; }

        [Parameter("Metrics Panel", Group = "Info", DefaultValue = true)]
        public bool ShowMetricsPanel { get; set; }

        [Parameter("Legend", Group = "Info", DefaultValue = true)]
        public bool ShowLegend { get; set; }

        #endregion
    }
}
