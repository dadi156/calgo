using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Renders FVG rectangles on the chart
    /// Single Responsibility: Rectangle drawing only
    /// </summary>
    public class FVGRectangleRenderer
    {
        private readonly Chart _chart;
        private readonly Bars _displayBars;
        private readonly FVGColorProvider _colorProvider;
        private readonly bool _extendFilledFVGs;
        private readonly HashSet<string> _drawnObjects;

        public FVGRectangleRenderer(Chart chart, Bars displayBars, FVGColorProvider colorProvider, 
                                     bool extendFilledFVGs, HashSet<string> drawnObjects)
        {
            _chart = chart;
            _displayBars = displayBars;
            _colorProvider = colorProvider;
            _extendFilledFVGs = extendFilledFVGs;
            _drawnObjects = drawnObjects;
        }

        /// <summary>
        /// Draw a single FVG rectangle on the chart
        /// </summary>
        public void DrawRectangle(FVGModel fvg, int currentIndex)
        {
            // Convert formation time to display index
            int startIndex = _displayBars.OpenTimes.GetIndexByTime(fvg.FormationTime);

            // If formation time not found in display bars, skip
            if (startIndex < 0)
                return;

            // Determine end index
            int endIndex = CalculateEndIndex(fvg, currentIndex);

            // Get color based on type and status
            Color color = _colorProvider.GetColor(fvg);

            // Create unique name using formation time ticks
            string rectName = $"FVG_{fvg.Type}_{fvg.FormationTime.Ticks}";

            // Draw rectangle
            var rectangle = _chart.DrawRectangle(rectName, startIndex, fvg.Top, endIndex, fvg.Bottom, color);
            rectangle.IsFilled = true;

            // Track this object
            _drawnObjects.Add(rectName);
        }

        /// <summary>
        /// Calculate end index for rectangle based on mitigation status and settings
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
