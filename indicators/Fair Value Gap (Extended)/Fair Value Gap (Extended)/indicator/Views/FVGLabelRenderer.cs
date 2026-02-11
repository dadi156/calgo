using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Renders information labels on FVG rectangles
    /// Single Responsibility: Label drawing and text formatting only
    /// </summary>
    public class FVGLabelRenderer
    {
        private readonly Chart _chart;
        private readonly Bars _displayBars;
        private readonly Symbol _symbol;
        private readonly string _fontFamily;
        private readonly int _fontSize;
        private readonly Color _labelColor;
        private readonly HashSet<string> _drawnObjects;
        private readonly bool _enableMTF;
        private readonly TimeFrame _selectedTF;

        public FVGLabelRenderer(Chart chart, Bars displayBars, Symbol symbol,
                                string fontFamily, int fontSize, Color labelColor,
                                HashSet<string> drawnObjects, bool enableMTF, TimeFrame selectedTF)
        {
            _chart = chart;
            _displayBars = displayBars;
            _symbol = symbol;
            _fontFamily = fontFamily;
            _fontSize = fontSize;
            _labelColor = labelColor;
            _drawnObjects = drawnObjects;
            _enableMTF = enableMTF;
            _selectedTF = selectedTF;
        }

        /// <summary>
        /// Draw information label on FVG rectangle
        /// Position: Top-left corner of rectangle
        /// </summary>
        public void DrawLabel(FVGModel fvg, int currentIndex)
        {
            // Convert formation time to display index
            int startIndex = _displayBars.OpenTimes.GetIndexByTime(fvg.FormationTime);
            if (startIndex < 0)
                return;

            // Build label text
            string labelText = BuildLabelText(fvg, currentIndex);

            // Create unique label name
            string labelName = $"FVG_Label_{fvg.Type}_{fvg.FormationTime.Ticks}";

            // Position: Top-left corner of FVG rectangle
            double yPosition = fvg.Top;

            // Draw text at the start index (formation time)
            var text = _chart.DrawText(labelName, labelText, startIndex, yPosition, _labelColor);
            text.FontSize = _fontSize;
            text.FontFamily = _fontFamily;

            // Track this object
            _drawnObjects.Add(labelName);
        }

        /// <summary>
        /// Build label text with all FVG information
        /// Format:
        /// Status: Unfilled
        /// Price: 1.2345
        /// Size: 20.5 pips
        /// Distance: 15.2 pips (only for unfilled/partial)
        /// Filled: 38% (only for partial)
        /// Start: 16/11/2025 08:00
        /// Age: 2d 4h 30m
        /// Timeframe: Daily (only if MTF enabled)
        /// </summary>
        private string BuildLabelText(FVGModel fvg, int currentIndex)
        {
            var lines = new List<string>();

            // 1. Status (format with spaces)
            string statusText = FormatStatus(fvg.Status);
            lines.Add($"Status: {statusText}");

            // 2. Price (formation price - 1st bar's high for bullish, 1st bar's low for bearish)
            double formationPrice = fvg.Type == FVGType.Bullish ? fvg.Bottom : fvg.Top;
            lines.Add($"Price: {formationPrice.ToString($"F{_symbol.Digits}")}");

            // 3. Gap size
            double gapSize = fvg.Top - fvg.Bottom;
            double gapSizePips = gapSize / _symbol.PipSize;
            lines.Add($"Size: {gapSizePips:F1} pips");

            // 4. Distance (only for unfilled/partial)
            if (fvg.Status == FVGStatus.Unfilled || fvg.Status == FVGStatus.PartiallyFilled)
            {
                double currentPrice = _displayBars.ClosePrices[currentIndex];
                double distance = Math.Abs(currentPrice - formationPrice);
                double distancePips = distance / _symbol.PipSize;
                lines.Add($"Distance: {distancePips:F1} pips");
            }

            // 5. Mitigation % (only for partial)
            if (fvg.Status == FVGStatus.PartiallyFilled)
            {
                double fillPercent = FVGCalculator.CalculateFillPercent(fvg);
                lines.Add($"Filled: {fillPercent:F0}%");
            }

            // 6. Start datetime (convert UTC to local time)
            DateTime localStartTime = fvg.FormationTime.ToLocalTime();
            string startTime = localStartTime.ToString("dd/MM/yyyy HH:mm");
            lines.Add($"Start: {startTime}");

            // 7. Age
            DateTime endTime = fvg.MitigationTime ?? _displayBars.OpenTimes[currentIndex];
            string age = CalculateAge(fvg.FormationTime, endTime);
            lines.Add($"Age: {age}");

            // 8. Timeframe (only if MTF enabled)
            if (_enableMTF)
            {
                string tfName = _selectedTF.ToString();
                lines.Add($"Timeframe: {tfName}");
            }

            // Combine all lines
            return string.Join("\n", lines);
        }

        /// <summary>
        /// Format status text for display
        /// PartiallyFilled → Partially Filled
        /// </summary>
        private string FormatStatus(FVGStatus status)
        {
            switch (status)
            {
                case FVGStatus.Unfilled:
                    return "Unfilled";
                case FVGStatus.PartiallyFilled:
                    return "Partially Filled";
                case FVGStatus.Filled:
                    return "Filled";
                default:
                    return status.ToString();
            }
        }

        /// <summary>
        /// Calculate age in readable format
        /// Examples: "2d 4h 30m" or "4w 2d 8h" or "3y 1M 18d" or "7h 4m" or "40m"
        /// </summary>
        private string CalculateAge(DateTime start, DateTime end)
        {
            TimeSpan duration = end - start;

            var parts = new List<string>();

            // Calculate years (approximate: 365 days = 1 year)
            int years = (int)(duration.TotalDays / 365);
            if (years > 0)
            {
                parts.Add($"{years}y");
                duration = duration.Subtract(TimeSpan.FromDays(years * 365));
            }

            // Calculate months (approximate: 30 days = 1 month)
            int months = (int)(duration.TotalDays / 30);
            if (months > 0)
            {
                parts.Add($"{months}M");
                duration = duration.Subtract(TimeSpan.FromDays(months * 30));
            }

            // Calculate weeks
            int weeks = (int)(duration.TotalDays / 7);
            if (weeks > 0)
            {
                parts.Add($"{weeks}w");
                duration = duration.Subtract(TimeSpan.FromDays(weeks * 7));
            }

            // Calculate days
            int days = duration.Days;
            if (days > 0)
            {
                parts.Add($"{days}d");
            }

            // Calculate hours
            int hours = duration.Hours;
            if (hours > 0)
            {
                parts.Add($"{hours}h");
            }

            // Calculate minutes
            int minutes = duration.Minutes;
            if (minutes > 0 || parts.Count == 0)  // Show minutes if nothing else or if there are minutes
            {
                parts.Add($"{minutes}m");
            }

            // Take maximum 3 most significant parts
            return string.Join(" ", parts.Take(3));
        }
    }
}
