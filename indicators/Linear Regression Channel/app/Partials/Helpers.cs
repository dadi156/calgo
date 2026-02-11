using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class LinearRegressionChannel : Indicator
    {
        #region Update Logic

        /// <summary>
        /// Simple update detection - use model's smart logic
        /// </summary>
        private bool ShouldUpdateIndicator()
        {
            // Always update if not initially loaded
            if (!_initialDataLoaded)
            {
                return true;
            }

            // Use model's enhanced update detection
            return _model.ShouldRefreshData();
        }

        #endregion

        #region Information Display

        /// <summary>
        /// Display mode information for user awareness
        /// </summary>
        private void DisplayModeInformation()
        {
            // Get mode information from the enhanced model
            string modeInfo = _model.GetModeInfo();

            // Log to cTrader's log (optional)
            // Print($"Regression Channel Mode: {modeInfo}");

            // You can also display on chart if needed:
            // var infoText = Chart.DrawStaticText("ModeInfo", modeInfo, VerticalAlignment.Top, HorizontalAlignment.Left, Color.Gray);
            // infoText.FontSize = 8;
        }

        #endregion

        #region Public Methods for External Access

        /// <summary>
        /// Get current mode information
        /// </summary>
        public string GetCurrentModeInfo()
        {
            return _model?.GetModeInfo() ?? "Not initialized";
        }

        /// <summary>
        /// Get current data collection method
        /// </summary>
        public string GetDataCollectionMethod()
        {
            if (_model == null) return "Not initialized";

            if (_model.IsDateTimeMode())
            {
                return "DateTime Mode";
            }
            else
            {
                return "Period Mode";
            }
        }

        /// <summary>
        /// Check if lock mechanism is active
        /// </summary>
        public bool IsLockActive()
        {
            return _model?.IsLockEnabled() ?? false;
        }

        /// <summary>
        /// Get lock date if active
        /// </summary>
        public DateTime GetLockDate()
        {
            return _model?.GetLockDateTime() ?? DateTime.MinValue;
        }

        #endregion

        #region Development and Debugging Helpers

        /// <summary>
        /// Get debug information about current state
        /// </summary>
        public string GetDebugInfo()
        {
            if (_model == null) return "Model not initialized";

            var priceData = _model.GetPriceData();
            int dataCount = priceData?.Count ?? 0;

            string debugInfo = $"Mode: {GetDataCollectionMethod()}\n";
            debugInfo += $"Data Points: {dataCount}\n";

            if (IsLockActive())
            {
                debugInfo += $"Lock Date: {GetLockDate():dd/MM/yyyy HH:mm}\n";
            }

            debugInfo += $"Historical Only: {_model.GetHistoricalBarsOnly()}\n";

            return debugInfo;
        }

        /// <summary>
        /// Force refresh for testing
        /// </summary>
        public void ForceRefresh()
        {
            if (_model != null)
            {
                _model.RefreshData();
                _regressionController?.ProcessData(_model.GetPriceData());
            }
        }

        #endregion
    }
}
