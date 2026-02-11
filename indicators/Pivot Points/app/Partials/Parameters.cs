using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class PivotPoints : Indicator
    {
        #region Parameters

        [Parameter("Timeframe", DefaultValue = "Weekly", Group = "Settings")]
        public TimeFrame PivotTimeframe { get; set; }

        [Parameter("Type", DefaultValue = PivotPointType.Fibonacci, Group = "Settings")]
        public PivotPointType SelectedPivotType { get; set; }

        [Parameter("Pivot to Draw", DefaultValue = 1, MinValue = 1, Group = "Settings")]
        public int PivotToDraw { get; set; }

        [Parameter("S/R Levels", DefaultValue = 1, MinValue = 0, MaxValue = 6, Group = "Settings")]
        public int SRLevelsToShow { get; set; }

        [Parameter("Price Labels", DefaultValue = true, Group = "Display")]
        public bool ShowPriceInLabels { get; set; }

        [Parameter("Pivot Color", DefaultValue = "FFDEB887", Group = "Display")]
        public Color PivotLineColor { get; set; }

        [Parameter("Resistance Color", DefaultValue = "FFDC143C", Group = "Display")]
        public Color ResistanceLineColor { get; set; }

        [Parameter("Support Color", DefaultValue = "FF2E8B57", Group = "Display")]
        public Color SupportLineColor { get; set; }

        [Parameter("Display", DefaultValue = PanelPosition.None, Group = "Metrics Panel")]
        public PanelPosition MetricsPanelDisplay { get; set; }

        [Parameter("Metrics Offset", DefaultValue = 0, MinValue = -10, Group = "Metrics Panel")]
        public int MetricsOffset { get; set; }

        [Parameter("Level Zone Size (%)", DefaultValue = 49.99, MinValue = 1, MaxValue = 50, Group = "Metrics Panel")]
        public double ZoneSizePercent { get; set; }

        [Parameter("Default Columns", DefaultValue = "Bar, Vol, Prs, Dom", Group = "Metrics Panel")]
        public string MetricsDefaultColumns { get; set; }

        [Parameter("Margin", DefaultValue = "10, 40, 10, 40", Group = "Metrics Panel")]
        public string MetricsPanelMargin { get; set; }

        [Parameter("Buttons Position", DefaultValue = ToggleButtonsPosition.BottomRight, Group = "Metrics Panel")]
        public ToggleButtonsPosition MetricsButtonsPosition { get; set; }

        [Parameter("Active Level Highlight", DefaultValue = "FF3B3B3B", Group = "Metrics Panel")]
        public Color ActiveLevelHighlight { get; set; }

        #endregion
    }
}
