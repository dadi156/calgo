using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    public class WeightedLinearDeviationCalculator : IDeviationCalculator
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

            double weightedHighSum = 0;
            double weightedLowSum = 0;
            double highWeightTotal = 0;
            double lowWeightTotal = 0;

            // Calculate weighted deviations for each bar
            for (int i = 0; i < sortedData.Count; i++)
            {
                // Linear weight: oldest = 1, newest = n
                double weight = i + 1;

                // Calculate the regression line value at this point
                double regressionValue = slope * i + intercept;

                // Calculate deviations
                double highDeviation = sortedData[i].High - regressionValue;
                double lowDeviation = regressionValue - sortedData[i].Low;

                // Only add positive deviations with their weights
                if (highDeviation > 0)
                {
                    weightedHighSum += highDeviation * weight;
                    highWeightTotal += weight;
                }

                if (lowDeviation > 0)
                {
                    weightedLowSum += lowDeviation * weight;
                    lowWeightTotal += weight;
                }
            }

            // Calculate weighted averages
            upperWidth = highWeightTotal > 0 ? weightedHighSum / highWeightTotal : 0;
            lowerWidth = lowWeightTotal > 0 ? weightedLowSum / lowWeightTotal : 0;

            // Safety check
            if (upperWidth == 0 && lowerWidth == 0)
            {
                upperWidth = lowerWidth = 0.0001;
            }
        }
    }
}
