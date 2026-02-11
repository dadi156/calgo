using System;
using System.Linq;
using cAlgo.API;

namespace cAlgo
{
    public class MomentumAdaptiveMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _ma;

        public MomentumAdaptiveMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }

        public void Initialize()
        {
            _ma = _indicator.CreateDataSeries();
        }

        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;

            // Need at least period+1 bars
            if (index < period + 1)
            {
                if (index == 0)
                {
                    _ma[0] = _indicator.Source[0];
                }
                return new MAResult(_indicator.Source[index]);
            }

            // Calculate the change ratio (Chande Momentum Oscillator component)
            double sumUp = 0;
            double sumDown = 0;

            for (int i = 0; i < period; i++)
            {
                double diff = _indicator.Source[index - i] - _indicator.Source[index - i - 1];
                if (diff > 0)
                    sumUp += diff;
                else
                    sumDown += Math.Abs(diff);
            }

            // Calculate CMO value (-100 to +100)
            double cmo = 0;
            if (sumUp + sumDown > 0)
                cmo = 100 * Math.Abs((sumUp - sumDown) / (sumUp + sumDown));

            // Convert to a scale factor (0 to 1)
            double scaleFactor = cmo / 100.0;

            // Calculate alpha with momentum adjustment
            double alpha = 2.0 / (period + 1.0) * scaleFactor;

            // Initialize MA with first value if needed
            if (double.IsNaN(_ma[index - 1]))
            {
                _ma[index - 1] = _indicator.Source[index - 1];
            }

            // Calculate Momentum Adaptive MA
            _ma[index] = alpha * _indicator.Source[index] + (1 - alpha) * _ma[index - 1];

            return new MAResult(_ma[index]);
        }
    }
}
