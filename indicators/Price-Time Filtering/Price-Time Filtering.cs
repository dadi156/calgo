using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PriceTimeFiltering : Indicator
    {
        [Parameter("TimeFrame", DefaultValue = "Monthly", Group = "Filtering")]
        public TimeFrame SelectedTimeFrame { get; set; }

        [Parameter("Mode", DefaultValue = FilterMode.RealTime, Group = "Filtering")]
        public FilterMode Mode { get; set; }

        [Output("Up Trend", LineColor = "ForestGreen", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries UpSeries { get; set; }

        [Output("Down Trend", LineColor = "Crimson", PlotType = PlotType.Histogram, Thickness = 3)]
        public IndicatorDataSeries DownSeries { get; set; }

        private Bars _higherTFBars;
        private int _prevHigherIndex = -1;
        private int _lastBarIndex = -1;
        private int _trend;
        private int _prevTrend = 0;
        private int _numBarsUp;
        private int _numBarsDn;

        protected override void Initialize()
        {
            _higherTFBars = MarketData.GetBars(SelectedTimeFrame);

            _trend = 0;
            _numBarsUp = 0;
            _numBarsDn = 0;
        }

        public override void Calculate(int index)
        {
            int currentHigherIndex = _higherTFBars.OpenTimes.GetIndexByTime(Bars.OpenTimes[index]);

            if (currentHigherIndex < 1)
                return;

            if (Mode == FilterMode.PeriodEnd)
            {
                // Skip evaluation if still in same higher TF period
                if (currentHigherIndex == _prevHigherIndex)
                {
                    if (index > _lastBarIndex)
                    {
                        _lastBarIndex = index;
                        if (_trend == 1)
                            UpSeries[index] = ++_numBarsUp;
                        else if (_trend == -1)
                            DownSeries[index] = -(++_numBarsDn);
                    }
                    return;
                }

                // New period started - evaluate completed period
                if (_prevHigherIndex >= 1)
                {
                    double completedClose = _higherTFBars.ClosePrices[_prevHigherIndex];
                    double priorHigh = _higherTFBars.HighPrices[_prevHigherIndex - 1];
                    double priorLow = _higherTFBars.LowPrices[_prevHigherIndex - 1];

                    if (completedClose > priorHigh)
                    {
                        if (_trend != 1)
                        {
                            _trend = 1;
                            _numBarsUp = 0;
                            _numBarsDn = 0;
                        }
                    }
                    else if (completedClose < priorLow)
                    {
                        if (_trend != -1)
                        {
                            _trend = -1;
                            _numBarsUp = 0;
                            _numBarsDn = 0;
                        }
                    }
                }
                _prevHigherIndex = currentHigherIndex;
                _lastBarIndex = index - 1;  // Force isNewBar = true for first bar of new period
            }
            else // RealTime mode
            {
                // Only evaluate trend on first tick of new bar
                if (index > _lastBarIndex)
                {
                    double currentClose = Bars.ClosePrices[index];
                    double priorHigh = _higherTFBars.HighPrices[currentHigherIndex - 1];
                    double priorLow = _higherTFBars.LowPrices[currentHigherIndex - 1];

                    if (currentClose > priorHigh)
                    {
                        if (_trend != 1)
                        {
                            _trend = 1;
                            _numBarsUp = 0;
                            _numBarsDn = 0;
                        }
                    }
                    else if (currentClose < priorLow)
                    {
                        if (_trend != -1)
                        {
                            _trend = -1;
                            _numBarsUp = 0;
                            _numBarsDn = 0;
                        }
                    }
                }
            }

            // Count and plot
            bool isNewBar = (index > _lastBarIndex);
            bool trendChanged = (_trend != _prevTrend);

            if (_trend == 1)
            {
                if (isNewBar && !trendChanged)
                    _numBarsUp++;
                else if (trendChanged)
                    _numBarsUp = 1;

                UpSeries[index] = _numBarsUp;
            }
            else if (_trend == -1)
            {
                if (isNewBar && !trendChanged)
                    _numBarsDn++;
                else if (trendChanged)
                    _numBarsDn = 1;

                DownSeries[index] = -_numBarsDn;
            }

            if (isNewBar)
                _lastBarIndex = index;
            _prevTrend = _trend;
        }

    }

    public enum FilterMode
    {
        RealTime,
        PeriodEnd
    }
}
