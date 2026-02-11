using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Configuration Manager for TrendChannelMovingAverage indicator
    /// </summary>
    public class ConfigurationManager
    {
        // Basic settings
        public int Period { get; }
        public MAType MAType { get; }
        public DataSeries Source { get; }

        // Multi-timeframe settings
        public bool IsMultiTimeframeEnabled { get; }
        public TimeFrame BaseTimeframe { get; }

        // Chart data
        public Bars CurrentBars { get; }
        public Bars BaseTimeframeBars { get; }

        public ConfigurationManager(Bars bars, Indicator indicator, int period, MAType maType,
                                  DataSeries source, bool multiTimeframeMode, TimeFrame baseTimeframe)
        {
            Period = period;
            MAType = maType;
            Source = source;
            CurrentBars = bars;

            IsMultiTimeframeEnabled = multiTimeframeMode && (baseTimeframe != bars.TimeFrame);

            if (IsMultiTimeframeEnabled)
            {
                try
                {
                    BaseTimeframeBars = indicator.MarketData.GetBars(baseTimeframe, indicator.Symbol.Name);
                    BaseTimeframe = baseTimeframe;
                }
                catch
                {
                    IsMultiTimeframeEnabled = false;
                    BaseTimeframeBars = null;
                    BaseTimeframe = bars.TimeFrame;
                }
            }
            else
            {
                BaseTimeframeBars = null;
                BaseTimeframe = bars.TimeFrame;
            }
        }

        /// <summary>
        /// Check if settings are valid
        /// </summary>
        public bool IsValid()
        {
            return Period > 0 && CurrentBars != null && Source != null;
        }

        /// <summary>
        /// Check if multi-timeframe data is available
        /// </summary>
        public bool HasMultiTimeframeData()
        {
            return IsMultiTimeframeEnabled && BaseTimeframeBars != null && BaseTimeframeBars.Count > 0;
        }

        /// <summary>
        /// Get base timeframe bars for multi-timeframe calculations
        /// </summary>
        public Bars GetBaseTimeframeBars()
        {
            return BaseTimeframeBars;
        }
    }
}
