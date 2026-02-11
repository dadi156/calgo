using System.Collections.Generic;

namespace cAlgo.Indicators
{
    public interface IDeviationCalculator
    {
        void Calculate(
            List<OHLC> priceData, 
            double[] x, 
            double[] y, 
            double slope, 
            double intercept, 
            out double upperWidth, 
            out double lowerWidth);
    }
}
