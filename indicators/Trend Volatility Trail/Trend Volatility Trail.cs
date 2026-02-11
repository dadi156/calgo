// Main Indicator - MVC Pattern Entry Point
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class TrendVolatilityTrail : Indicator
    {
        // MVC Components
        private RegimeController _controller;
        
        // Data series for direction steps (needed for trend memory)
        private IndicatorDataSeries _dirStepSeries;

        protected override void Initialize()
        {
            // Create data series here (CreateDataSeries only works in Indicator class)
            _dirStepSeries = CreateDataSeries();
            
            InitializeMVCComponents();
            _controller.Initialize(Bars, base.Indicators, _dirStepSeries);
        }

        public override void Calculate(int index)
        {
            _controller.Calculate(index, Bars);
        }

        private void InitializeMVCComponents()
        {
            // Create parameters object
            var parameters = new RegimeParameters(
                MaType, MaLength,
                AtrMaType, AtrLength, BaseMultiplier,
                VolAvgMaType, VolLookback, VolPower,
                TrendMaType, TrendLookback, TrendImpact,
                MultMin, MultMax, ConfirmBars);

            // Calculate array size
            int arraySize = Bars.Count + 1000;

            // Create model
            var model = new RegimeModel(arraySize, parameters);

            // Create view
            var view = new RegimeView(BullTrail, BearTrail);

            // Create controller
            _controller = new RegimeController(model, view, parameters);
        }
    }
}
