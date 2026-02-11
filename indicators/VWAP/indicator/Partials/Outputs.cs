using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class VWAP : Indicator
    {
        #region Outputs

        [Output("VWAP", LineColor = "LightSteelBlue")]
        public IndicatorDataSeries VWAP { get; set; }

        [Output("Fibo 100%", LineColor = "SeaGreen")]
        public IndicatorDataSeries UpperBand { get; set; }

        [Output("Fibo 88.6%", LineColor = "882E8B57")]
        public IndicatorDataSeries FiboLevel886 { get; set; }

        [Output("Fibo 76.4%", LineColor = "882E8B57")]
        public IndicatorDataSeries FiboLevel764 { get; set; }

        [Output("Fibo 62.8%", LineColor = "882E8B57")]
        public IndicatorDataSeries FiboLevel628 { get; set; }

        [Output("Fibo 38.2%", LineColor = "66DC143C")]
        public IndicatorDataSeries FiboLevel382 { get; set; }

        [Output("Fibo 23.6%", LineColor = "66DC143C")]
        public IndicatorDataSeries FiboLevel236 { get; set; }

        [Output("Fibo 11.4%", LineColor = "66DC143C")]
        public IndicatorDataSeries FiboLevel114 { get; set; }

        [Output("Fibo 0%", LineColor = "Crimson")]
        public IndicatorDataSeries LowerBand { get; set; }

        #endregion
    }
}
