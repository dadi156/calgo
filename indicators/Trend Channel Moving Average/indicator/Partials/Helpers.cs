using System;
using System.Globalization;
using cAlgo.API;

namespace cAlgo
{
    public partial class TrendChannelMovingAverage : Indicator
    {
        #region Helper Methods

        // ===============================================================
        // SECTION 1: INITIALIZATION METHODS
        // ===============================================================

        /// <summary>
        /// Initialize bar detection system
        /// </summary>
        public void InitializeBarDetection()
        {
            try
            {
                if (Bars.Count > 0)
                {
                    _lastBarCount = Bars.Count;
                    _lastBarTime = Bars.OpenTimes[Bars.Count - 1];
                }
                _lastReloadTime = Server.Time;
                _isFirstRun = true;
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        /// <summary>
        /// Initialize anchor date functionality
        /// </summary>
        public void InitializeAnchorDate()
        {
            try
            {
                if (PeriodCalculationType == PeriodCalculationType.AnchorDate)
                {
                    _anchorDateTime = ParseAnchorDateTime();
                    _anchorDateInitialized = true;
                }
                else
                {
                    _anchorDateTime = DateTime.MinValue;
                    _anchorDateInitialized = true;
                }
            }
            catch (Exception)
            {
                _anchorDateTime = DateTime.MinValue;
                _anchorDateInitialized = true;
            }
        }

        /// <summary>
        /// Initialize lines start point functionality
        /// </summary>
        public void InitializeLinesStartPoint()
        {
            try
            {
                ParseLinesStartPoint(out DateTime dateTime, out bool enabled);
                
                _linesStartPointEnabled = enabled;
                _linesStartDateTime = dateTime;
                _linesStartPointInitialized = true;
            }
            catch (Exception)
            {
                _linesStartPointEnabled = false;
                _linesStartDateTime = DateTime.MinValue;
                _linesStartPointInitialized = true;
            }
        }

        // ===============================================================
        // SECTION 2: DATE AND TIME PARSING METHODS
        // ===============================================================

        /// <summary>
        /// Parse anchor date from user string
        /// </summary>
        public DateTime ParseAnchorDateTime()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AnchorDateTime))
                {
                    throw new Exception("Anchor date string is empty");
                }

                DateTime parsedDate;
                string[] formats = {
                    "dd/MM/yyyy HH:mm",
                    "dd/MM/yyyy H:mm",
                    "d/MM/yyyy HH:mm",
                    "dd/M/yyyy HH:mm",
                    "d/M/yyyy HH:mm",
                    "dd/MM/yyyy",
                    "d/MM/yyyy",
                    "dd/M/yyyy",
                    "d/M/yyyy"
                };

                bool success = DateTime.TryParseExact(
                    AnchorDateTime.Trim(),
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsedDate);

                if (success)
                {
                    return parsedDate;
                }
                else
                {
                    if (DateTime.TryParse(AnchorDateTime, out parsedDate))
                    {
                        return parsedDate;
                    }
                    else
                    {
                        throw new Exception($"Cannot parse anchor date: {AnchorDateTime}");
                    }
                }
            }
            catch (Exception)
            {
                return Server.Time.AddDays(-30);
            }
        }

        /// <summary>
        /// Parse lines start point from user string (datetime, true/false format)
        /// </summary>
        public void ParseLinesStartPoint(out DateTime dateTime, out bool enabled)
        {
            try
            {
                // Set default values
                dateTime = DateTime.MinValue;
                enabled = false;

                // Check if input is empty
                if (string.IsNullOrWhiteSpace(LinesStartPoint))
                {
                    return;
                }

                // Split by comma
                string[] parts = LinesStartPoint.Split(',');
                
                if (parts.Length != 2)
                {
                    return; // Wrong format
                }

                // Get date part and enabled part
                string datePart = parts[0].Trim();
                string enabledPart = parts[1].Trim();

                // Parse enabled/disabled first
                if (!bool.TryParse(enabledPart, out enabled))
                {
                    enabled = false;
                    return;
                }

                // If disabled, no need to parse date
                if (!enabled)
                {
                    dateTime = DateTime.MinValue;
                    return;
                }

                // Parse date only if enabled
                if (string.IsNullOrWhiteSpace(datePart))
                {
                    enabled = false; // No date provided but enabled = true
                    return;
                }

                // Try to parse the date
                string[] formats = {
                    "dd/MM/yyyy HH:mm",
                    "dd/MM/yyyy H:mm",
                    "d/MM/yyyy HH:mm",
                    "dd/M/yyyy HH:mm",
                    "d/M/yyyy HH:mm",
                    "dd/MM/yyyy",
                    "d/MM/yyyy",
                    "dd/M/yyyy",
                    "d/M/yyyy"
                };

                bool success = DateTime.TryParseExact(
                    datePart,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dateTime);

                if (!success)
                {
                    success = DateTime.TryParse(datePart, out dateTime);
                }

                if (!success)
                {
                    // Could not parse date
                    dateTime = DateTime.MinValue;
                    enabled = false;
                }
            }
            catch (Exception)
            {
                dateTime = DateTime.MinValue;
                enabled = false;
            }
        }

        /// <summary>
        /// Parse lines start point date from user string (OLD METHOD - KEPT FOR COMPATIBILITY)
        /// </summary>
        public DateTime ParseLinesStartDateTime()
        {
            ParseLinesStartPoint(out DateTime dateTime, out bool enabled);
            return enabled ? dateTime : DateTime.MinValue;
        }

        // ===============================================================
        // SECTION 3: BAR DETECTION AND RELOAD METHODS
        // ===============================================================

        /// <summary>
        /// Check if new bar was formed
        /// </summary>
        public bool IsNewBarFormed()
        {
            try
            {
                int currentBarCount = Bars.Count;
                
                if (currentBarCount == 0)
                    return false;

                DateTime currentLastBarTime = Bars.OpenTimes[currentBarCount - 1];
                
                bool newBarByCount = (currentBarCount > _lastBarCount);
                bool newBarByTime = (currentLastBarTime != _lastBarTime);
                bool newBarFormed = newBarByCount || newBarByTime;
                
                if (newBarFormed)
                {
                    _lastBarCount = currentBarCount;
                    _lastBarTime = currentLastBarTime;
                    return true;
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if we should trigger reload
        /// </summary>
        public bool ShouldTriggerReload()
        {
            try
            {
                if (_isFirstRun)
                {
                    _isFirstRun = false;
                    return true;
                }

                double secondsSinceLastReload = (Server.Time - _lastReloadTime).TotalSeconds;
                if (secondsSinceLastReload < MIN_RELOAD_INTERVAL_SECONDS)
                    return false;
                    
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Trigger reload mechanism
        /// </summary>
        public void TriggerReload()
        {
            try
            {
                _model.ResetOptimizations();
                
                _lastProcessedBarCount = 0;
                _lastCalculatedIndex = -1;
                
                int barsToRecalculate = Math.Min(RELOAD_BARS_COUNT, Bars.Count);
                int startIndex = Math.Max(0, Bars.Count - barsToRecalculate);
                
                for (int i = startIndex; i < Bars.Count; i++)
                {
                    _controller.Calculate(i);
                    
                    // Also calculate fibonacci levels during reload
                    if (_fibonacciController != null)
                    {
                        _fibonacciController.Calculate(i, FibonacciDisplayMode);
                    }
                }
                
                _lastReloadTime = Server.Time;
                _lastCalculatedIndex = Bars.Count - 1;
                _lastProcessedBarCount = Bars.Count;
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        /// <summary>
        /// Manual reload method
        /// </summary>
        public void ManualReload()
        {
            try
            {
                TriggerReload();
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        // ===============================================================
        // SECTION 4: INDEX VALIDATION AND CHECKING METHODS
        // ===============================================================

        /// <summary>
        /// Check if a bar index is before anchor date
        /// </summary>
        public bool IsBeforeAnchorDate(int index)
        {
            if (PeriodCalculationType != PeriodCalculationType.AnchorDate || !_anchorDateInitialized)
                return false;

            if (index < 0 || index >= Bars.Count)
                return true;

            DateTime barTime = Bars.OpenTimes[index];
            return barTime < _anchorDateTime;
        }

        /// <summary>
        /// Check if a bar index is before lines start point
        /// </summary>
        public bool IsBeforeLinesStartPoint(int index)
        {
            if (!_linesStartPointEnabled || !_linesStartPointInitialized)
                return false;

            if (index < 0 || index >= Bars.Count)
                return true;

            DateTime barTime = Bars.OpenTimes[index];
            return barTime < _linesStartDateTime;
        }

        /// <summary>
        /// Check if we need to calculate this index
        /// </summary>
        public bool ShouldCalculateIndex(int index)
        {
            if (index > _lastCalculatedIndex)
                return true;
                
            if (index == Bars.Count - 1)
                return true;
                
            return false;
        }

        /// <summary>
        /// Get first valid bar index (after anchor date)
        /// </summary>
        public int GetFirstValidBarIndex()
        {
            if (PeriodCalculationType != PeriodCalculationType.AnchorDate || !_anchorDateInitialized)
                return 0;

            for (int i = 0; i < Bars.Count; i++)
            {
                if (!IsBeforeAnchorDate(i))
                    return i;
            }

            return Bars.Count;
        }

        /// <summary>
        /// Get first valid bar index for lines display (after lines start point)
        /// </summary>
        public int GetFirstValidLinesDisplayIndex()
        {
            if (!IsLinesStartPointEnabled() || !_linesStartPointInitialized)
                return 0;

            for (int i = 0; i < Bars.Count; i++)
            {
                if (!IsBeforeLinesStartPoint(i))
                    return i;
            }

            return Bars.Count;
        }

        // ===============================================================
        // SECTION 5: DISPLAY LOGIC METHODS
        // ===============================================================

        /// <summary>
        /// Check if lines should be displayed at given index
        /// This combines both anchor date and lines start point logic
        /// </summary>
        public bool ShouldDisplayLines(int index)
        {
            // Check anchor date first (affects calculations)
            if (IsBeforeAnchorDate(index))
                return false;

            // Check lines start point (affects display only)
            if (IsBeforeLinesStartPoint(index))
                return false;

            return true;
        }

        /// <summary>
        /// Set all output lines to NaN for given index (including fibonacci)
        /// </summary>
        public void SetAllLinesToNaN(int index)
        {
            try
            {
                // Set main MA lines to NaN
                OpenLine[index] = double.NaN;
                CloseLine[index] = double.NaN;
                MedianLine[index] = double.NaN;
                
                HighLineUptrend[index] = double.NaN;
                HighLineDowntrend[index] = double.NaN;
                HighLineNeutral[index] = double.NaN;
                
                LowLineUptrend[index] = double.NaN;
                LowLineDowntrend[index] = double.NaN;
                LowLineNeutral[index] = double.NaN;

                // Set Fibonacci lines to NaN
                SetAllFibonacciLinesToNaN(index);
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        // ===============================================================
        // SECTION 6: BAR COLORS METHODS
        // ===============================================================

        /// <summary>
        /// Apply bar colors based on trend direction
        /// </summary>
        public void ApplyBarColors(int index)
        {
            try
            {
                if (!EnableBarColors)
                    return;

                // Check if we should apply colors at this index
                // Respect LinesStartPoint setting
                if (IsBeforeLinesStartPoint(index))
                    return;  // Don't color bars before LinesStartPoint

                // Get trend direction for this bar
                string trendString = GetTrendDirection(index);
                
                // Convert trend string to color
                Color barColor = GetColorForTrend(trendString);
                
                // Apply the color to the bar
                Chart.SetBarColor(index, barColor);
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        /// <summary>
        /// Get color based on trend direction
        /// </summary>
        private Color GetColorForTrend(string trendDirection)
        {
            try
            {
                switch (trendDirection)
                {
                    case "Uptrend":
                        return UptrendBarColor;
                    
                    case "Downtrend":
                        return DowntrendBarColor;
                    
                    case "Neutral":
                        return NeutralBarColor;
                    
                    default:
                        return NeutralBarColor;
                }
            }
            catch (Exception)
            {
                // Return default color if error
                return Color.Gray;
            }
        }

        // ===============================================================
        // SECTION 7: SETTINGS AND PARAMETER GETTERS
        // ===============================================================

        // Anchor date parameter getters
        public PeriodCalculationType GetPeriodCalculationType() => PeriodCalculationType;
        public string GetAnchorDateString() => AnchorDateTime;
        public DateTime GetAnchorDate() => _anchorDateTime;

        // Lines start point parameter getters
        public bool IsLinesStartPointEnabled() => _linesStartPointEnabled;
        public DateTime GetLinesStartDateTime() => _linesStartDateTime;
        public string GetLinesStartPointString() => LinesStartPoint;

        // Moving average settings getters
        public MAType GetMAType() => _model.GetMAType();
        public DataSeries GetSource() => _model.GetSource();
        public int GetPeriod() => _model.GetPeriod();
        public bool IsMultiTimeframeEnabled() => _model.IsMultiTimeframeEnabled();

        // ===============================================================
        // SECTION 8: DATA ACCESS METHODS
        // ===============================================================

        // Get methods for MA lines
        public double GetOpenLine(int index) => _model.GetOpenLine(index);
        public double GetCloseLine(int index) => _model.GetCloseLine(index);
        public double GetMedianLine(int index) => _model.GetMedianLine(index);
        
        // Get methods for High/Low lines
        public double GetHighLine(int index) => _model.GetHighLine(index);
        public double GetLowLine(int index) => _model.GetLowLine(index);
        
        // Get methods for specific trend lines
        public double GetHighLineUptrend(int index) => _model.GetHighLineUptrend(index);
        public double GetHighLineDowntrend(int index) => _model.GetHighLineDowntrend(index);
        public double GetHighLineNeutral(int index) => _model.GetHighLineNeutral(index);
        
        public double GetLowLineUptrend(int index) => _model.GetLowLineUptrend(index);
        public double GetLowLineDowntrend(int index) => _model.GetLowLineDowntrend(index);
        public double GetLowLineNeutral(int index) => _model.GetLowLineNeutral(index);
        
        // Get current trend direction
        public string GetTrendDirection(int index) => _model.GetTrendDirection(index);

        /// <summary>
        /// Get current effective period for specific bar
        /// </summary>
        public int GetEffectivePeriod(int index)
        {
            if (_model == null)
                return Period;

            return _model.GetEffectivePeriod(index);
        }

        /// <summary>
        /// Get all values at once
        /// </summary>
        public CachedValues GetAllValues(int index)
        {
            if (IsBeforeAnchorDate(index))
                return CachedValues.Invalid();

            return _model.GetAllValues(index);
        }

        /// <summary>
        /// Get all values ignoring anchor date
        /// </summary>
        public CachedValues GetAllValuesIgnoreAnchor(int index)
        {
            return _model.GetAllValues(index);
        }

        /// <summary>
        /// Get all values for display (respects both anchor date and lines start point)
        /// </summary>
        public CachedValues GetAllValuesForDisplay(int index)
        {
            if (!ShouldDisplayLines(index))
                return CachedValues.Invalid();

            return _model.GetAllValues(index);
        }

        // ===============================================================
        // SECTION 9: UTILITY AND PERFORMANCE METHODS
        // ===============================================================

        /// <summary>
        /// Get cache performance info
        /// </summary>
        public double GetCachePerformance()
        {
            return _model.GetCacheHitRatio();
        }

        /// <summary>
        /// Reset optimizations
        /// </summary>
        public void ResetOptimizations()
        {
            _model.ResetOptimizations();
            _lastCalculatedIndex = -1;
            _lastProcessedBarCount = 0;

            // Reset fibonacci controller if available
            if (_fibonacciController != null)
            {
                _fibonacciController.Reset();
            }
        }
        
        #endregion
    }
}
