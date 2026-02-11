using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Manages FVG lifecycle: adding new FVGs and removing old ones
    /// Single Responsibility: FVG list management only
    /// </summary>
    public class FVGLifecycleManager
    {
        private readonly List<FVGModel> _fvgList;
        private readonly Bars _detectionBars;
        private readonly int _lookbackPeriod;
        private readonly FVGBarTracker _barTracker;

        public FVGLifecycleManager(List<FVGModel> fvgList, Bars detectionBars, 
                                   int lookbackPeriod, FVGBarTracker barTracker)
        {
            _fvgList = fvgList;
            _detectionBars = detectionBars;
            _lookbackPeriod = lookbackPeriod;
            _barTracker = barTracker;
        }

        /// <summary>
        /// Add a new FVG to the list
        /// </summary>
        public void AddFVG(FVGModel fvg)
        {
            if (fvg != null)
            {
                _fvgList.Add(fvg);
            }
        }

        /// <summary>
        /// Remove FVGs outside the lookback period (based on detection timeframe)
        /// Cleanup: Also unmark bars when FVGs are removed
        /// </summary>
        public void RemoveOldFVGs(int currentDetectionIndex)
        {
            if (currentDetectionIndex < 0)
                return;

            // Calculate lookback start time
            int startIndex = Math.Max(0, currentDetectionIndex - _lookbackPeriod);
            DateTime lookbackStartTime = _detectionBars.OpenTimes[startIndex];

            // Find FVGs to remove
            var fvgsToRemove = _fvgList.FindAll(fvg => fvg.FormationTime < lookbackStartTime);

            // Unmark bars for each FVG being removed
            foreach (var fvg in fvgsToRemove)
            {
                _barTracker.UnmarkBars(fvg);
            }

            // Remove FVGs formed before lookback start
            _fvgList.RemoveAll(fvg => fvg.FormationTime < lookbackStartTime);
        }
    }
}
