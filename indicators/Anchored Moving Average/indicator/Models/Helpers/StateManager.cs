using System;

namespace cAlgo
{
    /// <summary>
    /// Stores all calculation state variables
    /// </summary>
    public class StateManager
    {
        // Date and time state
        public DateTime StartDate { get; set; }
        public bool IsValidStartDate { get; set; }
        public int StartBarIndex { get; set; } = -1;
        public bool StartBarIndexFound { get; set; } = false;
        
        // Calculation state
        public double Sum { get; set; } = 0;
        public double Alpha { get; set; } = 0;
        public bool FirstValidBar { get; set; } = true;
        
        // Track current bar for calculation
        public int LastCalculatedIndex { get; set; } = -1;
        public double CurrentBarValue { get; set; } = 0;
        
        /// <summary>
        /// Reset all state to initial values
        /// </summary>
        public void Reset()
        {
            StartBarIndex = -1;
            StartBarIndexFound = false;
            Sum = 0;
            Alpha = 0;
            FirstValidBar = true;
            LastCalculatedIndex = -1;
            CurrentBarValue = 0;
        }
        
        /// <summary>
        /// Check if state is ready for calculations
        /// </summary>
        public bool IsReadyForCalculation()
        {
            return IsValidStartDate && StartBarIndexFound && StartBarIndex >= 0;
        }
    }
}
