using System;
using cAlgo.API;

namespace cAlgo
{
    public class McGinleyDynamicMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _price;
        private double[] _md;
        private bool _initialized;
        
        public McGinleyDynamicMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _price = new double[initialSize];
            _md = new double[initialSize];
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
                // Initialize with simple moving average
                double sum = 0;
                for (int i = index - _indicator.Period + 1; i <= index; i++)
                {
                    sum += _indicator.Source[i];
                }
                
                for (int i = 0; i < _indicator.Period; i++)
                {
                    _price[i] = _indicator.Source[i];
                    _md[i] = sum / _indicator.Period;
                }
                
                _initialized = true;
            }
            
            // Apply McGinley Dynamic formula
            if (_initialized)
            {
                // McGinley Dynamic formula: MD = MD_previous + (Price - MD_previous) / (N * (Price / MD_previous)^4)
                double ratio = _price[index] / _md[index - 1];
                double dynamicFactor = _indicator.Period * Math.Pow(ratio, 4);
                _md[index] = _md[index - 1] + ((_price[index] - _md[index - 1]) / dynamicFactor);
            }
            else
            {
                // Default to price before initialization
                _md[index] = _price[index];
            }
            
            return new MAResult(_md[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _price.Length)
            {
                // Double the array size
                int newSize = _price.Length * 2;
                Array.Resize(ref _price, newSize);
                Array.Resize(ref _md, newSize);
            }
        }
    }
}
