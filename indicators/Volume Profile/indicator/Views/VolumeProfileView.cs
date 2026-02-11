using System;
using System.Linq;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.Indicators.App.Models;

namespace cAlgo.Indicators.App.Views
{
    public class VolumeProfileView
    {
        private readonly Chart _chart;
        private readonly bool _showLevelInfo;
        private readonly bool _showAllLevels;
        private readonly TextPosition _textPosition;
        private readonly string _fontFamily;
        private readonly int _fontSize;
        private readonly double _pipSize; // Store pip size for margins
        private readonly double _pipMargin; // User-defined margin in pips
        private int _lookbackPeriods; // Store lookback periods for width calculation

        private readonly Color _positiveDeltaColor;
        private readonly Color _negativeDeltaColor;
        private readonly Color _positiveTextColor;
        private readonly Color _negativeTextColor;

        private readonly bool _showVolTPO;
        private readonly bool _fullWidthBars;
        private readonly Color _tpoTextColor;

        // Track the IDs of objects created by this indicator
        private readonly List<string> _chartObjectIds = new List<string>();

        private TimeSpan _barTimeSpan; // Calculated bar duration for offsets

        private double _cachedTotalVolume;

        public VolumeProfileView(
            Chart chart,
            Color positiveTextColor,
            Color negativeTextColor,
            bool showLevelInfo,
            bool showAllLevels,
            TextPosition textPosition,
            string fontFamily,
            int fontSize,
            double pipMargin,
            Color positiveDeltaColor,
            Color negativeDeltaColor,
            Color tpoTextColor,
            bool showVolTPO,
            bool fullWidthBars)
        {
            _chart = chart;
            _positiveTextColor = positiveTextColor;
            _negativeTextColor = negativeTextColor;
            _showLevelInfo = showLevelInfo;
            _showAllLevels = showAllLevels;
            _textPosition = textPosition;
            _fontFamily = fontFamily;
            _fontSize = fontSize;
            _pipSize = chart.Symbol.PipSize;
            _pipMargin = pipMargin;
            _positiveDeltaColor = positiveDeltaColor;
            _negativeDeltaColor = negativeDeltaColor;
            _tpoTextColor = tpoTextColor;
            _showVolTPO = showVolTPO;
            _fullWidthBars = fullWidthBars;
            _barTimeSpan = CalculateBarTimeSpan(chart.TimeFrame);
        }

        private TimeSpan CalculateBarTimeSpan(TimeFrame tf)
        {
            if (tf == TimeFrame.Minute) return TimeSpan.FromMinutes(1);
            if (tf == TimeFrame.Minute5) return TimeSpan.FromMinutes(5);
            if (tf == TimeFrame.Minute15) return TimeSpan.FromMinutes(15);
            if (tf == TimeFrame.Minute30) return TimeSpan.FromMinutes(30);
            if (tf == TimeFrame.Hour) return TimeSpan.FromHours(1);
            if (tf == TimeFrame.Hour4) return TimeSpan.FromHours(4);
            if (tf == TimeFrame.Daily) return TimeSpan.FromDays(1);
            if (tf == TimeFrame.Weekly) return TimeSpan.FromDays(7);
            if (tf == TimeFrame.Monthly) return TimeSpan.FromDays(30);
            return TimeSpan.FromHours(1); // Default
        }

        public void ClearChart()
        {
            // Only remove objects created by this indicator
            foreach (var objectId in _chartObjectIds)
            {
                _chart.RemoveObject(objectId);
            }

            // Clear the list
            _chartObjectIds.Clear();
        }

        public void DrawVolumeProfile(
            VolumeProfileModel model,
            int index,
            int startIndex,
            Bars bars,
            bool isDateTimeMode = false,
            int dateTimeSpan = 0,
            bool showTPO = false)
        {
            // Get the maximum volume for scaling
            double maxVolume = model.PointOfControl != null ? model.PointOfControl.Volume : 1;

            // Cache total volume for label calculations
            _cachedTotalVolume = 0;
            for (int i = 0; i < model.PriceLevelsList.Count; i++)
                _cachedTotalVolume += model.PriceLevelsList[i].Volume;

            // Determine profile width based on mode
            int effectiveWidth = isDateTimeMode ? Math.Max(20, dateTimeSpan) : _lookbackPeriods;

            // Determine base indices for time calculations
            int baseIndex, endIndex;
            if (isDateTimeMode)
            {
                baseIndex = startIndex;
                endIndex = Math.Min(bars.Count - 1, startIndex + effectiveWidth);
            }
            else
            {
                baseIndex = index;
                endIndex = Math.Max(0, index - effectiveWidth);
            }

            // Pre-calculate time indices for performance
            Dictionary<int, DateTime> cachedTimes = new Dictionary<int, DateTime>();

            // Cache times at various intervals for faster lookup
            int step = Math.Max(1, effectiveWidth / 20); // Create ~20 cache points
            for (int i = 0; i <= effectiveWidth; i += step)
            {
                int timeIndex;
                if (isDateTimeMode)
                {
                    timeIndex = Math.Min(bars.Count - 1, baseIndex + i);
                }
                else
                {
                    timeIndex = Math.Max(0, baseIndex - i);
                }
                cachedTimes[i] = bars.OpenTimes[timeIndex];
            }

            // Draw each price level
            foreach (var level in model.PriceLevelsList)
            {
                // Skip levels with zero volume
                if (level.Volume == 0)
                    continue;

                // Add user-defined pip margin to the top and bottom
                double marginSize = _pipSize * _pipMargin;
                double marginedPrice = level.Price + marginSize;
                double marginedUpperBound = level.UpperBound - marginSize;

                // Skip if the margin would make the rectangle too small
                if (marginedUpperBound <= marginedPrice)
                    continue;

                // Calculate proportions of buying and selling volume
                double totalLevelVolume = level.BuyingVolume + level.SellingVolume;
                if (totalLevelVolume <= 0.0001)
                    continue;

                // Calculate buy and sell proportions
                double buyRatio = level.BuyingVolume / totalLevelVolume;
                double sellRatio = level.SellingVolume / totalLevelVolume;

                // Scale the total bar width based on volume relative to max volume or use full width
                int scaledWidth;
                if (_fullWidthBars)
                {
                    // Use the same width for all bars when full width mode is enabled
                    scaledWidth = effectiveWidth;
                }
                else
                {
                    // Scale width based on volume (original behavior)
                    double volumeRatio = level.Volume / maxVolume;
                    scaledWidth = Math.Max(1, (int)Math.Round(effectiveWidth * volumeRatio));
                }

                // Calculate bar widths while maintaining proportions
                int sellBars = (int)Math.Round(scaledWidth * sellRatio);
                int buyBars = scaledWidth - sellBars;

                // Ensure minimum widths for non-zero values
                if (sellRatio > 0.001 && sellBars == 0) sellBars = 1;
                if (buyRatio > 0.001 && buyBars == 0) buyBars = 1;

                // If we adjusted minimums, update the total
                scaledWidth = sellBars + buyBars;

                // Fast time calculation using cache and linear interpolation
                DateTime sellStart, sellEnd, buyStart, buyEnd;

                if (isDateTimeMode)
                {
                    // Get times using cached values and fast calculation
                    sellStart = GetCachedTime(cachedTimes, 0, baseIndex, bars, isDateTimeMode);
                    sellEnd = GetCachedTime(cachedTimes, sellBars, baseIndex, bars, isDateTimeMode);
                    buyStart = sellEnd;
                    buyEnd = GetCachedTime(cachedTimes, sellBars + buyBars, baseIndex, bars, isDateTimeMode);
                }
                else
                {
                    // Backward calculation for regular mode
                    buyEnd = GetCachedTime(cachedTimes, 0, baseIndex, bars, isDateTimeMode);
                    buyStart = GetCachedTime(cachedTimes, buyBars, baseIndex, bars, isDateTimeMode);
                    sellEnd = buyStart;
                    sellStart = GetCachedTime(cachedTimes, buyBars + sellBars, baseIndex, bars, isDateTimeMode);
                }

                // Draw sell side (red) if significant
                if (sellRatio > 0.001 && sellBars > 0)
                {
                    var sellBar = _chart.DrawRectangle(
                        "VPSellBar_" + level.Price,
                        sellStart,
                        marginedPrice,
                        sellEnd,
                        marginedUpperBound,
                        _negativeDeltaColor
                    );

                    sellBar.ZIndex = 5;
                    sellBar.IsFilled = true;
                    _chartObjectIds.Add("VPSellBar_" + level.Price);
                }

                // Draw buy side (green) if significant
                if (buyRatio > 0.001 && buyBars > 0)
                {
                    var buyBar = _chart.DrawRectangle(
                        "VPBuyBar_" + level.Price,
                        buyStart,
                        marginedPrice,
                        buyEnd,
                        marginedUpperBound,
                        _positiveDeltaColor
                    );

                    buyBar.ZIndex = 5;
                    buyBar.IsFilled = true;
                    _chartObjectIds.Add("VPBuyBar_" + level.Price);
                }

                // Add information text for significant levels
                if (_showLevelInfo)
                {
                    bool shouldShowInfo = _showAllLevels ||
                                        level == model.PointOfControl ||
                                        model.ValueArea.Contains(level) ||
                                        level.IsVAH ||
                                        level.IsVAL;

                    if (shouldShowInfo)
                    {
                        DateTime textTime;
                        if (isDateTimeMode)
                        {
                            textTime = buyEnd; // Right edge of each bar
                        }
                        else
                        {
                            textTime = _textPosition == TextPosition.Left ? sellStart : buyEnd;
                        }
                        DrawLevelText(level, model, textTime, isDateTimeMode, _fullWidthBars);
                    }
                }
            }

            // Draw TPO information if enabled
            if (showTPO)
            {
                DrawTPOInformation(model, isDateTimeMode, startIndex, bars);
            }
        }

        // DrawLevelText method with fixed alignment logic
        private void DrawLevelText(
            PriceLevel level,
            VolumeProfileModel model,
            DateTime timePosition,
            bool isDateTimeMode = false,
            bool isFullWidth = false)
        {
            // Calculate text position with small offset for padding
            DateTime textTime;
            HorizontalAlignment hAlign;

            if (isDateTimeMode)
            {
                // DateTime mode: right side of bars
                if (isFullWidth)
                {
                    // Full width: all labels aligned at profile right edge
                    textTime = timePosition.Add(_barTimeSpan * 2);
                }
                else
                {
                    // Variable width: labels follow each bar's right edge
                    textTime = timePosition.Add(_barTimeSpan * 2);
                }
                hAlign = HorizontalAlignment.Right;
            }
            else
            {
                // Regular mode: controlled by TextPosition parameter
                if (_textPosition == TextPosition.Left)
                {
                    textTime = timePosition.Add(-_barTimeSpan * 2);
                    hAlign = HorizontalAlignment.Left;
                }
                else
                {
                    textTime = timePosition.Add(_barTimeSpan * 2);
                    hAlign = HorizontalAlignment.Right;
                }
            }

            double textPrice = (level.Price + level.UpperBound) / 2;

            double volumePercent = _cachedTotalVolume > 0
                ? (level.Volume / _cachedTotalVolume) * 100
                : 0;

            string label = CreateLabel(level, model, volumePercent);

            Color textColor = level.VolumeDelta >= 0 ? _positiveTextColor : _negativeTextColor;

            var text = _chart.DrawText(
                "VPText_" + level.Price,
                label,
                textTime,
                textPrice,
                textColor
            );

            text.FontFamily = _fontFamily;
            text.FontSize = _fontSize;
            text.VerticalAlignment = VerticalAlignment.Center;
            text.HorizontalAlignment = hAlign;
            text.ZIndex = 10;
            _chartObjectIds.Add("VPText_" + level.Price);
        }

        // CreateLabel method to show TPO letters
        private string CreateLabel(PriceLevel level, VolumeProfileModel model, double volumePercent)
        {
            // Format volumes with suffix (k, M) for readability
            string formattedTotalVolume = FormatVolumeWithSuffix(level.Volume);
            string formattedBuyVolume = FormatVolumeWithSuffix(level.BuyingVolume);
            string formattedSellVolume = FormatVolumeWithSuffix(level.SellingVolume);
            string formattedDelta = FormatVolumeWithSuffix(Math.Abs(level.VolumeDelta));

            // Calculate percentages
            double buyPercent = level.Volume > 0 ? (level.BuyingVolume / level.Volume) * 100 : 0;
            double sellPercent = level.Volume > 0 ? (level.SellingVolume / level.Volume) * 100 : 0;
            double deltaPercentage = level.Volume > 0 ? (level.VolumeDelta / level.Volume) * 100 : 0;
            string deltaSign = level.VolumeDelta >= 0 ? "+" : "-";

            // Make multi-line label for better readability
            string label;

            // Add special level identifiers - always visible
            if (level == model.PointOfControl)
            {
                label = "POC\n";
            }
            else if (level.IsVAH)
            {
                label = "VAH\n";
            }
            else if (level.IsVAL)
            {
                label = "VAL\n";
            }
            else
            {
                label = "";
            }

            // Add volume information - only if ShowVolTPO is enabled
            if (_showVolTPO)
            {
                label += $"Vol: {formattedTotalVolume} ({volumePercent:F1}%)\n";
            }

            // Add buy/sell breakdown in a single line - always visible
            label += $"Pressure: Buy {formattedBuyVolume} ({buyPercent:F1}%) | Sell {formattedSellVolume} ({sellPercent:F1}%)";

            // Add delta information - always visible
            if (Math.Abs(level.VolumeDelta) > 0)
            {
                label += $"\nDelta: {deltaSign}{formattedDelta} ({deltaPercentage:F1}%)";
            }
            else
            {
                label += "\nDelta: 0 (0.0%)";
            }

            // Add TPO information with letters if available - only if ShowVolTPO is enabled
            if (_showVolTPO && level.TPOCount > 0)
            {
                if (level.TPOLetters.Count > 0)
                {
                    var sortedLetters = level.TPOLetters.OrderBy(l => l).ToList();
                    string letters = string.Join("", sortedLetters);

                    // Limit to first 15 letters if too many, with "..." suffix
                    if (letters.Length > 15)
                    {
                        letters = letters.Substring(0, 15) + "...";
                    }

                    label += $"\nTPO: {level.TPOCount} ({letters})";
                }
                else
                {
                    label += $"\nTPO: {level.TPOCount}";
                }
            }

            return label;
        }

        private string FormatVolumeWithSuffix(double volume)
        {
            // Format volume with k or M suffix for better readability
            if (volume >= 1000000)
            {
                return $"{volume / 1000000:F1}M";
            }
            else if (volume >= 1000)
            {
                return $"{volume / 1000:F1}k";
            }
            else
            {
                return $"{volume:F0}";
            }
        }

        public void SetLookbackPeriods(int lookbackPeriods)
        {
            _lookbackPeriods = lookbackPeriods;
        }

        #region TPO

        private void DrawTPOInformation(
            VolumeProfileModel model,
            bool isDateTimeMode,
            int startIndex,
            Bars bars)
        {
            if (model.PriceLevelsList.Count == 0)
                return;

            // Draw TPO letters for each price level
            foreach (var level in model.PriceLevelsList)
            {
                // Skip levels with no TPO
                if (level.TPOCount == 0)
                    continue;

                // Determine position for TPO information
                DateTime tpoTextTime;
                if (isDateTimeMode)
                {
                    // For DateTime mode, place text to the right of profile
                    tpoTextTime = bars.OpenTimes[startIndex].Add(-_barTimeSpan * 5);
                }
                else
                {
                    // For regular mode, place text to the right of volume info
                    tpoTextTime = bars.OpenTimes[bars.Count - 1].AddDays(0);
                }

                double textPrice = (level.Price + level.UpperBound) / 2; // Middle of the level

                // Create TPO text content - now IB info will be included in this text
                string tpoText = CreateTPOText(level, isDateTimeMode);

                // Draw the TPO text
                var tpoTextObj = _chart.DrawText(
                    "TPOText_" + level.Price,
                    tpoText,
                    tpoTextTime,
                    textPrice,
                    _tpoTextColor
                );

                // Set text properties
                tpoTextObj.FontFamily = _fontFamily;
                tpoTextObj.FontSize = _fontSize;
                tpoTextObj.VerticalAlignment = VerticalAlignment.Center;

                if (isDateTimeMode)
                {
                    tpoTextObj.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else
                {
                    tpoTextObj.HorizontalAlignment = _textPosition == TextPosition.Left && _fullWidthBars == false ? HorizontalAlignment.Right: HorizontalAlignment.Left;
                }

                tpoTextObj.ZIndex = 10; // Make sure text is on top

                _chartObjectIds.Add("TPOText_" + level.Price);

                // We've removed the separate IB High/Low marker drawing code that was here
            }
        }

        // Helper method to create TPO text
        private string CreateTPOText(PriceLevel level, bool isDateTimeMode)
        {
            // Basic TPO count without letters
            string tpoText = $"TPO: {level.TPOCount}";

            // Add Initial Balance marker if applicable
            if (level.IsIBHigh)
            {
                tpoText += $"\nIB High";
            }
            else if (level.IsIBLow)
            {
                tpoText += $"\nIB Low";
            }

            return tpoText;
        }

        #endregion

        // Optimized method for getting time at a specific offset using cached values
        private DateTime GetCachedTime(Dictionary<int, DateTime> cachedTimes, int barOffset, int baseIndex, Bars bars, bool isDateTimeMode)
        {
            // If we have the exact value cached, return it directly
            if (cachedTimes.TryGetValue(barOffset, out DateTime cached))
            {
                return cached;
            }

            // Find the closest cached points for interpolation
            int lowerKey = 0;
            int upperKey = int.MaxValue;

            foreach (var key in cachedTimes.Keys)
            {
                if (key <= barOffset && key > lowerKey)
                    lowerKey = key;

                if (key >= barOffset && key < upperKey)
                    upperKey = key;
            }

            // If we don't have appropriate bounds, calculate directly
            if (lowerKey == 0 && !cachedTimes.ContainsKey(0) ||
                upperKey == int.MaxValue)
            {
                int timeIndex;
                if (isDateTimeMode)
                {
                    timeIndex = Math.Min(bars.Count - 1, baseIndex + barOffset);
                }
                else
                {
                    timeIndex = Math.Max(0, baseIndex - barOffset);
                }

                DateTime result = bars.OpenTimes[timeIndex];
                cachedTimes[barOffset] = result; // Cache for future use
                return result;
            }

            // Use the cached values
            if (lowerKey == barOffset)
                return cachedTimes[lowerKey];

            if (upperKey == barOffset)
                return cachedTimes[upperKey];

            // Calculate the actual index
            int calculatedIndex;
            if (isDateTimeMode)
            {
                calculatedIndex = Math.Min(bars.Count - 1, baseIndex + barOffset);
            }
            else
            {
                calculatedIndex = Math.Max(0, baseIndex - barOffset);
            }

            DateTime time = bars.OpenTimes[calculatedIndex];
            cachedTimes[barOffset] = time; // Cache this result
            return time;
        }
    }
}
