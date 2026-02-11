using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Detects new FVGs on detection timeframe
    /// Single Responsibility: FVG detection logic only
    /// </summary>
    public class FVGDetectionService
    {
        private readonly Bars _detectionBars;
        private readonly Symbol _symbol;
        private readonly double _minSizePoints;
        private readonly FVGDisplay _displayMode;
        private readonly FVGBarTracker _barTracker;

        public FVGDetectionService(Bars detectionBars, Symbol symbol, double minSizePips, 
                          FVGDisplay displayMode, FVGBarTracker barTracker)
        {
            _detectionBars = detectionBars;
            _symbol = symbol;
            _minSizePoints = minSizePips * symbol.PipSize;
            _displayMode = displayMode;
            _barTracker = barTracker;
        }

        /// <summary>
        /// Detect FVGs at given detection bar index
        /// Returns null if no FVG detected
        /// Prevention: Skip if bars are already used by existing FVGs
        /// </summary>
        public FVGModel DetectFVG(int index)
        {
            // Need at least 3 bars
            if (index < 2)
                return null;

            // Define the 3 bars for this potential FVG
            int firstBar = index - 2;
            int middleBar = index - 1;
            int thirdBar = index;

            // CHECK: Are these bars available? (not used by existing FVGs)
            if (!_barTracker.AreBarsAvailable(firstBar, middleBar, thirdBar))
                return null;

            // Try to detect bullish FVG
            var bullishFVG = TryDetectBullishFVG(firstBar, middleBar, thirdBar);
            if (bullishFVG != null)
            {
                _barTracker.MarkBarsAsUsed(firstBar, middleBar, thirdBar);
                return bullishFVG;
            }

            // Try to detect bearish FVG
            var bearishFVG = TryDetectBearishFVG(firstBar, middleBar, thirdBar);
            if (bearishFVG != null)
            {
                _barTracker.MarkBarsAsUsed(firstBar, middleBar, thirdBar);
                return bearishFVG;
            }

            return null;
        }

        /// <summary>
        /// Try to detect bullish FVG
        /// Bullish FVG: High of bar[i-2] < Low of bar[i]
        /// </summary>
        private FVGModel TryDetectBullishFVG(int firstBar, int middleBar, int thirdBar)
        {
            // Check if we should show bullish FVGs
            bool showBullish = _displayMode == FVGDisplay.Both || _displayMode == FVGDisplay.BullishFVG;
            if (!showBullish)
                return null;

            // Check bullish FVG condition
            if (_detectionBars.HighPrices[firstBar] >= _detectionBars.LowPrices[thirdBar])
                return null;

            double top = _detectionBars.LowPrices[thirdBar];
            double bottom = _detectionBars.HighPrices[firstBar];
            double size = top - bottom;

            // Check minimum size
            if (size < _minSizePoints)
                return null;

            // Create FVG model
            return new FVGModel
            {
                Type = FVGType.Bullish,
                Top = top,
                Bottom = bottom,
                FormationTime = _detectionBars.OpenTimes[firstBar],
                CompletionTime = GetCompletionTime(thirdBar),
                Status = FVGStatus.Unfilled,
                MitigationTime = null,
                FirstBarIndex = firstBar,
                MiddleBarIndex = middleBar,
                ThirdBarIndex = thirdBar,
                MaxPenetrationPrice = null
            };
        }

        /// <summary>
        /// Try to detect bearish FVG
        /// Bearish FVG: Low of bar[i-2] > High of bar[i]
        /// </summary>
        private FVGModel TryDetectBearishFVG(int firstBar, int middleBar, int thirdBar)
        {
            // Check if we should show bearish FVGs
            bool showBearish = _displayMode == FVGDisplay.Both || _displayMode == FVGDisplay.BearishFVG;
            if (!showBearish)
                return null;

            // Check bearish FVG condition
            if (_detectionBars.LowPrices[firstBar] <= _detectionBars.HighPrices[thirdBar])
                return null;

            double top = _detectionBars.LowPrices[firstBar];
            double bottom = _detectionBars.HighPrices[thirdBar];
            double size = top - bottom;

            // Check minimum size
            if (size < _minSizePoints)
                return null;

            // Create FVG model
            return new FVGModel
            {
                Type = FVGType.Bearish,
                Top = top,
                Bottom = bottom,
                FormationTime = _detectionBars.OpenTimes[firstBar],
                CompletionTime = GetCompletionTime(thirdBar),
                Status = FVGStatus.Unfilled,
                MitigationTime = null,
                FirstBarIndex = firstBar,
                MiddleBarIndex = middleBar,
                ThirdBarIndex = thirdBar,
                MaxPenetrationPrice = null
            };
        }

        /// <summary>
        /// Calculate completion time - when FVG is fully formed and can be checked for mitigation
        /// FVG completes when the third bar closes (next bar starts)
        /// </summary>
        private DateTime GetCompletionTime(int thirdBarIndex)
        {
            if (thirdBarIndex + 1 < _detectionBars.Count)
            {
                return _detectionBars.OpenTimes[thirdBarIndex + 1];
            }
            else
            {
                // If we don't have next bar yet, calculate it
                DateTime thirdBarTime = _detectionBars.OpenTimes[thirdBarIndex];
                TimeSpan timeframe = _detectionBars.TimeFrame.ToTimeSpan();
                return thirdBarTime.Add(timeframe);
            }
        }
    }
}
