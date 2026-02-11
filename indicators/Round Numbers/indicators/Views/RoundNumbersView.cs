using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class RoundNumbersView
    {
        private readonly Chart _chart;
        private readonly Symbol _symbol;

        public RoundNumbersView(Chart chart, Symbol symbol)
        {
            _chart = chart;
            _symbol = symbol;
        }

        public void DrawPriceLevels(
            IReadOnlyList<double> priceLevels,
            DateTime startTime,
            DateTime endTime,
            LineStyle lineStyle,
            int lineThickness,
            Color lineColor,
            bool enableFills,
            FillPattern fillPatternType,
            Color fillColor,
            bool enableAlternateColor,
            int alternateMultiple,
            Color alternateColor,
            bool enableHighlightLevel,
            double highlightLevelStart,
            double highlightLevelEnd,
            Color highlightColor,
            bool showLabels,
            int labelFontSize)
        {
            // Draw fills first (so they appear behind lines)
            if (enableFills)
            {
                int startIndex = (fillPatternType == FillPattern.Odd) ? 0 : 1;
                for (int i = startIndex; i < priceLevels.Count - 1; i += 2)
                {
                    string rectName = "RPL_Fill_" + i;
                    var rect = _chart.DrawRectangle(rectName, startTime, priceLevels[i], endTime, priceLevels[i + 1], fillColor);
                    rect.IsFilled = true;
                }
            }

            // Draw highlight level if enabled
            if (enableHighlightLevel && highlightLevelStart != highlightLevelEnd)
            {
                string highlightName = "RPL_Highlight";
                var highlight = _chart.DrawRectangle(highlightName, startTime, highlightLevelStart, endTime, highlightLevelEnd, highlightColor);
                highlight.IsFilled = true;
            }

            foreach (double price in priceLevels)
            {
                // Create unique name for this line
                string lineName = "RPL_Line_" + price.ToString("F" + _symbol.Digits).Replace(".", "_");

                // Determine which color to use
                Color actualLineColor = lineColor;

                // Check if this price is a multiple of the alternate value
                if (enableAlternateColor && alternateMultiple > 0)
                {
                    double alternatePipValue = _symbol.PipSize * alternateMultiple;
                    double remainder = Math.Abs(price % alternatePipValue);

                    // If remainder is very small (within tolerance), it's an alternate level
                    if (remainder < _symbol.PipSize * 0.1 || Math.Abs(remainder - alternatePipValue) < _symbol.PipSize * 0.1)
                    {
                        actualLineColor = alternateColor;
                    }
                }

                // Draw horizontal line
                var line = _chart.DrawTrendLine(lineName, startTime, price, endTime, price, actualLineColor);
                line.Thickness = lineThickness;
                line.LineStyle = lineStyle;

                // Draw label if enabled
                if (showLabels)
                {
                    string labelName = "RPL_Label_" + price.ToString("F" + _symbol.Digits).Replace(".", "_");
                    string labelText = price.ToString("F" + _symbol.Digits);

                    var label = _chart.DrawText(labelName, labelText, endTime, price, actualLineColor);
                    label.FontSize = labelFontSize;
                    label.HorizontalAlignment = HorizontalAlignment.Right;
                    label.VerticalAlignment = VerticalAlignment.Center;
                }
            }
        }

        public void ClearDrawings()
        {
            var objectsToRemove = new List<string>();

            foreach (var obj in _chart.Objects)
            {
                if (obj.Name.StartsWith("RPL_"))
                {
                    objectsToRemove.Add(obj.Name);
                }
            }

            foreach (var name in objectsToRemove)
            {
                _chart.RemoveObject(name);
            }
        }
    }
}
