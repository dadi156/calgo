using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Provides colors for FVG visualization based on type and status
    /// Single Responsibility: Color selection logic only
    /// </summary>
    public class FVGColorProvider
    {
        private readonly Color _bullishUnfilledColor;
        private readonly Color _bullishPartialColor;
        private readonly Color _bullishFilledColor;
        private readonly Color _bearishUnfilledColor;
        private readonly Color _bearishPartialColor;
        private readonly Color _bearishFilledColor;

        public FVGColorProvider(
            Color bullishUnfilled, Color bullishPartial, Color bullishFilled,
            Color bearishUnfilled, Color bearishPartial, Color bearishFilled)
        {
            _bullishUnfilledColor = bullishUnfilled;
            _bullishPartialColor = bullishPartial;
            _bullishFilledColor = bullishFilled;
            _bearishUnfilledColor = bearishUnfilled;
            _bearishPartialColor = bearishPartial;
            _bearishFilledColor = bearishFilled;
        }

        /// <summary>
        /// Get appropriate color based on FVG type and status
        /// </summary>
        public Color GetColor(FVGModel fvg)
        {
            if (fvg.Type == FVGType.Bullish)
            {
                switch (fvg.Status)
                {
                    case FVGStatus.Unfilled:
                        return _bullishUnfilledColor;
                    case FVGStatus.PartiallyFilled:
                        return _bullishPartialColor;
                    case FVGStatus.Filled:
                        return _bullishFilledColor;
                    default:
                        return _bullishUnfilledColor;
                }
            }
            else // Bearish
            {
                switch (fvg.Status)
                {
                    case FVGStatus.Unfilled:
                        return _bearishUnfilledColor;
                    case FVGStatus.PartiallyFilled:
                        return _bearishPartialColor;
                    case FVGStatus.Filled:
                        return _bearishFilledColor;
                    default:
                        return _bearishUnfilledColor;
                }
            }
        }
    }
}
