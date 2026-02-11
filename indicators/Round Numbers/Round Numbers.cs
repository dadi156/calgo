using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class RoundNumbers : Indicator
    {
        private RoundNumbersController _controller;

        protected override void Initialize()
        {
            var model = new RoundNumbersModel(Symbol);
            var view = new RoundNumbersView(Chart, Symbol);
            _controller = new RoundNumbersController(model, view, Bars);
        }

        public override void Calculate(int index)
        {
            _controller.Calculate(
                index,
                LookbackPeriod,
                Multiples,
                NumberOfLevels,
                ExtendForward,
                LineStyle,
                LineThickness,
                LineColor,
                EnableFills,
                FillPatternType,
                FillColor,
                EnableAlternateColor,
                AlternateMultiple,
                AlternateColor,
                EnableHighlightLevel,
                HighlightLevelStart,
                HighlightLevelEnd,
                HighlightColor,
                ShowLabels,
                LabelFontSize
            );
        }
    }
}
