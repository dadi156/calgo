using System;
using cAlgo.API;

namespace cAlgo
{
    public class HullMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private IndicatorDataSeries _wma1;
        private IndicatorDataSeries _wma2;
        private IndicatorDataSeries _rawHull;
        
        public HullMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Initialize temporary data series for calculation
            _wma1 = _indicator.CreateDataSeries();
            _wma2 = _indicator.CreateDataSeries();
            _rawHull = _indicator.CreateDataSeries();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Need at least period bars
            if (index < period)
                return new MAResult(double.NaN);
                
            // Calculate first WMA with period
            _wma1[index] = CalculateWMA(index, period, _indicator.Source);
            
            // Calculate second WMA with period/2
            int halfPeriod = period / 2;
            _wma2[index] = CalculateWMA(index, halfPeriod, _indicator.Source);
            
            // Calculate Raw Hull
            _rawHull[index] = 2 * _wma2[index] - _wma1[index];
            
            // Calculate final Hull MA using WMA of Raw Hull with sqrt(period)
            int sqrtPeriod = (int)Math.Sqrt(period);
            return new MAResult(CalculateWMA(index, sqrtPeriod, _rawHull));
        }
        
        private double CalculateWMA(int index, int period, DataSeries series)
        {
            if (index < period - 1)
                return double.NaN;
                
            double sum = 0;
            double weightSum = 0;
            
            for (int i = 0; i < period; i++)
            {
                int weight = period - i;
                double price = series[index - i];
                
                sum += price * weight;
                weightSum += weight;
            }
            
            return sum / weightSum;
        }
    }
}
