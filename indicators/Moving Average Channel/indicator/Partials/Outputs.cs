using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class MovingAverageChannel : Indicator
    {
        [Output("Open Line", LineColor = "SeaGreen")]
        public IndicatorDataSeries OpenLine { get; set; }

        [Output("Close Line", LineColor = "Crimson")]
        public IndicatorDataSeries CloseLine { get; set; }

        [Output("High Line", LineColor = "66B0C4DE")]
        public IndicatorDataSeries HighLine { get; set; }

        [Output("Low Line", LineColor = "66B0C4DE")]
        public IndicatorDataSeries LowLine { get; set; }

        [Output("Median Line", LineColor = "RoyalBlue")]
        public IndicatorDataSeries MedianLine { get; set; }

        [Output("High Reversion", LineColor = "Crimson")]
        public IndicatorDataSeries UpperReversionZone { get; set; }

        [Output("Low Reversion", LineColor = "SeaGreen")]
        public IndicatorDataSeries LowerReversionZone { get; set; }
    }
}
