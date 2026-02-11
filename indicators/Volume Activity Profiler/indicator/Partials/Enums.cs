namespace cAlgo
{
    public enum NormWindowMode
    {
        Adaptive,    // Smart defaults based on timeframe
        Custom       // User-defined value
    }

    public enum GraphTextDisplay
    {
        None,      // Hide text
        Concise,   // V: N%, N: N%, etc.
        Full,      // Volume: N%, Net: N%, etc.
        Numbers    // N% only
    }

    public enum GraphTextAlignment
    {
        BarStart,   // Align at bar start time
        FillEnd,    // Align at fill end time
        BarEnd      // Align at bar end time
    }

    public enum GraphPlacement
    {
        AboveHigh,   // Above high price
        Center,       // Centered between high and low
        BelowLow    // Below low price (default)
    }

    public enum MetricsDisplay
    {
        None,       // Hide metrics
        Complete,   // Show all metrics data
        Raw,        // Show raw data only
        Derived,    // Show derived only
        State,      // Show state only
        Summary     // Show summary only
    }

    public enum EfficiencyMode
    {
        Range,  // Range per effort (default)
        Net     // Net change per effort
    }
}
