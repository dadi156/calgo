namespace cAlgo
{
    /// <summary>
    /// Interface for all MA calculators
    /// All MA types must implement this
    /// </summary>
    public interface IMACalculator
    {
        /// <summary>
        /// Calculate MA value for current bar
        /// </summary>
        /// <param name="currentValue">Current price value</param>
        /// <param name="period">Current period (growing)</param>
        /// <param name="index">Current bar index</param>
        /// <param name="previousValue">Previous MA value</param>
        /// <param name="stateManager">State manager for calculations</param>
        /// <returns>Calculated MA value</returns>
        double Calculate(double currentValue, int period, int index, double previousValue, StateManager stateManager);
        
        /// <summary>
        /// Reset calculator state
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Get calculator name
        /// </summary>
        string GetName();
    }
}
