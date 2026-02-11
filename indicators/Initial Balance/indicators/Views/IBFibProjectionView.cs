using System;
using cAlgo.API;

namespace cAlgo
{
    public class IBFibProjectionView
    {
        private readonly Chart _chart;
        private readonly Color _fibLineColor;
        private readonly int _fibLineThickness;
        private readonly LineStyle _fibLineStyle;
        private readonly bool _showLabels;
        
        // For 0% and 100% anchor lines - use IB line styling
        private readonly Color _highLineColor;
        private readonly Color _lowLineColor;
        private readonly int _ibLineThickness;
        private readonly LineStyle _ibLineStyle;

        private const string UpPrefix = "IB_Proj_Up_";
        private const string DownPrefix = "IB_Proj_Down_";

        public IBFibProjectionView(Chart chart, 
            Color fibLineColor, int fibThickness, LineStyle fibLineStyle, bool showLabels,
            Color highLineColor, Color lowLineColor, int ibThickness, LineStyle ibLineStyle)
        {
            _chart = chart;
            _fibLineColor = fibLineColor;
            _fibLineThickness = fibThickness;
            _fibLineStyle = fibLineStyle;
            _showLabels = showLabels;
            
            _highLineColor = highLineColor;
            _lowLineColor = lowLineColor;
            _ibLineThickness = ibThickness;
            _ibLineStyle = ibLineStyle;
        }

        public void DrawProjectionLines(DateTime startTime, DateTime endTime, IBFibProjectionModel model,
            bool drawUpward, bool drawDownward,
            bool show_11_40, bool show_23_6, bool show_38_2, 
            bool show_50, bool show_61_8, bool show_78_6, bool show_88_60)
        {
            // Clear old lines first
            Clear();

            // Draw upward projection
            if (drawUpward)
            {
                // Skip 0% - it's on IB High line (already drawn)

                if (show_11_40 && !double.IsNaN(model.Up_11_40))
                    DrawFibLine(UpPrefix + "11_40", startTime, endTime, model.Up_11_40, "11.4%");

                if (show_23_6 && !double.IsNaN(model.Up_23_6))
                    DrawFibLine(UpPrefix + "23_6", startTime, endTime, model.Up_23_6, "23.6%");

                if (show_38_2 && !double.IsNaN(model.Up_38_2))
                    DrawFibLine(UpPrefix + "38_2", startTime, endTime, model.Up_38_2, "38.2%");

                if (show_50 && !double.IsNaN(model.Up_50))
                    DrawFibLine(UpPrefix + "50", startTime, endTime, model.Up_50, "50%");

                if (show_61_8 && !double.IsNaN(model.Up_61_8))
                    DrawFibLine(UpPrefix + "61_8", startTime, endTime, model.Up_61_8, "61.8%");

                if (show_78_6 && !double.IsNaN(model.Up_78_6))
                    DrawFibLine(UpPrefix + "78_6", startTime, endTime, model.Up_78_6, "78.6%");

                if (show_88_60 && !double.IsNaN(model.Up_88_60))
                    DrawFibLine(UpPrefix + "88_60", startTime, endTime, model.Up_88_60, "88.6%");

                // 100% level uses LowLineColor and IB line styling
                if (!double.IsNaN(model.Up_100))
                    DrawAnchorLine(UpPrefix + "100", startTime, endTime, model.Up_100, "100%", _lowLineColor);
            }

            // Draw downward projection
            if (drawDownward)
            {
                // Skip 100% - it's on IB Low line (already drawn)

                if (show_88_60 && !double.IsNaN(model.Down_88_60))
                    DrawFibLine(DownPrefix + "88_60", startTime, endTime, model.Down_88_60, "88.6%");

                if (show_78_6 && !double.IsNaN(model.Down_78_6))
                    DrawFibLine(DownPrefix + "78_6", startTime, endTime, model.Down_78_6, "78.6%");

                if (show_61_8 && !double.IsNaN(model.Down_61_8))
                    DrawFibLine(DownPrefix + "61_8", startTime, endTime, model.Down_61_8, "61.8%");

                if (show_50 && !double.IsNaN(model.Down_50))
                    DrawFibLine(DownPrefix + "50", startTime, endTime, model.Down_50, "50%");

                if (show_38_2 && !double.IsNaN(model.Down_38_2))
                    DrawFibLine(DownPrefix + "38_2", startTime, endTime, model.Down_38_2, "38.2%");

                if (show_23_6 && !double.IsNaN(model.Down_23_6))
                    DrawFibLine(DownPrefix + "23_6", startTime, endTime, model.Down_23_6, "23.6%");

                if (show_11_40 && !double.IsNaN(model.Down_11_40))
                    DrawFibLine(DownPrefix + "11_40", startTime, endTime, model.Down_11_40, "11.4%");

                // 0% level uses HighLineColor and IB line styling
                if (!double.IsNaN(model.Down_0))
                    DrawAnchorLine(DownPrefix + "0", startTime, endTime, model.Down_0, "0%", _highLineColor);
            }
        }

        private void DrawFibLine(string objectName, DateTime startTime, DateTime endTime, 
            double price, string labelText)
        {
            string labelName = objectName + "_Label";

            // Draw line with Fib styling
            _chart.DrawTrendLine(objectName, startTime, price, endTime, price,
                _fibLineColor, _fibLineThickness, _fibLineStyle);

            // Draw label if enabled with center vertical alignment
            if (_showLabels)
            {
                var text = _chart.DrawText(labelName, labelText, endTime, price, _fibLineColor);
                text.VerticalAlignment = VerticalAlignment.Center;
                text.FontFamily = "Consolas";
                text.FontSize = 10;
            }
        }

        private void DrawAnchorLine(string objectName, DateTime startTime, DateTime endTime, 
            double price, string labelText, Color lineColor)
        {
            string labelName = objectName + "_Label";

            // Draw line with IB line styling and custom color
            _chart.DrawTrendLine(objectName, startTime, price, endTime, price,
                lineColor, _ibLineThickness, _ibLineStyle);

            // Draw label if enabled with center vertical alignment
            if (_showLabels)
            {
                var text = _chart.DrawText(labelName, labelText, endTime, price, lineColor);
                text.VerticalAlignment = VerticalAlignment.Center;
                text.FontFamily = "Consolas";
                text.FontSize = 10;
            }
        }

        public void Clear()
        {
            // Remove all upward projection lines
            string[] levels = { "0", "11_40", "23_6", "38_2", "50", "61_8", "78_6", "88_60", "100" };

            foreach (var level in levels)
            {
                // Clear upward
                _chart.RemoveObject(UpPrefix + level);
                _chart.RemoveObject(UpPrefix + level + "_Label");

                // Clear downward
                _chart.RemoveObject(DownPrefix + level);
                _chart.RemoveObject(DownPrefix + level + "_Label");
            }
        }
    }
}
