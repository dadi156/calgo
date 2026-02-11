using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Handles rendering channel data to output series
    /// </summary>
    public class ChannelRenderer
    {
        private readonly OutputCollection _outputs;
        private readonly ChannelConfig _config;
        private readonly Regression _indicator;
        private bool _extendToInfinity = false;
        private Chart _chart;
        private List<ChartTrendLine> _trendLines = new List<ChartTrendLine>();
        private int _lastRenderMinIndex = -1;
        private int _lastRenderMaxIndex = -1;

        public ChannelRenderer(OutputCollection outputs, ChannelConfig config, Regression indicator, Chart chart = null)
        {
            _outputs = outputs ?? throw new ArgumentNullException(nameof(outputs));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _indicator = indicator ?? throw new ArgumentNullException(nameof(indicator));
            _chart = chart;
        }

        public void SetExtendToInfinity(bool extend)
        {
            if (_extendToInfinity != extend)
            {
                _extendToInfinity = extend;

                if (extend)
                {
                    for (int i = 0; i < _config.Bars.Count; i++)
                    {
                        Clear(i);
                    }
                }
                else
                {
                    ClearTrendLines();
                }
            }

            if (_trendLines.Count > 0)
            {
                foreach (var line in _trendLines)
                {
                    if (line != null)
                    {
                        line.ExtendToInfinity = extend;
                    }
                }
            }
        }

        public void Render(ChannelData channelData)
        {
            if (channelData == null)
                return;

            // Clear only previously rendered range instead of all bars
            if (_lastRenderMinIndex >= 0 && _lastRenderMaxIndex >= 0)
            {
                for (int i = _lastRenderMinIndex; i <= _lastRenderMaxIndex; i++)
                {
                    Clear(i);
                }
            }

            ClearTrendLines();

            if (_chart != null && _extendToInfinity)
            {
                DrawTrendLines(channelData);

                // Track rendered range for trendlines
                if (channelData.WindowLevels.Count > 0)
                {
                    _lastRenderMinIndex = int.MaxValue;
                    _lastRenderMaxIndex = int.MinValue;
                    foreach (int idx in channelData.WindowLevels.Keys)
                    {
                        if (idx < _lastRenderMinIndex) _lastRenderMinIndex = idx;
                        if (idx > _lastRenderMaxIndex) _lastRenderMaxIndex = idx;
                    }
                }
            }
            else
            {
                int minIndex = int.MaxValue;
                int maxIndex = int.MinValue;

                foreach (var entry in channelData.WindowLevels)
                {
                    int barIndex = entry.Key;
                    double[] levels = entry.Value;

                    if (barIndex < minIndex) minIndex = barIndex;
                    if (barIndex > maxIndex) maxIndex = barIndex;

                    _outputs.Fib100[barIndex] = levels[0];
                    _outputs.Level886[barIndex] = levels[1];
                    _outputs.Fib764[barIndex] = levels[2];
                    _outputs.Fib618[barIndex] = levels[3];
                    _outputs.Fib50[barIndex] = levels[4];
                    _outputs.Fib382[barIndex] = levels[5];
                    _outputs.Fib236[barIndex] = levels[6];
                    _outputs.Level114[barIndex] = levels[7];
                    _outputs.Fib0[barIndex] = levels[8];
                }

                // Track rendered range
                _lastRenderMinIndex = minIndex != int.MaxValue ? minIndex : -1;
                _lastRenderMaxIndex = maxIndex != int.MinValue ? maxIndex : -1;

                // Always clear forming bar
                if (_config.Bars.Count > 1)
                {
                    Clear(_config.Bars.Count - 1);
                }
            }
        }

        private void DrawLine(string name, DateTime startTime, double startPrice, DateTime endTime, double endPrice,
                            Color color, LineStyle lineStyle, float thickness)
        {
            if (_chart == null)
                return;

            try
            {
                var line = _chart.DrawTrendLine($"DSRegression_{name}", startTime, startPrice, endTime, endPrice, color);
                line.Thickness = (int)thickness;
                line.LineStyle = lineStyle;
                line.ExtendToInfinity = _extendToInfinity;
                _trendLines.Add(line);
            }
            catch (Exception)
            {
            }
        }

        private void ClearTrendLines()
        {
            if (_chart == null)
                return;

            foreach (var line in _trendLines)
            {
                try
                {
                    if (line != null)
                    {
                        _chart.RemoveObject(line.Name);
                    }
                }
                catch (Exception)
                {
                }
            }

            _trendLines.Clear();
        }

        private void DrawTrendLines(ChannelData channelData)
        {
            ClearTrendLines();

            int firstBarIndex = int.MaxValue;
            int lastBarIndex = int.MinValue;

            foreach (int barIndex in channelData.WindowLevels.Keys)
            {
                firstBarIndex = Math.Min(firstBarIndex, barIndex);
                lastBarIndex = Math.Max(lastBarIndex, barIndex);
            }

            if (firstBarIndex == int.MaxValue || lastBarIndex == int.MinValue)
                return;

            // Fetch levels once
            if (!channelData.WindowLevels.TryGetValue(firstBarIndex, out double[] firstLevels) ||
                !channelData.WindowLevels.TryGetValue(lastBarIndex, out double[] lastLevels))
                return;

            DateTime startTime = _config.Bars.OpenTimes[firstBarIndex];
            DateTime endTime = _config.Bars.OpenTimes[lastBarIndex];

            // 100%
            if (_indicator.Fib100.LineOutput.IsVisible)
            {
                DrawLine("Fib100", startTime, firstLevels[0], endTime, lastLevels[0],
                                _indicator.Fib100.LineOutput.Color,
                                _indicator.Fib100.LineOutput.LineStyle,
                                _indicator.Fib100.LineOutput.Thickness);
            }

            // 88.6%
            if (_indicator.Level886.LineOutput.IsVisible)
            {
                DrawLine("Fib886", startTime, firstLevels[1], endTime, lastLevels[1],
                                _indicator.Level886.LineOutput.Color,
                                _indicator.Level886.LineOutput.LineStyle,
                                _indicator.Level886.LineOutput.Thickness);
            }

            // 76.4%
            if (_indicator.Fib764.LineOutput.IsVisible)
            {
                DrawLine("Fib764", startTime, firstLevels[2], endTime, lastLevels[2],
                                _indicator.Fib764.LineOutput.Color,
                                _indicator.Fib764.LineOutput.LineStyle,
                                _indicator.Fib764.LineOutput.Thickness);
            }

            // 61.8%
            if (_indicator.Fib618.LineOutput.IsVisible)
            {
                DrawLine("Fib618", startTime, firstLevels[3], endTime, lastLevels[3],
                                _indicator.Fib618.LineOutput.Color,
                                _indicator.Fib618.LineOutput.LineStyle,
                                _indicator.Fib618.LineOutput.Thickness);
            }

            // 50%
            if (_indicator.Fib50.LineOutput.IsVisible)
            {
                DrawLine("Fib50", startTime, firstLevels[4], endTime, lastLevels[4],
                                _indicator.Fib50.LineOutput.Color,
                                _indicator.Fib50.LineOutput.LineStyle,
                                _indicator.Fib50.LineOutput.Thickness);
            }

            // 38.2%
            if (_indicator.Fib382.LineOutput.IsVisible)
            {
                DrawLine("Fib382", startTime, firstLevels[5], endTime, lastLevels[5],
                                _indicator.Fib382.LineOutput.Color,
                                _indicator.Fib382.LineOutput.LineStyle,
                                _indicator.Fib382.LineOutput.Thickness);
            }

            // 23.6%
            if (_indicator.Fib236.LineOutput.IsVisible)
            {
                DrawLine("Fib236", startTime, firstLevels[6], endTime, lastLevels[6],
                                _indicator.Fib236.LineOutput.Color,
                                _indicator.Fib236.LineOutput.LineStyle,
                                _indicator.Fib236.LineOutput.Thickness);
            }

            // 11.4%
            if (_indicator.Level114.LineOutput.IsVisible)
            {
                DrawLine("Fib114", startTime, firstLevels[7], endTime, lastLevels[7],
                                _indicator.Level114.LineOutput.Color,
                                _indicator.Level114.LineOutput.LineStyle,
                                _indicator.Level114.LineOutput.Thickness);
            }

            // 0%
            if (_indicator.Fib0.LineOutput.IsVisible)
            {
                DrawLine("Fib0", startTime, firstLevels[8], endTime, lastLevels[8],
                                _indicator.Fib0.LineOutput.Color,
                                _indicator.Fib0.LineOutput.LineStyle,
                                _indicator.Fib0.LineOutput.Thickness);
            }
        }

        public void Clear(int index)
        {
            _outputs.ClearValues(index);
        }

        public void ClearFutureValues(int fromIndex, int count)
        {
            for (int i = fromIndex; i < fromIndex + count; i++)
            {
                Clear(i);
            }
        }

        public void ClearValuesBefore(int beforeIndex)
        {
            for (int i = 0; i < beforeIndex; i++)
            {
                Clear(i);
            }
        }
    }
}
