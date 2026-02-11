using System;
using cAlgo.API;

namespace cAlgo
{
    [Indicator(AccessRights = AccessRights.None, IsOverlay = true, AutoRescale = false, TimeZone = TimeZones.UTC)]
    public partial class AdvancedRegressionChannel : Indicator
    {
        protected override void Initialize()
        {
            // Calculate timezone offset
            DateTime utcNow = Server.TimeInUtc;
            DateTime serverNow = Server.Time;
            DateTime userLocalNow = utcNow.AddHours(UserTimezone);
            _serverToUserOffset = (int)Math.Round((serverNow - userLocalNow).TotalHours);

            // Determine regression mode based on UseDateRange parameter (independent of Period)
            RegressionMode mode = UseDateRange ? RegressionMode.DateRange : RegressionMode.Periods;

            // Parse date strings if date range mode is enabled
            if (mode == RegressionMode.DateRange)
            {
                try
                {
                    DateTime startLocal = DateTimeUtils.ParseDate(StartDateStr);
                    DateTime endLocal = DateTimeUtils.ParseDate(EndDateStr);

                    // Convert from user local time to server time
                    _startDate = ConvertUserLocalToServer(startLocal);
                    _endDate = ConvertUserLocalToServer(endLocal);

                    // If either date is invalid, fall back to period mode
                    if (_startDate == DateTime.MinValue || _endDate == DateTime.MinValue)
                    {
                        mode = RegressionMode.Periods;
                        UseDateRange = false;
                    }
                }
                catch (Exception)
                {
                    // If parsing fails, revert to period mode
                    mode = RegressionMode.Periods;
                    UseDateRange = false;
                }
            }

            // If date range mode is active, disable multi-timeframe mode
            bool actualUseMultiTimeframe = UseDateRange ? false : UseMultiTimeframe;

            _config = CreateConfigFromParameters();

            // Override multi-timeframe setting if date range is active
            _config.UseMultiTimeframe = actualUseMultiTimeframe;

            // Set date range if using date mode
            if (mode == RegressionMode.DateRange && _startDate != DateTime.MinValue && _endDate != DateTime.MaxValue)
            {
                _config.SetDateRange(_startDate, _endDate);
                _config.RegressionMode = mode;
            }

            _outputs = CreateOutputCollection();

            // Create and initialize the controller
            _controller = new RegressionController(
                _config,
                _outputs,
                Chart,
                Symbol,
                MarketData,
                this  // Pass indicator reference
            );

            _controller.Initialize();

            // Set the extend to infinity option
            _controller.SetExtendToInfinity(ExtendToInfinity);

            // Store initial parameter values for change detection
            StoreParameterValues();

            // Process historical data (skip current forming bar for calculation)
            for (int i = 0; i < Bars.Count - 1; i++)
            {
                ProcessBar(i);
            }

            // Store the last processed index
            _lastProcessedIndex = Bars.Count - 2;
        }

        public override void Calculate(int index)
        {
            // Check for parameter changes
            if (ParametersChanged())
            {
                // Check if we need to update dates or regression mode
                if (_lastStartDateStr != StartDateStr || _lastEndDateStr != EndDateStr || _lastUseDateRange != UseDateRange)
                {
                    // Determine regression mode based on UseDateRange parameter
                    RegressionMode mode = UseDateRange ? RegressionMode.DateRange : RegressionMode.Periods;

                    // Parse date strings if date range mode is enabled
                    if (mode == RegressionMode.DateRange)
                    {
                        try
                        {
                            DateTime startLocal = DateTimeUtils.ParseDate(StartDateStr);
                            DateTime endLocal = DateTimeUtils.ParseDate(EndDateStr);

                            _startDate = ConvertUserLocalToServer(startLocal);
                            _endDate = ConvertUserLocalToServer(endLocal);

                            // If either date is invalid, fall back to period mode
                            if (_startDate == DateTime.MinValue || _endDate == DateTime.MinValue)
                            {
                                mode = RegressionMode.Periods;
                                UseDateRange = false;
                            }
                            else
                            {
                                // Update config with new date range
                                _config.SetDateRange(_startDate, _endDate);
                            }
                        }
                        catch (Exception)
                        {
                            // If parsing fails, revert to period mode
                            mode = RegressionMode.Periods;
                            UseDateRange = false;
                            _config.RegressionMode = mode;
                        }
                    }
                    else
                    {
                        // Using period mode
                        _config.RegressionMode = mode;
                    }

                    _lastStartDateStr = StartDateStr;
                    _lastEndDateStr = EndDateStr;
                    _lastUseDateRange = UseDateRange;
                    _lastRegressionMode = mode;
                }

                // If date range mode is active, disable multi-timeframe mode
                bool actualUseMultiTimeframe = UseDateRange ? false : UseMultiTimeframe;

                // Update controller with new parameters
                _controller.UpdateParameters(
                    Period,
                    RegressionType,
                    Degree,
                    ChannelWidth,
                    actualUseMultiTimeframe,  // Use the modified value
                    SelectedTimeFrame
                );

                // Update extend to infinity option if it changed
                if (_lastExtendToInfinity != ExtendToInfinity)
                {
                    _controller.SetExtendToInfinity(ExtendToInfinity);
                    _lastExtendToInfinity = ExtendToInfinity;
                }

                // Store new parameter values
                StoreParameterValues();

                // Reset the last processed index
                _lastProcessedIndex = -1;

                // Clear all values
                for (int i = 0; i < Bars.Count; i++)
                {
                    _controller.ClearBar(i);
                }

                // Recalculate historical bars only (skip current forming bar)
                for (int i = 0; i < Bars.Count - 1; i++)
                {
                    ProcessBar(i);
                }

                // Update last processed index
                _lastProcessedIndex = Bars.Count - 2;
            }
            else
            {
                // Process any bars that haven't been processed yet
                // This ensures that when a previously forming bar becomes complete, it will be processed
                for (int i = _lastProcessedIndex + 1; i < Bars.Count - 1; i++)
                {
                    ProcessBar(i);
                    _lastProcessedIndex = i;
                }

                // Force trendline refresh to pick up any color/style changes
                if (ExtendToInfinity && _lastProcessedIndex >= 0)
                {
                    ProcessBar(_lastProcessedIndex);
                }
            }
        }
    }
}
