using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// View: Main orchestrator for FVG visualization
    /// Delegates responsibilities to specialized renderers:
    /// - FVGFilterService: Filters which FVGs to display
    /// - FVGRectangleRenderer: Draws FVG rectangles
    /// - FVGLabelRenderer: Draws information labels
    /// - FVGFibonacciRenderer: Draws Fibonacci levels
    /// - FVGColorProvider: Provides colors for FVGs
    /// </summary>
    public class FVGView
    {
        private readonly Chart _chart;
        private readonly HashSet<string> _drawnObjects;
        
        // Specialized components
        private readonly FVGFilterService _filterService;
        private readonly FVGRectangleRenderer _rectangleRenderer;
        private readonly FVGLabelRenderer _labelRenderer;
        private readonly FVGFibonacciRenderer _fibonacciRenderer;
        
        // Control flags
        private readonly bool _enableLabels;
        private readonly bool _enableFibonacci;

        public FVGView(Chart chart, Bars displayBars, Symbol symbol,
                       Color bullishUnfilled, Color bullishPartial, Color bullishFilled,
                       Color bearishUnfilled, Color bearishPartial, Color bearishFilled,
                       bool showUnfilled, bool showPartial, bool showFilled,
                       bool enableLabels, string labelFontFamily, int labelFontSize, Color labelColor,
                       bool enableMTF, TimeFrame selectedTF, int maxFVGsToDisplay, bool extendFilledFVGs,
                       bool enableFibonacciLines, Color fibLinesColor, LineStyle fibLinesStyle,
                       bool enableFib236, bool enableFib382, bool enableFib500, bool enableFib618, bool enableFib786)
        {
            _chart = chart;
            _drawnObjects = new HashSet<string>();
            _enableLabels = enableLabels;
            _enableFibonacci = enableFibonacciLines;

            // Initialize color provider
            var colorProvider = new FVGColorProvider(
                bullishUnfilled, bullishPartial, bullishFilled,
                bearishUnfilled, bearishPartial, bearishFilled
            );

            // Initialize filter service
            _filterService = new FVGFilterService(
                showUnfilled, showPartial, showFilled, maxFVGsToDisplay
            );

            // Initialize rectangle renderer
            _rectangleRenderer = new FVGRectangleRenderer(
                chart, displayBars, colorProvider, extendFilledFVGs, _drawnObjects
            );

            // Initialize label renderer
            _labelRenderer = new FVGLabelRenderer(
                chart, displayBars, symbol, labelFontFamily, labelFontSize, labelColor,
                _drawnObjects, enableMTF, selectedTF
            );

            // Initialize Fibonacci renderer
            _fibonacciRenderer = new FVGFibonacciRenderer(
                chart, displayBars, fibLinesColor, fibLinesStyle,
                enableFib236, enableFib382, enableFib500, enableFib618, enableFib786,
                _drawnObjects, extendFilledFVGs,
                enableLabels, labelFontFamily, labelFontSize, labelColor
            );
        }

        /// <summary>
        /// Draw all FVGs on the chart
        /// Orchestrates filtering and delegates drawing to specialized renderers
        /// </summary>
        public void DrawFVGs(List<FVGModel> fvgList, int currentIndex)
        {
            // Remove existing FVG objects
            RemoveAllObjects();

            // Clear tracking set
            _drawnObjects.Clear();

            // Filter FVGs (status filter + limit + sort)
            var filteredFVGs = _filterService.FilterFVGs(fvgList);

            // Draw each FVG using specialized renderers
            foreach (var fvg in filteredFVGs)
            {
                // 1. Draw rectangle
                _rectangleRenderer.DrawRectangle(fvg, currentIndex);

                // 2. Draw Fibonacci levels (if enabled and applicable)
                if (_enableFibonacci)
                {
                    _fibonacciRenderer.DrawFibonacciLevels(fvg, currentIndex);
                }

                // 3. Draw label (if enabled)
                if (_enableLabels)
                {
                    _labelRenderer.DrawLabel(fvg, currentIndex);
                }
            }
        }

        /// <summary>
        /// Remove all FVG objects created by this indicator
        /// </summary>
        private void RemoveAllObjects()
        {
            foreach (var objectName in _drawnObjects.ToList())
            {
                var obj = _chart.FindObject(objectName);
                if (obj != null)
                {
                    _chart.RemoveObject(objectName);
                }
            }
        }
    }
}
