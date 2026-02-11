using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class AdvancedRegressionChannel : Indicator
    {
        /// <summary>
        /// Process a single bar
        /// </summary>
        private void ProcessBar(int index)
        {
            // Perform calculation for this index
            _controller.Calculate(index);
        }

        /// <summary>
        /// Creates configuration from indicator parameters
        /// </summary>
        private ChannelConfig CreateConfigFromParameters()
        {
            // If date range mode is active, disable multi-timeframe mode
            bool actualUseMultiTimeframe = UseDateRange ? false : UseMultiTimeframe;

            return new ChannelConfig(Bars)
            {
                Period = Period,
                Degree = Degree,
                ChannelWidth = ChannelWidth,
                RegressionType = RegressionType,
                UseMultiTimeframe = actualUseMultiTimeframe,  // Use the modified value
                SelectedTimeFrame = SelectedTimeFrame
            };
        }

        /// <summary>
        /// Creates output collection for the indicator
        /// </summary>
        private OutputCollection CreateOutputCollection()
        {
            return new OutputCollection(
                Fib100,
                Level886,
                Fib764,
                Fib618,
                Fib50,
                Fib382,
                Fib236,
                Level114,
                Fib0
            );
        }

        /// <summary>
        /// Stores current parameter values for change detection
        /// </summary>
        private void StoreParameterValues()
        {
            _lastPeriod = Period;
            _lastRegressionType = RegressionType;
            _lastDegree = Degree;
            _lastChannelWidth = ChannelWidth;
            _lastUseMultiTimeframe = UseMultiTimeframe;
            _lastSelectedTimeFrame = SelectedTimeFrame;
            _lastExtendToInfinity = ExtendToInfinity;
            _lastStartDateStr = StartDateStr;
            _lastEndDateStr = EndDateStr;
            _lastRegressionMode = _config.RegressionMode;
            _lastUseDateRange = UseDateRange;
        }

        /// <summary>
        /// Checks if any parameters have changed
        /// </summary>
        /// <returns>True if parameters have changed</returns>
        private bool ParametersChanged()
        {
            return
                _lastPeriod != Period ||
                _lastRegressionType != RegressionType ||
                _lastDegree != Degree ||
                Math.Abs(_lastChannelWidth - ChannelWidth) > 0.0001 ||
                _lastUseMultiTimeframe != UseMultiTimeframe ||
                _lastSelectedTimeFrame != SelectedTimeFrame ||
                _lastExtendToInfinity != ExtendToInfinity ||
                _lastStartDateStr != StartDateStr ||
                _lastEndDateStr != EndDateStr ||
                _lastUseDateRange != UseDateRange;
        }

        /// <summary>
        /// Converts user local time to server time
        /// </summary>
        private DateTime ConvertUserLocalToServer(DateTime userLocalTime)
        {
            return userLocalTime.AddHours(_serverToUserOffset);
        }
    }
}
