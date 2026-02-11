using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class WilderMACalculation : IMovingAverage
    {
        private double[] _wilderMA;
        private readonly int _period;
        private bool _isInitialized;

        public WilderMACalculation(int period, int arraySize)
        {
            _period = period;
            _wilderMA = new double[arraySize];
            _isInitialized = false;
        }

        public double Calculate(int index, DataSeries priceSource)
        {
            try
            {
                // Make sure array is big enough
                if (index >= _wilderMA.Length)
                {
                    Array.Resize(ref _wilderMA, Math.Max(index + 1000, _wilderMA.Length * 2));
                }

                // For first bar, use current price
                if (index == 0)
                {
                    _wilderMA[index] = priceSource[index];
                    _isInitialized = false;
                    return _wilderMA[index];
                }

                // Need minimum bars to calculate first Wilder value
                if (index < _period - 1)
                {
                    _wilderMA[index] = priceSource[index];
                    _isInitialized = false;
                    return _wilderMA[index];
                }

                // Calculate first Wilder MA value using Simple Average
                if (!_isInitialized)
                {
                    double sum = 0;
                    int validCount = 0;

                    for (int i = 0; i < _period; i++)
                    {
                        int lookbackIndex = index - i;
                        if (lookbackIndex >= 0 && lookbackIndex < priceSource.Count)
                        {
                            double price = priceSource[lookbackIndex];
                            if (!double.IsNaN(price) && !double.IsInfinity(price))
                            {
                                sum += price;
                                validCount++;
                            }
                        }
                    }

                    if (validCount > 0)
                    {
                        _wilderMA[index] = sum / validCount;
                        _isInitialized = true;
                    }
                    else
                    {
                        _wilderMA[index] = priceSource[index];
                    }

                    return _wilderMA[index];
                }

                // Wilder's Smoothing formula: 
                // Wilder[i] = (Wilder[i-1] * (Period - 1) + Price[i]) / Period
                // This is equivalent to: Wilder[i-1] + (Price[i] - Wilder[i-1]) / Period
                _wilderMA[index] = (_wilderMA[index - 1] * (_period - 1) + priceSource[index]) / _period;

                // Fix NaN values
                if (double.IsNaN(_wilderMA[index]) || double.IsInfinity(_wilderMA[index]))
                {
                    _wilderMA[index] = index > 0 ? _wilderMA[index - 1] : priceSource[index];
                }

                return _wilderMA[index];
            }
            catch (Exception)
            {
                // If error, return previous value or current price
                return index > 0 ? _wilderMA[index - 1] : priceSource[index];
            }
        }

        // Initialize MA with first value
        public void Initialize(double firstValue)
        {
            if (_wilderMA.Length > 0)
            {
                _wilderMA[0] = firstValue;
                _isInitialized = false;
            }
        }
    }
}
