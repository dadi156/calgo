using System;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Utility for calculating Fibonacci levels
    /// </summary>
    public static class FibonacciLevelUtility
    {
        /// <summary>
        /// Calculate Fibonacci level values based on VWAP and range
        /// </summary>
        public static void CalculateFibonacciLevels(
            double vwap, 
            double range, 
            out double upperBand, 
            out double lowerBand,
            out double fibLevel886,
            out double fibLevel764,
            out double fibLevel628,
            out double fibLevel382,
            out double fibLevel236,
            out double fibLevel114)
        {
            if (range <= 0)
            {
                upperBand = vwap;
                lowerBand = vwap;
                fibLevel886 = vwap;
                fibLevel764 = vwap;
                fibLevel628 = vwap;
                fibLevel382 = vwap;
                fibLevel236 = vwap;
                fibLevel114 = vwap;
                return;
            }
            
            // 100% and 0% levels
            upperBand = vwap + range;
            lowerBand = vwap - range;
            
            // Intermediate Fibonacci levels
            fibLevel886 = vwap + (0.772 * range);
            fibLevel764 = vwap + (0.528 * range);
            fibLevel628 = vwap + (0.256 * range);
            fibLevel382 = vwap - (0.256 * range);
            fibLevel236 = vwap - (0.528 * range);
            fibLevel114 = vwap - (0.772 * range);
        }
    }
}
