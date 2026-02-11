using System;
using cAlgo.API;

namespace cAlgo
{
    public class ExponentialHullMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        
        // Data series to store intermediate values
        private IndicatorDataSeries _slowEMA;
        private IndicatorDataSeries _fastEMA;
        private IndicatorDataSeries _diffSeries;
        private IndicatorDataSeries _result;
        
        public ExponentialHullMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Create data series to avoid recursion issues
            _slowEMA = _indicator.CreateDataSeries();
            _fastEMA = _indicator.CreateDataSeries();
            _diffSeries = _indicator.CreateDataSeries();
            _result = _indicator.CreateDataSeries();
            
            // Display debug info
            /*
            _indicator.Chart.DrawStaticText(
                "EHMA_Info", 
                "ExponentialHullMA initialized", 
                VerticalAlignment.Bottom, 
                HorizontalAlignment.Right, 
                Color.DodgerBlue
            );
            */
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            int halfPeriod = Math.Max(1, period / 2);
            int sqrtPeriod = Math.Max(1, (int)Math.Sqrt(period));
            
            // For initial values, just use the source price
            if (index == 0)
            {
                _slowEMA[0] = _indicator.Source[0];
                _fastEMA[0] = _indicator.Source[0];
                _diffSeries[0] = _indicator.Source[0];
                _result[0] = _indicator.Source[0];
                return new MAResult(_result[0]);
            }
            
            // Calculate slow EMA with period
            double alpha1 = 2.0 / (period + 1.0);
            _slowEMA[index] = index < period ? _indicator.Source[index] : 
                _indicator.Source[index] * alpha1 + _slowEMA[index - 1] * (1.0 - alpha1);
            
            // Calculate fast EMA with half period
            double alpha2 = 2.0 / (halfPeriod + 1.0);
            _fastEMA[index] = index < halfPeriod ? _indicator.Source[index] :
                _indicator.Source[index] * alpha2 + _fastEMA[index - 1] * (1.0 - alpha2);
            
            // Calculate hull formula: 2 * fast EMA - slow EMA
            _diffSeries[index] = 2.0 * _fastEMA[index] - _slowEMA[index];
            
            // Calculate final EMA on diff series with sqrt period
            double alpha3 = 2.0 / (sqrtPeriod + 1.0);
            _result[index] = index < sqrtPeriod ? _diffSeries[index] :
                _diffSeries[index] * alpha3 + _result[index - 1] * (1.0 - alpha3);
            
            // Show calculation info at the end
            if (index == _indicator.Bars.Count - 1)
            {
                string debugInfo = 
                    $"ExponentialHullMA Final Values:\n" +
                    $"Period={period}, HalfPeriod={halfPeriod}, SqrtPeriod={sqrtPeriod}\n" +
                    $"Source={_indicator.Source[index]:F5}\n" +
                    $"SlowEMA={_slowEMA[index]:F5}\n" +
                    $"FastEMA={_fastEMA[index]:F5}\n" +
                    $"DiffSeries={_diffSeries[index]:F5}\n" +
                    $"Result={_result[index]:F5}";
                    
                /*_indicator.Chart.DrawStaticText(
                    "EHMA_Debug", 
                    debugInfo, 
                    VerticalAlignment.Top, 
                    HorizontalAlignment.Right, 
                    Color.DodgerBlue
                );
                */
            }
            
            return new MAResult(_result[index]);
        }
    }
}
