using System;
using cAlgo.API;

namespace cAlgo
{
    public class VariableIndexDynamicMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double _previousVidya;
        private bool _isInitialized;
        
        // Default sigma value - you might want to make this configurable via a parameter
        private const double DEFAULT_SIGMA = 0.3629;
        
        public VariableIndexDynamicMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
            _isInitialized = false;
        }
        
        public void Initialize()
        {
            _isInitialized = false;
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Need at least period bars
            if (index < period)
                return new MAResult(double.NaN);
            
            // If not initialized, calculate the first VIDYA value as a simple average
            if (!_isInitialized)
            {
                double sum = 0;
                for (int i = 0; i < period; i++)
                {
                    sum += _indicator.Source[index - i];
                }
                _previousVidya = sum / period;
                _isInitialized = true;
                return new MAResult(_previousVidya);
            }
            
            // Calculate CMO (Chande Momentum Oscillator)
            double sumUp = 0;
            double sumDown = 0;
            
            for (int i = 1; i <= period; i++)
            {
                double change = _indicator.Source[index - i + 1] - _indicator.Source[index - i];
                
                if (change > 0)
                    sumUp += change;
                else
                    sumDown += Math.Abs(change);
            }
            
            double cmo = 0;
            if (sumUp + sumDown != 0)
                cmo = Math.Abs((sumUp - sumDown) / (sumUp + sumDown));
            
            // Calculate the smoothing factor k using sigma and CMO
            double sigma = DEFAULT_SIGMA; // Sensitivity parameter between 0.1 and 0.95
            double k = sigma * cmo;
            
            // Calculate VIDYA using the formula: VIDYA_t = k × Price_t + (1 - k) × VIDYA_(t-1)
            double currentPrice = _indicator.Source[index];
            double vidya = k * currentPrice + (1 - k) * _previousVidya;
            
            // Store for next calculation
            _previousVidya = vidya;
            
            return new MAResult(vidya);
        }
    }
}
