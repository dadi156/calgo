using System;
using cAlgo.API;

namespace cAlgo
{
    public class DoubleSmoothedEMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _price;
        private double[] _firstEMA;
        private double[] _dsema;
        private bool _initialized;
        
        public DoubleSmoothedEMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _price = new double[initialSize];
            _firstEMA = new double[initialSize];
            _dsema = new double[initialSize];
            _initialized = false;
        }
        
        public MAResult Calculate(int index)
        {
            // If not enough data to calculate, return current price
            if (index < _indicator.Period * 2 - 1)
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
            if (!_initialized && index >= _indicator.Period * 2 - 1)
            {
                // Initialize with SMA for the first EMA calculation
                double firstSum = 0;
                for (int i = index - _indicator.Period + 1; i <= index; i++)
                {
                    firstSum += _indicator.Source[i];
                }
                double firstSMA = firstSum / _indicator.Period;
                
                // Initialize all previous values
                for (int i = 0; i < index; i++)
                {
                    _price[i] = _indicator.Source[i];
                    _firstEMA[i] = firstSMA;
                    _dsema[i] = firstSMA;
                }
                
                // Calculate first real EMA value
                double alpha = 2.0 / (_indicator.Period + 1);
                _firstEMA[index] = (_price[index] * alpha) + (_firstEMA[index - 1] * (1 - alpha));
                _dsema[index] = _firstEMA[index];
                
                _initialized = true;
                return new MAResult(_dsema[index]);
            }
            
            // Apply Double Smoothed EMA formula
            if (_initialized)
            {
                double alpha = 2.0 / (_indicator.Period + 1);
                
                // Calculate first EMA
                _firstEMA[index] = (_price[index] * alpha) + (_firstEMA[index - 1] * (1 - alpha));
                
                // Calculate second EMA (DoubleSmoothedExponential)
                _dsema[index] = (_firstEMA[index] * alpha) + (_dsema[index - 1] * (1 - alpha));
            }
            else
            {
                // Calculate simple EMA if not yet initialized
                double alpha = 2.0 / (_indicator.Period + 1);
                
                // Use simple EMA calculation for the initial values
                if (index == 0)
                {
                    _firstEMA[index] = _price[index];
                    _dsema[index] = _price[index];
                }
                else
                {
                    _firstEMA[index] = (_price[index] * alpha) + (_firstEMA[index - 1] * (1 - alpha));
                    _dsema[index] = (_firstEMA[index] * alpha) + (_dsema[index - 1] * (1 - alpha));
                }
            }
            
            // Return the result (no FAMA for DoubleSmoothedExponential)
            return new MAResult(_dsema[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _price.Length)
            {
                // Double the array size
                int newSize = _price.Length * 2;
                Array.Resize(ref _price, newSize);
                Array.Resize(ref _firstEMA, newSize);
                Array.Resize(ref _dsema, newSize);
            }
        }
    }
}
