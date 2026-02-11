using cAlgo.API;

namespace cAlgo
{
    public partial class AnchoredMovingAverage : Indicator
    {
        [Parameter("Type", DefaultValue = CustomMAType.ArnaudLegoux, Group = "Moving Average")]
        public CustomMAType MaType { get; set; }

        [Parameter("Source", DefaultValue = 0, Group = "Moving Average")]
        public DataSeries Source { get; set; }

        // NEW: User can choose Manual or Pre-defined period
        [Parameter("Period Type", DefaultValue = AnchorPointPeriod.Manual, Group = "Moving Average")]
        public AnchorPointPeriod AnchorPeriodType { get; set; }

        // UPDATED: Now only used when AnchorPeriodType = Manual
        [Parameter("Manual DateTime", DefaultValue = "01/01/2026 05:00", Group = "Moving Average")]
        public string StartDateTime { get; set; }

        // Maximum Period Parameter
        [Parameter("Max Period", DefaultValue = 0, MinValue = 0, Group = "Moving Average")]
        public int MaxPeriod { get; set; }

        // UPDATED: Renamed from "Pivot Depth" to "Band Range"
        // Now uses Fibonacci percentage levels directly
        [Parameter("Range", DefaultValue = BandRangeLevel.Fib382, Group = "Bands")]
        public BandRangeLevel BandRange { get; set; }

        // Band Parameters
        [Parameter("Visibility", DefaultValue = MABandVisibility.None, Group = "Bands")]
        public MABandVisibility BandVisibility { get; set; }
    }
}
