using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class MAController
    {
        private readonly MAModel _model;
        private readonly MAView _view;
        private readonly MAParameters _parameters;
        private readonly DataManager _dataManager;

        public MAController(MAModel model, MAView view, MAParameters parameters,
                           DataManager dataManager)
        {
            _model = model;
            _view = view;
            _parameters = parameters;
            _dataManager = dataManager;
        }

        public void Initialize(MarketData marketData, Bars bars, Chart chart)
        {
            ValidateParameters();

            try
            {
                // CRITICAL: Pass parameters to DataManager FIRST
                // This creates _mtfModel before historical calculation
                // FIX: Moved SetParameters before Initialize to prevent empty MTF model
                _dataManager.SetParameters(_parameters);

                // Use LineStyle enum instead of boolean
                bool useTrendlines = _parameters.UseTrendlines();

                // Setup multi-timeframe if needed
                bool mtfSuccess = _dataManager.Initialize(
                    _parameters.EnableMultiTimeframe,
                    _parameters.SelectedTimeframe,
                    bars.TimeFrame,
                    useTrendlines);

                if (!mtfSuccess && _parameters.EnableMultiTimeframe)
                {
                    // Multi-timeframe setup failed - will use single timeframe
                }

                // Initialize view with chart reference
                _view.Initialize(chart, bars.TimeFrame, _parameters.SelectedTimeframe);

                // Set first values for all 4 MA lines
                if (bars.Count > 0)
                {
                    double firstHigh = bars.HighPrices[0];
                    double firstLow = bars.LowPrices[0];
                    double firstClose = bars.ClosePrices[0];
                    double firstOpen = bars.OpenPrices[0];

                    _model.InitializeFirstValues(firstHigh, firstLow, firstClose, firstOpen);
                }
            }
            catch (Exception)
            {
                // Setup failed - indicator will use basic mode
            }
        }

        public void Calculate(int index, Bars bars)
        {
            var currentTime = bars.OpenTimes[index];

            // Get MA values for all lines
            MAResult maResult = _dataManager.GetAllValues(currentTime, bars, index);

            // Update view - handles both output series and trendlines
            _view.UpdateValues(index, currentTime, maResult);
        }

        private void ValidateParameters()
        {
            // Validate periods based on MA type
            switch (_parameters.MAType)
            {
                case MATypes.Simple:
                case MATypes.Exponential:
                case MATypes.Wilder:
                    if (_parameters.GeneralMAPeriod < 1)
                        _parameters.GeneralMAPeriod = 1;
                    if (_parameters.GeneralMASmoothPeriod < 1)
                        _parameters.GeneralMASmoothPeriod = 1;
                    break;

                case MATypes.DeviationScaled:
                    if (_parameters.DSMAPeriod < 5)
                        _parameters.DSMAPeriod = 5;
                    break;

                case MATypes.SuperSmoother:
                    if (_parameters.SuperSmootherPeriod < 1)
                        _parameters.SuperSmootherPeriod = 1;
                    break;

                case MATypes.Hull:
                    if (_parameters.HullMAPeriod < 1)
                        _parameters.HullMAPeriod = 1;
                    break;
            }

            // Check multi-timeframe settings
            if (_parameters.EnableMultiTimeframe)
            {
                if (_parameters.SelectedTimeframe == TimeFrame.Tick)
                {
                    _parameters.EnableMultiTimeframe = false;
                }
            }
        }
    }
}
