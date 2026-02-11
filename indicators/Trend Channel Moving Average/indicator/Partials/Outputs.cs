using cAlgo.API;

namespace cAlgo
{
    public partial class TrendChannelMovingAverage : Indicator
    {
        #region Outputs

        // Main MA Lines
        [Output("Open Price", PlotType = PlotType.DiscontinuousLine, LineColor = "SeaGreen", LineStyle = LineStyle.Lines)]
        public IndicatorDataSeries OpenLine { get; set; }

        [Output("Close Price", PlotType = PlotType.DiscontinuousLine, LineColor = "Crimson")]
        public IndicatorDataSeries CloseLine { get; set; }

        [Output("Median Line", PlotType = PlotType.DiscontinuousLine, LineColor = "LightSteelBlue")]
        public IndicatorDataSeries MedianLine { get; set; }

        // High/Low Lines with Trend Colors
        [Output("High Line Uptrend", PlotType = PlotType.DiscontinuousLine, LineColor = "DodgerBlue")]
        public IndicatorDataSeries HighLineUptrend { get; set; }

        [Output("High Line Downtrend", PlotType = PlotType.DiscontinuousLine, LineColor = "Gold")]
        public IndicatorDataSeries HighLineDowntrend { get; set; }

        [Output("High Line Neutral", PlotType = PlotType.DiscontinuousLine, LineColor = "LightSlateGray")]
        public IndicatorDataSeries HighLineNeutral { get; set; }

        [Output("Low Line Uptrend", PlotType = PlotType.DiscontinuousLine, LineColor = "DodgerBlue")]
        public IndicatorDataSeries LowLineUptrend { get; set; }

        [Output("Low Line Downtrend", PlotType = PlotType.DiscontinuousLine, LineColor = "Gold")]
        public IndicatorDataSeries LowLineDowntrend { get; set; }

        [Output("Low Line Neutral", PlotType = PlotType.DiscontinuousLine, LineColor = "LightSlateGray")]
        public IndicatorDataSeries LowLineNeutral { get; set; }

        // Fibonacci Levels (reused for all zones - main, upper, and lower)
        [Output("Fib. 100.00%", PlotType = PlotType.DiscontinuousLine, LineColor = "DeepPink")]
        public IndicatorDataSeries Fib10000 { get; set; }

        [Output("Fib. 88.60%", PlotType = PlotType.DiscontinuousLine, LineColor = "88B0C4DE")]
        public IndicatorDataSeries Fib8860 { get; set; }

        [Output("Fib. 78.60%", PlotType = PlotType.DiscontinuousLine, LineColor = "88B0C4DE")]
        public IndicatorDataSeries Fib7860 { get; set; }

        [Output("Fib. 61.80%", PlotType = PlotType.DiscontinuousLine, LineColor = "88B0C4DE")]
        public IndicatorDataSeries Fib6180 { get; set; }

        [Output("Fib. 50.00%", PlotType = PlotType.DiscontinuousLine, LineColor = "DeepPink")]
        public IndicatorDataSeries Fib5000 { get; set; }

        [Output("Fib. 38.20%", PlotType = PlotType.DiscontinuousLine, LineColor = "88B0C4DE")]
        public IndicatorDataSeries Fib3820 { get; set; }

        [Output("Fib. 23.60%", PlotType = PlotType.DiscontinuousLine, LineColor = "88B0C4DE")]
        public IndicatorDataSeries Fib2360 { get; set; }

        [Output("Fib. 11.40%", PlotType = PlotType.DiscontinuousLine, LineColor = "88B0C4DE")]
        public IndicatorDataSeries Fib1140 { get; set; }

        [Output("Fib. 0.00%", PlotType = PlotType.DiscontinuousLine, LineColor = "DeepPink")]
        public IndicatorDataSeries Fib0000 { get; set; }

        #endregion
    }
}
