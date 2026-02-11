using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class DSMACalculation : IMovingAverage
    {
        // SuperSmoother filter coefficients
        private double _a1;
        private double _b1;
        private double _c1;
        private double _c2;
        private double _c3;
        
        // Settings
        private readonly double _period;
        
        // Data storage arrays
        private double[] _zeros;
        private double[] _filt;
        private double[] _dsmaValues;

        public DSMACalculation(double period, int arraySize)
        {
            _period = period;
            
            // Create arrays
            _zeros = new double[arraySize];
            _filt = new double[arraySize];
            _dsmaValues = new double[arraySize];
            
            // Initialize arrays with zero
            for (int i = 0; i < arraySize; i++)
            {
                _zeros[i] = 0;
                _filt[i] = 0;
                _dsmaValues[i] = 0;
            }
            
            // Calculate SuperSmoother coefficients
            CalculateCoefficients();
        }

        private void CalculateCoefficients()
        {
            double criticalPeriod = _period * 0.5;
            
            _a1 = Math.Exp(-1.414 * Math.PI / criticalPeriod);
            _b1 = 2 * _a1 * Math.Cos(1.414 * Math.PI / criticalPeriod);
            _c2 = _b1;
            _c3 = -_a1 * _a1;
            _c1 = 1 - _c2 - _c3;
        }

        public double Calculate(int index, DataSeries priceSource)
        {
            try
            {
                // Resize arrays if needed
                if (index >= _dsmaValues.Length)
                {
                    int newSize = Math.Max(index + 1000, _dsmaValues.Length * 2);
                    Array.Resize(ref _zeros, newSize);
                    Array.Resize(ref _filt, newSize);
                    Array.Resize(ref _dsmaValues, newSize);
                }

                // Handle first few bars
                if (index < 3)
                {
                    _zeros[index] = 0;
                    _filt[index] = 0;
                    _dsmaValues[index] = priceSource[index];
                    return _dsmaValues[index];
                }

                // Step 1: Calculate Zeros oscillator
                _zeros[index] = priceSource[index] - priceSource[index - 2];

                // Step 2: Apply SuperSmoother filter
                _filt[index] = _c1 * (_zeros[index] + _zeros[index - 1]) / 2 + 
                             _c2 * _filt[index - 1] + 
                             _c3 * _filt[index - 2];
                
                // Fix NaN values
                if (double.IsNaN(_filt[index]) || double.IsInfinity(_filt[index]))
                {
                    _filt[index] = 0;
                }

                // Step 3: Calculate DSMA
                if (index >= (int)_period + 2)
                {
                    double rms = CalculateRMS(index);
                    double scaledFilt = 0;
                    
                    if (rms > 0.000001)
                    {
                        scaledFilt = _filt[index] / rms;
                    }

                    double alpha = Math.Abs(scaledFilt) * 5.0 / _period;
                    alpha = Math.Max(0.001, Math.Min(0.999, alpha));

                    _dsmaValues[index] = alpha * priceSource[index] + 
                                       (1 - alpha) * _dsmaValues[index - 1];
                }
                else
                {
                    // Use simple EMA for early bars
                    double simpleAlpha = 2.0 / (_period + 1);
                    _dsmaValues[index] = simpleAlpha * priceSource[index] + 
                                       (1 - simpleAlpha) * _dsmaValues[index - 1];
                }

                // Fix NaN values
                if (double.IsNaN(_dsmaValues[index]) || double.IsInfinity(_dsmaValues[index]))
                {
                    _dsmaValues[index] = priceSource[index];
                }

                return _dsmaValues[index];
            }
            catch (Exception)
            {
                // If error, return previous value or current price
                return index > 0 ? _dsmaValues[index - 1] : priceSource[index];
            }
        }

        private double CalculateRMS(int index)
        {
            double sumSquares = 0;
            int validPoints = 0;
            
            // Convert double Period to int for the loop
            int periodInt = (int)_period;
            
            for (int i = 0; i < periodInt; i++)
            {
                int lookbackIndex = index - i;
                if (lookbackIndex >= 0 && lookbackIndex < _filt.Length && !double.IsNaN(_filt[lookbackIndex]))
                {
                    sumSquares += _filt[lookbackIndex] * _filt[lookbackIndex];
                    validPoints++;
                }
            }
            
            return validPoints > 0 ? Math.Sqrt(sumSquares / validPoints) : 0;
        }

        // Initialize MA with first value
        public void Initialize(double firstValue)
        {
            if (_dsmaValues.Length > 0)
            {
                _dsmaValues[0] = firstValue;
                _zeros[0] = 0;
                _filt[0] = 0;
            }
        }
    }
}
