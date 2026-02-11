using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class VolumeProfile : Indicator
    {
        #region Parameters

        [Parameter("Lookback Periods", DefaultValue = 120, Group = "Volume Profile")]
        public int LookbackPeriods { get; set; }

        [Parameter("Price Levels", DefaultValue = 10, Group = "Volume Profile")]
        public int PriceLevels { get; set; }

        [Parameter("Value Area %", DefaultValue = 70, Group = "Volume Profile")]
        public int ValueAreaPercent { get; set; }

        [Parameter("Full Width", DefaultValue = false, Group = "Volume Profile")]
        public bool FullWidthBars { get; set; }

        [Parameter("Margin (Pips)", DefaultValue = 1.0, Group = "Volume Profile")]
        public double PipMargin { get; set; }

        [Parameter("Custom Date", DefaultValue = false, Group = "Custom Date")]
        public bool UseDateTimeProfiles { get; set; }

        [Parameter("Start DateTime", DefaultValue = "01/10/2025 00:00", Group = "Custom Date")]
        public string StartDateTimeProfiles { get; set; }

        [Parameter("End DateTime", DefaultValue = "31/10/2025 00:00", Group = "Custom Date")]
        public string EndDateTimeProfiles { get; set; }

        [Parameter("Timezone Offset", DefaultValue = 0, MinValue = -12, MaxValue = 14, Group = "Custom Date")]
        public int TimezoneOffsetHours { get; set; }

        [Parameter("Enable TPO", DefaultValue = true, Group = "Time Price Opportunity (TPO)")]
        public bool EnableTPO { get; set; }

        [Parameter("TPO Period (minutes)", DefaultValue = 60, Group = "Time Price Opportunity (TPO)")]
        public int TPOPeriodMinutes { get; set; }

        [Parameter("Initial Balance", DefaultValue = true, Group = "Time Price Opportunity (TPO)")]
        public bool CalculateInitialBalance { get; set; }

        [Parameter("Initial Balance Period (hours)", DefaultValue = 1, Group = "Time Price Opportunity (TPO)")]
        public int InitialBalancePeriod { get; set; }

        [Parameter("Level Info", DefaultValue = true, Group = "Display")]
        public bool ShowLevelInfo { get; set; }

        [Parameter("All Levels", DefaultValue = true, Group = "Display")]
        public bool ShowAllLevels { get; set; }

        [Parameter("Volume and TPO", DefaultValue = false, Group = "Display")]
        public bool ShowVolTPO { get; set; }

        [Parameter("Text Position", DefaultValue = TextPosition.Right, Group = "Display")]
        public TextPosition InfoTextPosition { get; set; }

        [Parameter("Font Family", DefaultValue = "Bahnschrift", Group = "Display")]
        public string FontFamily { get; set; }

        [Parameter("Font Size", DefaultValue = 11, Group = "Display")]
        public int FontSize { get; set; }

        [Parameter("Buy Pressure", DefaultValue = "2200FF00", Group = "Colors")]
        public Color PositiveDeltaColor { get; set; }

        [Parameter("Sell Pressure", DefaultValue = "22FF0000", Group = "Colors")]
        public Color NegativeDeltaColor { get; set; }

        [Parameter("Positive Text", DefaultValue = "8800FF00", Group = "Colors")]
        public Color PositiveTextColor { get; set; }

        [Parameter("Negative Text", DefaultValue = "88FF0000", Group = "Colors")]
        public Color NegativeTextColor { get; set; }

        [Parameter("TPO", DefaultValue = "88FFFFFF", Group = "Colors")]
        public Color TPOTextColor { get; set; }

        #endregion
    }
}
