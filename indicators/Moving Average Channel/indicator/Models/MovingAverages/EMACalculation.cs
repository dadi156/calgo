using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class EMACalculation : IMovingAverage
    {
        private double[] _ema;
        private readonly int _period;

        public EMACalculation(int period, int arraySize)
        {
            _period = period;
            _ema = new double[arraySize];
        }

        public double Calculate(int index, DataSeries priceSource)
        {
            try
            {
                // Make sure array is big enough
                if (index >= _ema.Length)
                {
                    Array.Resize(ref _ema, Math.Max(index + 1000, _ema.Length * 2));
                }

                // For first bar, use current price
                if (index == 0)
                {
                    _ema[index] = priceSource[index];
                    return _ema[index];
                }

                // Calculate EMA smoothing factor (alpha)
                // Alpha = 2 / (period + 1)
                double alpha = 2.0 / (_period + 1);

                // EMA formula: EMA = alpha * price + (1 - alpha) * previous_EMA
                _ema[index] = alpha * priceSource[index] + (1 - alpha) * _ema[index - 1];

                // Fix NaN values
                if (double.IsNaN(_ema[index]) || double.IsInfinity(_ema[index]))
                {
                    _ema[index] = index > 0 ? _ema[index - 1] : priceSource[index];
                }

                return _ema[index];
            }
            catch (Exception)
            {
                // If error, return previous value or current price
                return index > 0 ? _ema[index - 1] : priceSource[index];
            }
        }

        // Initialize MA with first value
        public void Initialize(double firstValue)
        {
            if (_ema.Length > 0)
                _ema[0] = firstValue;
        }
    }
}
