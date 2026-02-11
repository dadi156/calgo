using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeActivityProfiler : Indicator
    {
        private void DrawBarGraphs(
            int offset,
            DateTime barTime,
            DateTime endTime,
            double anchorPrice,
            double highPrice,
            double volumeProp,
            double netProp,
            double rangeProp,
            double effPerc,
            double absPerc,
            double wastePerc,
            double commitPerc,
            double direction,
            bool isCurrentBar)
        {
            double barHeight = GetAdaptiveBarHeight();
            double barSpacing = GetAdaptiveBarSpacing();
            double yPosition = CalculateGraphYPosition(anchorPrice, highPrice, barHeight, barSpacing);

            int chartBarIndex = Bars.OpenTimes.GetIndexByTime(barTime);
            if (chartBarIndex < 0) return;

            int endChartBarIndex = Bars.OpenTimes.GetIndexByTime(endTime);
            if (endChartBarIndex < 0) endChartBarIndex = Bars.Count - 1;

            // Calculate adjusted end time and visible bars
            DateTime adjustedEndTime;
            int visibleBars;

            if (isCurrentBar)
            {
                // For current bar, use estimated period end directly (cTrader can draw to future times)
                adjustedEndTime = endTime;

                // Calculate visible bars from recent completed periods
                int selectedBarIndex = _selectedBars.OpenTimes.GetIndexByTime(barTime);
                int maxWidth = 1;
                int lookback = Math.Min(3, selectedBarIndex);
                for (int i = 1; i <= lookback; i++)
                {
                    int barStart = Bars.OpenTimes.GetIndexByTime(_selectedBars.OpenTimes[selectedBarIndex - i + 1]);
                    int prevStart = Bars.OpenTimes.GetIndexByTime(_selectedBars.OpenTimes[selectedBarIndex - i]);
                    maxWidth = Math.Max(maxWidth, barStart - prevStart);
                }
                visibleBars = maxWidth;
            }
            else
            {
                adjustedEndTime = Bars.OpenTimes[endChartBarIndex - 1];
                visibleBars = Math.Max(1, endChartBarIndex - chartBarIndex);
            }

            // Define labels based on GraphTextDisplay mode
            string[] labels = GraphTextDisplay == GraphTextDisplay.Concise
                ? new[] { "V", "N", "R", "E", "A", "W", "C" }
                : new[] { "Vol", "Net", "Rng", "Eff", "Abs", "Wst", "Cnv" };

            var bars = new[]
            {
        (value: volumeProp, label: labels[0], color: Color.FromHex("FF33C1F3")),
        (value: netProp, label: labels[1], color: direction > 0 ? Color.ForestGreen : Color.Crimson),
        (value: rangeProp, label: labels[2], color: Color.DarkOrange),
        (value: effPerc, label: labels[3], color: Color.RoyalBlue),
        (value: absPerc, label: labels[4], color: Color.Tomato),
        (value: wastePerc, label: labels[5], color: Color.MediumSlateBlue),
        (value: commitPerc, label: labels[6], color: direction > 0 ? Color.LimeGreen : Color.Red)
    };

            for (int i = 0; i < bars.Length; i++)
            {
                double currentY = yPosition - (i * (barHeight + barSpacing));
                double fillRatio = Math.Min(1.0, Math.Max(0.0, bars[i].value));

                // Calculate fill end time
                DateTime fillEndTime;
                if (isCurrentBar)
                {
                    // Time-based calculation for current bar (projects into future)
                    double periodDuration = (endTime - barTime).TotalMinutes;
                    fillEndTime = barTime.AddMinutes(periodDuration * fillRatio);
                }
                else
                {
                    int fillBarCount = (int)Math.Round(visibleBars * fillRatio);
                    int fillEndIndex = chartBarIndex + Math.Max(1, fillBarCount);
                    fillEndTime = Bars.OpenTimes[Math.Min(fillEndIndex, Bars.Count - 1)];
                }

                // Background
                if (GraphBackground)
                {
                    string bgName = $"bar_bg_{offset}_{i}";
                    Chart.RemoveObject(bgName);

                    // Create 10% opacity version of bar color
                    var bgColor = Color.FromArgb(15, bars[i].color.R, bars[i].color.G, bars[i].color.B);

                    var bgBar = Chart.DrawRectangle(bgName, barTime, currentY, adjustedEndTime, currentY - barHeight, bgColor);
                    bgBar.IsFilled = true;
                    bgBar.IsInteractive = false;
                }

                // Fill
                string fillName = $"bar_fill_{offset}_{i}";
                Chart.RemoveObject(fillName);
                var fillBar = Chart.DrawRectangle(fillName, barTime, currentY, fillEndTime, currentY - barHeight, bars[i].color);
                fillBar.IsFilled = true;
                fillBar.IsInteractive = false;

                // Text - conditional display based on GraphTextDisplay parameter
                if (GraphTextDisplay != GraphTextDisplay.None)
                {
                    string displayText = GraphTextDisplay switch
                    {
                        GraphTextDisplay.Concise => $"{bars[i].label}: {bars[i].value:P1}",
                        GraphTextDisplay.Full => GetFullLabel(bars[i].label, bars[i].value),
                        GraphTextDisplay.Numbers => $"{bars[i].value:P1}",
                        _ => ""
                    };

                    // Add directional arrow for Net and Conviction
                    if ((i == 1 || i == 6) && GraphTextDisplay != GraphTextDisplay.Numbers)
                        displayText += direction > 0 ? " ▲" : direction < 0 ? " ▼" : " ▶";

                    // Determine text time position and alignment based on TextAlignment
                    DateTime textTime;
                    HorizontalAlignment hAlign;

                    switch (TextAlignment)
                    {
                        case GraphTextAlignment.BarStart:
                            textTime = barTime;
                            hAlign = HorizontalAlignment.Right;
                            break;
                        case GraphTextAlignment.FillEnd:
                            textTime = fillEndTime;
                            hAlign = HorizontalAlignment.Right;
                            break;
                        case GraphTextAlignment.BarEnd:
                        default:
                            textTime = adjustedEndTime;
                            hAlign = HorizontalAlignment.Left;
                            break;
                    }

                    string textName = $"bar_text_{offset}_{i}";
                    Chart.RemoveObject(textName);
                    var textLabel = Chart.DrawText(textName, displayText, textTime, currentY - (barHeight / 2), GraphTextColor);
                    textLabel.IsInteractive = false;
                    textLabel.VerticalAlignment = VerticalAlignment.Center;
                    textLabel.HorizontalAlignment = hAlign;
                    textLabel.FontFamily = GraphFontFamily;
                    textLabel.FontSize = GraphFontSize;
                }
            }
        }

        private string GetFullLabel(string shortLabel, double value)
        {
            string fullLabel = shortLabel switch
            {
                "Vol" => "Volume",
                "Net" => "Net",
                "Rng" => "Range",
                "Eff" => "Efficiency",
                "Abs" => "Absorption",
                "Wst" => "Wasted",
                "Cnv" => "Conviction",
                _ => shortLabel
            };

            return $"{fullLabel}: {value:P1}";
        }
    }
}
