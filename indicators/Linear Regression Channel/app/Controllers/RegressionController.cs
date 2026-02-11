using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class RegressionController
    {
        private RegressionModel _model;
        private RegressionView _view;

        public RegressionController(RegressionModel model, RegressionView view)
        {
            _model = model;
            _view = view;
        }

        #region Data Processing

        /// <summary>
        /// Simple data processing - clean and fast
        /// </summary>
        public void ProcessData(List<OHLC> priceData)
        {
            if (priceData == null || priceData.Count < 2)
            {
                // Clear view if no valid data
                _view.ClearLines();
                return;
            }

            // Validate data quality
            List<OHLC> validData = ValidateDataQuality(priceData);

            // Set price data in the model
            _model.SetPriceData(validData);

            // Calculate regression
            _model.CalculateRegression();

            // Get regression channel data
            var channelData = _model.GetRegressionChannelData();

            if (channelData != null)
            {
                // Update view with regression channel
                _view.DrawRegressionChannel(channelData);
            }
            else
            {
                // Clear view if calculation failed
                _view.ClearLines();
            }
        }

        /// <summary>
        /// Validate data quality and remove invalid entries
        /// </summary>
        private List<OHLC> ValidateDataQuality(List<OHLC> data)
        {
            if (data == null || data.Count == 0)
                return new List<OHLC>();

            List<OHLC> validData = new List<OHLC>();

            foreach (var item in data)
            {
                // Check for valid OHLC data
                if (IsValidOHLCData(item))
                {
                    validData.Add(item);
                }
            }

            // Ensure we have at least 2 valid data points
            if (validData.Count < 2 && data.Count >= 2)
            {
                // If validation removed too much data, keep original data
                return data;
            }

            return validData;
        }

        /// <summary>
        /// Check if OHLC data is valid
        /// </summary>
        private bool IsValidOHLCData(OHLC data)
        {
            if (data == null) return false;

            // Check for non-zero prices
            if (data.Open <= 0 || data.High <= 0 || data.Low <= 0 || data.Close <= 0)
                return false;

            // Check for logical OHLC relationships
            if (data.High < data.Low) return false;
            if (data.High < data.Open || data.High < data.Close) return false;
            if (data.Low > data.Open || data.Low > data.Close) return false;

            // Check for valid date
            if (data.Time == DateTime.MinValue) return false;

            return true;
        }

        #endregion

        #region Configuration Methods

        public void SetChannelColors(Color regressionLineColor, Color upperLineColor, Color lowerLineColor)
        {
            _view.SetChannelColors(regressionLineColor, upperLineColor, lowerLineColor);
        }

        public void SetPriceType(PriceType priceType)
        {
            _model.SetPriceType(priceType);
            _view.SetPriceType(priceType);
        }

        /// <summary>
        /// NEW: Set deviation calculation method
        /// </summary>
        public void SetDeviationMethod(DeviationMethod deviationMethod)
        {
            _model.SetDeviationMethod(deviationMethod);
        }

        /// <summary>
        /// Set StdDev multiplier for Standard Deviation method
        /// </summary>
        public void SetStdDevMultiplier(double multiplier)
        {
            _model.SetStdDevMultiplier(multiplier);
        }

        /// <summary>
        /// Set ATR multiplier for ATR-based deviation method
        /// </summary>
        public void SetATRMultiplier(double multiplier)
        {
            _model.SetATRMultiplier(multiplier);
        }

        /// <summary>
        /// NEW: Set HistoricalBarsOnly setting for proper line extension
        /// </summary>
        public void SetHistoricalBarsOnly(bool historicalBarsOnly)
        {
            _model.SetHistoricalBarsOnly(historicalBarsOnly);
        }

        public void SetFibonacciLevels(bool level114, bool level236, bool level382, bool level618, bool level786, bool level886)
        {
            _view.SetFibonacciLevels(level114, level236, level382, level618, level786, level886);
        }

        public void SetFibonacciColor(Color fibLinesColor)
        {
            _view.SetFibonacciColor(fibLinesColor);
        }

        public void SetExtendToInfinity(bool extend)
        {
            _view.SetExtendToInfinity(extend);
        }

        #endregion

        #region Utility Methods

        public void Clear()
        {
            _view.ClearLines();
        }

        /// <summary>
        /// Get information about current data processing
        /// </summary>
        public string GetProcessingInfo()
        {
            var channelData = _model.GetRegressionChannelData();
            if (channelData == null)
                return "No data processed";

            string info = $"Data processed: {channelData.BarCount} bars\n";
            info += $"Time range: {channelData.DataStartTime:dd/MM/yyyy} to {channelData.DataEndTime:dd/MM/yyyy}\n";
            info += $"Slope: {channelData.Slope:F6}\n";
            info += $"Method: {channelData.DeviationMethod}\n";

            if (channelData.DeviationMethod == DeviationMethod.Average)
            {
                info += $"Upper width: {channelData.UpperChannelWidth:F5}\n";
                info += $"Lower width: {channelData.LowerChannelWidth:F5}";
            }
            else
            {
                info += $"Channel width: {channelData.ChannelWidth:F5}";
            }

            return info;
        }

        /// <summary>
        /// Force recalculation
        /// </summary>
        public void ForceRecalculation()
        {
            _model.CalculateRegression();
            var channelData = _model.GetRegressionChannelData();
            if (channelData != null)
            {
                _view.DrawRegressionChannel(channelData);
            }
        }

        #endregion
    }
}
