using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Tracks bars that are used by FVGs to prevent overlapping detections
    /// Single Responsibility: Bar usage tracking only
    /// </summary>
    public class FVGBarTracker
    {
        private readonly HashSet<int> _usedBars;

        public FVGBarTracker()
        {
            _usedBars = new HashSet<int>();
        }

        /// <summary>
        /// Check if bars are available for new FVG (not used by existing FVGs)
        /// </summary>
        public bool AreBarsAvailable(int firstBar, int middleBar, int thirdBar)
        {
            return !_usedBars.Contains(firstBar) && 
                   !_usedBars.Contains(middleBar) && 
                   !_usedBars.Contains(thirdBar);
        }

        /// <summary>
        /// Mark bars as used by a FVG
        /// </summary>
        public void MarkBarsAsUsed(int firstBar, int middleBar, int thirdBar)
        {
            _usedBars.Add(firstBar);
            _usedBars.Add(middleBar);
            _usedBars.Add(thirdBar);
        }

        /// <summary>
        /// Unmark bars when FVG is removed
        /// </summary>
        public void UnmarkBars(FVGModel fvg)
        {
            _usedBars.Remove(fvg.FirstBarIndex);
            _usedBars.Remove(fvg.MiddleBarIndex);
            _usedBars.Remove(fvg.ThirdBarIndex);
        }
    }
}
