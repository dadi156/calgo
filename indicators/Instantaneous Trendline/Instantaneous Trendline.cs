using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class InstantaneousTrendline : Indicator
    {
        [Parameter("Alpha", DefaultValue = 0.07, MinValue = 0.01, MaxValue = 1.0, Group = "Instantaneous Trendline")]
        public double Alpha { get; set; }

        [Parameter("Enable", DefaultValue = false, Group = "Multi-timeframe Mode")]
        public bool EnableMTF { get; set; }

        [Parameter("Timeframe", DefaultValue = "Daily", Group = "Multi-timeframe Mode")]
        public TimeFrame MTFTimeframe { get; set; }

        [Parameter("Length", DefaultValue = 120, MinValue = 1, Group = "Lines Length")]
        public int MaxTrendlines { get; set; }

        [Output("ITrend Up", LineColor = "ForestGreen", LineStyle = LineStyle.Solid, Thickness = 2, PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries ITrendUp { get; set; }

        [Output("ITrend Down", LineColor = "Crimson", LineStyle = LineStyle.Solid, Thickness = 2, PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries ITrendDown { get; set; }

        [Output("Trigger", LineColor = "LightSteelBlue", PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries Trigger { get; set; }

        private IndicatorDataSeries _price;
        private IndicatorDataSeries _iTrend; // Internal calculation series

        // MTF fields
        private Bars _mtfBars;
        private IndicatorDataSeries _mtfPrice;
        private IndicatorDataSeries _mtfITrend; // Internal calculation series
        private IndicatorDataSeries _mtfTrigger;

        private int _lastMTFBarIndex = -1;
        private Dictionary<string, int> _trendlineBarIndices = new Dictionary<string, int>();
        private Dictionary<string, ChartTrendLine> _activeTrendlines = new Dictionary<string, ChartTrendLine>();
        private bool _useMTFMode = false;
        private List<MTFBarData> _mtfHistory = new List<MTFBarData>();
        private const int MAX_HISTORY = 100;

        protected override void Initialize()
        {
            _price = CreateDataSeries();
            _iTrend = CreateDataSeries();

            if (EnableMTF)
            {
                _mtfBars = MarketData.GetBars(MTFTimeframe);
                _mtfPrice = CreateDataSeries();
                _mtfITrend = CreateDataSeries();
                _mtfTrigger = CreateDataSeries();
            }
        }

        public override void Calculate(int index)
        {
            // Calculate native price
            _price[index] = (Bars.HighPrices[index] + Bars.LowPrices[index]) / 2.0;

            // Calculate native ITrend
            CalculateITrend(index, _price, _iTrend, Trigger);

            // Populate directional outputs
            if (Trigger[index] > _iTrend[index])
            {
                ITrendUp[index] = _iTrend[index];
                ITrendDown[index] = double.NaN;
            }
            else
            {
                ITrendUp[index] = double.NaN;
                ITrendDown[index] = _iTrend[index];
            }

            // Determine if MTF mode should be active
            bool useMTFMode = EnableMTF && _mtfBars != null;

            // Clean up trendlines if switching from MTF mode to native mode
            if (!useMTFMode && _useMTFMode && IsLastBar)
            {
                RemoveAllTrendlines();
                _useMTFMode = false;
            }
            else if (useMTFMode && !_useMTFMode)
            {
                _useMTFMode = true;
            }

            if (useMTFMode)
            {
                CalculateMTF(index);

                // Hide native outputs in MTF mode
                if (IsLastBar)
                {
                    for (int i = 0; i < Bars.Count; i++)
                    {
                        ITrendUp[i] = double.NaN;
                        ITrendDown[i] = double.NaN;
                        Trigger[i] = double.NaN;
                    }
                }
            }
            else
            {
                // Standard mode - show last MaxTrendlines bars
                int startIndex = Math.Max(0, Bars.Count - MaxTrendlines);

                if (index < startIndex)
                {
                    ITrendUp[index] = double.NaN;
                    ITrendDown[index] = double.NaN;
                    Trigger[index] = double.NaN;
                }

                // Hide old bars on last bar
                if (IsLastBar && Bars.Count > MaxTrendlines)
                {
                    for (int i = 0; i < startIndex; i++)
                    {
                        ITrendUp[i] = double.NaN;
                        ITrendDown[i] = double.NaN;
                        Trigger[i] = double.NaN;
                    }
                }
            }
        }

        private void CalculateITrend(int index, IndicatorDataSeries price, IndicatorDataSeries iTrend, IndicatorDataSeries trigger)
        {
            if (index < 7)
            {
                if (index < 2)
                {
                    iTrend[index] = price[index];
                }
                else
                {
                    iTrend[index] = (price[index] + 2 * price[index - 1] + price[index - 2]) / 4.0;
                }

                trigger[index] = iTrend[index];
                return;
            }

            double alpha2 = Alpha * Alpha;
            double alpha3 = alpha2 * Alpha;
            double oneMinusAlpha = 1.0 - Alpha;
            double oneMinusAlpha2 = oneMinusAlpha * oneMinusAlpha;

            iTrend[index] = (Alpha - alpha2 / 4.0) * price[index] +
                           0.5 * alpha2 * price[index - 1] -
                           (Alpha - 0.75 * alpha2) * price[index - 2] +
                           2.0 * oneMinusAlpha * iTrend[index - 1] -
                           oneMinusAlpha2 * iTrend[index - 2];

            if (index >= 2)
            {
                trigger[index] = 2.0 * iTrend[index] - iTrend[index - 2];
            }
            else
            {
                trigger[index] = iTrend[index];
            }
        }

        private void CalculateMTF(int index)
        {
            // Calculate ALL MTF bars up to current chart time
            DateTime currentTime = Bars.OpenTimes[index];
            int lastMtfIndex = -1;

            for (int i = 0; i < _mtfBars.Count; i++)
            {
                if (_mtfBars.OpenTimes[i] > currentTime)
                    break;

                lastMtfIndex = i;

                // Calculate this MTF bar if not already done
                if (double.IsNaN(_mtfITrend[i]))
                {
                    _mtfPrice[i] = (_mtfBars.HighPrices[i] + _mtfBars.LowPrices[i]) / 2.0;
                    CalculateITrend(i, _mtfPrice, _mtfITrend, _mtfTrigger);
                }

                // Add to history if new MTF bar
                if (i > _lastMTFBarIndex)
                {
                    var result = new MTFBarResult
                    {
                        ITrendUp = _mtfTrigger[i] > _mtfITrend[i] ? _mtfITrend[i] : double.NaN,
                        ITrendDown = _mtfTrigger[i] <= _mtfITrend[i] ? _mtfITrend[i] : double.NaN,
                        Trigger = _mtfTrigger[i]
                    };

                    _mtfHistory.Add(new MTFBarData(_mtfBars.OpenTimes[i], result));

                    while (_mtfHistory.Count > MaxTrendlines + 1)
                    {
                        _mtfHistory.RemoveAt(0);
                    }
                }
            }

            if (lastMtfIndex >= 0)
                _lastMTFBarIndex = lastMtfIndex;

            // Hide native outputs
            ITrendUp[index] = double.NaN;
            ITrendDown[index] = double.NaN;
            Trigger[index] = double.NaN;

            RefreshTrendlineProperties();
            CleanupOldTrendlines();
            DrawAllHistoricalTrendlines();
        }

        private void DrawMTFTrendlines(DateTime time1, MTFBarResult result1, DateTime time2, MTFBarResult result2)
        {
            if (ITrendUp.LineOutput.IsVisible && !double.IsNaN(result1.ITrendUp) && !double.IsNaN(result2.ITrendUp))
                DrawTrendLine("ITrendUp", time2.Ticks, time1, result1.ITrendUp, time2, result2.ITrendUp, ITrendUp.LineOutput);

            if (ITrendDown.LineOutput.IsVisible && !double.IsNaN(result1.ITrendDown) && !double.IsNaN(result2.ITrendDown))
                DrawTrendLine("ITrendDown", time2.Ticks, time1, result1.ITrendDown, time2, result2.ITrendDown, ITrendDown.LineOutput);

            if (Trigger.LineOutput.IsVisible)
                DrawTrendLine("Trigger", time2.Ticks, time1, result1.Trigger, time2, result2.Trigger, Trigger.LineOutput);
        }

        private void DrawTrendLine(string lineName, long barId, DateTime time1, double y1, DateTime time2, double y2, IndicatorLineOutput output)
        {
            string name = $"ITL_{lineName}_{barId}";
            _trendlineBarIndices[name] = (int)(barId / 10000000);

            var line = Chart.DrawTrendLine(name, time1, y1, time2, y2, output.Color, (int)output.Thickness, output.LineStyle);
            if (line != null)
            {
                line.IsInteractive = false;
                _activeTrendlines[name] = line;
            }
        }

        private void RefreshTrendlineProperties()
        {
            try
            {
                if (ShouldRebuildTrendlines())
                {
                    RebuildAllTrendlines();
                    return;
                }

                UpdateLineProperties("ITrendUp", ITrendUp.LineOutput);
                UpdateLineProperties("ITrendDown", ITrendDown.LineOutput);
                UpdateLineProperties("Trigger", Trigger.LineOutput);
            }
            catch { }
        }

        private bool ShouldRebuildTrendlines()
        {
            if (HasTrendlines("ITrendUp") != ITrendUp.LineOutput.IsVisible)
                return true;
            if (HasTrendlines("ITrendDown") != ITrendDown.LineOutput.IsVisible)
                return true;
            if (HasTrendlines("Trigger") != Trigger.LineOutput.IsVisible)
                return true;

            return false;
        }

        private bool HasTrendlines(string lineName)
        {
            foreach (var key in _activeTrendlines.Keys)
            {
                if (key.Contains($"_{lineName}_"))
                    return true;
            }
            return false;
        }

        private void RebuildAllTrendlines()
        {
            ClearAllTrendlines();

            if (_mtfHistory.Count < 2)
                return;

            int startIndex = Math.Max(1, _mtfHistory.Count - MaxTrendlines);

            for (int i = startIndex; i < _mtfHistory.Count; i++)
            {
                var previousBar = _mtfHistory[i - 1];
                var currentBar = _mtfHistory[i];

                DrawMTFTrendlines(previousBar.Time, previousBar.Result,
                                currentBar.Time, currentBar.Result);
            }
        }

        private void ClearAllTrendlines()
        {
            foreach (var key in new List<string>(_activeTrendlines.Keys))
            {
                try
                {
                    Chart.RemoveObject(key);
                }
                catch { }
            }
            _activeTrendlines.Clear();
            _trendlineBarIndices.Clear();
        }

        private void UpdateLineProperties(string lineName, IndicatorLineOutput output)
        {
            foreach (var key in _activeTrendlines.Keys)
            {
                if (key.Contains($"_{lineName}_"))
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

        private void CleanupOldTrendlines()
        {
            if (_mtfHistory.Count <= MaxTrendlines)
                return;

            int oldestBarToKeep = _mtfHistory.Count - MaxTrendlines;
            var oldestTimeToKeep = _mtfHistory[oldestBarToKeep].Time;

            var toRemove = new List<string>();
            foreach (var key in _activeTrendlines.Keys)
            {
                var parts = key.Split('_');
                if (parts.Length >= 3)
                {
                    if (long.TryParse(parts[2], out long ticks))
                    {
                        var lineTime = new DateTime(ticks);
                        if (lineTime < oldestTimeToKeep)
                        {
                            toRemove.Add(key);
                        }
                    }
                }
            }

            foreach (var key in toRemove)
            {
                try
                {
                    Chart.RemoveObject(key);
                    _activeTrendlines.Remove(key);
                    _trendlineBarIndices.Remove(key);
                }
                catch { }
            }
        }

        private void DrawAllHistoricalTrendlines()
        {
            if (_mtfHistory.Count < 2)
                return;

            int startIndex = Math.Max(1, _mtfHistory.Count - MaxTrendlines);

            for (int i = startIndex; i < _mtfHistory.Count; i++)
            {
                var previousBar = _mtfHistory[i - 1];
                var currentBar = _mtfHistory[i];

                DrawMTFTrendlines(previousBar.Time, previousBar.Result,
                                currentBar.Time, currentBar.Result);
            }
        }

        private void RemoveAllTrendlines()
        {
            foreach (var name in _trendlineBarIndices.Keys.ToList())
            {
                Chart.RemoveObject(name);
            }
            _trendlineBarIndices.Clear();
            _lastMTFBarIndex = -1;
        }

        private bool IsLowerTimeframe(TimeFrame current, TimeFrame reference)
        {
            return GetTimeframeMinutes(current) < GetTimeframeMinutes(reference);
        }

        private int GetTimeframeMinutes(TimeFrame tf)
        {
            if (tf == TimeFrame.Minute) return 1;
            if (tf == TimeFrame.Minute2) return 2;
            if (tf == TimeFrame.Minute3) return 3;
            if (tf == TimeFrame.Minute4) return 4;
            if (tf == TimeFrame.Minute5) return 5;
            if (tf == TimeFrame.Minute6) return 6;
            if (tf == TimeFrame.Minute7) return 7;
            if (tf == TimeFrame.Minute8) return 8;
            if (tf == TimeFrame.Minute9) return 9;
            if (tf == TimeFrame.Minute10) return 10;
            if (tf == TimeFrame.Minute15) return 15;
            if (tf == TimeFrame.Minute20) return 20;
            if (tf == TimeFrame.Minute30) return 30;
            if (tf == TimeFrame.Minute45) return 45;
            if (tf == TimeFrame.Hour) return 60;
            if (tf == TimeFrame.Hour2) return 120;
            if (tf == TimeFrame.Hour3) return 180;
            if (tf == TimeFrame.Hour4) return 240;
            if (tf == TimeFrame.Hour6) return 360;
            if (tf == TimeFrame.Hour8) return 480;
            if (tf == TimeFrame.Hour12) return 720;
            if (tf == TimeFrame.Daily) return 1440;
            if (tf == TimeFrame.Day2) return 2880;
            if (tf == TimeFrame.Day3) return 4320;
            if (tf == TimeFrame.Weekly) return 10080;
            if (tf == TimeFrame.Monthly) return 43200;

            return int.MaxValue;
        }

        private class MTFBarData
        {
            public DateTime Time { get; }
            public MTFBarResult Result { get; }

            public MTFBarData(DateTime time, MTFBarResult result)
            {
                Time = time;
                Result = result;
            }
        }

        private class MTFBarResult
        {
            public double ITrendUp { get; set; }
            public double ITrendDown { get; set; }
            public double Trigger { get; set; }
        }
    }
}
