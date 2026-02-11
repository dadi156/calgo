using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeActivityProfiler : Indicator
    {
        private DateTime EstimateBarEndTime(DateTime startTime, TimeFrame timeframe)
        {
            if (timeframe == TimeFrame.Monthly)
                return startTime.AddMonths(1);
            if (timeframe == TimeFrame.Weekly)
                return startTime.AddDays(7);
            if (timeframe == TimeFrame.Daily)
                return startTime.AddDays(1);
            if (timeframe == TimeFrame.Day2)
                return startTime.AddDays(2);
            if (timeframe == TimeFrame.Day3)
                return startTime.AddDays(3);
            if (timeframe == TimeFrame.Hour12)
                return startTime.AddHours(12);
            if (timeframe == TimeFrame.Hour8)
                return startTime.AddHours(8);
            if (timeframe == TimeFrame.Hour6)
                return startTime.AddHours(6);
            if (timeframe == TimeFrame.Hour4)
                return startTime.AddHours(4);
            if (timeframe == TimeFrame.Hour3)
                return startTime.AddHours(3);
            if (timeframe == TimeFrame.Hour2)
                return startTime.AddHours(2);
            if (timeframe == TimeFrame.Hour)
                return startTime.AddHours(1);
            if (timeframe == TimeFrame.Minute45)
                return startTime.AddMinutes(45);
            if (timeframe == TimeFrame.Minute30)
                return startTime.AddMinutes(30);
            if (timeframe == TimeFrame.Minute20)
                return startTime.AddMinutes(20);
            if (timeframe == TimeFrame.Minute15)
                return startTime.AddMinutes(15);
            if (timeframe == TimeFrame.Minute10)
                return startTime.AddMinutes(10);
            if (timeframe == TimeFrame.Minute9)
                return startTime.AddMinutes(9);
            if (timeframe == TimeFrame.Minute8)
                return startTime.AddMinutes(8);
            if (timeframe == TimeFrame.Minute7)
                return startTime.AddMinutes(7);
            if (timeframe == TimeFrame.Minute6)
                return startTime.AddMinutes(6);
            if (timeframe == TimeFrame.Minute5)
                return startTime.AddMinutes(5);
            if (timeframe == TimeFrame.Minute4)
                return startTime.AddMinutes(4);
            if (timeframe == TimeFrame.Minute3)
                return startTime.AddMinutes(3);
            if (timeframe == TimeFrame.Minute2)
                return startTime.AddMinutes(2);
            if (timeframe == TimeFrame.Minute)
                return startTime.AddMinutes(1);

            return startTime.AddDays(1);
        }

        /// <summary>
        /// Returns adaptive normalization window based on selected timeframe.
        /// Uses intelligent defaults based on reference periods:
        /// - Minutes: 4h to 3 days
        /// - Hours: 1.5 weeks to 3 months
        /// - Days: 6 months
        /// - Weekly: 1 year
        /// - Monthly: 5 years
        /// </summary>
        private int GetAdaptiveNormWindow()
        {
            // M1: 4 hours reference (240 minutes)
            if (SelectedTimeframe == TimeFrame.Minute)
                return 240;

            // M2-M10: 8 hours reference (480 minutes)
            if (SelectedTimeframe == TimeFrame.Minute2)
                return 240;
            if (SelectedTimeframe == TimeFrame.Minute3)
                return 160;
            if (SelectedTimeframe == TimeFrame.Minute4)
                return 120;
            if (SelectedTimeframe == TimeFrame.Minute5)
                return 96;
            if (SelectedTimeframe == TimeFrame.Minute6)
                return 80;
            if (SelectedTimeframe == TimeFrame.Minute7)
                return 68;
            if (SelectedTimeframe == TimeFrame.Minute8)
                return 60;
            if (SelectedTimeframe == TimeFrame.Minute9)
                return 53;
            if (SelectedTimeframe == TimeFrame.Minute10)
                return 48;

            // M15-M20: 2 days reference (2,880 minutes)
            if (SelectedTimeframe == TimeFrame.Minute15)
                return 192;
            if (SelectedTimeframe == TimeFrame.Minute20)
                return 144;

            // M30-M45: 3 days reference (4,320 minutes)
            if (SelectedTimeframe == TimeFrame.Minute30)
                return 144;
            if (SelectedTimeframe == TimeFrame.Minute45)
                return 96;

            // H1: 1.5 weeks reference (180 hours)
            if (SelectedTimeframe == TimeFrame.Hour)
                return 180;

            // H2-H6: 2 weeks reference (240 hours = 10 trading days)
            if (SelectedTimeframe == TimeFrame.Hour2)
                return 120;
            if (SelectedTimeframe == TimeFrame.Hour3)
                return 80;
            if (SelectedTimeframe == TimeFrame.Hour4)
                return 60;
            if (SelectedTimeframe == TimeFrame.Hour6)
                return 40;

            // H8: 3 weeks reference (360 hours = 15 trading days)
            if (SelectedTimeframe == TimeFrame.Hour8)
                return 45;

            // H12: 3 months reference (1,440 hours = 60 trading days)
            if (SelectedTimeframe == TimeFrame.Hour12)
                return 120;

            // D1-D3: 6 months reference (120 trading days)
            if (SelectedTimeframe == TimeFrame.Daily)
                return 120;
            if (SelectedTimeframe == TimeFrame.Day2)
                return 60;
            if (SelectedTimeframe == TimeFrame.Day3)
                return 40;

            // Weekly: 1 year reference (50 weeks ≈ 250 trading days / 5)
            if (SelectedTimeframe == TimeFrame.Weekly)
                return 50;

            // Monthly: 5 years reference (60 months)
            if (SelectedTimeframe == TimeFrame.Monthly)
                return 60;

            // Fallback for unknown timeframes
            return 60;
        }

        private void InitializeHistory()
        {
            int windowSize = NormWindow;
            int startIndex = Math.Max(0, _selectedBars.Count - windowSize - Length - 1);

            // Backfill history with completed bars only
            for (int i = startIndex; i < _selectedBars.Count - 1; i++)
            {
                UpdateHistoryForBar(i);
                _processedBars.Add(i);
            }
        }

        private void UpdateHistoryForBar(int targetBarIndex)
        {
            // Skip if already processed
            if (_processedBars.Contains(targetBarIndex))
                return;

            // Calculate volume
            double effort = _selectedBars.TickVolumes[targetBarIndex];

            // Calculate metrics
            CalculateMetrics(
                targetBarIndex,
                effort,
                out double resultRange,
                out _,
                out double resultNetAbs,
                out double efficiency,
                out double wastedRatio,
                out double directionalCommitment);

            // Update histories
            UpdateHistory(efficiencyHist, efficiency);
            UpdateHistory(wastedRatioHist, wastedRatio);
            UpdateHistory(commitmentHist, Math.Abs(directionalCommitment));
            UpdateHistory(volumeHist, effort);
            UpdateHistory(netAbsHist, resultNetAbs);
            UpdateHistory(rangeHist, resultRange);
        }

        private void ComputeVolumeRanks(List<(int barIndex, int offset, double volume, double netAbs, double range)> items, Dictionary<int, int> ranks)
        {
            var sorted = items.OrderByDescending(x => x.volume).ToList();
            ranks.Clear();

            for (int i = 0; i < sorted.Count; i++)
                ranks[sorted[i].barIndex] = i + 1;
        }

        private void ComputeNetRanks(List<(int barIndex, int offset, double volume, double netAbs, double range)> items, Dictionary<int, int> ranks)
        {
            var sorted = items.OrderByDescending(x => x.netAbs).ToList();
            ranks.Clear();

            for (int i = 0; i < sorted.Count; i++)
                ranks[sorted[i].barIndex] = i + 1;
        }

        private void ComputeRangeRanks(List<(int barIndex, int offset, double volume, double netAbs, double range)> items, Dictionary<int, int> ranks)
        {
            var sorted = items.OrderByDescending(x => x.range).ToList();
            ranks.Clear();

            for (int i = 0; i < sorted.Count; i++)
                ranks[sorted[i].barIndex] = i + 1;
        }
    }
}
