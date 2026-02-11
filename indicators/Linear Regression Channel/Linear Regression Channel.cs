using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class LinearRegressionChannel : Indicator
    {
        #region MVC Components

        private PriceTrackingModel _model;
        private RegressionModel _regressionModel;
        private RegressionView _regressionView;
        private RegressionController _regressionController;
        private bool _initialDataLoaded = false;

        #endregion

        protected override void Initialize()
        {
            // Initialize enhanced model
            _model = new PriceTrackingModel(
                Periods,
                SelectedTimeFrame,
                Bars,
                Symbol,
                StartPointMethod,
                HistoricalBarsOnly,
                EnableLock,
                LockDate,
                StartDate
            );

            // Pass MarketData for accessing different timeframes
            _model.UpdateMarketData(MarketData);

            // Initialize regression components
            _regressionModel = new RegressionModel();
            _regressionView = new RegressionView(Chart, Symbol);
            _regressionController = new RegressionController(_regressionModel, _regressionView);

            // Configure regression channel
            _regressionController.SetPriceType(SelectedPriceType);

            // Set deviation calculation method
            _regressionController.SetDeviationMethod(SelectedDeviationMethod);

            // Set StdDev multiplier
            _regressionController.SetStdDevMultiplier(StdDevMultiplier);

            // Set ATR parameters
            _regressionController.SetATRMultiplier(ATRMultiplier);

            // FIXED: Pass HistoricalBarsOnly setting to controller for proper line extension
            _regressionController.SetHistoricalBarsOnly(_model.GetHistoricalBarsOnly());

            _regressionController.SetFibonacciLevels(ShowLevel114, ShowLevel236, ShowLevel382, ShowLevel618, ShowLevel786, ShowLevel886);
            _regressionController.SetExtendToInfinity(ExtendRegressionToInfinity);

            // Updated: Direct Color usage without conversion
            _regressionController.SetChannelColors(
                RegressionLineColor,
                UpperLineColor,
                LowerLineColor);

            _regressionController.SetFibonacciColor(FibonacciLinesColor);

            // Initial data load
            _model.RefreshData();

            // Update regression with initial data
            _regressionController.ProcessData(_model.GetPriceData());

            _initialDataLoaded = true;

            // Display mode information
            DisplayModeInformation();
        }

        public override void Calculate(int index)
        {
            // Only update on the last index (current bar)
            if (index == Bars.ClosePrices.Count - 1)
            {
                // Always pass the MarketData object to the model
                _model.UpdateMarketData(MarketData);

                // Check if data should be refreshed
                bool shouldUpdate = ShouldUpdateIndicator();

                if (shouldUpdate)
                {
                    // Update deviation method in case it changed
                    _regressionController.SetDeviationMethod(SelectedDeviationMethod);

                    // Update StdDev multiplier in case it changed
                    _regressionController.SetStdDevMultiplier(StdDevMultiplier);

                    // Update ATR parameters in case they changed
                    _regressionController.SetATRMultiplier(ATRMultiplier);

                    // FIXED: Update HistoricalBarsOnly setting in case it changed
                    _regressionController.SetHistoricalBarsOnly(_model.GetHistoricalBarsOnly());

                    // Refresh data and update visualization
                    _model.RefreshData();
                    _regressionController.ProcessData(_model.GetPriceData());
                }
            }
        }
    }
}
