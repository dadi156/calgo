using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Controller: Main orchestrator for FVG detection and management
    /// Delegates responsibilities to specialized components:
    /// - FVGDetectionService: Detects new FVGs
    /// - FVGMitigationChecker: Updates mitigation status
    /// - FVGBarTracker: Prevents overlapping FVGs
    /// - FVGLifecycleManager: Adds/removes FVGs
    /// Multi-Timeframe: Detects on selected TF, checks mitigation on current TF
    /// </summary>
    public class FVGController
    {
        private readonly Bars _displayBars;      // Current TF (for mitigation checks)
        private readonly Bars _detectionBars;    // Selected TF (for FVG detection)
        private readonly List<FVGModel> _fvgList;

        // Specialized components
        private readonly FVGBarTracker _barTracker;
        private readonly FVGDetectionService _detector;
        private readonly FVGMitigationChecker _mitigationChecker;
        private readonly FVGLifecycleManager _lifecycleManager;

        // Track last detection bar to avoid re-detecting
        private int _lastDetectionBarProcessed = -1;

        public FVGController(Bars displayBars, Bars detectionBars, Symbol symbol,
                           int lookbackPeriod, double minSizePips, FVGDisplay displayMode, int partialFillThreshold)
        {
            _displayBars = displayBars;
            _detectionBars = detectionBars;
            _fvgList = new List<FVGModel>();

            // Initialize bar tracker
            _barTracker = new FVGBarTracker();

            // Initialize detector
            _detector = new FVGDetectionService(
                detectionBars, symbol, minSizePips, displayMode, _barTracker
            );

            // Initialize mitigation checker
            _mitigationChecker = new FVGMitigationChecker(
                displayBars, partialFillThreshold
            );

            // Initialize lifecycle manager
            _lifecycleManager = new FVGLifecycleManager(
                _fvgList, detectionBars, lookbackPeriod, _barTracker
            );
        }

        /// <summary>
        /// Get the list of all FVGs
        /// </summary>
        public List<FVGModel> GetFVGList()
        {
            return _fvgList;
        }

        /// <summary>
        /// Process FVG detection and updates for current bar
        /// Multi-TF: Only detect when new detection TF bar forms, always check mitigation
        /// </summary>
        public void ProcessBar(int displayIndex)
        {
            // Get corresponding detection bar index
            int detectionIndex = GetDetectionBarIndex(displayIndex);

            if (detectionIndex < 0)
                return;

            // Only detect FVGs when new detection bar forms
            if (detectionIndex > _lastDetectionBarProcessed)
            {
                DetectNewFVG(detectionIndex);
                _lastDetectionBarProcessed = detectionIndex;
            }

            // ALWAYS check mitigation on current display timeframe
            UpdateAllMitigations(displayIndex);

            // Remove old FVGs outside lookback
            _lifecycleManager.RemoveOldFVGs(detectionIndex);
        }

        /// <summary>
        /// Get detection bar index from display bar index
        /// </summary>
        private int GetDetectionBarIndex(int displayIndex)
        {
            if (displayIndex < 0 || displayIndex >= _displayBars.Count)
                return -1;

            DateTime displayTime = _displayBars.OpenTimes[displayIndex];
            return _detectionBars.OpenTimes.GetIndexByTime(displayTime);
        }

        /// <summary>
        /// Detect new FVG at detection index
        /// Delegates to FairValueGap component
        /// </summary>
        private void DetectNewFVG(int detectionIndex)
        {
            var newFVG = _detector.DetectFVG(detectionIndex);
            _lifecycleManager.AddFVG(newFVG);
        }

        /// <summary>
        /// Update mitigation status for all FVGs
        /// Delegates to FVGMitigationChecker component
        /// </summary>
        private void UpdateAllMitigations(int displayIndex)
        {
            foreach (var fvg in _fvgList)
            {
                _mitigationChecker.UpdateMitigation(fvg, displayIndex);
            }
        }
    }
}
