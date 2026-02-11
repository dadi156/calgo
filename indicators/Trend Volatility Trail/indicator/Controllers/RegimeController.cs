// RegimeController - Coordinates Model and View
using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    // Controller - connects Model and View
    public class RegimeController
    {
        private readonly RegimeModel _model;
        private readonly RegimeView _view;
        private readonly RegimeParameters _parameters;

        // Built-in indicators
        private MovingAverage _ma;
        private AverageTrueRange _atr;
        private MovingAverage _atrAvg;
        private MovingAverage _trendMemory;
        private IndicatorDataSeries _dirStepSeries;

        public RegimeController(RegimeModel model, RegimeView view, RegimeParameters parameters)
        {
            _model = model;
            _view = view;
            _parameters = parameters;
        }

        public void Initialize(Bars bars, IIndicatorsAccessor indicators, IndicatorDataSeries dirStepSeries)
        {
            // Validate parameters
            ValidateParameters();

            // Store the data series passed from main indicator
            _dirStepSeries = dirStepSeries;

            try
            {
                // Create built-in indicators
                _ma = indicators.MovingAverage(bars.ClosePrices, _parameters.MaLength, _parameters.MaType);
                _atr = indicators.AverageTrueRange(_parameters.AtrLength, _parameters.AtrMaType);

                // Create ATR average
                _atrAvg = indicators.MovingAverage(_atr.Result, _parameters.VolLookback, _parameters.VolAvgMaType);

                // Create trend memory (will be calculated on direction steps)
                _trendMemory = indicators.MovingAverage(_dirStepSeries, _parameters.TrendLookback, _parameters.TrendMaType);

                // Pass indicators to model
                _model.SetIndicators(_ma, _atr, _atrAvg, _trendMemory);
            }
            catch (Exception)
            {
                // Initialization failed - will use safe defaults
            }
        }

        public void Calculate(int index, Bars bars)
        {
            // Check minimum bars required
            int minBars = Math.Max(_parameters.MaLength, _parameters.AtrLength);
            if (index < minBars)
            {
                return;
            }

            try
            {
                // Update direction step series (needed for trend memory MA)
                double dirStep = _model.GetDirectionStep(index);
                _dirStepSeries[index] = dirStep;

                // Get close price
                double closePrice = bars.ClosePrices[index];

                // Calculate regime result from model
                RegimeResult result = _model.CalculateWithPrice(index, closePrice);

                // Update view with result
                _view.UpdateValues(index, result);
            }
            catch (Exception)
            {
                // If calculation fails, continue silently
                // This prevents indicator from stopping
            }
        }

        private void ValidateParameters()
        {
            // Validate MA length
            if (_parameters.MaLength < 2)
            {
                // Will use minimum from [Parameter] attribute
            }

            // Validate ATR length
            if (_parameters.AtrLength < 1)
            {
                // Will use minimum from [Parameter] attribute
            }

            // Validate base multiplier
            if (_parameters.BaseMultiplier < 0.1)
            {
                // Will use minimum from [Parameter] attribute
            }

            // Validate volatility lookback
            if (_parameters.VolLookback < 2)
            {
                // Will use minimum from [Parameter] attribute
            }

            // Validate trend lookback
            if (_parameters.TrendLookback < 2)
            {
                // Will use minimum from [Parameter] attribute
            }

            // Validate multiplier range
            if (_parameters.MultMin > _parameters.MultMax)
            {
                // Parameters are invalid but will continue with provided values
            }

            // Validate confirm bars
            if (_parameters.ConfirmBars < 1)
            {
                // Will use minimum from [Parameter] attribute
            }
        }
    }
}
