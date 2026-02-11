namespace cAlgo.Indicators
{
    /// <summary>
    /// Defines the VWAP reset period options
    /// </summary>
    public enum VwapResetPeriod
    {
        /// <summary>
        /// Start calculation from a specific anchor point
        /// </summary>
        AnchorPoint,

        /// <summary>
        /// Reset at the beginning of each day
        /// </summary>
        Daily,

        /// <summary>
        /// Reset at the beginning of each week (Monday)
        /// </summary>
        Weekly,

        /// <summary>
        /// Reset at the beginning of each month
        /// </summary>
        Monthly,

        /// <summary>
        /// Reset at the beginning of each year
        /// </summary>
        Yearly,
        
        /// <summary>
        /// Reset at the beginning of Asian/Tokyo session (default: 00:00 UTC)
        /// </summary>
        AsianSession,
        
        /// <summary>
        /// Reset at the beginning of London/European session (default: 07:00 UTC)
        /// </summary>
        LondonSession,
        
        /// <summary>
        /// Reset at the beginning of New York/US session (default: 12:00 UTC)
        /// </summary>
        NewYorkSession,

        /// <summary>
        /// Reset every hour
        /// </summary>
        OneHour,

        /// <summary>
        /// Reset every 2 hours (00:00, 02:00, 04:00, ...)
        /// </summary>
        TwoHour,
        
        /// <summary>
        /// Reset every 3 hours (00:00, 03:00, 06:00, ...)
        /// </summary>
        ThreeHour,
        
        /// <summary>
        /// Reset every 4 hours (00:00, 04:00, 08:00, 12:00, 16:00, 20:00)
        /// </summary>
        FourHour,
        
        /// <summary>
        /// Reset every 6 hours (00:00, 06:00, 12:00, 18:00)
        /// </summary>
        SixHour,
        
        /// <summary>
        /// Reset every 8 hours (00:00, 08:00, 16:00)
        /// </summary>
        EightHour,

        /// <summary>
        /// Reset every 12 hours (00:00, 12:00)
        /// </summary>
        TwelveHour,
        
        /// <summary>
        /// Never reset - calculate from the beginning of available data
        /// </summary>
        Never
    }

    /// <summary>
    /// Defines the possible price sources for VWAP calculation
    /// </summary>
    public enum SourcePrice
    {
        /// <summary>
        /// Close price
        /// </summary>
        Close,

        /// <summary>
        /// Open price
        /// </summary>
        Open,

        /// <summary>
        /// High price
        /// </summary>
        High,

        /// <summary>
        /// Low price
        /// </summary>
        Low,

        /// <summary>
        /// Median price (High + Low) / 2
        /// </summary>
        Median,

        /// <summary>
        /// Typical price (High + Low + Close) / 3
        /// </summary>
        Typical,

        /// <summary>
        /// Weighted price (High + Low + Close * 2) / 4
        /// </summary>
        Weighted
    }

    /// <summary>
    /// Defines the method used to calculate VWAP bands/envelopes
    /// </summary>
    public enum VwapBandType
    {
        /// <summary>
        /// Calculate bands using previous period's high/low price range
        /// </summary>
        HighLowRange,

        /// <summary>
        /// Calculate bands using Fibonacci pivot points from previous period's data
        /// </summary>
        FibonacciPivot,

        /// <summary>
        /// Calculate bands using standard deviation from VWAP
        /// </summary>
        StandardDeviation
    }
}
