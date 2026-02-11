using System;
using cAlgo.API;

namespace cAlgo
{
    public class IBHighLowTrendlines
    {
        private readonly Chart _chart;
        private readonly Color _highLineColor;
        private readonly Color _lowLineColor;
        private readonly int _lineThickness;
        private readonly LineStyle _lineStyle;
        private readonly bool _showLabels;

        private const string HighLineName = "IB_High";
        private const string LowLineName = "IB_Low";
        private const string HighLabelName = "IB_High_Label";
        private const string LowLabelName = "IB_Low_Label";

        public IBHighLowTrendlines(Chart chart, Color highLineColor, Color lowLineColor, 
            int thickness, LineStyle lineStyle, bool showLabels)
        {
            _chart = chart;
            _highLineColor = highLineColor;
            _lowLineColor = lowLineColor;
            _lineThickness = thickness;
            _lineStyle = lineStyle;
            _showLabels = showLabels;
        }

        public void DrawLines(DateTime startTime, DateTime endTime, double highPrice, double lowPrice)
        {
            // Remove old lines and labels
            _chart.RemoveObject(HighLineName);
            _chart.RemoveObject(LowLineName);
            _chart.RemoveObject(HighLabelName);
            _chart.RemoveObject(LowLabelName);

            // Draw new lines
            _chart.DrawTrendLine(HighLineName, startTime, highPrice, endTime, highPrice,
                _highLineColor, _lineThickness, _lineStyle);

            _chart.DrawTrendLine(LowLineName, startTime, lowPrice, endTime, lowPrice,
                _lowLineColor, _lineThickness, _lineStyle);

            // Draw labels with price if enabled
            if (_showLabels)
            {
                string highLabel = string.Format("IB High ({0})", highPrice.ToString("F5"));
                string lowLabel = string.Format("IB Low ({0})", lowPrice.ToString("F5"));

                var highText = _chart.DrawText(HighLabelName, highLabel, endTime, highPrice, _highLineColor);
                highText.VerticalAlignment = VerticalAlignment.Center;
                highText.FontFamily = "Consolas";
                highText.FontSize = 11;

                var lowText = _chart.DrawText(LowLabelName, lowLabel, endTime, lowPrice, _lowLineColor);
                lowText.VerticalAlignment = VerticalAlignment.Center;
                lowText.FontFamily = "Consolas";
                lowText.FontSize = 11;
            }
        }

        public void Clear()
        {
            _chart.RemoveObject(HighLineName);
            _chart.RemoveObject(LowLineName);
            _chart.RemoveObject(HighLabelName);
            _chart.RemoveObject(LowLabelName);
        }
    }
}
