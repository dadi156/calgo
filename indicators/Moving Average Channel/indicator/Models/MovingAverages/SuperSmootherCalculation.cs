using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class SuperSmootherCalculation : IMovingAverage
    {
        // SuperSmoother coefficients
        private double _a1;
        private double _b1;
        private double _c1;
        private double _c2;
        private double _c3;
        
        // Settings
        private readonly int _bandEdge;
        
        // Data storage
        private double[] _filt;

        public SuperSmootherCalculation(int bandEdge, int arraySize)
        {
            _bandEdge = bandEdge;
            
            // Create array
            _filt = new double[arraySize];
            
            // Initialize with zero
            for (int i = 0; i < arraySize; i++)
            {
                _filt[i] = 0;
            }
            
            // Calculate coefficients
            CalculateCoefficients();
        }

        private void CalculateCoefficients()
        {
            // Exact translation of Ehlers (2013) SuperSmoother coefficients
            const double SQRT2 = 1.4142135623730951; // high-precision sqrt(2)
            double pi = Math.PI;

            _a1 = Math.Exp(-SQRT2 * pi / _bandEdge);
            _b1 = 2.0 * _a1 * Math.Cos(SQRT2 * pi / _bandEdge);
            _c2 = _b1;
            _c3 = -_a1 * _a1;
            _c1 = 1.0 - _c2 - _c3;
        }

        public double Calculate(int index, DataSeries priceSource)
        {
            try
            {
                // Resize array if needed
                if (index >= _filt.Length)
                {
                    int newSize = Math.Max(index + 1000, _filt.Length * 2);
                    Array.Resize(ref _filt, newSize);
                }

                // Handle first bar
                if (index == 0)
                {
                    _filt[index] = priceSource[index];
                    return _filt[index];
                }

                // Handle second bar
                if (index == 1)
                {
                    _filt[index] = _c1 * (priceSource[index] + priceSource[index - 1]) * 0.5
                                  + _c2 * _filt[index - 1];
                    return _filt[index];
                }

                // General case (index >= 2): exact Ehlers SuperSmoother recursion
                _filt[index] = _c1 * (priceSource[index] + priceSource[index - 1]) * 0.5
                             + _c2 * _filt[index - 1]
                             + _c3 * _filt[index - 2];

                // Fix NaN values
                if (double.IsNaN(_filt[index]) || double.IsInfinity(_filt[index]))
                {
                    _filt[index] = index > 0 ? _filt[index - 1] : priceSource[index];
                }

                return _filt[index];
            }
            catch (Exception)
            {
                // If error, return previous value or current price
                return index > 0 ? _filt[index - 1] : priceSource[index];
            }
        }

        // Initialize MA with first value
        public void Initialize(double firstValue)
        {
            if (_filt.Length > 0)
            {
                _filt[0] = firstValue;
            }
        }
    }
}
