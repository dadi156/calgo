using System;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Interface for band calculators that determine the Fibonacci levels
    /// </summary>
    public interface IBandCalculator
    {
        /// <summary>
        /// Process a single bar
        /// </summary>
        void ProcessBar(int index, double price, double volume, double vwap);
        
        /// <summary>
        /// Reset calculation state
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Update calculator parameters without creating a new instance
        /// </summary>
        void UpdateParameters(VwapResetPeriod resetPeriod, int pivotDepth, DateTime? anchorPoint);
        
        /// <summary>
        /// Get the upper band value (100% Fibonacci level)
        /// </summary>
        double GetUpperBand();
        
        /// <summary>
        /// Get the lower band value (0% Fibonacci level)
        /// </summary>
        double GetLowerBand();
        
        /// <summary>
        /// Get the 88.6% Fibonacci level
        /// </summary>
        double GetFibLevel886();
        
        /// <summary>
        /// Get the 76.4% Fibonacci level
        /// </summary>
        double GetFibLevel764();
        
        /// <summary>
        /// Get the 62.8% Fibonacci level
        /// </summary>
        double GetFibLevel628();
        
        /// <summary>
        /// Get the 38.2% Fibonacci level
        /// </summary>
        double GetFibLevel382();
        
        /// <summary>
        /// Get the 23.6% Fibonacci level
        /// </summary>
        double GetFibLevel236();
        
        /// <summary>
        /// Get the 11.4% Fibonacci level
        /// </summary>
        double GetFibLevel114();
    }
}
