using cAlgo.API;

namespace cAlgo
{
    public partial class AnchoredMovingAverage : Indicator
    {
        [Output("Growing MA", LineColor = "LightSteelBlue")]
        public IndicatorDataSeries Result { get; set; }

        // Fibonacci Band Output Series
        [Output("Upper Band (100%)", LineColor = "SeaGreen")]
        public IndicatorDataSeries UpperBand { get; set; }

        [Output("Fib 88.6%", LineColor = "882E8B57")]
        public IndicatorDataSeries FiboLevel886 { get; set; }

        [Output("Fib 76.4%", LineColor = "882E8B57")]
        public IndicatorDataSeries FiboLevel764 { get; set; }

        [Output("Fib 62.8%", LineColor = "660071C1")]
        public IndicatorDataSeries FiboLevel628 { get; set; }

        [Output("Fib 38.2%", LineColor = "660071C1")]
        public IndicatorDataSeries FiboLevel382 { get; set; }

        [Output("Fib 23.6%", LineColor = "66DC143C")]
        public IndicatorDataSeries FiboLevel236 { get; set; }

        [Output("Fib 11.4%", LineColor = "66DC143C")]
        public IndicatorDataSeries FiboLevel114 { get; set; }

        [Output("Lower Band (0%)", LineColor = "Crimson")]
        public IndicatorDataSeries LowerBand { get; set; }
    }
}
