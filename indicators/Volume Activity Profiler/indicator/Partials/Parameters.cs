using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeActivityProfiler : Indicator
    {
        [Parameter("Timeframe", DefaultValue = "Weekly", Group = "Analysis")]
        public TimeFrame SelectedTimeframe { get; set; }

        [Parameter("Length", DefaultValue = 4, MinValue = 1, Group = "Analysis")]
        public int Length { get; set; }

        [Parameter("Efficiency Mode", DefaultValue = EfficiencyMode.Range, Group = "Analysis")]
        public EfficiencyMode EfficiencyCalc { get; set; }

        [Parameter("Norm Window Mode", DefaultValue = NormWindowMode.Adaptive, Group = "Normalization")]
        public NormWindowMode NormMode { get; set; }

        [Parameter("Custom Norm Window", DefaultValue = 60, MinValue = 20, MaxValue = 500, Group = "Normalization")]
        public int CustomNormWindow { get; set; }

        [Parameter("Placement", DefaultValue = GraphPlacement.BelowLow, Group = "Bar Graph")]
        public GraphPlacement Placement { get; set; }

        [Parameter("Height (%)", DefaultValue = 3.5, MinValue = 0.5, MaxValue = 10, Group = "Bar Graph")]
        public double BarHeightPercent { get; set; }

        [Parameter("Spacing (%)", DefaultValue = 0.5, MinValue = 0.1, MaxValue = 5, Group = "Bar Graph")]
        public double BarSpacingPercent { get; set; }

        [Parameter("Fixed Height (pips)", DefaultValue = 0, MinValue = 0, Group = "Bar Graph")]
        public double FixedBarHeightPips { get; set; }

        [Parameter("Fixed Spacing (pips)", DefaultValue = 0, MinValue = 0, Group = "Bar Graph")]
        public double FixedBarSpacingPips { get; set; }

        [Parameter("Background Graph", DefaultValue = true, Group = "Bar Graph")]
        public bool GraphBackground { get; set; }

        [Parameter("Graph Info", DefaultValue = GraphTextDisplay.Full, Group = "Bar Graph")]
        public GraphTextDisplay GraphTextDisplay { get; set; }

        [Parameter("Info Alignment", DefaultValue = GraphTextAlignment.FillEnd, Group = "Bar Graph")]
        public GraphTextAlignment TextAlignment { get; set; }

        [Parameter("Font Family", DefaultValue = "Bahnschrift", Group = "Bar Graph")]
        public string GraphFontFamily { get; set; }

        [Parameter("Font Size", DefaultValue = 9, MinValue = 6, MaxValue = 24, Group = "Bar Graph")]
        public int GraphFontSize { get; set; }

        [Parameter("Text Color", DefaultValue = "Gray", Group = "Bar Graph")]
        public Color GraphTextColor { get; set; }

        [Parameter("Metrics", DefaultValue = MetricsDisplay.None, Group = "Metrics")]
        public MetricsDisplay ShowMetricsInfo { get; set; }

        [Parameter("Font Family", DefaultValue = "Bahnschrift", Group = "Metrics")]
        public string MetricsFontFamily { get; set; }

        [Parameter("Font Size", DefaultValue = 11, MinValue = 6, MaxValue = 24, Group = "Metrics")]
        public int MetricsFontSize { get; set; }

        [Parameter("Text Color", DefaultValue = "AliceBLue", Group = "Metrics")]
        public Color MetricsTextColor { get; set; }

        [Parameter("Separator", DefaultValue = false, Group = "Separator")]
        public bool ShowSeparator { get; set; }

        [Parameter("Line Style", DefaultValue = LineStyle.DotsRare, Group = "Separator")]
        public LineStyle SeparatorStyle { get; set; }

        [Parameter("Thickness", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "Separator")]
        public int SeparatorThickness { get; set; }

        [Parameter("Color", DefaultValue = "FF525252", Group = "Separator")]
        public Color SeparatorColor { get; set; }
    }
}
