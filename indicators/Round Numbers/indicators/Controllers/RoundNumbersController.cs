using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class RoundNumbersController
    {
        private readonly RoundNumbersModel _model;
        private readonly RoundNumbersView _view;
        private readonly Bars _bars;

        public RoundNumbersController(RoundNumbersModel model, RoundNumbersView view, Bars bars)
        {
            _model = model;
            _view = view;
            _bars = bars;
        }

        public void Calculate(
            int index,
            int lookbackPeriod,
            int multiples,
            int numberOfLevels,
            int extendForward,
            LineStyle lineStyle,
            int lineThickness,
            Color lineColor,
            bool enableFills,
            FillPattern fillPatternType,
            Color fillColor,
            bool enableAlternateColor,
            int alternateMultiple,
            Color alternateColor,
            bool enableHighlightLevel,
            double highlightLevelStart,
            double highlightLevelEnd,
            Color highlightColor,
            bool showLabels,
            int labelFontSize)
        {
            // Only process on the last bar
            if (index != _bars.Count - 1)
                return;

            // Clear previous drawings
            _view.ClearDrawings();

            // Calculate start index for lookback period
            int startIndex = Math.Max(0, index - lookbackPeriod + 1);

            // Get current price
            double currentPrice = _bars.ClosePrices[index];

            // Calculate price levels in model
            _model.CalculatePriceLevels(currentPrice, multiples, numberOfLevels);

            // Get start and end time for lines
            DateTime startTime = _bars.OpenTimes[startIndex];
            DateTime currentTime = _bars.OpenTimes[index];

            // Calculate extended end time
            TimeSpan barDuration = _bars.OpenTimes[index] - _bars.OpenTimes[index - 1];
            DateTime endTime = currentTime.Add(barDuration * extendForward);

            // Draw levels
            _view.DrawPriceLevels(
                _model.PriceLevels,
                startTime,
                endTime,
                lineStyle,
                lineThickness,
                lineColor,
                enableFills,
                fillPatternType,
                fillColor,
                enableAlternateColor,
                alternateMultiple,
                alternateColor,
                enableHighlightLevel,
                highlightLevelStart,
                highlightLevelEnd,
                highlightColor,
                showLabels,
                labelFontSize
            );
        }
    }
}
