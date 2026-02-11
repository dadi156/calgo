using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Checks and updates FVG mitigation status on display timeframe
    /// Single Responsibility: Mitigation checking and status updates only
    /// </summary>
    public class FVGMitigationChecker
    {
        private readonly Bars _displayBars;
        private readonly int _partialFillThreshold;

        public FVGMitigationChecker(Bars displayBars, int partialFillThreshold)
        {
            _displayBars = displayBars;
            _partialFillThreshold = partialFillThreshold;
        }

        /// <summary>
        /// Check and update mitigation status for a FVG
        /// Only checks AFTER CompletionTime (when FVG is fully formed)
        /// Tracks maximum penetration for partial fills
        /// </summary>
        public void UpdateMitigation(FVGModel fvg, int displayIndex)
        {
            if (displayIndex < 0 || displayIndex >= _displayBars.Count)
                return;

            DateTime currentTime = _displayBars.OpenTimes[displayIndex];

            // Skip FVGs that haven't completed yet
            if (fvg.CompletionTime > currentTime)
                return;

            // Skip already filled FVGs
            if (fvg.Status == FVGStatus.Filled)
                return;

            if (fvg.Type == FVGType.Bullish)
            {
                UpdateBullishFVG(fvg, displayIndex, currentTime);
            }
            else // Bearish
            {
                UpdateBearishFVG(fvg, displayIndex, currentTime);
            }
        }

        /// <summary>
        /// Update bullish FVG mitigation status
        /// Bullish FVG is filled when price comes back down into gap
        /// </summary>
        private void UpdateBullishFVG(FVGModel fvg, int displayIndex, DateTime currentTime)
        {
            double currentLow = _displayBars.LowPrices[displayIndex];

            // Check if current bar came back down into the gap
            if (currentLow <= fvg.Bottom)
            {
                // Fully filled - price reached bottom
                fvg.Status = FVGStatus.Filled;
                fvg.MitigationTime = currentTime;
                fvg.MaxPenetrationPrice = fvg.Bottom;
            }
            else if (currentLow < fvg.Top)
            {
                // Partially filled - price is inside the gap
                fvg.Status = FVGStatus.PartiallyFilled;

                // Track maximum penetration (deepest low)
                if (!fvg.MaxPenetrationPrice.HasValue || currentLow < fvg.MaxPenetrationPrice.Value)
                {
                    fvg.MaxPenetrationPrice = currentLow;
                }

                // Check if partial fill is deep enough to count as filled
                double fillPercent = FVGCalculator.CalculateFillPercent(fvg);
                if (fillPercent >= _partialFillThreshold)
                {
                    // Treat as fully filled
                    fvg.Status = FVGStatus.Filled;
                    fvg.MitigationTime = currentTime;
                    fvg.MaxPenetrationPrice = fvg.Bottom;
                }
            }
        }

        /// <summary>
        /// Update bearish FVG mitigation status
        /// Bearish FVG is filled when price comes back up into gap
        /// </summary>
        private void UpdateBearishFVG(FVGModel fvg, int displayIndex, DateTime currentTime)
        {
            double currentHigh = _displayBars.HighPrices[displayIndex];

            // Check if current bar came back up into the gap
            if (currentHigh >= fvg.Top)
            {
                // Fully filled - price reached top
                fvg.Status = FVGStatus.Filled;
                fvg.MitigationTime = currentTime;
                fvg.MaxPenetrationPrice = fvg.Top;
            }
            else if (currentHigh > fvg.Bottom)
            {
                // Partially filled - price is inside the gap
                fvg.Status = FVGStatus.PartiallyFilled;

                // Track maximum penetration (highest high)
                if (!fvg.MaxPenetrationPrice.HasValue || currentHigh > fvg.MaxPenetrationPrice.Value)
                {
                    fvg.MaxPenetrationPrice = currentHigh;
                }

                // Check if partial fill is deep enough to count as filled
                double fillPercent = FVGCalculator.CalculateFillPercent(fvg);
                if (fillPercent >= _partialFillThreshold)
                {
                    // Treat as fully filled
                    fvg.Status = FVGStatus.Filled;
                    fvg.MitigationTime = currentTime;
                    fvg.MaxPenetrationPrice = fvg.Top;
                }
            }
        }
    }
}
