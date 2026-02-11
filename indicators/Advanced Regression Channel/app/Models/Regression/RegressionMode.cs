namespace cAlgo
{
    /// <summary>
    /// Defines the mode for regression calculation
    /// </summary>
    public enum RegressionMode
    {
        /// <summary>
        /// Use a fixed number of periods for regression calculation
        /// </summary>
        Periods,
        
        /// <summary>
        /// Use a custom date range for regression calculation
        /// </summary>
        DateRange
    }
}
