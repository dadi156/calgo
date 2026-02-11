using System;
using cAlgo.API;

namespace cAlgo
{
    public class IBFibView
    {
        private readonly Chart _chart;
        private readonly Color _fibLineColor;
        private readonly int _lineThickness;
        private readonly LineStyle _lineStyle;
        private readonly bool _showLabels;

        private const string FibPrefix = "IB_Fib_";

        public IBFibView(Chart chart, Color fibLineColor, int thickness, LineStyle lineStyle, bool showLabels)
        {
            _chart = chart;
            _fibLineColor = fibLineColor;
            _lineThickness = thickness;
            _lineStyle = lineStyle;
            _showLabels = showLabels;
        }

        public void DrawFibLines(DateTime startTime, DateTime endTime, IBFibModel fibModel,
            bool show_11_40, bool show_23_6, bool show_38_2, bool show_50, 
            bool show_61_8, bool show_78_6, bool show_88_60)
        {
            // Clear old lines
            Clear();

            // Draw enabled fib levels
            if (show_11_40 && !double.IsNaN(fibModel.Fib_11_40))
                DrawFibLine("11_40", startTime, endTime, fibModel.Fib_11_40, "11.4%");

            if (show_23_6 && !double.IsNaN(fibModel.Fib_23_6))
                DrawFibLine("23_6", startTime, endTime, fibModel.Fib_23_6, "23.6%");

            if (show_38_2 && !double.IsNaN(fibModel.Fib_38_2))
                DrawFibLine("38_2", startTime, endTime, fibModel.Fib_38_2, "38.2%");

            if (show_50 && !double.IsNaN(fibModel.Fib_50))
                DrawFibLine("50", startTime, endTime, fibModel.Fib_50, "50%");

            if (show_61_8 && !double.IsNaN(fibModel.Fib_61_8))
                DrawFibLine("61_8", startTime, endTime, fibModel.Fib_61_8, "61.8%");

            if (show_78_6 && !double.IsNaN(fibModel.Fib_78_6))
                DrawFibLine("78_6", startTime, endTime, fibModel.Fib_78_6, "78.6%");

            if (show_88_60 && !double.IsNaN(fibModel.Fib_88_60))
                DrawFibLine("88_60", startTime, endTime, fibModel.Fib_88_60, "88.6%");
        }

        private void DrawFibLine(string levelName, DateTime startTime, DateTime endTime, double price, string labelText)
        {
            string lineName = FibPrefix + levelName;
            string labelName = FibPrefix + levelName + "_Label";

            // Draw line
            _chart.DrawTrendLine(lineName, startTime, price, endTime, price, 
                _fibLineColor, _lineThickness, _lineStyle);

            // Draw label if enabled with center vertical alignment
            if (_showLabels)
            {
                var text = _chart.DrawText(labelName, labelText, endTime, price, _fibLineColor);
                text.VerticalAlignment = VerticalAlignment.Center;
                text.FontFamily = "Consolas";
                text.FontSize = 10;
            }
        }

        public void Clear()
        {
            // Remove all fib lines and labels
            string[] levels = { "11_40", "23_6", "38_2", "50", "61_8", "78_6", "88_60" };
            
            foreach (var level in levels)
            {
                _chart.RemoveObject(FibPrefix + level);
                _chart.RemoveObject(FibPrefix + level + "_Label");
            }
        }
    }
}
