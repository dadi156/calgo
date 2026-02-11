using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class RegressionView
    {
        private Chart _chart;
        private ChartTrendLine _midLine;
        private ChartTrendLine _upperLine;
        private ChartTrendLine _lowerLine;
        private Symbol _symbol;

        private PriceType _priceType = PriceType.Median;
        private bool _extendToInfinity = false; // Flag for extending lines

        // Colors for the regression channel
        private Color _regressionLineColor = Color.Yellow;
        private Color _upperLineColor = Color.Red;
        private Color _lowerLineColor = Color.Green;
        private Color _fibLinesColor = Color.MediumBlue;

        // Fibonacci lines - UPDATED: Added 11.4% and 88.6%
        private ChartTrendLine[] _fibLines;
        private readonly double[] _fibLevels = new double[] { 0.114, 0.236, 0.382, 0.618, 0.786, 0.886 };
        private bool[] _showFibLevels = new bool[] { true, true, true, true, true, true };

        public RegressionView(Chart chart, Symbol symbol)
        {
            _chart = chart;
            _symbol = symbol;
        }

        public void DrawRegressionChannel(RegressionChannelData channelData)
        {
            if (channelData == null)
                return;

            ClearLines();

            // Draw mid regression line
            _midLine = _chart.DrawTrendLine(
                "RegressionMidLine",
                channelData.MidLineStart.Time,
                channelData.MidLineStart.Price,
                channelData.MidLineEnd.Time,
                channelData.MidLineEnd.Price,
                _regressionLineColor);

            // Draw upper channel line
            _upperLine = _chart.DrawTrendLine(
                "RegressionUpperLine",
                channelData.UpperLineStart.Time,
                channelData.UpperLineStart.Price,
                channelData.UpperLineEnd.Time,
                channelData.UpperLineEnd.Price,
                _upperLineColor);

            // Draw lower channel line
            _lowerLine = _chart.DrawTrendLine(
                "RegressionLowerLine",
                channelData.LowerLineStart.Time,
                channelData.LowerLineStart.Price,
                channelData.LowerLineEnd.Time,
                channelData.LowerLineEnd.Price,
                _lowerLineColor);

            // Style the lines
            StyleLines();

            // Draw Fibonacci lines if any are enabled
            if (ShouldDrawFibonacciLines())
            {
                DrawFibonacciLines(channelData);
            }
        }

        private void DrawFibonacciLines(RegressionChannelData channelData)
        {
            // Initialize array for fibonacci lines if needed
            if (_fibLines == null || _fibLines.Length != _fibLevels.Length)
            {
                _fibLines = new ChartTrendLine[_fibLevels.Length];
            }

            // Calculate the total distance between lower and upper lines
            double startChannelHeight = channelData.UpperLineStart.Price - channelData.LowerLineStart.Price;
            double endChannelHeight = channelData.UpperLineEnd.Price - channelData.LowerLineEnd.Price;

            // Draw Fibonacci lines for each level that is enabled
            for (int i = 0; i < _fibLevels.Length; i++)
            {
                // Skip this level if it's not enabled
                if (!_showFibLevels[i])
                    continue;

                double level = _fibLevels[i];

                // Calculate Fibonacci line start and end points (from lower line)
                double fibStartPrice = channelData.LowerLineStart.Price + (startChannelHeight * level);
                double fibEndPrice = channelData.LowerLineEnd.Price + (endChannelHeight * level);

                // Create color for fibonacci line
                Color fibColor = GetFibonacciColor(level);

                // Draw fib line
                _fibLines[i] = _chart.DrawTrendLine(
                    $"RegressionFib{i}",
                    channelData.LowerLineStart.Time,
                    fibStartPrice,
                    channelData.LowerLineEnd.Time,
                    fibEndPrice,
                    fibColor);

                // Style the line
                _fibLines[i].Thickness = 1;
                _fibLines[i].LineStyle = LineStyle.Solid;

                // Apply ExtendToInfinity setting to Fibonacci lines too
                if (_extendToInfinity)
                {
                    _fibLines[i].ExtendToInfinity = true;
                }
            }
        }

        private bool ShouldDrawFibonacciLines()
        {
            // Return true if any fibonacci level is enabled
            for (int i = 0; i < _showFibLevels.Length; i++)
            {
                if (_showFibLevels[i])
                    return true;
            }
            return false;
        }

        private Color GetFibonacciColor(double level)
        {
            // All Fibonacci levels use the same color
            return _fibLinesColor;
        }

        private void StyleLines()
        {
            if (_midLine != null)
            {
                _midLine.Thickness = 1;
                _midLine.LineStyle = LineStyle.Solid;
                // Apply ExtendToInfinity setting if enabled
                if (_extendToInfinity)
                {
                    _midLine.ExtendToInfinity = true;
                }
            }

            if (_upperLine != null)
            {
                _upperLine.Thickness = 1;
                _upperLine.LineStyle = LineStyle.Solid;
                // Apply ExtendToInfinity setting if enabled
                if (_extendToInfinity)
                {
                    _upperLine.ExtendToInfinity = true;
                }
            }

            if (_lowerLine != null)
            {
                _lowerLine.Thickness = 1;
                _lowerLine.LineStyle = LineStyle.Solid;
                // Apply ExtendToInfinity setting if enabled
                if (_extendToInfinity)
                {
                    _lowerLine.ExtendToInfinity = true;
                }
            }
        }

        public void ClearLines()
        {
            // Remove existing trend lines if they exist
            if (_midLine != null)
            {
                _chart.RemoveObject(_midLine.Name);
                _midLine = null;
            }

            if (_upperLine != null)
            {
                _chart.RemoveObject(_upperLine.Name);
                _upperLine = null;
            }

            if (_lowerLine != null)
            {
                _chart.RemoveObject(_lowerLine.Name);
                _lowerLine = null;
            }

            // Clear fibonacci lines
            ClearFibonacciLines();
        }

        private void ClearFibonacciLines()
        {
            // Remove fibonacci lines if they exist
            if (_fibLines != null)
            {
                for (int i = 0; i < _fibLines.Length; i++)
                {
                    if (_fibLines[i] != null)
                    {
                        _chart.RemoveObject(_fibLines[i].Name);
                        _fibLines[i] = null;
                    }
                }
            }
        }

        public void SetChannelColors(Color regressionLineColor, Color upperLineColor, Color lowerLineColor)
        {
            _regressionLineColor = regressionLineColor;
            _upperLineColor = upperLineColor;
            _lowerLineColor = lowerLineColor;
        }

        public void SetPriceType(PriceType priceType)
        {
            _priceType = priceType;
        }

        // UPDATED: Added level114 and level886 parameters
        public void SetFibonacciLevels(bool level114, bool level236, bool level382, bool level618, bool level786, bool level886)
        {
            _showFibLevels[0] = level114;  // 11.4%
            _showFibLevels[1] = level236;  // 23.6%
            _showFibLevels[2] = level382;  // 38.2%
            _showFibLevels[3] = level618;  // 61.8%
            _showFibLevels[4] = level786;  // 78.6%
            _showFibLevels[5] = level886;  // 88.6%
        }

        public void SetFibonacciColor(Color fibLinesColor)
        {
            _fibLinesColor = fibLinesColor;
        }

        // Method to set the ExtendToInfinity property
        public void SetExtendToInfinity(bool extend)
        {
            _extendToInfinity = extend;
        }
    }
}
