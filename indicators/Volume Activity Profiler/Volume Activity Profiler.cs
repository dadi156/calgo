using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class VolumeActivityProfiler : Indicator
    {
        private Bars _selectedBars;
        private int _lastProcessedIndex = -1;
        private HashSet<int> _processedBars = new HashSet<int>();

        // Reusable collections to avoid allocations
        private readonly List<(int barIndex, int offset, double volume, double netAbs, double range)> _barData = new();
        private readonly Dictionary<int, int> _volumeRanks = new();
        private readonly Dictionary<int, int> _netRanks = new();
        private readonly Dictionary<int, int> _rangeRanks = new();

        protected override void Initialize()
        {
            _selectedBars = MarketData.GetBars(SelectedTimeframe);

            // Backfill history for normalization
            InitializeHistory();
        }

        public override void Calculate(int index)
        {
            int selectedBarIndex = _selectedBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

            if (selectedBarIndex < 0)
                return;

            // Process on new bar
            if (selectedBarIndex != _lastProcessedIndex)
            {
                // Update history for the previous completed bar (not current forming bar)
                if (_lastProcessedIndex >= 0 &&
                    _lastProcessedIndex < _selectedBars.Count - 1 &&
                    !_processedBars.Contains(_lastProcessedIndex))
                {
                    UpdateHistoryForBar(_lastProcessedIndex);
                    _processedBars.Add(_lastProcessedIndex);
                }

                ClearAllDrawings();
                _lastProcessedIndex = selectedBarIndex;
            }

            // --- STEP 1: Single pass - collect data AND calculate totals
            double totalVolume = 0;
            double totalNetAbs = 0;
            double totalRange = 0;
            double totalEfficiency = 0;
            double totalWasted = 0;
            double totalCommitment = 0;

            _barData.Clear();

            // Temporary lists for raw values (for percentile calculation)
            var efficiencyValues = new List<double>();
            var wastedValues = new List<double>();
            var commitmentValues = new List<double>();

            for (int i = 0; i < Length; i++)
            {
                int targetBarIndex = selectedBarIndex - i;

                if (targetBarIndex < 0)
                    continue;

                double effort = _selectedBars.TickVolumes[targetBarIndex];

                CalculateMetrics(
                    targetBarIndex,
                    effort,
                    out double resultRange,
                    out _,
                    out double resultNetAbs,
                    out double efficiency,
                    out double wastedRatio,
                    out double directionalCommitment);

                totalVolume += effort;
                totalNetAbs += resultNetAbs;
                totalRange += resultRange;
                totalEfficiency += efficiency;
                totalWasted += wastedRatio;
                totalCommitment += Math.Abs(directionalCommitment);

                // Store raw values for percentile calculation
                efficiencyValues.Add(efficiency);
                wastedValues.Add(wastedRatio);
                commitmentValues.Add(Math.Abs(directionalCommitment));

                _barData.Add((targetBarIndex, i, effort, resultNetAbs, resultRange));
            }

            // --- STEP 2: Calculate proportions for each bar
            var effProps = new List<double>();
            var wastedProps = new List<double>();
            var commitProps = new List<double>();

            foreach (var bar in _barData)
            {
                int idx = _barData.IndexOf(bar);
                double effProp = totalEfficiency > 0 ? efficiencyValues[idx] / totalEfficiency : 0;
                double wasteProp = totalWasted > 0 ? wastedValues[idx] / totalWasted : 0;
                double commitProp = totalCommitment > 0 ? commitmentValues[idx] / totalCommitment : 0;

                effProps.Add(effProp);
                wastedProps.Add(wasteProp);
                commitProps.Add(commitProp);
            }

            // --- STEP 3: Calculate percentile thresholds
            double effLow = GetPercentile(effProps, 25);
            double effHigh = GetPercentile(effProps, 75);
            double wastedLow = GetPercentile(wastedProps, 25);
            double wastedHigh = GetPercentile(wastedProps, 75);
            double commitLow = GetPercentile(commitProps, 25);
            double commitHigh = GetPercentile(commitProps, 75);

            // --- STEP 4: Compute ranks (1 = highest)
            ComputeVolumeRanks(_barData, _volumeRanks);
            ComputeNetRanks(_barData, _netRanks);
            ComputeRangeRanks(_barData, _rangeRanks);
            int totalBars = _barData.Count;

            // --- STEP 5: Draw all bars
            foreach (var bar in _barData)
            {
                DrawVolumeLineAndLabel(
                    bar.barIndex,
                    bar.offset,
                    index,
                    totalVolume,
                    totalNetAbs,
                    totalRange,
                    totalEfficiency,
                    totalWasted,
                    totalCommitment,
                    _volumeRanks[bar.barIndex],
                    _netRanks[bar.barIndex],
                    _rangeRanks[bar.barIndex],
                    totalBars,
                    effLow, effHigh,
                    wastedLow, wastedHigh,
                    commitLow, commitHigh);
            }
        }
    }
}
