using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class MAView
    {
        // The 3 managers - each has one job
        private readonly OutputSeriesManager _outputManager;
        private readonly TrendlineManager _trendlineManager;
        private readonly ProjectionManager _projectionManager;

        // Settings and state
        private readonly MAParameters _parameters;
        private bool _isInitialized = false;
        private TimeFrame _currentTimeframe;
        private TimeFrame _selectedTimeframe;

        // Constructor - Now 7 output lines (added Median)
        public MAView(IndicatorDataSeries highLine, IndicatorDataSeries lowLine,
                      IndicatorDataSeries closeLine, IndicatorDataSeries openLine,
                      IndicatorDataSeries medianLine,
                      IndicatorDataSeries lowerReversionZone, IndicatorDataSeries upperReversionZone,
                      Chart chart, MAParameters parameters, MovingAverageChannel indicator,
                      DataManager dataManager)
        {
            _parameters = parameters;

            // Create the 3 managers - pass 7 lines now (added Median)
            _outputManager = new OutputSeriesManager(highLine, lowLine, closeLine, openLine,
                                                   medianLine, lowerReversionZone, upperReversionZone);

            _trendlineManager = new TrendlineManager(chart, indicator);

            _projectionManager = new ProjectionManager(chart, indicator, dataManager);
        }

        public void Initialize(Chart chart, TimeFrame currentTimeframe, TimeFrame selectedTimeframe)
        {
            _currentTimeframe = currentTimeframe;
            _selectedTimeframe = selectedTimeframe;
            _isInitialized = true;
        }

        // Main method - decides what to do
        public void UpdateValues(int index, DateTime currentTime, MAResult result)
        {
            // Check what to show
            if (ShouldUseTrendlines())
            {
                // Using trendlines - hide output lines
                _outputManager.HideOutputLines(index);

                // Draw main trendlines when new MTF bar forms
                if (result.IsNewMTFBar)
                {
                    _trendlineManager.DrawMainTrendlines(currentTime, result);
                }

                // NEW: Refresh trendline properties every Calculate (picks up color/style changes)
                _trendlineManager.RefreshTrendlineProperties();

                // Draw projections when current MTF bar updates
                if (!result.IsNewMTFBar && _parameters.ShowProjections)
                {
                    _projectionManager.DrawProjectionLines();
                }
            }
            else
            {
                // Not using trendlines - show output lines
                _outputManager.UpdateOutputLines(index, result);
            }
        }

        private bool ShouldUseTrendlines()
        {
            if (!_isInitialized)
                return false;

            // Use trendlines only if:
            // 1. User selected TrendLines (not StairSteps)
            // 2. Multi-timeframe is enabled
            // 3. Current timeframe is lower than selected timeframe
            return _parameters.UseTrendlines() &&
                   _parameters.EnableMultiTimeframe &&
                   IsCurrentTimeframeLower();
        }

        // Check if current timeframe is smaller than selected timeframe
        // Handles non-time-based timeframes correctly
        private bool IsCurrentTimeframeLower()
        {
            // If current chart is non-time-based (Tick/Renko/Range),
            // always treat it as lower than any time-based selected timeframe
            if (_currentTimeframe.IsNonTimeBased())
            {
                // Non-time-based chart is always "lower" for trendline purposes
                return true;
            }

            // For time-based charts, compare normally
            var selectedMinutes = _selectedTimeframe.ToTimeSpan().TotalMinutes;
            var currentMinutes = _currentTimeframe.ToTimeSpan().TotalMinutes;

            return currentMinutes < selectedMinutes;
        }
    }
}
