using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Balance of Power (BOP) - Measures the strength of buyers versus sellers in the market
    /// </summary>
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]

    public class BalanceOfPower : Indicator
    {
        #region Parameters

        [Parameter("Smoothing Type", DefaultValue = MovingAverageType.Simple, Group = "Balance of Power")]
        public MovingAverageType SmoothingType { get; set; }

        [Parameter("Smoothing Period", DefaultValue = 14, MinValue = 1, Group = "Balance of Power")]
        public int SmoothingPeriod { get; set; }

        [Parameter("Type", DefaultValue = MovingAverageType.Simple, Group = "Signal")]
        public MovingAverageType SignalType { get; set; }

        [Parameter("Period", DefaultValue = 14, MinValue = 1, Group = "Signal")]
        public int SignalPeriod { get; set; }

        [Parameter("Mode", DefaultValue = HistogramMode.BalanceOfPower, Group = "Histogram Mode")]
        public HistogramMode HistoMode { get; set; }

        [Output("Result (+)", LineColor = "FF00843B", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries PositiveResult { get; set; }

        [Output("Result (-)", LineColor = "FFF15923", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries NegativeResult { get; set; }

        [Output("Balance of Power", LineColor = "Crimson")]
        public IndicatorDataSeries BopResult { get; set; }

        [Output("Signal", LineColor = "LightSteelBlue")]
        public IndicatorDataSeries SignalResult { get; set; }

        #endregion

        #region Private Members

        private IndicatorDataSeries _rawBop;
        private MovingAverage _ma;
        private MovingAverage _signalMa;

        #endregion

        protected override void Initialize()
        {
            // Initialize raw BOP data series
            _rawBop = CreateDataSeries();

            // Initialize moving average
            _ma = Indicators.MovingAverage(_rawBop, SmoothingPeriod, SmoothingType);

            _signalMa = Indicators.MovingAverage(BopResult, SignalPeriod, SignalType);
        }

        public override void Calculate(int index)
        {
            // Calculate raw BOP value
            double high = Bars.HighPrices[index];
            double low = Bars.LowPrices[index];
            double open = Bars.OpenPrices[index];
            double close = Bars.ClosePrices[index];

            // Check for potential division by zero
            if (Math.Abs(high - low) < double.Epsilon)
            {
                // If high equals low, use previous value or zero
                _rawBop[index] = index > 0 ? _rawBop[index - 1] : 0;
            }
            else
            {
                // Calculate BOP: (Close - Open) / (High - Low)
                _rawBop[index] = (close - open) / (high - low);
            }

            // Apply smoothing to raw BOP
            BopResult[index] = _ma.Result[index];

            SignalResult[index] = _signalMa.Result[index];

            double histogramValue = HistoMode == HistogramMode.BalanceOfPower ? BopResult[index] : BopResult[index] - SignalResult[index];

            if (histogramValue > 0)
            {
                PositiveResult[index] = histogramValue;
                NegativeResult[index] = 0;
            }
            else
            {
                PositiveResult[index] = 0;
                NegativeResult[index] = histogramValue;
            }
        }
    }

    public enum HistogramMode
    {
        BalanceOfPower,
        Signal
    }
}
