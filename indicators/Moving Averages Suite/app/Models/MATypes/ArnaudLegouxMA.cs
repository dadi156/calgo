using System;
using cAlgo.API;

namespace cAlgo
{
    public class ArnaudLegouxMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _price;
        private double[] _alma;
        private double[] _weights;
        private int _lastPeriod;
        private double _lastOffset;
        private double _lastSigma;
        private bool _weightsInitialized;
        
        public ArnaudLegouxMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _price = new double[initialSize];
            _alma = new double[initialSize];
            _weights = null;
            _lastPeriod = 0;
            _lastOffset = 0;
            _lastSigma = 0;
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
            
            // Initialize weights if not done already or if parameters changed
            if (!_weightsInitialized || _lastPeriod != _indicator.Period || 
                Math.Abs(_lastOffset - _indicator.Offset) > 0.0001 || 
                Math.Abs(_lastSigma - _indicator.Sigma) > 0.0001)
            {
                InitializeWeights(_indicator.Period, _indicator.Offset, _indicator.Sigma);
                _lastPeriod = _indicator.Period;
                _lastOffset = _indicator.Offset;
                _lastSigma = _indicator.Sigma;
            }
            
            // Calculate ArnaudLegoux
            double sum = 0;
            double weightSum = 0;
            
            for (int i = 0; i < _indicator.Period; i++)
            {
                double price = _indicator.Source[index - i];
                double weight = _weights[i];
                
                sum += price * weight;
                weightSum += weight;
            }
            
            // Avoid division by zero
            if (weightSum != 0)
                _alma[index] = sum / weightSum;
            else
                _alma[index] = _indicator.Source[index];
            
            // Return the result (no FAMA for ArnaudLegoux)
            return new MAResult(_alma[index]);
        }
        
        private void InitializeWeights(int period, double offset, double sigma)
        {
            // Create a new weights array for this period
            _weights = new double[period];
            
            // Calculate the m (offset position) and s (sigma) parameters
            double m = Math.Floor(offset * (period - 1));
            double s = period / sigma;
            
            // Calculate the Gaussian weights
            double wSum = 0;
            
            for (int i = 0; i < period; i++)
            {
                // Calculate the weight using Gaussian formula
                double w = Math.Exp(-Math.Pow(i - m, 2) / (2 * Math.Pow(s, 2)));
                _weights[i] = w;
                wSum += w;
            }
            
            // Normalize the weights to sum to 1
            if (wSum != 0)
            {
                for (int i = 0; i < period; i++)
                {
                    _weights[i] /= wSum;
                }
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
                Array.Resize(ref _alma, newSize);
            }
        }
    }
}
