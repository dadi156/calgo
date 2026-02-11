using System;
using cAlgo.API;

namespace cAlgo
{
    public class KaufmanAdaptiveMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _kama;
        private const double _fastSC = 0.666; // Fast smoothing constant (2/(2+1))
        private const double _slowSC = 0.0645; // Slow smoothing constant (2/(30+1))
        
        public KaufmanAdaptiveMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            _kama = _indicator.CreateDataSeries();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Need at least period+1 bars
            if (index < period)
                return new MAResult(double.NaN);
                
            // Initialize KAMA with source value for first calculation
            if (index == period)
            {
                _kama[index] = _indicator.Source[index];
                return new MAResult(_kama[index]);
            }
            
            // Calculate price change (direction)
            double change = Math.Abs(_indicator.Source[index] - _indicator.Source[index - period]);
            
            // Calculate volatility (noise)
            double volatility = 0;
            for (int i = 0; i < period; i++)
            {
                volatility += Math.Abs(_indicator.Source[index - i] - _indicator.Source[index - i - 1]);
            }
            
            // Calculate efficiency ratio (ER)
            double er = (volatility == 0) ? 0 : change / volatility;
            
            // Calculate smoothing constant (SC)
            double sc = er * (_fastSC - _slowSC) + _slowSC;
            
            // Square the smoothing constant for more responsiveness
            sc = sc * sc;
            
            // Calculate KAMA
            _kama[index] = _kama[index - 1] + sc * (_indicator.Source[index] - _kama[index - 1]);
            
            return new MAResult(_kama[index]);
        }
    }
}
