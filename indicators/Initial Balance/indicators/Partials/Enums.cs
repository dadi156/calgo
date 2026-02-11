using cAlgo.API;

namespace cAlgo
{
    public enum IBPeriodType
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        FourMonthly,
        SemiAnnual,
        Yearly,
        CustomRange
    }

    public enum DailyIBMode
    {
        Hours,
        MarketSession
    }

    public enum DailyIBHours
    {
        Hour1 = 1,
        Hour2 = 2,
        Hour3 = 3,
        Hour4 = 4,
        Hour6 = 6,
        Hour8 = 8,
        Hour12 = 12
    }

    public enum MarketSession
    {
        Sydney,
        Tokyo,
        London,
        NewYork
    }

    public enum FibProjectionMode
    {
        None,
        Upward,
        Downward,
        Both
    }
}
