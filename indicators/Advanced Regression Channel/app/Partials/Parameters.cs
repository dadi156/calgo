using cAlgo.API;

namespace cAlgo
{
    public partial class AdvancedRegressionChannel : Indicator
    {
        #region Settings

        [Parameter("Regression Type", DefaultValue = RegressionType.Linear, Group = "Settings")]
        public RegressionType RegressionType { get; set; }

        [Parameter("Period", DefaultValue = 120, MinValue = 1, MaxValue = 5000, Group = "Settings")]
        public int Period { get; set; }

        [Parameter("Multi-Timeframe", DefaultValue = false, Group = "Settings")]
        public bool UseMultiTimeframe { get; set; }

        [Parameter("Timeframe", DefaultValue = "Daily", Group = "Settings")]
        public TimeFrame SelectedTimeFrame { get; set; }

        #endregion

        #region Custom Range Settings

        [Parameter("Custom Range", DefaultValue = false, Group = "Custom Range")]
        public bool UseDateRange { get; set; }

        [Parameter("Start DateTime", DefaultValue = "03/11/2025 05:00", Group = "Custom Range")]
        public string StartDateStr { get; set; }

        [Parameter("End DateTime", DefaultValue = "29/11/2025 04:00", Group = "Custom Range")]
        public string EndDateStr { get; set; }

        #endregion

        #region Polynomial Regression Settings

        [Parameter("Polynomial Degree", DefaultValue = 2, MinValue = 1, MaxValue = 5, Group = "Polynomial Regression")]
        public int Degree { get; set; }

        [Parameter("Channel Width (StdDev)", DefaultValue = 2.0, MinValue = 0.1, Group = "Polynomial Regression")]
        public double ChannelWidth { get; set; }

        #endregion

        #region Channel Lines

        [Parameter("Extend to Infinity", DefaultValue = false, Group = "Channel Lines")]
        public bool ExtendToInfinity { get; set; }

        #endregion

        #region Timezone Settings

        [Parameter("Timezone (UTC+/-)", DefaultValue = 7, MinValue = -12, MaxValue = 14, Group = "Timezone")]
        public int UserTimezone { get; set; }

        #endregion
    }
}
