using System;
using cAlgo.API;

namespace cAlgo
{
    public class VolumeWeightedMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        
        public VolumeWeightedMA(MovingAveragesSuite indicator)
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
                
            double sumProductPriceVolume = 0;
            double sumVolume = 0;
            
            // Calculate sum of price * volume and sum of volume
            for (int i = 0; i < period; i++)
            {
                double price = _indicator.Source[index - i];
                double volume = _indicator.Bars.TickVolumes[index - i];
                
                sumProductPriceVolume += price * volume;
                sumVolume += volume;
            }
            
            // Avoid division by zero
            if (sumVolume == 0)
                return new MAResult(double.NaN);
                
            // Calculate VWMA
            double vwma = sumProductPriceVolume / sumVolume;
            
            return new MAResult(vwma);
        }
    }
}
