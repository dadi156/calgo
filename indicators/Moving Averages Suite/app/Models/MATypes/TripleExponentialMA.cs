using System;
using cAlgo.API;

namespace cAlgo
{
    public class TripleExponentialMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _ema1;
        private IndicatorDataSeries _ema2;
        private IndicatorDataSeries _ema3;
        
        public TripleExponentialMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            _ema1 = _indicator.CreateDataSeries();
            _ema2 = _indicator.CreateDataSeries();
            _ema3 = _indicator.CreateDataSeries();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Handle first value
            if (index == 0)
            {
                _ema1[0] = _indicator.Source[0];
                _ema2[0] = _ema1[0];
                _ema3[0] = _ema2[0];
                return new MAResult(_ema1[0]);
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
                double avg = sum / (index + 1);
                _ema1[index] = avg;
                _ema2[index] = avg;
                _ema3[index] = avg;
            }
            else
            {
                // Calculate first EMA (EMA of price)
                _ema1[index] = _indicator.Source[index] * alpha + _ema1[index - 1] * (1 - alpha);
                
                // Calculate second EMA (EMA of EMA)
                _ema2[index] = _ema1[index] * alpha + _ema2[index - 1] * (1 - alpha);
                
                // Calculate third EMA (EMA of EMA of EMA)
                _ema3[index] = _ema2[index] * alpha + _ema3[index - 1] * (1 - alpha);
            }
            
            // TEMA = 3 * EMA1 - 3 * EMA2 + EMA3
            double tema = 3 * _ema1[index] - 3 * _ema2[index] + _ema3[index];
            
            return new MAResult(tema);
        }
    }
}
