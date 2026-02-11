using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    public class MaximumDeviationCalculator : IDeviationCalculator
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

            double maxDeviation = 0;

            // Calculate the regression line for each point
            for (int i = 0; i < sortedData.Count; i++)
            {
                // Calculate the regression line value at this point
                double regressionValue = slope * i + intercept;

                // Find the maximum deviation from the regression line
                double highDeviation = Math.Abs(sortedData[i].High - regressionValue);
                double lowDeviation = Math.Abs(sortedData[i].Low - regressionValue);

                // Take the maximum deviation found
                double maxPointDeviation = Math.Max(highDeviation, lowDeviation);

                // Update the overall maximum deviation if needed
                if (maxPointDeviation > maxDeviation)
                {
                    maxDeviation = maxPointDeviation;
                }
            }

            // Set both upper and lower to same value
            upperWidth = lowerWidth = maxDeviation;
        }
    }
}
