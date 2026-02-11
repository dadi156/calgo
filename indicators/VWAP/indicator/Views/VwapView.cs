using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    /// <summary>
    /// View class for VWAP indicator - handles visual representation and output data series
    /// Optimized for efficient band visibility updates with group controls
    /// </summary>
    public class VwapView
    {
        private readonly Chart _chart;
        private readonly Bars _bars;
        private readonly IndicatorDataSeries _vwapSeries;
        private readonly IndicatorDataSeries _upperBandSeries;
        private readonly IndicatorDataSeries _lowerBandSeries;
        private readonly IndicatorDataSeries _fiboLevel886Series;
        private readonly IndicatorDataSeries _fiboLevel764Series;
        private readonly IndicatorDataSeries _fiboLevel628Series;
        private readonly IndicatorDataSeries _fiboLevel382Series;
        private readonly IndicatorDataSeries _fiboLevel236Series;
        private readonly IndicatorDataSeries _fiboLevel114Series;
        private VwapBandType _bandType;
        
        // Individual level visibility flags
        private bool _showFibo114;
        private bool _showFibo236;
        private bool _showFibo382;
        private bool _showFibo628;
        private bool _showFibo764;
        private bool _showFibo886;

        // Group visibility flags
        private bool _showUpperBand;
        private bool _showLowerBand;

        // Constants for optimization
        private const int DEFAULT_VISIBLE_BARS = 300;
        private const int UPDATE_BUFFER = 100;

        /// <summary>
        /// Gets the current band type
        /// </summary>
        public VwapBandType GetBandType()
        {
            return _bandType;
        }

        public VwapView(
            Chart chart,
            Bars bars,
            IndicatorDataSeries vwapSeries,
            IndicatorDataSeries upperBandSeries,
            IndicatorDataSeries lowerBandSeries,
            IndicatorDataSeries fiboLevel886Series,
            IndicatorDataSeries fiboLevel764Series,
            IndicatorDataSeries fiboLevel628Series,
            IndicatorDataSeries fiboLevel382Series,
            IndicatorDataSeries fiboLevel236Series,
            IndicatorDataSeries fiboLevel114Series,
            VwapBandType bandType,
            bool showFibo114,
            bool showFibo236,
            bool showFibo382,
            bool showFibo628,
            bool showFibo764,
            bool showFibo886,
            bool showUpperBand,
            bool showLowerBand)
        {
            _chart = chart;
            _bars = bars;
            _vwapSeries = vwapSeries;
            _upperBandSeries = upperBandSeries;
            _lowerBandSeries = lowerBandSeries;
            _fiboLevel886Series = fiboLevel886Series;
            _fiboLevel764Series = fiboLevel764Series;
            _fiboLevel628Series = fiboLevel628Series;
            _fiboLevel382Series = fiboLevel382Series;
            _fiboLevel236Series = fiboLevel236Series;
            _fiboLevel114Series = fiboLevel114Series;
            _bandType = bandType;
            
            // Individual level settings
            _showFibo114 = showFibo114;
            _showFibo236 = showFibo236;
            _showFibo382 = showFibo382;
            _showFibo628 = showFibo628;
            _showFibo764 = showFibo764;
            _showFibo886 = showFibo886;
            
            // Group settings
            _showUpperBand = showUpperBand;
            _showLowerBand = showLowerBand;
        }

        /// <summary>
        /// Helper method to determine if a line should be visible
        /// Combines individual level setting with group setting
        /// </summary>
        private bool ShouldShowLevel(bool individualSetting, bool isUpperLine)
        {
            // Combine individual setting with group setting using AND logic
            bool groupSetting = isUpperLine ? _showUpperBand : _showLowerBand;
            return individualSetting && groupSetting;
        }

        /// <summary>
        /// Update the indicator output series with the calculated values
        /// </summary>
        public void UpdateSeries(
            int index,
            double vwap,
            double upperBand,
            double lowerBand,
            double fiboLevel886,
            double fiboLevel764,
            double fiboLevel628,
            double fiboLevel382,
            double fiboLevel236,
            double fiboLevel114)
        {
            // Don't update series when NaN values are provided
            if (double.IsNaN(vwap))
                return;

            _vwapSeries[index] = vwap;

            if (_showUpperBand || _showLowerBand)
            {
                // Main bounds controlled by group visibility
                _upperBandSeries[index] = _showUpperBand ? upperBand : double.NaN;
                _lowerBandSeries[index] = _showLowerBand ? lowerBand : double.NaN;
                
                // Update Fibonacci levels using combined visibility logic
                _fiboLevel886Series[index] = ShouldShowLevel(_showFibo886, true) ? fiboLevel886 : double.NaN;
                _fiboLevel764Series[index] = ShouldShowLevel(_showFibo764, true) ? fiboLevel764 : double.NaN;
                _fiboLevel628Series[index] = ShouldShowLevel(_showFibo628, true) ? fiboLevel628 : double.NaN;
                
                _fiboLevel382Series[index] = ShouldShowLevel(_showFibo382, false) ? fiboLevel382 : double.NaN;
                _fiboLevel236Series[index] = ShouldShowLevel(_showFibo236, false) ? fiboLevel236 : double.NaN;
                _fiboLevel114Series[index] = ShouldShowLevel(_showFibo114, false) ? fiboLevel114 : double.NaN;
            }
            else
            {
                // When both groups are disabled, use NaN to hide all bands
                _upperBandSeries[index] = double.NaN;
                _lowerBandSeries[index] = double.NaN;
                _fiboLevel886Series[index] = double.NaN;
                _fiboLevel764Series[index] = double.NaN;
                _fiboLevel628Series[index] = double.NaN;
                _fiboLevel382Series[index] = double.NaN;
                _fiboLevel236Series[index] = double.NaN;
                _fiboLevel114Series[index] = double.NaN;
            }
        }

        /// <summary>
        /// Copy values from the previous bar when we have no calculation
        /// </summary>
        public void CopyPreviousValues(int index)
        {
            if (index > 0)
            {
                // Only copy if previous values are not NaN
                if (!double.IsNaN(_vwapSeries[index - 1]))
                {
                    _vwapSeries[index] = _vwapSeries[index - 1];

                    if (_showUpperBand || _showLowerBand)
                    {
                        // Main bounds controlled by group visibility
                        _upperBandSeries[index] = _showUpperBand ? _upperBandSeries[index - 1] : double.NaN;
                        _lowerBandSeries[index] = _showLowerBand ? _lowerBandSeries[index - 1] : double.NaN;
                        
                        // Copy Fibonacci levels using combined visibility logic
                        _fiboLevel886Series[index] = ShouldShowLevel(_showFibo886, true) ? _fiboLevel886Series[index - 1] : double.NaN;
                        _fiboLevel764Series[index] = ShouldShowLevel(_showFibo764, true) ? _fiboLevel764Series[index - 1] : double.NaN;
                        _fiboLevel628Series[index] = ShouldShowLevel(_showFibo628, true) ? _fiboLevel628Series[index - 1] : double.NaN;
                        
                        _fiboLevel382Series[index] = ShouldShowLevel(_showFibo382, false) ? _fiboLevel382Series[index - 1] : double.NaN;
                        _fiboLevel236Series[index] = ShouldShowLevel(_showFibo236, false) ? _fiboLevel236Series[index - 1] : double.NaN;
                        _fiboLevel114Series[index] = ShouldShowLevel(_showFibo114, false) ? _fiboLevel114Series[index - 1] : double.NaN;
                    }
                    else
                    {
                        // When both groups are disabled, use NaN to hide all bands
                        _upperBandSeries[index] = double.NaN;
                        _lowerBandSeries[index] = double.NaN;
                        _fiboLevel886Series[index] = double.NaN;
                        _fiboLevel764Series[index] = double.NaN;
                        _fiboLevel628Series[index] = double.NaN;
                        _fiboLevel382Series[index] = double.NaN;
                        _fiboLevel236Series[index] = double.NaN;
                        _fiboLevel114Series[index] = double.NaN;
                    }
                }
            }
        }

        /// <summary>
        /// Update the band visibility when band type changes
        /// </summary>
        public void UpdateBandVisibility(VwapBandType bandType)
        {
            if (_bandType == bandType)
                return;

            _bandType = bandType;

            if (!_showUpperBand && !_showLowerBand)
            {
                int visibleBarsCount = DEFAULT_VISIBLE_BARS;
                int startIndex = Math.Max(0, _bars.Count - visibleBarsCount - UPDATE_BUFFER);
                int endIndex = _bars.Count - 1;

                for (int i = startIndex; i <= endIndex; i++)
                {
                    _upperBandSeries[i] = double.NaN;
                    _lowerBandSeries[i] = double.NaN;
                    _fiboLevel886Series[i] = double.NaN;
                    _fiboLevel764Series[i] = double.NaN;
                    _fiboLevel628Series[i] = double.NaN;
                    _fiboLevel382Series[i] = double.NaN;
                    _fiboLevel236Series[i] = double.NaN;
                    _fiboLevel114Series[i] = double.NaN;
                }
            }
        }
        
        /// <summary>
        /// Update Fibonacci level visibility settings
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
            // Update individual visibility flags
            _showFibo114 = showFibo114;
            _showFibo236 = showFibo236;
            _showFibo382 = showFibo382;
            _showFibo628 = showFibo628;
            _showFibo764 = showFibo764;
            _showFibo886 = showFibo886;
            
            // Update group visibility flags
            _showUpperBand = showUpperBand;
            _showLowerBand = showLowerBand;
            
            // Only update recent bars for better performance
            int visibleBarsCount = DEFAULT_VISIBLE_BARS;
            int startIndex = Math.Max(0, _bars.Count - visibleBarsCount - UPDATE_BUFFER);
            int endIndex = _bars.Count - 1;
            
            // Update visible bars with new visibility settings
            for (int i = startIndex; i <= endIndex; i++)
            {
                // Only update if we have a valid VWAP value
                if (!double.IsNaN(_vwapSeries[i]) && (_showUpperBand || _showLowerBand))
                {
                    // Apply group visibility to main bounds
                    if (!_showUpperBand) _upperBandSeries[i] = double.NaN;
                    if (!_showLowerBand) _lowerBandSeries[i] = double.NaN;
                    
                    // Apply combined visibility settings (individual AND group)
                    if (!ShouldShowLevel(_showFibo886, true)) _fiboLevel886Series[i] = double.NaN;
                    if (!ShouldShowLevel(_showFibo764, true)) _fiboLevel764Series[i] = double.NaN;
                    if (!ShouldShowLevel(_showFibo628, true)) _fiboLevel628Series[i] = double.NaN;
                    
                    if (!ShouldShowLevel(_showFibo382, false)) _fiboLevel382Series[i] = double.NaN;
                    if (!ShouldShowLevel(_showFibo236, false)) _fiboLevel236Series[i] = double.NaN;
                    if (!ShouldShowLevel(_showFibo114, false)) _fiboLevel114Series[i] = double.NaN;
                }
            }
        }

        /// <summary>
        /// Updates the most recent bars when band visibility changes
        /// </summary>
        public void UpdateRecentBars(int barCount)
        {
            if (!_showUpperBand && !_showLowerBand)
            {
                int startIndex = Math.Max(0, _bars.Count - barCount);
                int endIndex = _bars.Count - 1;

                for (int i = startIndex; i <= endIndex; i++)
                {
                    _upperBandSeries[i] = double.NaN;
                    _lowerBandSeries[i] = double.NaN;
                    _fiboLevel886Series[i] = double.NaN;
                    _fiboLevel764Series[i] = double.NaN;
                    _fiboLevel628Series[i] = double.NaN;
                    _fiboLevel382Series[i] = double.NaN;
                    _fiboLevel236Series[i] = double.NaN;
                    _fiboLevel114Series[i] = double.NaN;
                }
            }
        }

        /// <summary>
        /// Updates all bars instead of just recent ones
        /// </summary>
        public void UpdateAllBars()
        {
            if (_bars.Count > 0)
            {
                for (int i = 0; i < _bars.Count; i++)
                {
                    if (!_showUpperBand && !_showLowerBand)
                    {
                        // Hide all bands
                        _upperBandSeries[i] = double.NaN;
                        _lowerBandSeries[i] = double.NaN;
                        _fiboLevel886Series[i] = double.NaN;
                        _fiboLevel764Series[i] = double.NaN;
                        _fiboLevel628Series[i] = double.NaN;
                        _fiboLevel382Series[i] = double.NaN;
                        _fiboLevel236Series[i] = double.NaN;
                        _fiboLevel114Series[i] = double.NaN;
                    }
                    else
                    {
                        // Apply group visibility to main bounds
                        if (!_showUpperBand) _upperBandSeries[i] = double.NaN;
                        if (!_showLowerBand) _lowerBandSeries[i] = double.NaN;
                        
                        // Apply combined visibility settings for each level
                        if (!ShouldShowLevel(_showFibo886, true)) _fiboLevel886Series[i] = double.NaN;
                        if (!ShouldShowLevel(_showFibo764, true)) _fiboLevel764Series[i] = double.NaN;
                        if (!ShouldShowLevel(_showFibo628, true)) _fiboLevel628Series[i] = double.NaN;
                        
                        if (!ShouldShowLevel(_showFibo382, false)) _fiboLevel382Series[i] = double.NaN;
                        if (!ShouldShowLevel(_showFibo236, false)) _fiboLevel236Series[i] = double.NaN;
                        if (!ShouldShowLevel(_showFibo114, false)) _fiboLevel114Series[i] = double.NaN;
                    }
                }
            }
        }
    }
}
