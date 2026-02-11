using System;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Controller responsible for performing regression and channel calculations
    /// </summary>
    public class CalculationController
    {
        private readonly ChannelConfig _config;
        private readonly Symbol _symbol;
        private readonly ChannelCalculationService _channelService;
        private readonly TimeframeDataProvider _dataProvider;

        public CalculationController(ChannelConfig config, Symbol symbol)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            
            // Validate configuration before proceeding
            _config.Validate();
            
            // Create data provider
            _dataProvider = new TimeframeDataProvider(_config);
            
            // Create services
            _channelService = new ChannelCalculationService(_config, _dataProvider);
        }
        
        /// <summary>
        /// Performs calculations for a specific bar index
        /// </summary>
        /// <param name="index">Bar index to calculate</param>
        /// <returns>Calculated channel data, or null if unable to calculate</returns>
        public ChannelData CalculateChannel(int index)
        {
            return _channelService.CalculateChannel(index);
        }
        
        /// <summary>
        /// Updates controller with new configuration
        /// </summary>
        /// <param name="config">New configuration</param>
        public void UpdateConfiguration(ChannelConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            
            // Update config
            _config.Period = config.Period;
            _config.RegressionType = config.RegressionType;
            _config.Degree = config.Degree;
            _config.ChannelWidth = config.ChannelWidth;
            _config.UseMultiTimeframe = config.UseMultiTimeframe;
            _config.SelectedTimeFrame = config.SelectedTimeFrame;
            _config.RegressionMode = config.RegressionMode;
            
            // Update date range if in date range mode
            if (config.RegressionMode == RegressionMode.DateRange)
            {
                _config.StartDate = config.StartDate;
                _config.EndDate = config.EndDate;
            }
            
            // Update channel service
            _channelService.UpdateConfig(_config);
        }
        
        /// <summary>
        /// Sets the date range for custom regression range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        public void SetDateRange(DateTime startDate, DateTime endDate)
        {
            _config.SetDateRange(startDate, endDate);
            _channelService.ClearCache();
        }
        
        /// <summary>
        /// Sets the regression mode
        /// </summary>
        /// <param name="mode">Regression mode</param>
        public void SetRegressionMode(RegressionMode mode)
        {
            if (_config.RegressionMode != mode)
            {
                _config.RegressionMode = mode;
                _channelService.ClearCache();
            }
        }
    }
}
