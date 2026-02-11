using cAlgo.API;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class PivotPoints : Indicator
    {
        #region MVC Components

        private PivotPointsModel _model;
        private PivotPointsView _view;
        private PivotPointsController _controller;
        private MetricsController _metricsController;
        private bool _initialDataLoaded = false;

        #endregion

        protected override void Initialize()
        {
            // Initialize MVC components
            _model = new PivotPointsModel(PivotTimeframe, MarketData, SelectedPivotType, SRLevelsToShow, PivotToDraw);
            _view = new PivotPointsView(Chart);
            _view.SetColors(PivotLineColor, ResistanceLineColor, SupportLineColor, ShowPriceInLabels);
            _controller = new PivotPointsController(_model, _view);

            // Initialize metrics controller
            _metricsController = new MetricsController(this);

            // Initial data load
            _model.RefreshData();
            _controller.UpdateView();
            _metricsController.Update(_model.GetPeriodForMetrics(MetricsOffset));
            _initialDataLoaded = true;
        }

        public override void Calculate(int index)
        {
            // Only update on the last index (current bar)
            if (index == Bars.ClosePrices.Count - 1)
            {
                // Check if a new bar has formed in the selected timeframe
                bool newBarFormed = _model.CheckForNewBar();

                // Refresh data if:
                // 1. This is the first calculation, OR
                // 2. We're on the selected timeframe, OR
                // 3. A new bar has formed in the selected timeframe
                if (!_initialDataLoaded || Bars.TimeFrame == PivotTimeframe || newBarFormed)
                {
                    _model.RefreshData();
                    _initialDataLoaded = true;
                }

                // Always update the view with the latest data
                _controller.UpdateView();
                _metricsController.Update(_model.GetPeriodForMetrics(MetricsOffset));
            }
        }
    }
}
