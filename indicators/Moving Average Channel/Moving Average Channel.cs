using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class MovingAverageChannel : Indicator
    {
        // MVC Components
        private MAController _controller;

        protected override void Initialize()
        {
            InitializeMVCComponents();
            _controller.Initialize(MarketData, Bars, Chart);
        }

        public override void Calculate(int index)
        {
            _controller.Calculate(index, Bars);
        }

        private void InitializeMVCComponents()
        {
            // Create parameters with all MA settings (includes Hull MA period)
            var parameters = new MAParameters(
                MAType,                     // MA Type selector
                GeneralMAPeriod,            // General MA period (for Simple/Exponential/Wilder)
                GeneralMASmoothPeriod,      // General MA smooth period (for Simple/Exponential only)
                DSMAPeriod,                 // DSMA period
                SuperSmootherPeriod,        // SuperSmoother period
                HullMAPeriod,               // Hull MA period
                EnableMultiTimeframe,       // MTF enabled?
                SelectedTimeframe,          // MTF timeframe
                LineStyle,                  // Line style (StairSteps/TrendLines)
                ShowProjections);           // Show projections?

            // Create model
            int arraySize = CalculateArraySize();
            var model = new MAModel(arraySize, parameters);

            // Create services
            var dataManager = new DataManager(MarketData, model);

            // Create view - Now pass 7 lines (OHLC + Median + 2 Reversion Zones)
            var view = new MAView(HighLine, LowLine, CloseLine, OpenLine, MedianLine,
                                LowerReversionZone, UpperReversionZone,
                                Chart, parameters, this, dataManager);

            // Create controller
            _controller = new MAController(model, view, parameters, dataManager);
        }

        private int CalculateArraySize()
        {
            var workingBars = EnableMultiTimeframe && SelectedTimeframe != Bars.TimeFrame ?
                             MarketData.GetBars(SelectedTimeframe) : Bars;

            return (workingBars?.Count ?? Bars.Count) + 1000;
        }
    }
}
