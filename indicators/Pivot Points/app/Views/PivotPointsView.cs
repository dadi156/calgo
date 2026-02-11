using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo.Indicators
{
    /// <summary>
    /// View for rendering pivot points on the chart
    /// </summary>
    public class PivotPointsView
    {
        private Chart _chart;
        private Dictionary<string, ChartObject> _pivotLines;
        private bool _showPriceInLabels = true;

        // Colors for pivot lines
        private Color _pivotColor = Color.Yellow;
        private Color _resistanceColor = Color.Red;
        private Color _supportColor = Color.Green;

        public PivotPointsView(Chart chart)
        {
            _chart = chart;
            _pivotLines = new Dictionary<string, ChartObject>();
        }

        /// <summary>
        /// Draws pivot points for a specific period
        /// </summary>
        public void DrawPivotPointsForPeriod(PivotPointsData pivotData, DateTime startTime, DateTime endTime, string periodName)
        {
            if (pivotData == null)
                return;

            // Always draw pivot level with label
            DrawPivotLine("Pivot_" + periodName, "PP", pivotData.PivotLevel, _pivotColor, startTime, endTime);

            // Only draw resistance and support levels if LevelsToShow > 0
            if (pivotData.LevelsToShow > 0)
            {
                // Draw resistance levels with labels
                for (int i = 0; i < pivotData.LevelsToShow && i < pivotData.ResistanceLevels.Length; i++)
                {
                    string name = $"R{i + 1}_{periodName}";
                    string label = $"R{i + 1}";
                    DrawResistanceLine(name, label, pivotData.ResistanceLevels[i], _resistanceColor, startTime, endTime);
                }

                // Draw support levels with labels
                for (int i = 0; i < pivotData.LevelsToShow && i < pivotData.SupportLevels.Length; i++)
                {
                    string name = $"S{i + 1}_{periodName}";
                    string label = $"S{i + 1}";
                    DrawSupportLine(name, label, pivotData.SupportLevels[i], _supportColor, startTime, endTime);
                }
            }

            // Add a period label if needed
            var periodLabel = _chart.DrawText($"Period_{periodName}", periodName, startTime, pivotData.PivotLevel, _pivotColor);
            periodLabel.HorizontalAlignment = HorizontalAlignment.Left;
            periodLabel.VerticalAlignment = VerticalAlignment.Bottom;
            periodLabel.FontSize = 10;
            _pivotLines[$"Period_{periodName}"] = periodLabel;
        }

        private void DrawPivotLine(string name, string label, double price, Color color, DateTime startTime, DateTime endTime)
        {
            string lineName = $"PivotPoint_{name}";

            // Create horizontal ray with start and end times
            var line = _chart.DrawTrendLine(lineName, startTime, price, endTime, price, color);

            // Set line properties
            line.LineStyle = LineStyle.Solid;
            line.Thickness = 1;

            // Store the line reference
            _pivotLines[lineName] = line;

            // Add a label for the pivot line with price
            var labelName = $"{lineName}_Label";
            string labelText = _showPriceInLabels ? $"{label}: {price:F5}" : label;
            var textLabel = _chart.DrawText(labelName, labelText, startTime, price, color);
            textLabel.HorizontalAlignment = HorizontalAlignment.Left;
            textLabel.VerticalAlignment = VerticalAlignment.Center;
            textLabel.FontSize = 10;
            _pivotLines[labelName] = textLabel;
        }

        private void DrawResistanceLine(string name, string label, double price, Color color, DateTime startTime, DateTime endTime)
        {
            string lineName = $"PivotPoint_{name}";

            // Create resistance line
            var line = _chart.DrawTrendLine(lineName, startTime, price, endTime, price, color);
            line.LineStyle = LineStyle.Solid;
            line.Thickness = 1;
            _pivotLines[lineName] = line;

            // Add a label to identify the level with price
            var labelName = $"{lineName}_Label";
            string labelText = _showPriceInLabels ? $"{label}: {price:F5}" : label;
            var textLabel = _chart.DrawText(labelName, labelText, startTime, price, color);
            textLabel.HorizontalAlignment = HorizontalAlignment.Left;
            textLabel.VerticalAlignment = VerticalAlignment.Center;
            textLabel.FontSize = 10;
            _pivotLines[labelName] = textLabel;
        }

        private void DrawSupportLine(string name, string label, double price, Color color, DateTime startTime, DateTime endTime)
        {
            string lineName = $"PivotPoint_{name}";

            // Create support line
            var line = _chart.DrawTrendLine(lineName, startTime, price, endTime, price, color);
            line.LineStyle = LineStyle.Solid;
            line.Thickness = 1;
            _pivotLines[lineName] = line;

            // Add a label to identify the level with price
            var labelName = $"{lineName}_Label";
            string labelText = _showPriceInLabels ? $"{label}: {price:F5}" : label;
            var textLabel = _chart.DrawText(labelName, labelText, startTime, price, color);
            textLabel.HorizontalAlignment = HorizontalAlignment.Left;
            textLabel.VerticalAlignment = VerticalAlignment.Center;
            textLabel.FontSize = 10;
            _pivotLines[labelName] = textLabel;
        }

        /// <summary>
        /// Clears all pivot lines
        /// </summary>
        public void ClearLines()
        {
            // Remove existing pivot point lines
            foreach (var lineName in _pivotLines.Keys)
            {
                _chart.RemoveObject(lineName);
            }

            // Clear the list
            _pivotLines.Clear();
        }

        /// <summary>
        /// Sets the colors for pivot lines
        /// </summary>
        public void SetColors(Color pivotColor, Color resistanceColor, Color supportColor, bool showPrice)
        {
            _pivotColor = pivotColor;
            _resistanceColor = resistanceColor;
            _supportColor = supportColor;
            _showPriceInLabels = showPrice;
        }


    }
}
