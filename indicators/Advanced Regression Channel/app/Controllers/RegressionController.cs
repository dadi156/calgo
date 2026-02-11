using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Main controller for Regression Indicator
    /// </summary>
    public class RegressionController
    {
        private readonly ChannelConfig _config;
        private readonly OutputCollection _outputs;
        private readonly CalculationController _calculationController;
        private readonly ChannelRenderer _channelRenderer;
        private readonly UpdateController _updateController;

        /// <summary>
        /// Creates a new instance of the RegressionController
        /// </summary>
        /// <param name="config">Configuration settings</param>
        /// <param name="outputs">Output series collection</param>
        /// <param name="chart">Chart for rendering</param>
        /// <param name="symbol">Market symbol</param>
        /// <param name="marketData">Market data for timeframe access</param>
        public RegressionController(
            ChannelConfig config,
            OutputCollection outputs,
            Chart chart,
            Symbol symbol,
            MarketData marketData,
            Regression indicator)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _outputs = outputs ?? throw new ArgumentNullException(nameof(outputs));

            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (marketData == null)
                throw new ArgumentNullException(nameof(marketData));

            if (indicator == null)
                throw new ArgumentNullException(nameof(indicator));

            _config.InitializeTimeframeBars(marketData);

            _calculationController = new CalculationController(config, symbol);
            _channelRenderer = new ChannelRenderer(outputs, config, indicator, chart);

            _updateController = new UpdateController(
                config,
                _calculationController,
                _channelRenderer
            );
        }

        /// <summary>
        /// Initialize the controller
        /// </summary>
        public void Initialize()
        {
            // Validate configuration
            _config.Validate();

            // Make sure the forming bar is always cleared
            Clear(_config.Bars.Count - 1);
        }

        /// <summary>
        /// Calculate and display regression channel for the specified bar index
        /// </summary>
        /// <param name="index">Bar index to calculate</param>
        public void Calculate(int index)
        {
            // In date range mode, we process all bars at once rather than a specific index
            if (_config.RegressionMode == RegressionMode.DateRange)
            {
                ProcessDateRangeMode();
                return;
            }

            // Skip calculation for forming bar
            if (index >= _config.Bars.Count - 1)
                return;

            // Calculate channel data
            ChannelData channelData = _calculationController.CalculateChannel(index);

            // Render channel data if it was calculated successfully
            if (channelData != null)
            {
                if (_config.UseMultiTimeframe)
                {
                    // More aggressive clearing for multi-timeframe mode
                    // Clear all values that aren't explicitly in the current calculation
                    for (int i = 0; i < _config.Bars.Count; i++) // Include forming bar
                    {
                        if (!channelData.WindowLevels.ContainsKey(i))
                        {
                            _channelRenderer.Clear(i);
                        }
                    }
                }
                else
                {
                    // Standard clearing for single timeframe mode
                    int startIndex = Math.Max(0, index - _config.Period + 1);
                    _channelRenderer.ClearValuesBefore(startIndex);
                }

                // Render the channel
                _channelRenderer.Render(channelData);
            }
        }

        /// <summary>
        /// Process calculation specifically for date range mode
        /// </summary>
        private void ProcessDateRangeMode()
        {
            // First clear all existing values
            for (int i = 0; i < _config.Bars.Count; i++)
            {
                _channelRenderer.Clear(i);
            }

            // Calculate channel data for the date range
            ChannelData channelData = _calculationController.CalculateChannel(-1); // Special index to trigger date range mode

            // Render channel data if it was calculated successfully
            if (channelData != null)
            {
                _channelRenderer.Render(channelData);
            }
        }

        /// <summary>
        /// Clear all channel values
        /// </summary>
        public void Clear(int index)
        {
            ClearBar(index);
        }

        /// <summary>
        /// Clear values for a specific bar
        /// </summary>
        /// <param name="index">Bar index to clear</param>
        public void ClearBar(int index)
        {
            if (index >= 0 && index < _config.Bars.Count)
            {
                _channelRenderer.Clear(index);
            }
        }

        /// <summary>
        /// Sets whether regression lines should extend to infinity
        /// </summary>
        /// <param name="extend">True to extend lines to infinity</param>
        public void SetExtendToInfinity(bool extend)
        {
            // Disable extend to infinity for certain regression types
            if (extend && (
                _config.RegressionType == RegressionType.Logarithmic ||
                _config.RegressionType == RegressionType.Polynomial ||
                _config.RegressionType == RegressionType.LOWESS))
            {
                extend = false;
            }

            _channelRenderer.SetExtendToInfinity(extend);
        }

        /// <summary>
        /// Updates indicator parameters
        /// </summary>
        public void UpdateParameters(
            int period,
            RegressionType regressionType,
            int degree,
            double channelWidth,
            bool useMultiTimeframe,
            TimeFrame selectedTimeFrame)
        {
            _updateController.UpdateAllParameters(
                period,
                regressionType,
                degree,
                channelWidth,
                useMultiTimeframe,
                selectedTimeFrame
            );
        }

        /// <summary>
        /// Updates period parameter
        /// </summary>
        /// <param name="period">New period</param>
        public void UpdatePeriod(int period)
        {
            _updateController.UpdatePeriod(period);
        }

        /// <summary>
        /// Updates regression type
        /// </summary>
        /// <param name="regressionType">New regression type</param>
        public void UpdateRegressionType(RegressionType regressionType)
        {
            _updateController.UpdateRegressionType(regressionType);
        }

        /// <summary>
        /// Updates channel width
        /// </summary>
        /// <param name="channelWidth">New channel width</param>
        public void UpdateChannelWidth(double channelWidth)
        {
            _updateController.UpdateChannelWidth(channelWidth);
        }
    }
}
