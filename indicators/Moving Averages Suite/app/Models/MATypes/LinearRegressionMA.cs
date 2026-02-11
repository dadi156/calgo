using System;
using cAlgo.API;

namespace cAlgo
{
    public class LinearRegressionMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        
        public LinearRegressionMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Nothing to initialize for LinearRegression as it doesn't maintain state
        }
        
        public MAResult Calculate(int index)
        {
            // Skip calculation for the early bars where not enough data is available
            if (index < _indicator.Period - 1)
            {
                if (index >= 0)
                {
                    return new MAResult(_indicator.Source[index]);
                }
                return new MAResult(0);
            }
            
            // Calculate linear regression endpoint
            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumX2 = 0;
            
            for (int i = 0; i < _indicator.Period; i++)
            {
                double x = i + 1; // x values are the positions (1, 2, 3, ...)
                double y = _indicator.Source[index - _indicator.Period + 1 + i]; // y values are the prices
                
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }
            
            // Calculate slope and intercept using least squares method
            double slope = (_indicator.Period * sumXY - sumX * sumY) / (_indicator.Period * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / _indicator.Period;
            
            // Calculate the endpoint of the linear regression line
            double lrma = intercept + slope * _indicator.Period;
            
            // Return the result (no FAMA for LinearRegression)
            return new MAResult(lrma);
        }
    }
}
