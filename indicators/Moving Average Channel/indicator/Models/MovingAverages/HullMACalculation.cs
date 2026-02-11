using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class HullMACalculation : IMovingAverage
    {
        private double[] _hullMA;
        private double[] _wmaHalf;      // WMA(n/2)
        private double[] _wmaFull;      // WMA(n)
        private double[] _rawHull;      // 2 * WMA(n/2) - WMA(n)
        
        private readonly int _period;
        private readonly int _halfPeriod;
        private readonly int _sqrtPeriod;

        public HullMACalculation(int period, int arraySize)
        {
            _period = period;
            _halfPeriod = period / 2;
            _sqrtPeriod = (int)Math.Round(Math.Sqrt(period));
            
            // Create arrays
            _hullMA = new double[arraySize];
            _wmaHalf = new double[arraySize];
            _wmaFull = new double[arraySize];
            _rawHull = new double[arraySize];
            
            // Initialize arrays with zero
            for (int i = 0; i < arraySize; i++)
            {
                _hullMA[i] = 0;
                _wmaHalf[i] = 0;
                _wmaFull[i] = 0;
                _rawHull[i] = 0;
            }
        }

        public double Calculate(int index, DataSeries priceSource)
        {
            try
            {
                // Resize arrays if needed
                if (index >= _hullMA.Length)
                {
                    int newSize = Math.Max(index + 1000, _hullMA.Length * 2);
                    Array.Resize(ref _hullMA, newSize);
                    Array.Resize(ref _wmaHalf, newSize);
                    Array.Resize(ref _wmaFull, newSize);
                    Array.Resize(ref _rawHull, newSize);
                }

                // Need minimum bars for calculation
                if (index < _period - 1)
                {
                    // For first few bars, just use current price
                    _hullMA[index] = priceSource[index];
                    return _hullMA[index];
                }

                // Step 1: Calculate WMA(n/2)
                _wmaHalf[index] = CalculateWMA(index, priceSource, _halfPeriod);

                // Step 2: Calculate WMA(n)
                _wmaFull[index] = CalculateWMA(index, priceSource, _period);

                // Step 3: Calculate Raw Hull: 2 * WMA(n/2) - WMA(n)
                _rawHull[index] = (2 * _wmaHalf[index]) - _wmaFull[index];

                // Step 4: Calculate final HMA: WMA(sqrt(n)) of Raw Hull values
                if (index >= _sqrtPeriod - 1)
                {
                    _hullMA[index] = CalculateWMAFromArray(index, _rawHull, _sqrtPeriod);
                }
                else
                {
                    // Not enough bars for final WMA yet
                    _hullMA[index] = _rawHull[index];
                }

                // Fix NaN values
                if (double.IsNaN(_hullMA[index]) || double.IsInfinity(_hullMA[index]))
                {
                    _hullMA[index] = index > 0 ? _hullMA[index - 1] : priceSource[index];
                }

                return _hullMA[index];
            }
            catch (Exception)
            {
                // If error, return previous value or current price
                return index > 0 ? _hullMA[index - 1] : priceSource[index];
            }
        }

        // Calculate Weighted Moving Average from price source
        private double CalculateWMA(int index, DataSeries priceSource, int period)
        {
            double sum = 0;
            double weightSum = 0;

            for (int i = 0; i < period; i++)
            {
                int lookbackIndex = index - i;
                if (lookbackIndex >= 0 && lookbackIndex < priceSource.Count)
                {
                    double price = priceSource[lookbackIndex];
                    if (!double.IsNaN(price) && !double.IsInfinity(price))
                    {
                        int weight = period - i;  // Weight decreases linearly
                        sum += price * weight;
                        weightSum += weight;
                    }
                }
            }

            return weightSum > 0 ? sum / weightSum : 0;
        }

        // Calculate Weighted Moving Average from array (for Raw Hull values)
        private double CalculateWMAFromArray(int index, double[] values, int period)
        {
            double sum = 0;
            double weightSum = 0;

            for (int i = 0; i < period; i++)
            {
                int lookbackIndex = index - i;
                if (lookbackIndex >= 0 && lookbackIndex < values.Length)
                {
                    double value = values[lookbackIndex];
                    if (!double.IsNaN(value) && !double.IsInfinity(value))
                    {
                        int weight = period - i;  // Weight decreases linearly
                        sum += value * weight;
                        weightSum += weight;
                    }
                }
            }

            return weightSum > 0 ? sum / weightSum : 0;
        }

        // Initialize MA with first value
        public void Initialize(double firstValue)
        {
            if (_hullMA.Length > 0)
            {
                _hullMA[0] = firstValue;
                _wmaHalf[0] = firstValue;
                _wmaFull[0] = firstValue;
                _rawHull[0] = firstValue;
            }
        }
    }
}
