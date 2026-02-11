using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Controller for handling updates to indicator parameters
    /// </summary>
    public class UpdateController
    {
        private readonly ChannelConfig _config;
        private readonly CalculationController _calculationController;
        private readonly ChannelRenderer _channelRenderer;

        /// <summary>
        /// Creates a new update controller
        /// </summary>
        /// <param name="config">Channel configuration</param>
        /// <param name="calculationController">Calculation controller</param>
        /// <param name="channelRenderer">Channel renderer</param>
        public UpdateController(
            ChannelConfig config,
            CalculationController calculationController,
            ChannelRenderer channelRenderer)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _calculationController = calculationController ?? throw new ArgumentNullException(nameof(calculationController));
            _channelRenderer = channelRenderer ?? throw new ArgumentNullException(nameof(channelRenderer));
        }

        /// <summary>
        /// Updates period parameter
        /// </summary>
        /// <param name="period">New period value</param>
        public void UpdatePeriod(int period)
        {
            if (period <= 0)
                throw new ArgumentException("Period must be positive", nameof(period));

            _config.Period = period;
            UpdateConfiguration();
        }

        /// <summary>
        /// Updates regression type parameter
        /// </summary>
        /// <param name="regressionType">New regression type</param>
        public void UpdateRegressionType(RegressionType regressionType)
        {
            _config.RegressionType = regressionType;
            UpdateConfiguration();
        }

        /// <summary>
        /// Updates polynomial degree parameter
        /// </summary>
        /// <param name="degree">New degree value</param>
        public void UpdateDegree(int degree)
        {
            if (_config.RegressionType == RegressionType.Polynomial && (degree < 1 || degree > 5))
                throw new ArgumentException("Polynomial degree must be between 1 and 5", nameof(degree));

            _config.Degree = degree;
            UpdateConfiguration();
        }

        /// <summary>
        /// Updates channel width parameter
        /// </summary>
        /// <param name="channelWidth">New channel width</param>
        public void UpdateChannelWidth(double channelWidth)
        {
            if (channelWidth <= 0)
                throw new ArgumentException("Channel width must be positive", nameof(channelWidth));

            _config.ChannelWidth = channelWidth;
            UpdateConfiguration();
        }

        /// <summary>
        /// Updates multi-timeframe settings
        /// </summary>
        /// <param name="useMultiTimeframe">Whether to use multi-timeframe</param>
        /// <param name="selectedTimeFrame">Selected timeframe</param>
        /// <param name="useInterpolation">Whether to use interpolation</param>
        public void UpdateMultiTimeframeSettings(bool useMultiTimeframe, TimeFrame selectedTimeFrame)
        {
            _config.UseMultiTimeframe = useMultiTimeframe;
            _config.SelectedTimeFrame = selectedTimeFrame;

            UpdateConfiguration();
        }

        /// <summary>
        /// Updates all parameters at once
        /// </summary>
        /// <param name="period">New period</param>
        /// <param name="regressionType">New regression type</param>
        /// <param name="degree">New polynomial degree</param>
        /// <param name="channelWidth">New channel width</param>
        /// <param name="useMultiTimeframe">Whether to use multi-timeframe</param>
        /// <param name="selectedTimeFrame">Selected timeframe</param>
        /// <param name="useInterpolation">Whether to use interpolation</param>
        /// <param name="showFib100">Show 100% level</param>
        /// <param name="showFib886">Show 88.6% level</param>
        /// <param name="showFib764">Show 76.4% level</param>
        /// <param name="showFib618">Show 61.8% level</param>
        /// <param name="showFib50">Show 50% level</param>
        /// <param name="showFib382">Show 38.2% level</param>
        /// <param name="showFib236">Show 23.6% level</param>
        /// <param name="showFib114">Show 11.4% level</param>
        /// <param name="showFib0">Show 0% level</param>
        public void UpdateAllParameters(
            int period,
            RegressionType regressionType,
            int degree,
            double channelWidth,
            bool useMultiTimeframe,
            TimeFrame selectedTimeFrame)
        {
            // Validate parameters
            if (period <= 0)
                throw new ArgumentException("Period must be positive", nameof(period));

            if (regressionType == RegressionType.Polynomial && (degree < 1 || degree > 5))
                throw new ArgumentException("Polynomial degree must be between 1 and 5", nameof(degree));

            if (channelWidth <= 0)
                throw new ArgumentException("Channel width must be positive", nameof(channelWidth));

            // Update configuration
            _config.Period = period;
            _config.RegressionType = regressionType;
            _config.Degree = degree;
            _config.ChannelWidth = channelWidth;
            _config.UseMultiTimeframe = useMultiTimeframe;
            _config.SelectedTimeFrame = selectedTimeFrame;

            UpdateConfiguration();
        }

        /// <summary>
        /// Updates configuration in all controllers
        /// </summary>
        private void UpdateConfiguration()
        {
            // Update calculation controller
            _calculationController.UpdateConfiguration(_config);

            // Force redraw
            ForceRedraw();
        }

        /// <summary>
        /// Forces a redraw of the indicator by clearing all visible bars
        /// </summary>
        private void ForceRedraw()
        {
            // Force redraw by clearing all visible bars
            for (int i = 0; i < _config.Bars.Count; i++)
            {
                _channelRenderer.Clear(i);
            }
        }
    }
}
