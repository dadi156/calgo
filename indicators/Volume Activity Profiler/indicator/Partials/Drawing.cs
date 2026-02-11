using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeActivityProfiler : Indicator
    {
        private void DrawVolumeLineAndLabel(
    int targetBarIndex,
    int offset,
    int currentIndex,
    double totalVolume,
    double totalNetAbs,
    double totalRange,
    double totalEfficiency,
    double totalWasted,
    double totalCommitment,
    int volumeRank,
    int netRank,
    int rangeRank,
    int totalBars,
    double effLow, double effHigh,
    double wastedLow, double wastedHigh,
    double commitLow, double commitHigh)
        {
            DateTime barOpenTime = _selectedBars.OpenTimes[targetBarIndex];
            double lowPrice = _selectedBars.LowPrices[targetBarIndex];
            double highPrice = _selectedBars.HighPrices[targetBarIndex];

            DateTime endTime;
            bool isCurrentBar = targetBarIndex == _selectedBars.Count - 1;

            DrawSeparator(offset, barOpenTime);

            if (isCurrentBar)
                endTime = EstimateBarEndTime(barOpenTime, SelectedTimeframe);
            else if (targetBarIndex + 1 < _selectedBars.Count)
                endTime = _selectedBars.OpenTimes[targetBarIndex + 1];
            else
                return;

            // --- STEP 1: Get volume from selected timeframe (FIXED)
            double effort = GetBarVolume(targetBarIndex);

            // --- STEP 2: Calculate RAW metrics
            CalculateMetrics(
                targetBarIndex,
                effort,
                out double resultRange,
                out double resultNet,
                out double resultNetAbs,
                out double efficiency,
                out double wastedRatio,
                out double directionalCommitment);

            double closePosition = GetClosePositionInRange(targetBarIndex);

            // --- STEP 3: Calculate proportions from visible period only
            double volumeProportion = totalVolume > 0 ? effort / totalVolume : 0;
            double netProportion = totalNetAbs > 0 ? resultNetAbs / totalNetAbs : 0;
            double rangeProportion = totalRange > 0 ? resultRange / totalRange : 0;
            double efficiencyProportion = totalEfficiency > 0 ? efficiency / totalEfficiency : 0;
            double wastedProportion = totalWasted > 0 ? wastedRatio / totalWasted : 0;
            double commitmentProportion = totalCommitment > 0 ? Math.Abs(directionalCommitment) / totalCommitment : 0;

            // --- STEP 4: Derive absorption as inverse of efficiency proportion
            double absorptionProportion = 1.0 - efficiencyProportion;

            // --- STEP 5: Draw metrics info label (conditional)
            if (ShowMetricsInfo != MetricsDisplay.None)
            {
                DrawMetrics(
                    offset,
                    barOpenTime,
                    endTime,
                    lowPrice,
                    highPrice,
                    effort,
                    resultNet,
                    resultNetAbs,
                    resultRange,
                    efficiency,
                    efficiencyProportion,
                    absorptionProportion,
                    wastedRatio,
                    wastedProportion,
                    directionalCommitment,
                    commitmentProportion,
                    closePosition,
                    isCurrentBar,
                    volumeProportion,
                    netProportion,
                    rangeProportion,
                    volumeRank,
                    netRank,
                    rangeRank,
                    totalBars,
                    effLow, effHigh,
                    wastedLow, wastedHigh,
                    commitLow, commitHigh);
            }

            // --- STEP 6: Draw bar graphs (always visible)
            DrawBarGraphs(
                offset,
                barOpenTime,
                endTime,
                lowPrice,
                highPrice,
                volumeProportion,
                netProportion,
                rangeProportion,
                efficiencyProportion,
                absorptionProportion,
                wastedProportion,
                commitmentProportion,
                directionalCommitment,
                isCurrentBar);
        }

        private double GetAdaptiveBarHeight()
        {
            if (FixedBarHeightPips > 0)
                return FixedBarHeightPips * Symbol.PipSize;

            // Get visible chart price range
            double visibleRange = Chart.TopY - Chart.BottomY;

            // Bar height = 0.5% of visible chart height (consistent across all instruments)
            return visibleRange * (BarHeightPercent / 100);
        }

        private double GetAdaptiveBarSpacing()
        {
            if (FixedBarSpacingPips > 0)
                return FixedBarSpacingPips * Symbol.PipSize;

            double visibleRange = Chart.TopY - Chart.BottomY;

            // Spacing = 0.1% of visible chart height
            return visibleRange * (BarSpacingPercent / 100);
        }

        private string GetAbsorptionInterpretation(double absorptionProp, double closePosition, int totalBars)
        {
            double avgProportion = 1.0 / totalBars;
            string level = absorptionProp > avgProportion * 1.5 ? "High" :
                           absorptionProp < avgProportion * 0.5 ? "Low" : "Moderate";

            if (absorptionProp > avgProportion * 1.5)
            {
                if (closePosition < 0.3)
                    return $"{level} ({absorptionProp:P1}) — buyers absorbing selling pressure at lows";
                else if (closePosition > 0.7)
                    return $"{level} ({absorptionProp:P1}) — sellers absorbing buying pressure at highs";
                else
                    return $"{level} ({absorptionProp:P1}) — contested — neither side dominating";
            }
            else if (absorptionProp < avgProportion * 0.5)
            {
                return $"{level} ({absorptionProp:P1}) — minimal resistance — price moving freely";
            }
            else
            {
                return $"{level} ({absorptionProp:P1}) — moderate resistance";
            }
        }

        private string GetRankText(int rank, int total)
        {
            if (rank == 1)
                return $"Highest among {total} profiled bars";
            else if (rank == total)
                return $"Lowest among {total} profiled bars";
            else
                return $"{GetOrdinal(rank)} of {total} profiled bars";
        }

        private string GetOrdinal(int num)
        {
            if (num % 100 >= 11 && num % 100 <= 13)
                return num + "th";

            return (num % 10) switch
            {
                1 => num + "st",
                2 => num + "nd",
                3 => num + "rd",
                _ => num + "th"
            };
        }

        private string GetProportionLevel(double proportion, int totalBars)
        {
            double expected = 1.0 / totalBars;

            if (proportion > expected * 1.5)
                return "High";
            else if (proportion < expected * 0.5)
                return "Low";
            else
                return "Moderate";
        }

        private double CalculateGraphYPosition(double lowPrice, double highPrice, double barHeight, double barSpacing)
        {
            double offset = 3 * Symbol.PipSize;

            switch (Placement)
            {
                case GraphPlacement.BelowLow:
                    return lowPrice - offset;

                case GraphPlacement.AboveHigh:
                    // Calculate total height of all bars
                    double totalBarsHeight = (7 * barHeight) + (6 * barSpacing);
                    return highPrice + offset + totalBarsHeight;

                case GraphPlacement.Center:
                    double midPrice = (highPrice + lowPrice) / 2;
                    totalBarsHeight = (7 * barHeight) + (6 * barSpacing);
                    return midPrice + (totalBarsHeight / 2);

                default:
                    return lowPrice - offset;
            }
        }

        private void ClearAllDrawings()
        {
            for (int i = 0; i < Length + 5; i++)
            {
                Chart.RemoveObject($"separator_{i}");

                if (ShowMetricsInfo != MetricsDisplay.None)
                    Chart.RemoveObject($"vol_label_{i}");

                // Clear bar graphs
                for (int j = 0; j < 7; j++)
                {
                    Chart.RemoveObject($"bar_bg_{i}_{j}");
                    Chart.RemoveObject($"bar_fill_{i}_{j}");
                    Chart.RemoveObject($"bar_text_{i}_{j}");
                }
            }
        }
    }
}
