// Outputs - Separated from main indicator
using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class TrendVolatilityTrail : Indicator
    {
        [Output("Bull Trail", PlotType = PlotType.DiscontinuousLine, LineColor = "Lime", LineStyle = LineStyle.Solid, Thickness = 2)]
        public IndicatorDataSeries BullTrail { get; set; }

        [Output("Bear Trail", PlotType = PlotType.DiscontinuousLine, LineColor = "Red", LineStyle = LineStyle.Solid, Thickness = 2)]
        public IndicatorDataSeries BearTrail { get; set; }
    }
}
