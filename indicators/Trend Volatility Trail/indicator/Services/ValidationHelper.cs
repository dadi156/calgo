// ValidationHelper - Simple validation for calculations
using System;

namespace cAlgo.Indicators
{
    // Helper class for validation - keeps code clean
    public static class ValidationHelper
    {
        // Check if a value is valid (not NaN or Infinity)
        public static bool IsValidValue(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
