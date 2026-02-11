using System;
using cAlgo.API;

namespace cAlgo
{
    public class SineWeightedMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _price;
        private double[] _swma;
        private double[] _weights;
        private double _weightSum;
        private int _lastPeriod;
        private bool _weightsInitialized;
        
        public SineWeightedMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _price = new double[initialSize];
            _swma = new double[initialSize];
            _weights = null;
            _weightSum = 0;
            _lastPeriod = 0;
            _weightsInitialized = false;
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
            
            // Initialize sine weights if not done already or if period changed
            if (!_weightsInitialized || _lastPeriod != _indicator.Period)
            {
                InitializeWeights(_indicator.Period);
                _lastPeriod = _indicator.Period;
            }
            
            // Calculate Sine Weighted Moving Average
            double sum = 0;
            
            for (int i = 0; i < _indicator.Period; i++)
            {
                sum += _indicator.Source[index - i] * _weights[i];
            }
            
            _swma[index] = sum / _weightSum;
            
            return new MAResult(_swma[index]);
        }
        
        private void InitializeWeights(int period)
        {
            // Create a new weights array for this period
            _weights = new double[period];
            _weightSum = 0;
            
            // Calculate sine-based weights
            // Using sin(x) from 0 to π where π is stretched over the period
            for (int i = 0; i < period; i++)
            {
                // Calculate position between 0 and π
                double angle = Math.PI * (i + 1) / (period + 1);
                _weights[i] = Math.Sin(angle);
                _weightSum += _weights[i];
            }
            
            _weightsInitialized = true;
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _price.Length)
            {
                // Double the array size
                int newSize = _price.Length * 2;
                Array.Resize(ref _price, newSize);
                Array.Resize(ref _swma, newSize);
            }
        }
    }
}
