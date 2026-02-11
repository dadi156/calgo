using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Model class for VWAP indicator - handles all calculation logic and data storage
    /// Optimized with lazy calculation to reduce computational overhead
    /// </summary>
    public class VwapModel
    {
        // Configuration
        private readonly Bars _bars;
        private readonly DataSeries _source;
        private VwapResetPeriod _resetPeriod;
        private VwapBandType _bandType;
        private VwapBandType _previousBandType;
        private int _pivotDepth;
        private DateTime? _anchorPoint;
        private double _stdDevMultiplier;

        // Band calculators
        private readonly IBandCalculator _standardDeviationCalculator;
        private readonly IBandCalculator _highLowBandCalculator;
        private readonly IBandCalculator _fibonacciPivotCalculator;
        private readonly IBandCalculator _emptyCalculator;
        private IBandCalculator _activeCalculator;

        // Calculation state
        private double _cumulativePriceVolume;
        private double _cumulativeVolume;
        private int _lastProcessedIndex = -1;
        private DateTime _lastResetTime;

        // Period tracking cache
        private DateTime _currentBarDate;
        private int _currentBarYear;
        private int _currentBarMonth;
        private int _currentBarWeek;
        private bool _needsReset = false;

        // Lazy calculation state and caching
        private bool _isVwapDirty = true;
        private bool _areBandsDirty = true;
        private double _cachedVwap;
        private double _cachedUpperBand;
        private double _cachedLowerBand;
        private double _cachedFibLevel886;
        private double _cachedFibLevel764;
        private double _cachedFibLevel628;
        private double _cachedFibLevel382;
        private double _cachedFibLevel236;
        private double _cachedFibLevel114;
        private int _lastCalculatedIndex = -1;

        // Output properties with lazy calculation
        public double Vwap
        {
            get
            {
                if (_isVwapDirty)
                {
                    _cachedVwap = CalculateVwap();
                    _isVwapDirty = false;
                }
                return _cachedVwap;
            }
        }

        public double UpperBand
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return _showUpperBand ? _cachedUpperBand : double.NaN;
            }
        }

        public double LowerBand
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return _showLowerBand ? _cachedLowerBand : double.NaN;
            }
        }

        public double FibLevel886
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return (_showUpperBand || _showLowerBand) ? _cachedFibLevel886 : double.NaN;
            }
        }

        public double FibLevel764
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return (_showUpperBand || _showLowerBand) ? _cachedFibLevel764 : double.NaN;
            }
        }

        public double FibLevel628
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return (_showUpperBand || _showLowerBand) ? _cachedFibLevel628 : double.NaN;
            }
        }

        public double FibLevel382
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return (_showUpperBand || _showLowerBand) ? _cachedFibLevel382 : double.NaN;
            }
        }

        public double FibLevel236
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return (_showUpperBand || _showLowerBand) ? _cachedFibLevel236 : double.NaN;
            }
        }

        public double FibLevel114
        {
            get
            {
                if (_areBandsDirty && (_showUpperBand || _showLowerBand))
                {
                    UpdateBandValues();
                }
                return (_showUpperBand || _showLowerBand) ? _cachedFibLevel114 : double.NaN;
            }
        }

        // Getter methods for configuration values
        public VwapResetPeriod GetResetPeriod() => _resetPeriod;
        public DateTime? GetAnchorPoint() => _anchorPoint;
        public VwapBandType GetBandType() => _bandType;
        public int GetPivotDepth() => _pivotDepth;

        public VwapModel(
            Bars bars,
            DataSeries source,
            VwapResetPeriod resetPeriod,
            double stdDevMultiplier,
            VwapBandType bandType,
            int pivotDepth,
            bool showUpperBand,
            bool showLowerBand,
            DateTime? anchorPoint = null)
        {
            _bars = bars;
            _source = source;
            _resetPeriod = resetPeriod;
            _bandType = bandType;
            _previousBandType = bandType;
            _pivotDepth = Math.Max(1, Math.Min(3, pivotDepth)); // Constrain between 1-3
            _anchorPoint = anchorPoint;
            _stdDevMultiplier = stdDevMultiplier;
            _showUpperBand = showUpperBand;
            _showLowerBand = showLowerBand;

            // Initialize band calculators - create once and reuse
            _standardDeviationCalculator = new StandardDeviationBandCalculator(stdDevMultiplier);
            _highLowBandCalculator = new HighLowBandCalculator(bars, resetPeriod, anchorPoint);
            _fibonacciPivotCalculator = new FibonacciPivotBandCalculator(bars, resetPeriod, _pivotDepth, anchorPoint);
            _emptyCalculator = new EmptyBandCalculator(); // For when both groups are disabled

            // Set active calculator based on selected method
            _activeCalculator = GetCalculator(bandType);

            // Initialize state
            _cumulativePriceVolume = 0;
            _cumulativeVolume = 0;
            _lastResetTime = GetResetTime(_bars.OpenTimes[0]);

            // Initialize period tracking cache
            if (_bars.Count > 0)
            {
                UpdatePeriodCache(_bars.OpenTimes[0]);
            }
        }

        /// <summary>
        /// Updates the cached period values for a given datetime
        /// </summary>
        private void UpdatePeriodCache(DateTime time)
        {
            _currentBarDate = time.Date;
            _currentBarYear = time.Year;
            _currentBarMonth = time.Month;
            _currentBarWeek = PeriodUtility.GetCachedISOWeekNumber(time);
        }

        /// <summary>
        /// Efficiently check if a period reset is needed without expensive DateTime operations
        /// </summary>
        private bool CheckPeriodReset(DateTime currentTime)
        {
            switch (_resetPeriod)
            {
                case VwapResetPeriod.Daily:
                    return currentTime.Date != _currentBarDate;

                case VwapResetPeriod.Weekly:
                    int currentWeek = PeriodUtility.GetCachedISOWeekNumber(currentTime);
                    return currentWeek != _currentBarWeek || currentTime.Year != _currentBarYear;

                case VwapResetPeriod.Monthly:
                    return currentTime.Month != _currentBarMonth || currentTime.Year != _currentBarYear;

                case VwapResetPeriod.Yearly:
                    return currentTime.Year != _currentBarYear;

                // For more complex cases, use the full calculation
                default:
                    return PeriodUtility.IsDifferentPeriod(currentTime, _lastResetTime, _resetPeriod, _anchorPoint, _bars);
            }
        }

        /// <summary>
        /// Process all unprocessed bars up to the current index
        /// </summary>
        public void ProcessBars(int currentIndex)
        {
            // Process any bars that haven't been processed yet
            for (int i = _lastProcessedIndex + 1; i <= currentIndex; i++)
            {
                if (i < _bars.Count - 1) // Skip the forming bar
                {
                    ProcessBar(i);
                    _lastProcessedIndex = i;
                }
            }
        }

        // Track group visibility for calculations
        private bool _showUpperBand;
        private bool _showLowerBand;

        /// <summary>
        /// Process a single bar at the specified index
        /// </summary>
        public void ProcessBar(int index)
        {
            // Check if we've already processed this bar (optimization)
            if (index <= _lastCalculatedIndex && index != 0)
                return;

            DateTime currentBarTime = _bars.OpenTimes[index];

            // Skip bars before anchor point when in anchor point mode
            if (_resetPeriod == VwapResetPeriod.AnchorPoint &&
                _anchorPoint.HasValue &&
                currentBarTime < _anchorPoint.Value)
            {
                // Use double.NaN to prevent drawing lines
                _cachedVwap = double.NaN;
                _cachedUpperBand = double.NaN;
                _cachedLowerBand = double.NaN;
                _cachedFibLevel886 = double.NaN;
                _cachedFibLevel764 = double.NaN;
                _cachedFibLevel628 = double.NaN;
                _cachedFibLevel382 = double.NaN;
                _cachedFibLevel236 = double.NaN;
                _cachedFibLevel114 = double.NaN;
                _isVwapDirty = false;
                _areBandsDirty = false;
                _lastCalculatedIndex = index;
                return;
            }

            // Use optimized period check
            bool needsReset = CheckPeriodReset(currentBarTime);
            if (needsReset)
            {
                ResetCalculations();
                _lastResetTime = GetResetTime(currentBarTime);

                // Update the period cache with the new period
                UpdatePeriodCache(currentBarTime);
            }

            double price = _source[index];
            double volume = _bars.TickVolumes[index];

            // Skip calculation if volume is zero
            if (volume == 0)
            {
                // For weekly timeframe, use a small synthetic volume instead of skipping
                if (_bars.TimeFrame >= TimeFrame.Weekly)
                {
                    // Use a small non-zero volume so calculation continues
                    volume = 0.001;
                }
                else
                {
                    // For non-weekly timeframes, keep the original behavior
                    if (index > 0)
                    {
                        _isVwapDirty = false;
                        _areBandsDirty = false;
                    }
                    _lastCalculatedIndex = index;
                    return;
                }
            }

            // Update cumulative values for VWAP
            _cumulativePriceVolume += price * volume;
            _cumulativeVolume += volume;

            // Mark values as dirty - will be calculated on-demand
            _isVwapDirty = true;
            _areBandsDirty = true;

            // Only process bands if at least one group is visible
            if (_showUpperBand || _showLowerBand)
            {
                // We need to calculate VWAP here to pass to the calculator
                double vwap = CalculateVwap();
                _activeCalculator.ProcessBar(index, price, volume, vwap);
            }

            _lastCalculatedIndex = index;
        }

        /// <summary>
        /// Calculate the VWAP value - called only when needed
        /// </summary>
        private double CalculateVwap()
        {
            return _cumulativeVolume > 0 ? _cumulativePriceVolume / _cumulativeVolume : 0;
        }

        /// <summary>
        /// Update all band values from the calculator - called only when needed
        /// </summary>
        private void UpdateBandValues()
        {
            // Ensure VWAP is calculated first
            if (_isVwapDirty)
            {
                _cachedVwap = CalculateVwap();
                _isVwapDirty = false;
            }

            // Get band values from the active calculator
            _cachedUpperBand = _activeCalculator.GetUpperBand();
            _cachedLowerBand = _activeCalculator.GetLowerBand();
            _cachedFibLevel886 = _activeCalculator.GetFibLevel886();
            _cachedFibLevel764 = _activeCalculator.GetFibLevel764();
            _cachedFibLevel628 = _activeCalculator.GetFibLevel628();
            _cachedFibLevel382 = _activeCalculator.GetFibLevel382();
            _cachedFibLevel236 = _activeCalculator.GetFibLevel236();
            _cachedFibLevel114 = _activeCalculator.GetFibLevel114();

            // Mark as not dirty since we've updated all values
            _areBandsDirty = false;
        }

        private bool ShouldReset(DateTime currentTime)
        {
            // Use the cached value if available
            if (_needsReset)
            {
                _needsReset = false;
                return true;
            }

            // Otherwise use the optimized period check
            return CheckPeriodReset(currentTime);
        }

        private DateTime GetResetTime(DateTime time)
        {
            return PeriodUtility.GetPeriodStartTime(time, _resetPeriod, _anchorPoint, _bars);
        }

        private void ResetCalculations()
        {
            _cumulativePriceVolume = 0;
            _cumulativeVolume = 0;

            // Reset the active calculator
            _activeCalculator.Reset();

            // Mark values as dirty
            _isVwapDirty = true;
            _areBandsDirty = true;
        }

        // Get the appropriate calculator based on the band type and group visibility
        private IBandCalculator GetCalculator(VwapBandType bandType)
        {
            // If both groups are disabled, use empty calculator for performance
            if (!_showUpperBand && !_showLowerBand)
            {
                return _emptyCalculator;
            }
            
            switch (bandType)
            {
                case VwapBandType.StandardDeviation:
                    return _standardDeviationCalculator;
                case VwapBandType.HighLowRange:
                    return _highLowBandCalculator;
                case VwapBandType.FibonacciPivot:
                    return _fibonacciPivotCalculator;
                default:
                    return _highLowBandCalculator; // Default fallback
            }
        }

        // Update configuration
        public void UpdateConfiguration(
            VwapResetPeriod resetPeriod,
            VwapBandType bandType,
            int pivotDepth,
            bool showUpperBand,
            bool showLowerBand,
            DateTime? anchorPoint = null)
        {
            // Save old values to check if they changed
            bool resetPeriodChanged = _resetPeriod != resetPeriod;
            bool bandTypeChanged = _bandType != bandType;
            bool pivotDepthChanged = _pivotDepth != pivotDepth;
            bool groupVisibilityChanged = _showUpperBand != showUpperBand || _showLowerBand != showLowerBand;
            bool anchorPointChanged =
                (_anchorPoint.HasValue != anchorPoint.HasValue) ||
                (_anchorPoint.HasValue && anchorPoint.HasValue && _anchorPoint.Value != anchorPoint.Value);

            // Update values
            _resetPeriod = resetPeriod;
            _previousBandType = _bandType;
            _bandType = bandType;
            _pivotDepth = Math.Max(1, Math.Min(3, pivotDepth)); // Constrain between 1-3
            _showUpperBand = showUpperBand;
            _showLowerBand = showLowerBand;
            _anchorPoint = anchorPoint;

            // Update parameters for all calculators that need period info
            _highLowBandCalculator.UpdateParameters(resetPeriod, pivotDepth, anchorPoint);
            _fibonacciPivotCalculator.UpdateParameters(resetPeriod, pivotDepth, anchorPoint);

            // If period parameters changed, we need to reset the VWAP calculation too
            if (resetPeriodChanged || anchorPointChanged)
            {
                // Reset calculation state
                _cumulativePriceVolume = 0;
                _cumulativeVolume = 0;
                _lastResetTime = GetResetTime(_bars.OpenTimes[0]);

                // Mark that a reset is needed on the next calculation
                _needsReset = true;

                // Update period cache
                if (_bars.Count > 0)
                {
                    UpdatePeriodCache(_bars.OpenTimes[0]);
                }

                // Mark values as dirty
                _isVwapDirty = true;
                _areBandsDirty = true;
            }

            // Update the active calculator if band type or group visibility changed
            if (bandTypeChanged || groupVisibilityChanged)
            {
                _activeCalculator = GetCalculator(bandType);
                // Mark band values as dirty because we changed the calculator or visibility
                _areBandsDirty = true;
            }
        }

        /// <summary>
        /// Reset all calculation state
        /// </summary>
        public void Reset()
        {
            // Reset calculation state
            _cumulativePriceVolume = 0;
            _cumulativeVolume = 0;

            // Reset all calculators
            _standardDeviationCalculator.Reset();
            _highLowBandCalculator.Reset();
            _fibonacciPivotCalculator.Reset();

            // Reset period tracking
            if (_bars.Count > 0)
            {
                _lastResetTime = GetResetTime(_bars.OpenTimes[0]);
                UpdatePeriodCache(_bars.OpenTimes[0]);
            }

            // Mark values as dirty
            _isVwapDirty = true;
            _areBandsDirty = true;

            // Reset calculation tracking
            _lastCalculatedIndex = -1;
        }
    }
}
