using System;
using cAlgo.API;

namespace cAlgo
{
    public class JurikMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _filt;
        private double[] _jma;
        
        public JurikMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _filt = new double[initialSize];
            _jma = new double[initialSize];
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
            
            double phaseRatio = (_indicator.Phase < -100) ? 0.5 : 
                               (_indicator.Phase > 100) ? 2.5 : 
                               (_indicator.Phase / 100.0 + 1.5);
                               
            double beta = 0.45 * (_indicator.Period - 1) / (0.45 * (_indicator.Period - 1) + 2);
            double alpha = Math.Pow(beta, Math.Sqrt(phaseRatio));

            _filt[index] = (1 - alpha) * _indicator.Source[index] + alpha * _filt[index - 1];

            double sum1 = 0, sum2 = 0;
            for (int i = 0; i < _indicator.Period; i++)
            {
                double weight = Math.Pow(beta, i);
                sum1 += weight * _filt[index - i];
                sum2 += weight;
            }

            _jma[index] = sum1 / sum2;
            
            // Return the result (no FAMA for Jurik)
            return new MAResult(_jma[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _filt.Length)
            {
                // Double the array size
                int newSize = _filt.Length * 2;
                Array.Resize(ref _filt, newSize);
                Array.Resize(ref _jma, newSize);
            }
        }
    }
}
