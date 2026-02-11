namespace cAlgo
{
    /// <summary>
    /// Line display modes
    /// </summary>
    public enum LineDisplayMode
    {
        Channel,    // Always show both high and low lines
        TrendBased  // Show/hide lines based on trend
    }

    /// <summary>
    /// Types of moving averages available
    /// </summary>
    public enum MAType
    {
        ArnaudLegouxMA,
        JurikMA,
        ExponentialMA,
        SimpleMA,
        T3,
        DoubleSmoothedEMA,
        LaguerreMA,
        McGinleyDynamic,
        ZeroLagEMA,
        HullMA,
        KaufmanAdaptiveMA,
        VIDYA,
        DeviationScaledMA,
        SuperSmootherMA,
        UltimateSmootherMA,
    }

    /// <summary>
    /// Start Point Type for Anchor Date functionality
    /// </summary>
    public enum PeriodCalculationType
    {
        Period,     // Draw on all bars
        AnchorDate   // Draw only from anchor date
    }

    /// <summary>
    /// Fibonacci display modes for different zones
    /// </summary>
    public enum FibonacciDisplayMode
    {
        None,                       // Hide all fibonacci lines
        LowHighFibonacciLines,      // Show Zone 1: Between low and high lines
        MedianHighFibonacciLines,   // Show Zone 3: Between median and high lines
        LowMedianFibonacciLines,    // Show Zone 2: Between low and median lines
        UpperFibonacciLines,        // Show Zone 4: Above high line (fibonacci extensions)
        LowerFibonacciLines,        // Show Zone 5: Below low line (fibonacci extensions)
        TotalRangeFibonacciLines    // Show Zone 6: Total range from lower extension to upper extension
    }
    
    /// <summary>
    /// Fibonacci level percentages and names
    /// </summary>
    public static class FibonacciLevels
    {
        public static readonly double[] Percentages = new double[]
        {
            0.0000,  // 0.00% (lower boundary)
            0.1140,  // 11.40%
            0.2360,  // 23.60%
            0.3820,  // 38.20%
            0.5000,  // 50.00%
            0.6180,  // 61.80%
            0.7860,  // 78.60%
            0.8860,  // 88.60%
            1.0000   // 100.00% (upper boundary)
        };
        
        public static readonly string[] Names = new string[]
        {
            "0.00%",
            "11.40%",
            "23.60%",
            "38.20%",
            "50.00%",
            "61.80%",
            "78.60%",
            "88.60%",
            "100.00%"
        };
        
        public const int Count = 9;
    }
}
