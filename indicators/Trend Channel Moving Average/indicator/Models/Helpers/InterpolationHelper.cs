using System;

namespace cAlgo
{
    /// <summary>
    /// Helper class for smooth calculations between timeframes
    /// </summary>
    public static class InterpolationHelper
    {
        /// <summary>
        /// Calculate how much to blend between two values
        /// </summary>
        public static double CalculateInterpolationFactor(DateTime currentTime, 
            DateTime startTime, DateTime endTime)
        {
            double totalMinutes = (endTime - startTime).TotalMinutes;
            if (totalMinutes <= 0)
                return 0.0;

            double passedMinutes = (currentTime - startTime).TotalMinutes;
            double factor = passedMinutes / totalMinutes;
            
            return Math.Max(0.0, Math.Min(1.0, factor));
        }

        /// <summary>
        /// Blend between two values smoothly
        /// </summary>
        public static double InterpolateValue(double startValue, double endValue, double factor)
        {
            if (double.IsNaN(startValue) || double.IsNaN(endValue))
                return startValue;
            
            return startValue + (endValue - startValue) * factor;
        }
    }
}
