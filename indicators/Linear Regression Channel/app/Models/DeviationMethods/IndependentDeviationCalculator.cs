using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    public class IndependentDeviationCalculator : IDeviationCalculator
    {
        public void Calculate(
            List<OHLC> priceData, 
            double[] x, 
            double[] y, 
            double slope, 
            double intercept, 
            out double upperWidth, 
            out double lowerWidth)
        {
            if (priceData == null || priceData.Count < 2)
            {
                upperWidth = lowerWidth = 0.0001;
                return;
            }

            // Sort data by time (oldest first)
            var sortedData = priceData.OrderBy(p => p.Time).ToList();

            double maxHighDeviation = 0;
            double maxLowDeviation = 0;

            // Calculate the regression line for each point
            for (int i = 0; i < sortedData.Count; i++)
            {
                // Calculate the regression line value at this point
                double regressionValue = slope * i + intercept;

                // Calculate high deviation (High price - regression line)
                double highDeviation = sortedData[i].High - regressionValue;
                
                // Calculate low deviation (regression line - Low price)
                double lowDeviation = regressionValue - sortedData[i].Low;

                // Update maximum high deviation if this one is bigger
                if (highDeviation > maxHighDeviation)
                {
                    maxHighDeviation = highDeviation;
                }

                // Update maximum low deviation if this one is bigger
                if (lowDeviation > maxLowDeviation)
                {
                    maxLowDeviation = lowDeviation;
                }
            }

            // Set separate widths
            upperWidth = maxHighDeviation;
            lowerWidth = maxLowDeviation;
            
            // Safety check
            if (upperWidth <= 0 && lowerWidth <= 0)
            {
                upperWidth = lowerWidth = 0.0001;
            }
        }
    }
}
