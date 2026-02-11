using System;

namespace cAlgo
{
    /// <summary>
    /// Calculator for Fibonacci levels between MA lines
    /// Handles calculations for different zones based on display mode
    /// </summary>
    public class FibonacciLevelsCalculator
    {
        /// <summary>
        /// Calculate fibonacci levels for the specified zone
        /// </summary>
        /// <param name="lowValue">Low MA line value</param>
        /// <param name="medianValue">Median MA line value</param>
        /// <param name="highValue">High MA line value</param>
        /// <param name="displayMode">Which zone to calculate</param>
        /// <returns>Array of 9 fibonacci level values</returns>
        public double[] CalculateFibonacciLevels(double lowValue, double medianValue, double highValue, FibonacciDisplayMode displayMode)
        {
            try
            {
                // Initialize result array with NaN values
                double[] fibLevels = new double[FibonacciLevels.Count];
                for (int i = 0; i < fibLevels.Length; i++)
                {
                    fibLevels[i] = double.NaN;
                }

                // Check if we should calculate anything
                if (displayMode == FibonacciDisplayMode.None)
                    return fibLevels;

                // Get zone boundaries based on display mode
                double lowerBound, upperBound;
                if (!GetZoneBoundaries(lowValue, medianValue, highValue, displayMode, out lowerBound, out upperBound))
                    return fibLevels;

                // Calculate fibonacci levels for the zone
                double range = upperBound - lowerBound;
                
                for (int i = 0; i < FibonacciLevels.Count; i++)
                {
                    double fibPercentage = FibonacciLevels.Percentages[i];
                    fibLevels[i] = lowerBound + (range * fibPercentage);
                }

                return fibLevels;
            }
            catch (Exception)
            {
                // Return NaN array on any error
                double[] errorResult = new double[FibonacciLevels.Count];
                for (int i = 0; i < errorResult.Length; i++)
                {
                    errorResult[i] = double.NaN;
                }
                return errorResult;
            }
        }

        /// <summary>
        /// Get zone boundaries based on display mode
        /// </summary>
        /// <param name="lowValue">Low MA value</param>
        /// <param name="medianValue">Median MA value</param>
        /// <param name="highValue">High MA value</param>
        /// <param name="displayMode">Display mode</param>
        /// <param name="lowerBound">Output: lower boundary of the zone</param>
        /// <param name="upperBound">Output: upper boundary of the zone</param>
        /// <returns>True if boundaries are valid, false otherwise</returns>
        private bool GetZoneBoundaries(double lowValue, double medianValue, double highValue, 
                                     FibonacciDisplayMode displayMode, out double lowerBound, out double upperBound)
        {
            lowerBound = double.NaN;
            upperBound = double.NaN;

            switch (displayMode)
            {
                case FibonacciDisplayMode.LowHighFibonacciLines:
                    // Zone 1: Between low and high lines
                    lowerBound = lowValue;
                    upperBound = highValue;
                    break;

                case FibonacciDisplayMode.LowMedianFibonacciLines:
                    // Zone 2: Between low and median lines
                    lowerBound = lowValue;
                    upperBound = medianValue;
                    break;

                case FibonacciDisplayMode.MedianHighFibonacciLines:
                    // Zone 3: Between median and high lines
                    lowerBound = medianValue;
                    upperBound = highValue;
                    break;

                case FibonacciDisplayMode.UpperFibonacciLines:
                    // Zone 4: Above high line (fibonacci extensions/projections)
                    // Range = high - low, extend above high line
                    double upperRange = highValue - lowValue;
                    lowerBound = highValue;
                    upperBound = highValue + upperRange;
                    break;

                case FibonacciDisplayMode.LowerFibonacciLines:
                    // Zone 5: Below low line (fibonacci extensions/projections)
                    // Range = high - low, extend below low line
                    double lowerRange = highValue - lowValue;
                    lowerBound = lowValue - lowerRange;
                    upperBound = lowValue;
                    break;

                case FibonacciDisplayMode.TotalRangeFibonacciLines:
                    // Zone 6: NEW - Total range from lower extension to upper extension
                    // This creates a big range that includes both extensions
                    double totalRange = highValue - lowValue;
                    lowerBound = lowValue - totalRange;  // Bottom of lower extension
                    upperBound = highValue + totalRange; // Top of upper extension
                    break;

                default:
                    return false;
            }

            // Validate boundaries
            if (double.IsNaN(lowerBound) || double.IsNaN(upperBound))
                return false;

            // For extension zones and total range, allow equal bounds (edge case)
            // For normal zones, ensure proper order (lower < upper)
            if (displayMode == FibonacciDisplayMode.UpperFibonacciLines || 
                displayMode == FibonacciDisplayMode.LowerFibonacciLines ||
                displayMode == FibonacciDisplayMode.TotalRangeFibonacciLines)
            {
                // Extension zones and total range: allow equal bounds but validate range
                if (lowerBound > upperBound)
                    return false;
            }
            else
            {
                // Normal zones: ensure proper order
                if (lowerBound >= upperBound)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get zone description for debugging
        /// </summary>
        /// <param name="displayMode">Display mode</param>
        /// <returns>Human-readable zone description</returns>
        public string GetZoneDescription(FibonacciDisplayMode displayMode)
        {
            switch (displayMode)
            {
                case FibonacciDisplayMode.LowHighFibonacciLines:
                    return "Low-High Zone";

                case FibonacciDisplayMode.LowMedianFibonacciLines:
                    return "Low-Median Zone";

                case FibonacciDisplayMode.MedianHighFibonacciLines:
                    return "Median-High Zone";

                case FibonacciDisplayMode.UpperFibonacciLines:
                    return "Upper Extension Zone (Above High)";

                case FibonacciDisplayMode.LowerFibonacciLines:
                    return "Lower Extension Zone (Below Low)";

                case FibonacciDisplayMode.TotalRangeFibonacciLines:
                    return "Total Range Zone (Lower Extension to Upper Extension)";

                case FibonacciDisplayMode.None:
                    return "No Zone";

                default:
                    return "Unknown Zone";
            }
        }

        /// <summary>
        /// Validate if fibonacci calculation is possible for given values
        /// </summary>
        /// <param name="lowValue">Low MA value</param>
        /// <param name="medianValue">Median MA value</param>
        /// <param name="highValue">High MA value</param>
        /// <param name="displayMode">Display mode</param>
        /// <returns>True if calculation is possible</returns>
        public bool CanCalculateFibonacci(double lowValue, double medianValue, double highValue, FibonacciDisplayMode displayMode)
        {
            if (displayMode == FibonacciDisplayMode.None)
                return false;

            double lowerBound, upperBound;
            return GetZoneBoundaries(lowValue, medianValue, highValue, displayMode, out lowerBound, out upperBound);
        }
    }
}
