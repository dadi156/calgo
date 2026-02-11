using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class RoundNumbers
    {
        [Parameter("Lookback Period", DefaultValue = 1000, MinValue = 10, Group = "Round Levels")]
        public int LookbackPeriod { get; set; }

        [Parameter("Multiples", DefaultValue = 100, MinValue = 1, Group = "Round Levels")]
        public int Multiples { get; set; }

        [Parameter("Levels", DefaultValue = 10, MinValue = 1, Group = "Round Levels")]
        public int NumberOfLevels { get; set; }

        [Parameter("Extend Forward", DefaultValue = 50, MinValue = 0, Group = "Line Styles")]
        public int ExtendForward { get; set; }

        [Parameter("Style", DefaultValue = LineStyle.Solid, Group = "Line Styles")]
        public LineStyle LineStyle { get; set; }

        [Parameter("Thickness", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "Line Styles")]
        public int LineThickness { get; set; }

        [Parameter("Color", DefaultValue = "#22FFFFFF", Group = "Line Styles")]
        public Color LineColor { get; set; }

        [Parameter("Enable Fills", DefaultValue = true, Group = "Fills")]
        public bool EnableFills { get; set; }

        [Parameter("Fill Pattern", DefaultValue = FillPattern.Odd, Group = "Fills")]
        public FillPattern FillPatternType { get; set; }

        [Parameter("Fill Color", DefaultValue = "#11778899", Group = "Fills")]
        public Color FillColor { get; set; }

        [Parameter("Enable Alternate Lines", DefaultValue = false, Group = "Alternate Lines")]
        public bool EnableAlternateColor { get; set; }

        [Parameter("Multiples", DefaultValue = 500, MinValue = 0, Group = "Alternate Lines")]
        public int AlternateMultiple { get; set; }

        [Parameter("Lines Color", DefaultValue = "#44FFAA00", Group = "Alternate Lines")]
        public Color AlternateColor { get; set; }

        [Parameter("Enable Highlight Level", DefaultValue = false, Group = "Highlight Level")]
        public bool EnableHighlightLevel { get; set; }

        [Parameter("Start Price", DefaultValue = 0.0, Group = "Highlight Level")]
        public double HighlightLevelStart { get; set; }

        [Parameter("End Price", DefaultValue = 0.0, Group = "Highlight Level")]
        public double HighlightLevelEnd { get; set; }

        [Parameter("Highlight Color", DefaultValue = "#224169E1", Group = "Highlight Level")]
        public Color HighlightColor { get; set; }

        [Parameter("Labels", DefaultValue = true, Group = "Labels")]
        public bool ShowLabels { get; set; }

        [Parameter("Font Size", DefaultValue = 10, MinValue = 8, MaxValue = 16, Group = "Labels")]
        public int LabelFontSize { get; set; }
    }

    public enum FillPattern
    {
        Odd,
        Even
    }
}
