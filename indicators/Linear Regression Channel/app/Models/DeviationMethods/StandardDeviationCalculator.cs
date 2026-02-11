using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    public class StandardDeviationCalculator : IDeviationCalculator
    {
        private double _multiplier = 2.0;
        
        public void SetMultiplier(double multiplier)
        {
            _multiplier = multiplier;
        }
        
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

            // Calculate deviations for each bar - INCLUDE ALL POINTS
            for (int i = 0; i < sortedData.Count; i++)
            {
                // Calculate the regression line value at this point
                double regressionValue = slope * i + intercept;

                // Calculate deviations - ADD ALL, not just positive
                double highDeviation = sortedData[i].High - regressionValue;
                double lowDeviation = regressionValue - sortedData[i].Low;

                highDeviations.Add(highDeviation);
                lowDeviations.Add(lowDeviation);
            }

            // Calculate standard deviation for upper channel
            if (highDeviations.Count > 0)
            {
                double meanHigh = highDeviations.Average();
                double sumSquaredDiffs = 0;
                
                foreach (double dev in highDeviations)
                {
                    double diff = dev - meanHigh;
                    sumSquaredDiffs += diff * diff;
                }
                
                double variance = sumSquaredDiffs / highDeviations.Count;
                upperWidth = Math.Sqrt(variance) * _multiplier;
            }
            else
            {
                upperWidth = 0;
            }

            // Calculate standard deviation for lower channel
            if (lowDeviations.Count > 0)
            {
                double meanLow = lowDeviations.Average();
                double sumSquaredDiffs = 0;
                
                foreach (double dev in lowDeviations)
                {
                    double diff = dev - meanLow;
                    sumSquaredDiffs += diff * diff;
                }
                
                double variance = sumSquaredDiffs / lowDeviations.Count;
                lowerWidth = Math.Sqrt(variance) * _multiplier;
            }
            else
            {
                lowerWidth = 0;
            }

            // Safety check
            if (upperWidth == 0 && lowerWidth == 0)
            {
                upperWidth = lowerWidth = 0.0001;
            }
        }
    }
}
