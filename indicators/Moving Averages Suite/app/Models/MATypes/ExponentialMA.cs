using System;
using cAlgo.API;

namespace cAlgo
{
    public class ExponentialMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _ema;
        
        public ExponentialMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            _ema = _indicator.CreateDataSeries();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // First value is source
            if (index == 0)
            {
                _ema[0] = _indicator.Source[0];
                return new MAResult(_ema[0]);
            }
            
            // Calculate alpha (smoothing factor)
            double alpha = 2.0 / (period + 1.0);
            
            // For initial periods, use SMA to seed EMA
            if (index < period)
            {
                double sum = 0;
                for (int i = 0; i <= index; i++)
                {
                    sum += _indicator.Source[i];
                }
                _ema[index] = sum / (index + 1);
            }
            else
            {
                // EMA = Price(t) * alpha + EMA(y) * (1 – alpha)
                _ema[index] = _indicator.Source[index] * alpha + _ema[index - 1] * (1 - alpha);
            }
            
            return new MAResult(_ema[index]);
        }
    }
}
