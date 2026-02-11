using System;
using cAlgo.API;

namespace cAlgo
{
    public class WeightedMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        
        public WeightedMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // No special initialization needed
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
                int weight = period - i;
                sum += _indicator.Source[index - i] * weight;
                weightSum += weight;
            }
            
            double wma = sum / weightSum;
            
            return new MAResult(wma);
        }
    }
}
