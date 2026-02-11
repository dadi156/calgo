using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    public class ATRDeviationCalculator : IDeviationCalculator
    {
        private double _multiplier = 1.5;
        
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

            // Calculate True Range for each bar
            List<double> trueRanges = new List<double>();
            
            for (int i = 0; i < sortedData.Count; i++)
            {
                double trueRange;
                
                if (i == 0)
                {
                    // First bar: just use High - Low
                    trueRange = sortedData[i].High - sortedData[i].Low;
                }
                else
                {
                    // True Range = max of:
                    // 1. High - Low
                    // 2. |High - Previous Close|
                    // 3. |Low - Previous Close|
                    double highLow = sortedData[i].High - sortedData[i].Low;
                    double highPrevClose = Math.Abs(sortedData[i].High - sortedData[i - 1].Close);
                    double lowPrevClose = Math.Abs(sortedData[i].Low - sortedData[i - 1].Close);
                    
                    trueRange = Math.Max(highLow, Math.Max(highPrevClose, lowPrevClose));
                }
                
                trueRanges.Add(trueRange);
            }

            // Calculate ATR using all available bars (Simple Moving Average)
            // Automatically uses the same bars as the regression data
            double atr = trueRanges.Average();

            // Apply multiplier
            double channelWidth = atr * _multiplier;

            // ATR is symmetric - same width for upper and lower
            upperWidth = channelWidth;
            lowerWidth = channelWidth;

            // Safety check
            if (upperWidth == 0 && lowerWidth == 0)
            {
                upperWidth = lowerWidth = 0.0001;
            }
        }
    }
}
