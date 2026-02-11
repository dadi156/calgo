using System;
using cAlgo.API;

namespace cAlgo
{
    public class FractalAdaptiveMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _frama;
        private const double _fastSC = 0.5; // Fast smoothing constant (SC)
        private const double _slowSC = 0.05; // Slow smoothing constant (SC)
        
        public FractalAdaptiveMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            _frama = _indicator.CreateDataSeries();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Need at least 2*period bars for this calculation
            if (index < period * 2)
                return new MAResult(double.NaN);
            
            // Initialize FRAMA with simple average if first calculation
            if (index == period * 2)
            {
                double sum = 0;
                for (int i = 0; i < period; i++)
                {
                    sum += _indicator.Source[index - i];
                }
                _frama[index] = sum / period;
                return new MAResult(_frama[index]);
            }
            
            // Calculate N1 (first half of the period)
            double highest1 = double.MinValue;
            double lowest1 = double.MaxValue;
            
            for (int i = 0; i < period; i++)
            {
                highest1 = Math.Max(highest1, _indicator.Source[index - i]);
                lowest1 = Math.Min(lowest1, _indicator.Source[index - i]);
            }
            
            // Calculate N2 (second half of the period)
            double highest2 = double.MinValue;
            double lowest2 = double.MaxValue;
            
            for (int i = period; i < period * 2; i++)
            {
                highest2 = Math.Max(highest2, _indicator.Source[index - i]);
                lowest2 = Math.Min(lowest2, _indicator.Source[index - i]);
            }
            
            // Calculate N3 (full period)
            double highest3 = Math.Max(highest1, highest2);
            double lowest3 = Math.Min(lowest1, lowest2);
            
            // Calculate fractal dimension
            double n1 = (highest1 - lowest1) / period;
            double n2 = (highest2 - lowest2) / period;
            double n3 = (highest3 - lowest3) / (period * 2);
            
            double dimen = 0;
            if (n1 > 0 && n2 > 0 && n3 > 0)
            {
                dimen = (Math.Log(n1 + n2) - Math.Log(n3)) / Math.Log(2);
            }
            
            // Calculate alpha
            double alpha = Math.Exp(-4.6 * (dimen - 1));
            
            // Ensure alpha is between _slowSC and _fastSC
            alpha = Math.Max(_slowSC, Math.Min(_fastSC, alpha));
            
            // Calculate FRAMA
            _frama[index] = alpha * _indicator.Source[index] + (1 - alpha) * _frama[index - 1];
            
            return new MAResult(_frama[index]);
        }
    }
}
