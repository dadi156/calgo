using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeActivityProfiler : Indicator
    {
        private void DrawMetrics(
    int offset,
    DateTime barOpenTime,
    DateTime endTime,
    double lowPrice,
    double highPrice,
    double effort,
    double netPips,
    double netPipsAbs,
    double rangePips,
    double efficiency,
    double effProportion,
    double absProportion,
    double wastedRatio,
    double wasteProportion,
    double directionalCommitment,
    double commitProportion,
    double closePosition,
    bool isCurrentBar,
    double volumeProportion,
    double netProportion,
    double rangeProportion,
    int volumeRank,
    int netRank,
    int rangeRank,
    int totalBars,
    double effLow, double effHigh,
    double wastedLow, double wastedHigh,
    double commitLow, double commitHigh)
        {
            string labelName = $"vol_label_{offset}";

            // --- Activity classification using PERCENTILE-BASED THRESHOLDS
            string activity;
            if (effProportion <= effLow && commitProportion <= commitLow && wasteProportion <= wastedLow)
                activity = "Compression";
            else if (effProportion >= effHigh && commitProportion >= commitHigh && wasteProportion <= wastedLow)
                activity = "Expansion";
            else if (wasteProportion >= wastedHigh && commitProportion <= commitLow)
                activity = "Conflict";
            else if (effProportion <= effLow)
            {
                if (directionalCommitment > 0)
                    activity = closePosition > 0.7 ? "Selling Absorbed" :
                               closePosition < 0.3 ? "Buying Absorbed" : "Balanced Absorption";
                else if (directionalCommitment < 0)
                    activity = closePosition < 0.3 ? "Selling Absorbed" :
                               closePosition > 0.7 ? "Buying Absorbed" : "Balanced Absorption";
                else
                    activity = "Balanced Absorption";
            }
            else if (commitProportion >= commitHigh && ((directionalCommitment > 0 && closePosition < 0.3) ||
                                                         (directionalCommitment < 0 && closePosition > 0.7)))
                activity = "Weak Close";
            else
                activity = "Neutral";

            // Determine absorption side when absorption is high
            string absorptionSide = "";
            if (absProportion > 0.25)
            {
                if (closePosition < 0.3)
                    absorptionSide = " (Buyers)";
                else if (closePosition > 0.7)
                    absorptionSide = " (Sellers)";
                else
                    absorptionSide = " (Neutral)";
            }

            string direction = directionalCommitment > 0 ? "▲" : directionalCommitment < 0 ? "▼" : "▶";

            // Add live indicator for current forming bar
            string liveIndicator = isCurrentBar ? " [LIVE]" : "";
            string proportionNote = isCurrentBar ? "\n* Proportions calculated from visible bars only\n* Values update as bar forms" : "";

            string rawSection = ShowMetricsInfo == MetricsDisplay.Complete || ShowMetricsInfo == MetricsDisplay.Raw
                ? $"RAW{liveIndicator}\n" +
                $"─────────────────────────\n" +
                $"Volume: {effort:N0}\n" +
                $"Net: {netPips:F1} pips {direction}\n" +
                $"Range: {rangePips:F1} pips\n\n"
                : "";

            string derivedSection = ShowMetricsInfo == MetricsDisplay.Complete || ShowMetricsInfo == MetricsDisplay.Derived
                ? $"DERIVED\n" +
                  $"────────────────────────\n" +
                  $"Efficiency: {efficiency:F6} ({effProportion:P1})\n" +
                  $"Absorption: {absProportion:P1}{absorptionSide}\n" +
                  $"Wasted Ratio: {wastedRatio:F3} ({wasteProportion:P1})\n" +
                  $"Conviction: {directionalCommitment:F3} ({commitProportion:P1})\n\n" +
                  $"{proportionNote}\n"
                : "";

            string stateSection = ShowMetricsInfo == MetricsDisplay.Complete || ShowMetricsInfo == MetricsDisplay.State
                ? $"STATE\n" +
                  $"────────────────────────\n" +
                  $"{activity}\n\n"
                : "";

            string summarySection = ShowMetricsInfo == MetricsDisplay.Complete || ShowMetricsInfo == MetricsDisplay.Summary
                ? $"SUMMARY\n" +
                  $"────────────────────────\n" +
                  $"• Volume: {GetProportionLevel(volumeProportion, totalBars)} activity — {GetRankText(volumeRank, totalBars)} ({volumeProportion:P0})\n" +
                  $"• Net: {GetProportionLevel(netProportion, totalBars)} movement — {GetRankText(netRank, totalBars)} ({netProportion:P0})\n" +
                  $"• Range: {GetProportionLevel(rangeProportion, totalBars)} range — {GetRankText(rangeRank, totalBars)} ({rangeProportion:P0})\n" +
                  $"• Efficiency: {(effProportion >= 1.0 / totalBars ? "Above average" : "Below average")} — this bar represents {effProportion:P1} of total efficiency\n" +
                  $"• Absorption: {GetAbsorptionInterpretation(absProportion, closePosition, totalBars)}\n" +
                  $"• Wasted: {(wasteProportion >= 1.0 / totalBars ? "Above average" : "Below average")} retracement — {wastedRatio:P0} of range lost ({wasteProportion:P1})\n" +
                  $"• Conviction: {(directionalCommitment >= 0 ? "Bullish" : "Bearish")} — {(commitProportion >= 1.0 / totalBars ? "stronger" : "weaker")} than average ({commitProportion:P1})"
                : "";

            string labelText = rawSection + derivedSection + stateSection + summarySection;

            // Calculate label position based on placement
            double barHeight = GetAdaptiveBarHeight();
            double barSpacing = GetAdaptiveBarSpacing();
            int barCount = 7;
            double totalBarSpace = barCount * barHeight + (barCount - 1) * barSpacing;
            double graphYPosition = CalculateGraphYPosition(lowPrice, highPrice, barHeight, barSpacing);

            double labelY;

            if (Placement == GraphPlacement.AboveHigh)
            {
                // For AboveHigh: place metrics below low price (simpler)
                labelY = lowPrice - (3 * Symbol.PipSize);
            }
            else
            {
                // For BelowLow and Center: metrics below all bars
                labelY = graphYPosition - totalBarSpace - (3 * Symbol.PipSize);
            }

            var label = Chart.FindObject(labelName) as ChartText;

            if (label != null)
            {
                label.Text = labelText;
                label.Time = barOpenTime;
                label.Y = labelY;
                label.Color = MetricsTextColor;
            }
            else
            {
                label = Chart.DrawText(labelName, labelText, barOpenTime, labelY, MetricsTextColor);
                label.IsInteractive = false;
                label.VerticalAlignment = VerticalAlignment.Bottom;
                label.FontFamily = MetricsFontFamily;
                label.FontSize = MetricsFontSize;
            }
        }
    }
}
