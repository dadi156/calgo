using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Controller class for VWAP indicator - coordinates between model and view
    /// Optimized for performance with lazy calculation and efficient band updates
    /// </summary>
    public class VwapController
    {
        private readonly VwapModel _model;
        private readonly VwapView _view;
        private readonly DataSeries _source;
        private readonly Bars _bars;

        private int _lastProcessedIndex = -1;
        private bool _needsFullRecalculation = false;

        // Constants for optimization
        private const int DEFAULT_RECENT_BARS = 500;

        public VwapController(
            Bars bars,
            DataSeries source,
            IndicatorDataSeries vwapSeries,
            IndicatorDataSeries upperBandSeries,
            IndicatorDataSeries lowerBandSeries,
            IndicatorDataSeries fiboLevel886Series,
            IndicatorDataSeries fiboLevel764Series,
            IndicatorDataSeries fiboLevel628Series,
            IndicatorDataSeries fiboLevel382Series,
            IndicatorDataSeries fiboLevel236Series,
            IndicatorDataSeries fiboLevel114Series,
            Chart chart,
            VwapResetPeriod resetPeriod,
            VwapBandType bandType,
            double stdDevMultiplier,
            int pivotDepth,
            bool showFibo114,
            bool showFibo236,
            bool showFibo382,
            bool showFibo628,
            bool showFibo764,
            bool showFibo886,
            bool showUpperBand,
            bool showLowerBand,
            DateTime? anchorPoint = null)
        {
            _bars = bars;
            _source = source;

            // Initialize model and view
            _model = new VwapModel(
                bars,
                source,
                resetPeriod,
                stdDevMultiplier,
                bandType,
                pivotDepth,
                showUpperBand,
                showLowerBand,
                anchorPoint);

            _view = new VwapView(
                chart,
                bars,
                vwapSeries,
                upperBandSeries,
                lowerBandSeries,
                fiboLevel886Series,
                fiboLevel764Series,
                fiboLevel628Series,
                fiboLevel382Series,
                fiboLevel236Series,
                fiboLevel114Series,
                bandType,
                showFibo114,
                showFibo236,
                showFibo382,
                showFibo628,
                showFibo764,
                showFibo886,
                showUpperBand,
                showLowerBand);
        }

        /// <summary>
        /// Initialize the indicator by processing historical data
        /// </summary>
        public void Initialize()
        {
            ProcessHistoricalData();
            _lastProcessedIndex = _bars.Count - 2;
        }

        /// <summary>
        /// Process historical data in an optimized way
        /// </summary>
        private void ProcessHistoricalData()
        {
            int batchSize = 100;

            for (int i = 0; i < _bars.Count - 1; i += batchSize)
            {
                int endBatch = Math.Min(i + batchSize, _bars.Count - 1);

                for (int j = i; j < endBatch; j++)
                {
                    ProcessBar(j);
                }
            }
        }

        /// <summary>
        /// Process a new calculation request
        /// </summary>
        public void Calculate(int index)
        {
            if (_needsFullRecalculation)
            {
                ProcessHistoricalData();
                _lastProcessedIndex = _bars.Count - 2;
                _needsFullRecalculation = false;
                return;
            }

            for (int i = _lastProcessedIndex + 1; i < _bars.Count - 1; i++)
            {
                ProcessBar(i);
                _lastProcessedIndex = i;
            }
        }

        /// <summary>
        /// Process a single bar
        /// </summary>
        private void ProcessBar(int index)
        {
            if (index == _bars.Count - 1)
                return;

            _model.ProcessBar(index);

            if (_bars.TickVolumes[index] == 0 && _bars.TimeFrame < TimeFrame.Weekly)
            {
                _view.CopyPreviousValues(index);
            }
            else
            {
                _view.UpdateSeries(
                    index,
                    _model.Vwap,
                    _model.UpperBand,
                    _model.LowerBand,
                    _model.FibLevel886,
                    _model.FibLevel764,
                    _model.FibLevel628,
                    _model.FibLevel382,
                    _model.FibLevel236,
                    _model.FibLevel114);
            }
        }

        /// <summary>
        /// Update the indicator configuration
        /// </summary>
        public void UpdateConfiguration(
            VwapResetPeriod resetPeriod,
            VwapBandType bandType,
            int pivotDepth,
            bool showUpperBand,
            bool showLowerBand,
            DateTime? anchorPoint = null)
        {
            _model.UpdateConfiguration(resetPeriod, bandType, pivotDepth, showUpperBand, showLowerBand, anchorPoint);

            bool bandMethodChanged = (_view.GetBandType() != bandType);

            if (bandMethodChanged)
            {
                _view.UpdateBandVisibility(bandType);
                _view.UpdateRecentBars(DEFAULT_RECENT_BARS);
            }

            bool needFullRecalc =
                resetPeriod != _model.GetResetPeriod() ||
                (anchorPoint != null && _model.GetAnchorPoint() != null && anchorPoint != _model.GetAnchorPoint());

            _needsFullRecalculation = needFullRecalc;
        }

        /// <summary>
        /// Update Fibonacci level visibility
        /// </summary>
        public void UpdateFiboLevelVisibility(
            bool showFibo114,
            bool showFibo236,
            bool showFibo382,
            bool showFibo628,
            bool showFibo764,
            bool showFibo886,
            bool showUpperBand,
            bool showLowerBand)
        {
            // Update visibility settings in the view
            _view.UpdateFiboLevelVisibility(
                showFibo114,
                showFibo236,
                showFibo382,
                showFibo628,
                showFibo764,
                showFibo886,
                showUpperBand,
                showLowerBand);

            // Update the most recent bars for immediate visual feedback
            _view.UpdateRecentBars(DEFAULT_RECENT_BARS);
        }

        /// <summary>
        /// Force a full recalculation of all data
        /// </summary>
        public void ForceFullRecalculation()
        {
            _model.Reset();
            ProcessHistoricalData();
            _lastProcessedIndex = _bars.Count - 2;
            _view.UpdateAllBars();
        }
    }
}
