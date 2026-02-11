using System;
using cAlgo.API;

namespace cAlgo
{
    public class DoubleExponentialMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _ema;
        private IndicatorDataSeries _emaOfEma;
        
        public DoubleExponentialMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            _ema = _indicator.CreateDataSeries();
            _emaOfEma = _indicator.CreateDataSeries();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Handle first value
            if (index == 0)
            {
                _ema[0] = _indicator.Source[0];
                _emaOfEma[0] = _ema[0];
                return new MAResult(2 * _ema[0] - _emaOfEma[0]);
            }
            
            // Calculate alpha (smoothing factor)
            double alpha = 2.0 / (period + 1.0);
            
            // For initial periods, use simpler calculation
            if (index < period)
            {
                double sum = 0;
                for (int i = 0; i <= index; i++)
                {
                    sum += _indicator.Source[i];
                }
                _ema[index] = sum / (index + 1);
                _emaOfEma[index] = _ema[index];
            }
            else
            {
                // Calculate EMA
                _ema[index] = _indicator.Source[index] * alpha + _ema[index - 1] * (1 - alpha);
                
                // Calculate EMA of EMA
                _emaOfEma[index] = _ema[index] * alpha + _emaOfEma[index - 1] * (1 - alpha);
            }
            
            // DEMA = 2 * EMA - EMA of EMA
            double dema = 2 * _ema[index] - _emaOfEma[index];
            
            return new MAResult(dema);
        }
    }
}
