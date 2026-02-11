using System;

namespace cAlgo
{
    /// <summary>
    /// Model: Contains data structures for FVG (Fair Value Gap)
    /// Multi-Timeframe: Uses DateTime for universal reference across timeframes
    /// Bar Index Tracking: Prevents overlapping FVGs by tracking used bars
    /// Max Penetration: Tracks deepest price penetration for partial fills
    /// </summary>
    public class FVGModel
    {
        public FVGType Type { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public FVGStatus Status { get; set; }
        
        // Time-based storage for multi-timeframe support
        public DateTime FormationTime { get; set; }        // When first bar opened (for display)
        public DateTime CompletionTime { get; set; }       // When FVG is complete (after third bar)
        public DateTime? MitigationTime { get; set; }
        
        // Bar indices that created this FVG (on detection timeframe)
        // Used to prevent overlapping FVGs
        public int FirstBarIndex { get; set; }      // index-2 (oldest bar)
        public int MiddleBarIndex { get; set; }     // index-1 (gap bar)
        public int ThirdBarIndex { get; set; }      // index (newest bar)
        
        // Maximum penetration price for partial fills
        // Bullish FVG: Lowest low that entered the gap
        // Bearish FVG: Highest high that entered the gap
        public double? MaxPenetrationPrice { get; set; }
    }
    
    public enum FVGType
    {
        Bullish,
        Bearish
    }
    
    public enum FVGStatus
    {
        Unfilled,
        PartiallyFilled,
        Filled
    }
    
    public enum FVGDisplay
    {
        Both,
        BullishFVG,
        BearishFVG
    }
}
