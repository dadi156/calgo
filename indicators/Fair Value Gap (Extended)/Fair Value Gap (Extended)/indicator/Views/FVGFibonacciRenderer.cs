using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Renders Fibonacci levels inside FVG rectangles
    /// Single Responsibility: Fibonacci line drawing only
    /// </summary>
    public class FVGFibonacciRenderer
    {
        private readonly Chart _chart;
        private readonly Bars _displayBars;
        private readonly Color _fibLinesColor;
        private readonly LineStyle _fibLinesStyle;
        private readonly bool _enableFib236;
        private readonly bool _enableFib382;
        private readonly bool _enableFib500;
        private readonly bool _enableFib618;
        private readonly bool _enableFib786;
        private readonly HashSet<string> _drawnObjects;
        private readonly bool _extendFilledFVGs;
        
        // Label settings (optional)
        private readonly bool _enableLabels;
        private readonly string _labelFontFamily;
        private readonly int _labelFontSize;
        private readonly Color _labelColor;

        public FVGFibonacciRenderer(Chart chart, Bars displayBars, 
                                     Color fibLinesColor, LineStyle fibLinesStyle,
                                     bool enableFib236, bool enableFib382, bool enableFib500, 
                                     bool enableFib618, bool enableFib786,
                                     HashSet<string> drawnObjects,
                                     bool extendFilledFVGs,
                                     bool enableLabels, string labelFontFamily, int labelFontSize, Color labelColor)
        {
            _chart = chart;
            _displayBars = displayBars;
            _fibLinesColor = fibLinesColor;
            _fibLinesStyle = fibLinesStyle;
            _enableFib236 = enableFib236;
            _enableFib382 = enableFib382;
            _enableFib500 = enableFib500;
            _enableFib618 = enableFib618;
            _enableFib786 = enableFib786;
            _drawnObjects = drawnObjects;
            _extendFilledFVGs = extendFilledFVGs;
            _enableLabels = enableLabels;
            _labelFontFamily = labelFontFamily;
            _labelFontSize = labelFontSize;
            _labelColor = labelColor;
        }

        /// <summary>
        /// Draw Fibonacci levels inside FVG
        /// Levels: 23.6%, 38.2%, 50%, 61.8%, 78.6%
        /// Applied to: Partial and Filled FVGs only
        /// Calculation: 100% at 1st bar (starting point), 0% at 3rd bar (ending point)
        /// Bullish FVG: 100% at 1st bar high (bottom), 0% at 3rd bar low (top)
        /// Bearish FVG: 100% at 1st bar low (top), 0% at 3rd bar high (bottom)
        /// </summary>
        public void DrawFibonacciLevels(FVGModel fvg, int currentIndex)
        {
            // Only draw for Partial and Filled FVGs
            if (fvg.Status != FVGStatus.PartiallyFilled && fvg.Status != FVGStatus.Filled)
                return;

            // Convert formation time to display index
            int startIndex = _displayBars.OpenTimes.GetIndexByTime(fvg.FormationTime);
            if (startIndex < 0)
                return;

            // Determine end index (same logic as rectangle)
            int endIndex = CalculateEndIndex(fvg, currentIndex);

            double gapSize = fvg.Top - fvg.Bottom;

            // Calculate Fibonacci levels
            var fibLevels = CalculateFibonacciLevels(fvg, gapSize);

            // Draw enabled levels
            foreach (var level in fibLevels)
            {
                if (!level.enabled)
                    continue;

                DrawFibLine(fvg, level.price, level.label, startIndex, endIndex);
            }
        }

        /// <summary>
        /// Calculate Fibonacci price levels for FVG
        /// Bullish: Calculate from top (0%) down towards bottom (100%)
        /// Bearish: Calculate from bottom (0%) up towards top (100%)
        /// </summary>
        private List<(double price, string label, bool enabled)> CalculateFibonacciLevels(FVGModel fvg, double gapSize)
        {
            var fibLevels = new List<(double price, string label, bool enabled)>();

            if (fvg.Type == FVGType.Bullish)
            {
                // Bullish: Calculate from top (0%) down towards bottom (100%)
                fibLevels.Add((fvg.Top - (gapSize * 0.236), "23.6%", _enableFib236));
                fibLevels.Add((fvg.Top - (gapSize * 0.382), "38.2%", _enableFib382));
                fibLevels.Add((fvg.Top - (gapSize * 0.500), "50%", _enableFib500));
                fibLevels.Add((fvg.Top - (gapSize * 0.618), "61.8%", _enableFib618));
                fibLevels.Add((fvg.Top - (gapSize * 0.786), "78.6%", _enableFib786));
            }
            else // Bearish
            {
                // Bearish: Calculate from bottom (0%) up towards top (100%)
                fibLevels.Add((fvg.Bottom + (gapSize * 0.236), "23.6%", _enableFib236));
                fibLevels.Add((fvg.Bottom + (gapSize * 0.382), "38.2%", _enableFib382));
                fibLevels.Add((fvg.Bottom + (gapSize * 0.500), "50%", _enableFib500));
                fibLevels.Add((fvg.Bottom + (gapSize * 0.618), "61.8%", _enableFib618));
                fibLevels.Add((fvg.Bottom + (gapSize * 0.786), "78.6%", _enableFib786));
            }

            return fibLevels;
        }

        /// <summary>
        /// Draw a single Fibonacci line with optional label
        /// </summary>
        private void DrawFibLine(FVGModel fvg, double price, string label, int startIndex, int endIndex)
        {
            // Create unique name for line
            string lineName = $"FVG_Fib_{fvg.Type}_{fvg.FormationTime.Ticks}_{label.Replace(".", "_").Replace("%", "")}";

            // Draw line from startIndex to endIndex
            var line = _chart.DrawTrendLine(lineName, startIndex, price, endIndex, price, _fibLinesColor);
            line.Thickness = 1;
            line.LineStyle = _fibLinesStyle;

            // Track this object
            _drawnObjects.Add(lineName);

            // Draw label at the end of line (if main labels are enabled)
            if (_enableLabels)
            {
                DrawFibLabel(fvg, price, label, endIndex);
            }
        }

        /// <summary>
        /// Draw label for Fibonacci level
        /// </summary>
        private void DrawFibLabel(FVGModel fvg, double price, string label, int endIndex)
        {
            string labelName = $"FVG_FibLabel_{fvg.Type}_{fvg.FormationTime.Ticks}_{label.Replace(".", "_").Replace("%", "")}";

            var text = _chart.DrawText(labelName, label, endIndex, price, _labelColor);
            text.FontSize = _labelFontSize;
            text.FontFamily = _labelFontFamily;
            text.HorizontalAlignment = HorizontalAlignment.Left;
            text.VerticalAlignment = VerticalAlignment.Center;

            // Track this object
            _drawnObjects.Add(labelName);
        }

        /// <summary>
        /// Calculate end index for Fib lines (same logic as rectangles)
        /// </summary>
        private int CalculateEndIndex(FVGModel fvg, int currentIndex)
        {
            if (fvg.MitigationTime.HasValue && !_extendFilledFVGs)
            {
                // Filled FVG with ExtendFilledFVGs disabled: Stop at mitigation time
                int endIndex = _displayBars.OpenTimes.GetIndexByTime(fvg.MitigationTime.Value);
                return endIndex < 0 ? currentIndex : endIndex;
            }
            else
            {
                // Unfilled, Partial, or Filled with ExtendFilledFVGs enabled: Extend to current bar
                return currentIndex;
            }
        }
    }
}
