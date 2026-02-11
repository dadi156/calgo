using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, AccessRights = AccessRights.None)]
    public class WaddahAttarExplosion : Indicator
    {
        #region Parameters

        [Parameter("Sensitivity", DefaultValue = 150, MinValue = 1, Group = "WAE")]
        public int Sensitivity { get; set; }

        [Parameter("Fast EMA Period", DefaultValue = 20, MinValue = 1, Group = "MACD")]
        public int FastPeriod { get; set; }

        [Parameter("Slow EMA Period", DefaultValue = 40, MinValue = 1, Group = "MACD")]
        public int SlowPeriod { get; set; }

        [Parameter("BB Period", DefaultValue = 20, MinValue = 1, Group = "Bollinger Bands")]
        public int BbPeriod { get; set; }

        [Parameter("BB Multiplier", DefaultValue = 2.0, MinValue = 0.1, Group = "Bollinger Bands")]
        public double BbMultiplier { get; set; }

        [Parameter("Dead Zone Method", DefaultValue = DeadZoneMethod.ATR, Group = "Dead Zone")]
        public DeadZoneMethod DzMethod { get; set; }

        [Parameter("ATR Period", DefaultValue = 100, MinValue = 1, Group = "Dead Zone")]
        public int AtrPeriod { get; set; }

        [Parameter("ATR Multiplier", DefaultValue = 3.7, MinValue = 0.1, Group = "Dead Zone")]
        public double AtrMultiplier { get; set; }

        [Parameter("Fixed Dead Zone (Pips)", DefaultValue = 20, MinValue = 0, Group = "Dead Zone")]
        public double FixedDeadZonePips { get; set; }

        #endregion

        #region Outputs

        [Output("Trend Up", LineColor = "FF00BB22", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries TrendUp { get; set; }

        [Output("Trend Up Weak", LineColor = "Green", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries TrendUpWeak { get; set; }

        [Output("Trend Down", LineColor = "Red", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries TrendDown { get; set; }

        [Output("Trend Down Weak", LineColor = "FF990000", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries TrendDownWeak { get; set; }

        [Output("Explosion Line", LineColor = "GhostWhite")]
        public IndicatorDataSeries ExplosionLine { get; set; }

        [Output("Dead Zone", LineColor = "Yellow")]
        public IndicatorDataSeries DeadZone { get; set; }

        #endregion

        #region Private Fields

        private ExponentialMovingAverage _fastEma;
        private ExponentialMovingAverage _slowEma;
        private BollingerBands _bb;
        private AverageTrueRange _atr;
        private IndicatorDataSeries _macd;

        // Cached values
        private double _fixedDeadZoneValue;
        private double _prevTrendUp;
        private double _prevTrendDown;

        #endregion

        protected override void Initialize()
        {
            _fastEma = Indicators.ExponentialMovingAverage(Bars.ClosePrices, FastPeriod);
            _slowEma = Indicators.ExponentialMovingAverage(Bars.ClosePrices, SlowPeriod);
            _bb = Indicators.BollingerBands(Bars.ClosePrices, BbPeriod, BbMultiplier, MovingAverageType.Simple);
            _atr = Indicators.AverageTrueRange(AtrPeriod, MovingAverageType.Exponential);
            _macd = CreateDataSeries();

            // Pre-calculate fixed dead zone
            _fixedDeadZoneValue = FixedDeadZonePips * Symbol.PipSize;
        }

        public override void Calculate(int index)
        {
            _macd[index] = _fastEma.Result[index] - _slowEma.Result[index];

            ExplosionLine[index] = _bb.Top[index] - _bb.Bottom[index];

            DeadZone[index] = DzMethod == DeadZoneMethod.ATR
                ? _atr.Result[index] * AtrMultiplier
                : _fixedDeadZoneValue;

            if (index < 1)
                return;

            double macdDelta = (_macd[index] - _macd[index - 1]) * Sensitivity;

            if (macdDelta >= 0)
            {
                TrendDown[index] = double.NaN;
                TrendDownWeak[index] = double.NaN;

                if (macdDelta >= _prevTrendUp)
                {
                    TrendUp[index] = macdDelta;
                    TrendUpWeak[index] = double.NaN;
                }
                else
                {
                    TrendUpWeak[index] = macdDelta;
                    TrendUp[index] = double.NaN;
                }

                _prevTrendUp = macdDelta;
                _prevTrendDown = 0;
            }
            else
            {
                TrendUp[index] = double.NaN;
                TrendUpWeak[index] = double.NaN;

                double absValue = -macdDelta; // Faster than Math.Abs() for known negative

                if (absValue >= _prevTrendDown)
                {
                    TrendDown[index] = absValue;
                    TrendDownWeak[index] = double.NaN;
                }
                else
                {
                    TrendDownWeak[index] = absValue;
                    TrendDown[index] = double.NaN;
                }

                _prevTrendDown = absValue;
                _prevTrendUp = 0;
            }
        }

        public enum DeadZoneMethod
        {
            ATR,
            FixedPips
        }
    }
}
