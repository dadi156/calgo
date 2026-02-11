using System;
using cAlgo.API;

namespace cAlgo
{
    public class RunningMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _price;
        private double[] _rma;
        private bool _initialized;
        
        public RunningMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _price = new double[initialSize];
            _rma = new double[initialSize];
            _initialized = false;
        }
        
        public MAResult Calculate(int index)
        {
            // If not enough data to calculate, return current price
            if (index < _indicator.Period)
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
            if (!_initialized && index >= _indicator.Period)
            {
                // Initialize with simple moving average for the first value
                double sum = 0;
                for (int i = index - _indicator.Period + 1; i <= index; i++)
                {
                    sum += _indicator.Source[i];
                }
                
                double initialSMA = sum / _indicator.Period;
                
                for (int i = 0; i < _indicator.Period; i++)
                {
                    _price[i] = _indicator.Source[i];
                    _rma[i] = initialSMA;
                }
                
                _initialized = true;
                _rma[index] = initialSMA;
                return new MAResult(initialSMA);
            }
            
            // Apply Running Moving Average formula
            if (_initialized)
            {
                // Running = ((period - 1) * previousRMA + currentPrice) / period
                // Or equivalently: Running = previousRMA + (currentPrice - previousRMA) / period
                _rma[index] = (_rma[index - 1] * (_indicator.Period - 1) + _price[index]) / _indicator.Period;
            }
            else
            {
                // Default to price before initialization
                _rma[index] = _price[index];
            }
            
            return new MAResult(_rma[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _price.Length)
            {
                // Double the array size
                int newSize = _price.Length * 2;
                Array.Resize(ref _price, newSize);
                Array.Resize(ref _rma, newSize);
            }
        }
    }
}
