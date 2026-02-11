using System;
using cAlgo.API;

namespace cAlgo
{
    public class LaguerreFilterMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _l0;
        private IndicatorDataSeries _l1;
        private IndicatorDataSeries _l2;
        private IndicatorDataSeries _l3;
        private double _gamma;
        
        public LaguerreFilterMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Initialize filter components
            _l0 = _indicator.CreateDataSeries();
            _l1 = _indicator.CreateDataSeries();
            _l2 = _indicator.CreateDataSeries();
            _l3 = _indicator.CreateDataSeries();
            
            // Use period to determine gamma (0.1 to 0.9)
            // Lower periods need higher gamma for faster response
            _gamma = Math.Max(0.1, Math.Min(0.9, 1.0 - (3.0 / _indicator.Period)));
        }
        
        public MAResult Calculate(int index)
        {
            // Can't calculate until we have at least one bar
            if (index < 1)
            {
                _l0[index] = _indicator.Source[index];
                _l1[index] = _indicator.Source[index];
                _l2[index] = _indicator.Source[index];
                _l3[index] = _indicator.Source[index];
                return new MAResult(_indicator.Source[index]);
            }
            
            // Calculate Laguerre filter components
            _l0[index] = (1 - _gamma) * _indicator.Source[index] + _gamma * _l0[index - 1];
            _l1[index] = -_gamma * _l0[index] + _l0[index - 1] + _gamma * _l1[index - 1];
            _l2[index] = -_gamma * _l1[index] + _l1[index - 1] + _gamma * _l2[index - 1];
            _l3[index] = -_gamma * _l2[index] + _l2[index - 1] + _gamma * _l3[index - 1];
            
            // Calculate final Laguerre Filter value (average of all components)
            double lma = (_l0[index] + _l1[index] + _l2[index] + _l3[index]) / 4.0;
            
            return new MAResult(lma);
        }
    }
}
