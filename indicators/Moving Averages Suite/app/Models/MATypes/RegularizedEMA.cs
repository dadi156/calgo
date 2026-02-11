using System;
using cAlgo.API;

namespace cAlgo
{
    public class RegularizedEMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _price;
        private double[] _rema;
        private bool _initialized;
        
        public RegularizedEMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _price = new double[initialSize];
            _rema = new double[initialSize];
            _initialized = false;
        }
        
        public MAResult Calculate(int index)
        {
            // If not enough data to calculate, return current price
            if (index < _indicator.Period - 1)
            {
                if (index >= 0)
                {
                    return new MAResult(_indicator.Source[index]);
                }
                return new MAResult(0);
            }
            
            // Ensure arrays have sufficient size
            EnsureArraySize(index);
            
            // Store price
            _price[index] = _indicator.Source[index];
            
            // For first run, initialize the arrays with starting values
            if (!_initialized && index >= _indicator.Period - 1)
            {
                // Initialize with SMA
                double sum = 0;
                for (int i = index - _indicator.Period + 1; i <= index; i++)
                {
                    sum += _indicator.Source[i];
                }
                double initialSMA = sum / _indicator.Period;
                
                // Initialize all previous values
                for (int i = 0; i < index; i++)
                {
                    _price[i] = _indicator.Source[i];
                    _rema[i] = initialSMA;
                }
                
                _rema[index] = initialSMA;
                _initialized = true;
                return new MAResult(_rema[index]);
            }
            
            // Apply Regularized EMA formula
            if (_initialized)
            {
                // Standard EMA alpha
                double alpha = 2.0 / (_indicator.Period + 1);
                
                // Calculate regularized alpha
                // The lambda parameter controls how much regularization is applied
                // Higher lambda = more regularization = smoother RegularizedExponential
                double regAlpha = alpha / (1 + _indicator.Lambda * Math.Abs(_price[index] - _rema[index - 1]));
                
                // Apply RegularizedExponential formula with regularized alpha
                _rema[index] = (_price[index] * regAlpha) + (_rema[index - 1] * (1 - regAlpha));
            }
            else
            {
                // Default to price before initialization
                _rema[index] = _price[index];
            }
            
            return new MAResult(_rema[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _price.Length)
            {
                // Double the array size
                int newSize = _price.Length * 2;
                Array.Resize(ref _price, newSize);
                Array.Resize(ref _rema, newSize);
            }
        }
    }
}
