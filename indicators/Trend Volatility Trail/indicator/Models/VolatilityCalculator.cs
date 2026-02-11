// VolatilityCalculator - Calculates volatility stretch
using System;

namespace cAlgo.Indicators
{
    // Calculates volatility stretch based on ATR
    public class VolatilityCalculator
    {
        // Calculate volatility stretch
        // volStretch = (atr / atrAvg) ^ volPower
        public double CalculateVolatilityStretch(double atr, double atrAvg, double volPower)
        {
            try
            {
                // Avoid division by zero
                if (atrAvg == 0.0 || !ValidationHelper.IsValidValue(atrAvg))
                {
                    return 1.0;
                }

                // Calculate raw stretch
                double volStretchRaw = atr / atrAvg;

                // Apply power (sensitivity)
                double volStretch = Math.Pow(volStretchRaw, volPower);

                // Validate result
                if (!ValidationHelper.IsValidValue(volStretch))
                {
                    return 1.0;
                }

                return volStretch;
            }
            catch (Exception)
            {
                // If error, return neutral value
                return 1.0;
            }
        }
    }
}
