using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Outputs

        [Output("Bullish", LineColor = "ForestGreen", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries BullishOutput { get; set; }

        [Output("Bearish", LineColor = "Crimson", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries BearishOutput { get; set; }

        [Output("Climax Buying", LineColor = "Yellow", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries ClimaxBuyingOutput { get; set; }

        [Output("Climax Selling", LineColor = "Orange", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries ClimaxSellingOutput { get; set; }

        [Output("No Demand", LineColor = "Gray", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries NoDemandOutput { get; set; }

        [Output("No Supply", LineColor = "Silver", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries NoSupplyOutput { get; set; }

        [Output("Absorption Buying", LineColor = "DodgerBlue", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries AbsorptionBuyingOutput { get; set; }

        [Output("Absorption Selling", LineColor = "DeepPink", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries AbsorptionSellingOutput { get; set; }

        [Output("ENR Bullish", LineColor = "Cyan", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries ENRBullishOutput { get; set; }

        [Output("ENR Bearish", LineColor = "Magenta", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries ENRBearishOutput { get; set; }

        [Output("Volume Average", LineColor = "White")]
        public IndicatorDataSeries AverageLine { get; set; }

        #endregion
    }
}
