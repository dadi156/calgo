using System;
using System.Globalization;
using cAlgo.API;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None, AutoRescale = false)]
    [Cloud("Fib. 61.80%", "Fib. 38.20%", Opacity = 0.2)]
    public partial class TrendChannelMovingAverage : Indicator
    {
        // Main components
        private MAHLModel _model;
        private MAHLView _view;
        private MAHLController _controller;

        // Fibonacci components
        private FibonacciLevelsCalculator _fibonacciCalculator;
        private FibonacciLevelsView _fibonacciView;
        private FibonacciLevelsController _fibonacciController;

        // Date and time functionality
        private DateTime _anchorDateTime;
        private bool _anchorDateInitialized;
        private DateTime _linesStartDateTime;
        private bool _linesStartPointEnabled;
        private bool _linesStartPointInitialized;

        // Performance optimization tracking
        private int _lastProcessedBarCount = 0;
        private int _lastCalculatedIndex = -1;

        // Bar detection for reload system
        private int _lastBarCount = 0;
        private DateTime _lastBarTime = DateTime.MinValue;
        private DateTime _lastReloadTime = DateTime.MinValue;
        private bool _isFirstRun = true;

        // Reload settings (constants)
        private const int RELOAD_BARS_COUNT = 60;
        private const int MIN_RELOAD_INTERVAL_SECONDS = 60;

        protected override void Initialize()
        {
            try
            {
                InitializeAnchorDate();
                InitializeLinesStartPoint();

                // Initialize main TrendChannelMovingAverage components
                // CHANGED: Now pass Source instead of SourcePrice
                _model = new MAHLModel(Bars, this, Period, MovingAverageType, Source,
                                      MultiTimeframeMode, BaseTimeframe, TrendAveragingPeriod);

                _view = new MAHLView(OpenLine, CloseLine, MedianLine,
                                   HighLineUptrend, HighLineDowntrend, HighLineNeutral,
                                   LowLineUptrend, LowLineDowntrend, LowLineNeutral);

                _controller = new MAHLController(_model, _view, this);

                // Initialize Fibonacci components
                _fibonacciCalculator = new FibonacciLevelsCalculator();

                _fibonacciView = new FibonacciLevelsView(Fib0000, Fib1140, Fib2360, Fib3820, Fib5000,
                                                        Fib6180, Fib7860, Fib8860, Fib10000);

                _fibonacciController = new FibonacciLevelsController(_fibonacciCalculator, _fibonacciView,
                                                                   _model, this);

                InitializeBarDetection();
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        /// <summary>
        /// Main calculation method with reload mechanism, bar colors, and fibonacci levels
        /// </summary>
        public override void Calculate(int index)
        {
            try
            {
                if (IsNewBarFormed() && ShouldTriggerReload())
                {
                    TriggerReload();
                    return;
                }

                if (_anchorDateInitialized && PeriodCalculationType == PeriodCalculationType.AnchorDate)
                {
                    DateTime currentBarTime = Bars.OpenTimes[index];

                    if (currentBarTime < _anchorDateTime)
                    {
                        SetAllLinesToNaN(index);
                        SetAllFibonacciLinesToNaN(index);
                        return;
                    }
                }

                int currentBarCount = Bars.Count;

                if (currentBarCount > _lastProcessedBarCount && _lastProcessedBarCount > 0)
                {
                    int previousBarIndex = _lastProcessedBarCount - 1;
                    if (previousBarIndex >= 0)
                    {
                        // Calculate main TrendChannelMovingAverage lines
                        _controller.Calculate(previousBarIndex);
                        _lastCalculatedIndex = previousBarIndex;

                        // Calculate Fibonacci levels
                        _fibonacciController.Calculate(previousBarIndex, FibonacciDisplayMode);

                        // Apply bar colors to previous bar
                        ApplyBarColors(previousBarIndex);
                    }
                }

                _lastProcessedBarCount = currentBarCount;

                if (ShouldCalculateIndex(index))
                {
                    // Calculate main TrendChannelMovingAverage lines
                    _controller.Calculate(index);
                    _lastCalculatedIndex = index;

                    // Calculate Fibonacci levels
                    _fibonacciController.Calculate(index, FibonacciDisplayMode);

                    // Apply bar colors to current bar
                    ApplyBarColors(index);
                }
            }
            catch (Exception)
            {
                SetAllLinesToNaN(index);
                SetAllFibonacciLinesToNaN(index);
            }
        }
    }
}
