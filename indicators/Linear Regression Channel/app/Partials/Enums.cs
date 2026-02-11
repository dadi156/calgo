namespace cAlgo.Indicators
{
    /// <summary>
    /// Source price options
    /// </summary>
    public enum PriceType
    {
        Open,
        High,
        Low,
        Close,
        Median,
        Typical
    }

    /// <summary>
    /// Method for determining the start point for price data collection
    /// </summary>
    public enum StartPointMethod
    {
        Period,
        DateTime
    }

    /// <summary>
    /// Method for calculating channel width/deviation
    /// </summary>
    public enum DeviationMethod
    {
        ATR,
        Average,
        Independent,
        Maximum,
        StandardDeviation,
        WeightedLinear
    }
}
