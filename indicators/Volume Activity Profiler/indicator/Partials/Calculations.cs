using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeActivityProfiler : Indicator
    {
        // Caching for performance
        private int _cachedNormWindow = -1;
        private int _lastNormWindowIndex = -1;

        private int NormWindow
        {
            get
            {
                // Cache per bar to avoid multiple evaluations
                int currentIndex = Bars.Count - 1;
                if (_lastNormWindowIndex != currentIndex || _cachedNormWindow == -1)
                {
                    _cachedNormWindow = NormMode == NormWindowMode.Custom
                        ? CustomNormWindow
                        : GetAdaptiveNormWindow();
                    _lastNormWindowIndex = currentIndex;
                }
                return _cachedNormWindow;
            }
        }

        private readonly Queue<double> efficiencyHist = new();
        private readonly Queue<double> wastedRatioHist = new();
        private readonly Queue<double> commitmentHist = new();
        private readonly Queue<double> volumeHist = new();
        private readonly Queue<double> netAbsHist = new();
        private readonly Queue<double> rangeHist = new();

        // =========================
        // VOLUME (Effort) - FIXED
        // =========================

        /// <summary>
        /// Gets the tick volume for the specified bar index.
        /// Uses the selected timeframe's data directly for accuracy.
        /// </summary>
        private double GetBarVolume(int targetBarIndex)
        {
            if (targetBarIndex < 0 || targetBarIndex >= _selectedBars.Count)
                return 0;

            return _selectedBars.TickVolumes[targetBarIndex];
        }

        // =========================
        // ATOMIC METRICS
        // =========================
        private void CalculateMetrics(
            int targetBarIndex,
            double effort,
            out double resultRange,
            out double resultNet,
            out double resultNetAbs,
            out double efficiency,
            out double wastedRatio,
            out double directionalCommitment)
        {
            double open = _selectedBars.OpenPrices[targetBarIndex];
            double high = _selectedBars.HighPrices[targetBarIndex];
            double low = _selectedBars.LowPrices[targetBarIndex];
            double close = _selectedBars.ClosePrices[targetBarIndex];

            double range = high - low;
            double net = close - open;

            resultRange = range / Symbol.PipSize;
            resultNet = net / Symbol.PipSize;
            resultNetAbs = Math.Abs(resultNet);

            // --- Efficiency - based on mode
            efficiency = effort > 0
                ? (EfficiencyCalc == EfficiencyMode.Range ? resultRange : resultNetAbs) / effort
                : 0;

            // --- Wasted Ratio (What % of range was retracement) - RAW
            wastedRatio = resultRange > 0
                ? (resultRange - resultNetAbs) / resultRange
                : 0;

            // --- Directional Commitment (Signed: -1 bearish, +1 bullish) - RAW
            directionalCommitment = resultRange > 0
                ? resultNet / resultRange
                : 0;
        }

        private double GetClosePositionInRange(int targetBarIndex)
        {
            double high = _selectedBars.HighPrices[targetBarIndex];
            double low = _selectedBars.LowPrices[targetBarIndex];
            double close = _selectedBars.ClosePrices[targetBarIndex];

            double range = high - low;

            return range > 0
                ? (close - low) / range  // 0 = at low, 1 = at high
                : 0.5;
        }

        // =========================
        // NORMALIZATION (Percentile)
        // =========================
        private double NormalizePercentile(double value, Queue<double> history)
        {
            if (history.Count == 0)
                return 0.5;

            int count = 0;
            foreach (var v in history)
                if (v <= value)
                    count++;

            return (double)count / history.Count;
        }

        private void UpdateHistory(Queue<double> q, double value)
        {
            q.Enqueue(value);
            if (q.Count > NormWindow)
                q.Dequeue();
        }

        private double GetPercentile(List<double> values, double percentile)
        {
            if (values.Count == 0) return 0;

            var sorted = values.OrderBy(x => x).ToList();
            double index = (percentile / 100.0) * (sorted.Count - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper) return sorted[lower];

            double weight = index - lower;
            return sorted[lower] * (1 - weight) + sorted[upper] * weight;
        }
    }
}
