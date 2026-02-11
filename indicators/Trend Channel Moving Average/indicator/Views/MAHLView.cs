using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// View for TrendChannelMovingAverage indicator with trend-based line display
    /// </summary>
    public class MAHLView
    {
        // Fixed lines (always show)
        private readonly IndicatorDataSeries _openLine;
        private readonly IndicatorDataSeries _closeLine;
        private readonly IndicatorDataSeries _medianLine;
        
        // Dynamic High lines (show based on trend)
        private readonly IndicatorDataSeries _highLineUptrend;
        private readonly IndicatorDataSeries _highLineDowntrend;
        private readonly IndicatorDataSeries _highLineNeutral;
        
        // Dynamic Low lines (show based on trend)
        private readonly IndicatorDataSeries _lowLineUptrend;
        private readonly IndicatorDataSeries _lowLineDowntrend;
        private readonly IndicatorDataSeries _lowLineNeutral;

        public MAHLView(IndicatorDataSeries openLine, IndicatorDataSeries closeLine, IndicatorDataSeries medianLine,
                        IndicatorDataSeries highLineUptrend, IndicatorDataSeries highLineDowntrend, IndicatorDataSeries highLineNeutral,
                        IndicatorDataSeries lowLineUptrend, IndicatorDataSeries lowLineDowntrend, IndicatorDataSeries lowLineNeutral)
        {
            _openLine = openLine;
            _closeLine = closeLine;
            _medianLine = medianLine;
            
            _highLineUptrend = highLineUptrend;
            _highLineDowntrend = highLineDowntrend;
            _highLineNeutral = highLineNeutral;
            
            _lowLineUptrend = lowLineUptrend;
            _lowLineDowntrend = lowLineDowntrend;
            _lowLineNeutral = lowLineNeutral;
        }

        /// <summary>
        /// Update all lines with dynamic colors and display mode
        /// </summary>
        public void UpdateOutputs(int index, CachedValues values, LineDisplayMode displayMode)
        {
            // Update fixed lines (always show)
            _openLine[index] = values.Open;
            _closeLine[index] = values.Close;
            _medianLine[index] = values.Median;
            
            TrendDirection trend = values.Trend;
            
            // Update High and Low lines based on display mode
            if (displayMode == LineDisplayMode.Channel)
            {
                UpdateHighLinesChannel(index, values.High, trend);
                UpdateLowLinesChannel(index, values.Low, trend);
            }
            else // TrendBased mode
            {
                UpdateHighLinesTrendBased(index, values.High, trend);
                UpdateLowLinesTrendBased(index, values.Low, trend);
            }
        }

        /// <summary>
        /// Update outputs with default channel mode
        /// </summary>
        public void UpdateOutputs(int index, CachedValues values)
        {
            UpdateOutputs(index, values, LineDisplayMode.Channel);
        }

        /// <summary>
        /// Channel mode: Update High lines (always show)
        /// </summary>
        private void UpdateHighLinesChannel(int index, double highValue, TrendDirection trend)
        {
            _highLineUptrend[index] = double.NaN;
            _highLineDowntrend[index] = double.NaN;
            _highLineNeutral[index] = double.NaN;
            
            switch (trend)
            {
                case TrendDirection.Uptrend:
                    _highLineUptrend[index] = highValue;
                    break;
                    
                case TrendDirection.Downtrend:
                    _highLineDowntrend[index] = highValue;
                    break;
                    
                case TrendDirection.Neutral:
                    _highLineNeutral[index] = highValue;
                    break;
            }
        }

        /// <summary>
        /// Channel mode: Update Low lines (always show)
        /// </summary>
        private void UpdateLowLinesChannel(int index, double lowValue, TrendDirection trend)
        {
            _lowLineUptrend[index] = double.NaN;
            _lowLineDowntrend[index] = double.NaN;
            _lowLineNeutral[index] = double.NaN;
            
            switch (trend)
            {
                case TrendDirection.Uptrend:
                    _lowLineUptrend[index] = lowValue;
                    break;
                    
                case TrendDirection.Downtrend:
                    _lowLineDowntrend[index] = lowValue;
                    break;
                    
                case TrendDirection.Neutral:
                    _lowLineNeutral[index] = lowValue;
                    break;
            }
        }

        /// <summary>
        /// Trend-based mode: Update High lines (hide/show based on trend)
        /// </summary>
        private void UpdateHighLinesTrendBased(int index, double highValue, TrendDirection trend)
        {
            _highLineUptrend[index] = double.NaN;
            _highLineDowntrend[index] = double.NaN;
            _highLineNeutral[index] = double.NaN;
            
            switch (trend)
            {
                case TrendDirection.Uptrend:
                    // Hide high line in uptrend
                    break;
                    
                case TrendDirection.Downtrend:
                    _highLineDowntrend[index] = highValue;
                    break;
                    
                case TrendDirection.Neutral:
                    _highLineNeutral[index] = highValue;
                    break;
            }
        }

        /// <summary>
        /// Trend-based mode: Update Low lines (hide/show based on trend)
        /// </summary>
        private void UpdateLowLinesTrendBased(int index, double lowValue, TrendDirection trend)
        {
            _lowLineUptrend[index] = double.NaN;
            _lowLineDowntrend[index] = double.NaN;
            _lowLineNeutral[index] = double.NaN;
            
            switch (trend)
            {
                case TrendDirection.Uptrend:
                    _lowLineUptrend[index] = lowValue;
                    break;
                    
                case TrendDirection.Downtrend:
                    // Hide low line in downtrend
                    break;
                    
                case TrendDirection.Neutral:
                    _lowLineNeutral[index] = lowValue;
                    break;
            }
        }
    }
}
