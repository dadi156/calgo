using cAlgo.API;

namespace cAlgo
{
    public partial class AdvancedRegressionChannel : Indicator
    {
        [Output("100.00%", LineColor = "Crimson")]
        public IndicatorDataSeries Fib100 { get; set; }

        [Output("88.60%", LineColor = "LightSlateGray")]
        public IndicatorDataSeries Level886 { get; set; }

        [Output("76.40%", LineColor = "LightSlateGray")]
        public IndicatorDataSeries Fib764 { get; set; }

        [Output("61.80%", LineColor = "LightSlateGray")]
        public IndicatorDataSeries Fib618 { get; set; }

        [Output("50.00%", LineColor = "DarkGoldenRod")]
        public IndicatorDataSeries Fib50 { get; set; }

        [Output("38.20%", LineColor = "LightSlateGray")]
        public IndicatorDataSeries Fib382 { get; set; }

        [Output("23.60%", LineColor = "LightSlateGray")]
        public IndicatorDataSeries Fib236 { get; set; }

        [Output("11.40%", LineColor = "LightSlateGray")]
        public IndicatorDataSeries Level114 { get; set; }

        [Output("0.00%", LineColor = "DarkSeaGreen")]
        public IndicatorDataSeries Fib0 { get; set; }
    }
}
