namespace cAlgo
{
    /// <summary>
    /// Helper class for FVG calculations
    /// Shared by Controller and View for consistent calculations
    /// </summary>
    public static class FVGCalculator
    {
        /// <summary>
        /// Calculate how much % of FVG has been filled
        /// Uses MaxPenetrationPrice (deepest price that entered the gap)
        /// Returns: 0-100 (percentage)
        /// </summary>
        public static double CalculateFillPercent(FVGModel fvg)
        {
            // No penetration data = 0% filled
            if (!fvg.MaxPenetrationPrice.HasValue)
                return 0;
            
            double gapSize = fvg.Top - fvg.Bottom;
            
            // Prevent division by zero
            if (gapSize == 0)
                return 0;
            
            if (fvg.Type == FVGType.Bullish)
            {
                // Bullish FVG: How far down from top
                double penetration = fvg.Top - fvg.MaxPenetrationPrice.Value;
                return (penetration / gapSize) * 100.0;
            }
            else // Bearish
            {
                // Bearish FVG: How far up from bottom
                double penetration = fvg.MaxPenetrationPrice.Value - fvg.Bottom;
                return (penetration / gapSize) * 100.0;
            }
        }
    }
}
