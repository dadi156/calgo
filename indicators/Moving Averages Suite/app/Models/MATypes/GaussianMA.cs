using System;
using cAlgo.API;

namespace cAlgo
{
    public class GaussianMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _weights;
        
        public GaussianMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Calculate Gaussian weights based on period
            CalculateWeights();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Need at least period bars
            if (index < period - 1)
                return new MAResult(double.NaN);
                
            double sum = 0;
            double weightSum = 0;
            
            for (int i = 0; i < period; i++)
            {
                sum += _indicator.Source[index - i] * _weights[i];
                weightSum += _weights[i];
            }
            
            double gma = sum / weightSum;
            
            return new MAResult(gma);
        }
        
        private void CalculateWeights()
        {
            int period = _indicator.Period;
            _weights = new double[period];
            
            // Calculate Gaussian distribution weights
            double sigma = period / 6.0; // Standard deviation (about 99% of values within period)
            double halfPeriod = (period - 1) / 2.0;
            
            for (int i = 0; i < period; i++)
            {
                double x = i - halfPeriod;
                // Gaussian function: exp(-x²/(2*sigma²))
                _weights[i] = Math.Exp(-(x * x) / (2 * sigma * sigma));
            }
        }
    }
}
