using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class TrendlineManager
    {
        private readonly Chart _chart;
        private readonly MovingAverageChannel _indicator;
        
        // NEW: Store MTF history for redrawing
        private List<MTFBarData> _mtfHistory = new List<MTFBarData>();
        private const int MAX_HISTORY = 100; // Keep last 100 MTF bars

        // Store references to drawn trendlines
        private Dictionary<string, ChartTrendLine> _activeTrendlines = new Dictionary<string, ChartTrendLine>();

        public TrendlineManager(Chart chart, MovingAverageChannel indicator)
        {
            _chart = chart;
            _indicator = indicator;
        }

        // Draw main trendlines when new MTF bar forms
        public void DrawMainTrendlines(DateTime currentTime, MAResult currentResult)
        {
            // Store this MTF bar in history
            _mtfHistory.Add(new MTFBarData(currentTime, currentResult));
            
            // Keep only recent history
            if (_mtfHistory.Count > MAX_HISTORY)
            {
                _mtfHistory.RemoveAt(0);
            }

            // Need at least 2 bars to draw a line
            if (_mtfHistory.Count < 2)
                return;

            try
            {
                // Get last two bars
                var previousBar = _mtfHistory[_mtfHistory.Count - 2];
                var currentBar = _mtfHistory[_mtfHistory.Count - 1];

                // Draw lines between previous and current
                DrawAllTrendlines(previousBar.Time, previousBar.Result, 
                                currentBar.Time, currentBar.Result);
            }
            catch (Exception)
            {
                // If drawing fails, continue silently
            }
        }

        // Update properties of existing trendlines (called every Calculate)
        public void RefreshTrendlineProperties()
        {
            try
            {
                // Check if we need to rebuild everything
                if (ShouldRebuildTrendlines())
                {
                    RebuildAllTrendlines();
                    return;
                }

                // Otherwise just update properties
                UpdateLineProperties("AMA_High", _indicator.HighLine.LineOutput);
                UpdateLineProperties("AMA_Low", _indicator.LowLine.LineOutput);
                UpdateLineProperties("AMA_Close", _indicator.CloseLine.LineOutput);
                UpdateLineProperties("AMA_Open", _indicator.OpenLine.LineOutput);
                UpdateLineProperties("AMA_Median", _indicator.MedianLine.LineOutput);
                UpdateLineProperties("AMA_LowerReversion", _indicator.LowerReversionZone.LineOutput);
                UpdateLineProperties("AMA_UpperReversion", _indicator.UpperReversionZone.LineOutput);
            }
            catch (Exception) { }
        }

        // Check if we need to rebuild trendlines (visibility changed)
        private bool ShouldRebuildTrendlines()
        {
            // Check High line
            bool hasHighTrendlines = HasTrendlines("AMA_High");
            if (hasHighTrendlines != _indicator.HighLine.LineOutput.IsVisible)
                return true;

            // Check Low line
            bool hasLowTrendlines = HasTrendlines("AMA_Low");
            if (hasLowTrendlines != _indicator.LowLine.LineOutput.IsVisible)
                return true;

            // Check Close line
            bool hasCloseTrendlines = HasTrendlines("AMA_Close");
            if (hasCloseTrendlines != _indicator.CloseLine.LineOutput.IsVisible)
                return true;

            // Check Open line
            bool hasOpenTrendlines = HasTrendlines("AMA_Open");
            if (hasOpenTrendlines != _indicator.OpenLine.LineOutput.IsVisible)
                return true;

            // Check Median line
            bool hasMedianTrendlines = HasTrendlines("AMA_Median");
            if (hasMedianTrendlines != _indicator.MedianLine.LineOutput.IsVisible)
                return true;

            // Check Lower Reversion
            bool hasLowerRevTrendlines = HasTrendlines("AMA_LowerReversion");
            if (hasLowerRevTrendlines != _indicator.LowerReversionZone.LineOutput.IsVisible)
                return true;

            // Check Upper Reversion
            bool hasUpperRevTrendlines = HasTrendlines("AMA_UpperReversion");
            if (hasUpperRevTrendlines != _indicator.UpperReversionZone.LineOutput.IsVisible)
                return true;

            return false;
        }

        // Check if trendlines exist for a line type
        private bool HasTrendlines(string baseName)
        {
            foreach (var key in _activeTrendlines.Keys)
            {
                if (key.StartsWith(baseName + "_"))
                    return true;
            }
            return false;
        }

        // Rebuild all trendlines from history
        private void RebuildAllTrendlines()
        {
            // Clear all existing trendlines
            ClearAllTrendlines();

            // Need at least 2 bars
            if (_mtfHistory.Count < 2)
                return;

            // Redraw all segments from history
            for (int i = 1; i < _mtfHistory.Count; i++)
            {
                var previousBar = _mtfHistory[i - 1];
                var currentBar = _mtfHistory[i];

                DrawAllTrendlines(previousBar.Time, previousBar.Result,
                                currentBar.Time, currentBar.Result);
            }
        }

        // Clear all trendlines
        private void ClearAllTrendlines()
        {
            foreach (var key in new List<string>(_activeTrendlines.Keys))
            {
                try
                {
                    _chart.RemoveObject(key);
                }
                catch { }
            }
            _activeTrendlines.Clear();
        }

        private void UpdateLineProperties(string baseName, IndicatorLineOutput output)
        {
            // Find all trendlines with this base name
            foreach (var key in _activeTrendlines.Keys)
            {
                if (key.StartsWith(baseName + "_"))
                {
                    if (_activeTrendlines.TryGetValue(key, out ChartTrendLine line))
                    {
                        if (line != null)
                        {
                            line.Color = output.Color;
                            line.LineStyle = output.LineStyle;
                            line.Thickness = (int)output.Thickness;
                        }
                    }
                }
            }
        }

        // Draw all 7 trendlines between two MTF bars
        private void DrawAllTrendlines(DateTime time1, MAResult result1, 
                                      DateTime time2, MAResult result2)
        {
            // High Line
            if (_indicator.HighLine.LineOutput.IsVisible)
            {
                DrawSingleTrendline("AMA_High", time1, result1.HighMA, 
                                  time2, result2.HighMA, 
                                  _indicator.HighLine.LineOutput.Color,
                                  _indicator.HighLine.LineOutput.LineStyle,
                                  _indicator.HighLine.LineOutput.Thickness);
            }

            // Low Line
            if (_indicator.LowLine.LineOutput.IsVisible)
            {
                DrawSingleTrendline("AMA_Low", time1, result1.LowMA, 
                                  time2, result2.LowMA, 
                                  _indicator.LowLine.LineOutput.Color,
                                  _indicator.LowLine.LineOutput.LineStyle,
                                  _indicator.LowLine.LineOutput.Thickness);
            }

            // Close Line
            if (_indicator.CloseLine.LineOutput.IsVisible)
            {
                DrawSingleTrendline("AMA_Close", time1, result1.CloseMA, 
                                  time2, result2.CloseMA, 
                                  _indicator.CloseLine.LineOutput.Color,
                                  _indicator.CloseLine.LineOutput.LineStyle,
                                  _indicator.CloseLine.LineOutput.Thickness);
            }

            // Open Line
            if (_indicator.OpenLine.LineOutput.IsVisible)
            {
                DrawSingleTrendline("AMA_Open", time1, result1.OpenMA, 
                                  time2, result2.OpenMA, 
                                  _indicator.OpenLine.LineOutput.Color,
                                  _indicator.OpenLine.LineOutput.LineStyle,
                                  _indicator.OpenLine.LineOutput.Thickness);
            }

            // Median Line
            if (_indicator.MedianLine.LineOutput.IsVisible)
            {
                DrawSingleTrendline("AMA_Median", time1, result1.MedianMA, 
                                  time2, result2.MedianMA, 
                                  _indicator.MedianLine.LineOutput.Color,
                                  _indicator.MedianLine.LineOutput.LineStyle,
                                  _indicator.MedianLine.LineOutput.Thickness);
            }

            // Lower Reversion Zone
            if (_indicator.LowerReversionZone.LineOutput.IsVisible)
            {
                DrawSingleTrendline("AMA_LowerReversion", time1, result1.Fib382MA, 
                                  time2, result2.Fib382MA, 
                                  _indicator.LowerReversionZone.LineOutput.Color,
                                  _indicator.LowerReversionZone.LineOutput.LineStyle,
                                  _indicator.LowerReversionZone.LineOutput.Thickness);
            }

            // Upper Reversion Zone
            if (_indicator.UpperReversionZone.LineOutput.IsVisible)
            {
                DrawSingleTrendline("AMA_UpperReversion", time1, result1.Fib618MA, 
                                  time2, result2.Fib618MA, 
                                  _indicator.UpperReversionZone.LineOutput.Color,
                                  _indicator.UpperReversionZone.LineOutput.LineStyle,
                                  _indicator.UpperReversionZone.LineOutput.Thickness);
            }
        }

        // Draw one trendline
        private void DrawSingleTrendline(string baseName, DateTime time1, double y1, 
                                       DateTime time2, double y2, Color color, 
                                       LineStyle lineStyle, float thickness)
        {
            // Create unique name with timestamp
            string trendlineName = baseName + "_" + time2.Ticks;

            // Draw the trendline
            var trendLine = _chart.DrawTrendLine(trendlineName, time1, y1, time2, y2, color);
            
            // Set line properties
            if (trendLine != null)
            {
                trendLine.LineStyle = lineStyle;
                trendLine.Thickness = (int)thickness;

                // Store reference
                _activeTrendlines[trendlineName] = trendLine;
            }
        }

        // Helper class to store MTF bar data
        private class MTFBarData
        {
            public DateTime Time { get; }
            public MAResult Result { get; }

            public MTFBarData(DateTime time, MAResult result)
            {
                Time = time;
                Result = result;
            }
        }
    }
}
