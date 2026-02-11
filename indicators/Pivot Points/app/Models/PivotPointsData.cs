namespace cAlgo.Indicators
{
    /// <summary>
    /// Data structure for pivot points
    /// </summary>
    public class PivotPointsData
    {
        /// <summary>
        /// The main pivot level
        /// </summary>
        public double PivotLevel { get; set; }
        
        /// <summary>
        /// Array of resistance levels (R1, R2, R3)
        /// </summary>
        public double[] ResistanceLevels { get; set; }
        
        /// <summary>
        /// Array of support levels (S1, S2, S3)
        /// </summary>
        public double[] SupportLevels { get; set; }
        
        /// <summary>
        /// Number of support/resistance levels to show
        /// </summary>
        public int LevelsToShow { get; set; }
        
        /// <summary>
        /// Type of pivot point calculation used
        /// </summary>
        public PivotPointType PivotType { get; set; }
    }
    
    /// <summary>
    /// Pivot point calculation types
    /// </summary>
    public enum PivotPointType
    {
        Camarilla,
        DeMark,
        Fibonacci,
        Standard,
        Woodie,
    }
}
