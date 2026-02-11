using System;
using cAlgo.API;

namespace cAlgo
{
    public class SimpleMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        
        public SimpleMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // No special initialization needed for SMA
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Need at least period bars
            if (index < period - 1)
                return new MAResult(double.NaN);
                
            double sum = 0;
            
            // Calculate sum of last 'period' values
            for (int i = 0; i < period; i++)
            {
                sum += _indicator.Source[index - i];
            }
            
            // Calculate average
            double sma = sum / period;
            
            return new MAResult(sma);
        }
    }
}
