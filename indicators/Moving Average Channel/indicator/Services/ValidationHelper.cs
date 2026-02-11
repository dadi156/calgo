using System;

namespace cAlgo.Indicators
{
    // Helper class for validation - used by all managers
    public static class ValidationHelper
    {
        // Check if a single value is valid
        public static bool IsValidValue(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        // Check if MAResult has all valid values
        public static bool IsValidResult(MAResult result)
        {
            if (result == null)
                return false;

            return IsValidValue(result.HighMA) && 
                   IsValidValue(result.LowMA) &&
                   IsValidValue(result.CloseMA) && 
                   IsValidValue(result.OpenMA) &&
                   IsValidValue(result.MedianMA) &&
                   IsValidValue(result.Fib618MA) && 
                   IsValidValue(result.Fib382MA);
        }
    }
}
