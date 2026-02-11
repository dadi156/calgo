using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    public class AverageDeviationCalculator : IDeviationCalculator
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

            List<double> highDeviations = new List<double>();
            List<double> lowDeviations = new List<double>();

            // Calculate deviations for each bar
            for (int i = 0; i < sortedData.Count; i++)
            {
                // Calculate the regression line value at this point
                double regressionValue = slope * i + intercept;

                // Calculate deviations (only positive values)
                double highDeviation = sortedData[i].High - regressionValue;
                double lowDeviation = regressionValue - sortedData[i].Low;

                // Only add positive deviations
                if (highDeviation > 0)
                {
                    highDeviations.Add(highDeviation);
                }

                if (lowDeviation > 0)
                {
                    lowDeviations.Add(lowDeviation);
                }
            }

            // Calculate averages
            upperWidth = highDeviations.Count > 0 ? highDeviations.Average() : 0;
            lowerWidth = lowDeviations.Count > 0 ? lowDeviations.Average() : 0;

            // Safety check
            if (upperWidth == 0 && lowerWidth == 0)
            {
                upperWidth = lowerWidth = 0.0001;
            }
        }
    }
}
