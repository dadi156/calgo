using System;

namespace cAlgo.Indicators
{
    // Helper class for calculations - used by all managers
    public static class CalculationHelper
    {
        // NEW: Calculate median (50% level between High and Low)
        public static double CalculateMedian(double highMA, double lowMA)
        {
            // Check if values are valid
            if (!ValidationHelper.IsValidValue(highMA) || !ValidationHelper.IsValidValue(lowMA))
            {
                return double.NaN;
            }

            // Calculate median as midpoint
            return (highMA + lowMA) / 2.0;
        }

        // Calculate only the 2 Fibonacci levels we use (38.2% and 61.8%)
        public static (double Fib618, double Fib382) CalculateFibonacciLevels(double highMA, double lowMA)
        {
            // Check if values are valid
            if (!ValidationHelper.IsValidValue(highMA) || !ValidationHelper.IsValidValue(lowMA))
            {
                return (double.NaN, double.NaN);
            }

            // Calculate range between high and low
            double range = highMA - lowMA;

            // Calculate only 2 Fibonacci levels we actually use
            double fib618 = lowMA + (range * 0.618); // 61.8% - Upper Reversion Zone
            double fib382 = lowMA + (range * 0.382); // 38.2% - Lower Reversion Zone

            return (fib618, fib382);
        }
    }
}
