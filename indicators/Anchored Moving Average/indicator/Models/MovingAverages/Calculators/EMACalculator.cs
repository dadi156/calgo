namespace cAlgo
{
    /// <summary>
    /// Exponential Moving Average Calculator
    /// Calculates growing period EMA
    /// </summary>
    public class EMACalculator : IMACalculator
    {
        /// <summary>
        /// Calculate Exponential MA value
        /// </summary>
        public double Calculate(double currentValue, int period, int index, double previousValue, StateManager stateManager)
        {
            // First bar - set alpha and return current value
            if (stateManager.FirstValidBar)
            {
                stateManager.Alpha = 2.0 / (period + 1);
                stateManager.FirstValidBar = false;
                return currentValue;
            }

            // Update alpha for growing period
            stateManager.Alpha = 2.0 / (period + 1);
            
            // If previous value is not valid, return current value
            if (double.IsNaN(previousValue))
            {
                return currentValue;
            }

            // Calculate EMA: Alpha * Current + (1 - Alpha) * Previous
            return stateManager.Alpha * currentValue + (1 - stateManager.Alpha) * previousValue;
        }
        
        /// <summary>
        /// Reset EMA state
        /// </summary>
        public void Reset()
        {
            // State is managed by StateManager
            // This calculator is stateless
        }
        
        /// <summary>
        /// Get calculator name
        /// </summary>
        public string GetName()
        {
            return "Exponential Moving Average";
        }
        
        /// <summary>
        /// Check if value is valid
        /// </summary>
        private bool IsValidValue(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
