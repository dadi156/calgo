using System;
using cAlgo.API;

namespace cAlgo
{
    public class Tillson3MovingAverage : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _e1;
        private IndicatorDataSeries _e2;
        private IndicatorDataSeries _e3;
        private IndicatorDataSeries _e4;
        private IndicatorDataSeries _e5;
        private IndicatorDataSeries _e6;
        private double _vFactor = 0.7; // Default volume factor
        
        public Tillson3MovingAverage(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            _e1 = _indicator.CreateDataSeries();
            _e2 = _indicator.CreateDataSeries();
            _e3 = _indicator.CreateDataSeries();
            _e4 = _indicator.CreateDataSeries();
            _e5 = _indicator.CreateDataSeries();
            _e6 = _indicator.CreateDataSeries();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Handle first value
            if (index == 0)
            {
                _e1[0] = _indicator.Source[0];
                _e2[0] = _indicator.Source[0];
                _e3[0] = _indicator.Source[0];
                _e4[0] = _indicator.Source[0];
                _e5[0] = _indicator.Source[0];
                _e6[0] = _indicator.Source[0];
                return new MAResult(_indicator.Source[0]);
            }
            
            // Calculate alpha (smoothing factor)
            double alpha = 2.0 / (period + 1.0);
            
            // Calculate coefficients based on volume factor
            double c1 = -(_vFactor * _vFactor * _vFactor);
            double c2 = 3 * (_vFactor * _vFactor) + 3 * (_vFactor * _vFactor * _vFactor);
            double c3 = -6 * (_vFactor * _vFactor) - 3 * _vFactor - 3 * (_vFactor * _vFactor * _vFactor);
            double c4 = 1 + 3 * _vFactor + (_vFactor * _vFactor * _vFactor) + 3 * (_vFactor * _vFactor);
            
            // Calculate first EMA (EMA of price)
            _e1[index] = _indicator.Source[index] * alpha + _e1[index - 1] * (1 - alpha);
            
            // Calculate second EMA (EMA of E1)
            _e2[index] = _e1[index] * alpha + _e2[index - 1] * (1 - alpha);
            
            // Calculate third EMA (EMA of E2)
            _e3[index] = _e2[index] * alpha + _e3[index - 1] * (1 - alpha);
            
            // Calculate fourth EMA (EMA of E3)
            _e4[index] = _e3[index] * alpha + _e4[index - 1] * (1 - alpha);
            
            // Calculate fifth EMA (EMA of E4)
            _e5[index] = _e4[index] * alpha + _e5[index - 1] * (1 - alpha);
            
            // Calculate sixth EMA (EMA of E5)
            _e6[index] = _e5[index] * alpha + _e6[index - 1] * (1 - alpha);
            
            // Calculate T3 using the coefficients
            double t3 = c1 * _e6[index] + c2 * _e5[index] + c3 * _e4[index] + c4 * _e3[index];
            
            return new MAResult(t3);
        }
    }
}
