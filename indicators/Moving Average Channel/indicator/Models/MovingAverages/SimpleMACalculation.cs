using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class SimpleMACalculation : IMovingAverage
    {
        private double[] _sma;
        private readonly int _period;

        public SimpleMACalculation(int period, int arraySize)
        {
            _period = period;
            _sma = new double[arraySize];
        }

        public double Calculate(int index, DataSeries priceSource)
        {
            try
            {
                // Need minimum bars for calculation
                if (index < _period - 1)
                {
                    // For first few bars, just use current price
                    double fallbackPrice = priceSource[Math.Max(0, index)];
                    if (double.IsNaN(fallbackPrice) || double.IsInfinity(fallbackPrice))
                        return 0;

                    _sma[index] = fallbackPrice;
                    return fallbackPrice;
                }

                // Make sure array is big enough
                if (index >= _sma.Length)
                {
                    Array.Resize(ref _sma, Math.Max(index + 1000, _sma.Length * 2));
                }

                // Calculate Simple Moving Average
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

                // Calculate average
                if (validCount > 0)
                {
                    _sma[index] = sum / validCount;
                }
                else
                {
                    // If no valid prices, use previous SMA or current price
                    _sma[index] = index > 0 ? _sma[index - 1] : priceSource[index];
                }

                // Final check
                if (double.IsNaN(_sma[index]) || double.IsInfinity(_sma[index]))
                {
                    _sma[index] = index > 0 ? _sma[index - 1] : priceSource[index];
                }

                return _sma[index];
            }
            catch (Exception)
            {
                // If error, return previous value or current price
                return index > 0 ? _sma[index - 1] : priceSource[index];
            }
        }

        // Initialize MA with first value - NEEDED by MultiMAManager
        public void Initialize(double firstValue)
        {
            if (_sma.Length > 0)
                _sma[0] = firstValue;
        }

        // REMOVED DEAD METHODS:
        // - GetValue() - never called
        // - GetPeriod() - never called
    }
}
