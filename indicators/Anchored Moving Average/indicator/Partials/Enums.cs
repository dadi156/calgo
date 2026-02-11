using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Custom Moving Average Types for Growing Period MA
    /// This prevents conflicts with cTrader's MovingAverageType
    /// </summary>
    public enum CustomMAType
    {
        ArnaudLegoux,
        Exponential,
        Simple
    }

    /// <summary>
    /// Band Visibility Options
    /// </summary>
    public enum MABandVisibility
    {
        None,
        ReversionZone,
        Band,
        UpperBand,
        LowerBand,
    }

    /// <summary>
    /// Band Range Level - Fibonacci percentage of price range
    /// Controls how wide the bands are from the MA line
    /// </summary>
    public enum BandRangeLevel
    {
        Fib114,   // 11.40% of price range
        Fib236,   // 23.6% of price range
        Fib382,   // 38.2% of price range
        Fib500,   // 50.0% of price range
        Fib618,   // 61.8% of price range
        Fib764,   // 76.4% of price range
        Fib886,   // 88.60% of price range
        Fib1000   // 100.0% of price range (full range)
    }

    /// <summary>
    /// Anchor Point Period Selection
    /// User can choose manual datetime OR pre-defined relative periods
    /// </summary>
    public enum AnchorPointPeriod
    {
        Manual,
        Last1Hour,
        Last2Hours,
        Last4Hours,
        Last6Hours,
        Last8Hours,
        Last12Hours,
        LastDay,
        Last2Days,
        Last3Days,
        LastWeek,
        Last2Weeks,
        LastMonth,
        Last3Months,
        Last6Months,
        Last9Months,
        LastYear,
        Last3Years,
        Last5Years,
        Last10Years
    }

    public partial class AnchoredMovingAverage : Indicator
    {
        // Enums are now outside the class
    }
}
