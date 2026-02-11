using System;

namespace cAlgo
{
    /// <summary>
    /// Handle period calculations for growing MA
    /// </summary>
    public class PeriodManager
    {
        /// <summary>
        /// Calculate current period based on bar index and start bar
        /// </summary>
        /// <param name="currentIndex">Current bar index</param>
        /// <param name="startBarIndex">Start bar index</param>
        /// <param name="maxPeriod">Maximum period limit (0 = unlimited)</param>
        /// <returns>Calculated period with max limit applied</returns>
        public int CalculatePeriod(int currentIndex, int startBarIndex, int maxPeriod = 0)
        {
            if (startBarIndex < 0 || currentIndex < startBarIndex)
            {
                return 1;
            }
            
            int calculatedPeriod = Math.Max(1, currentIndex - startBarIndex + 1);
            
            if (maxPeriod > 0)
            {
                calculatedPeriod = Math.Min(calculatedPeriod, maxPeriod);
            }
            
            return calculatedPeriod;
        }
        
        /// <summary>
        /// Check if period is valid for calculation
        /// </summary>
        public bool IsPeriodValid(int period)
        {
            return period > 0;
        }
        
        /// <summary>
        /// Get safe period value (never less than 1)
        /// </summary>
        public int GetSafePeriod(int period)
        {
            return Math.Max(1, period);
        }
        
        /// <summary>
        /// Check if period has reached maximum limit
        /// </summary>
        /// <param name="currentIndex">Current bar index</param>
        /// <param name="startBarIndex">Start bar index</param>
        /// <param name="maxPeriod">Maximum period limit</param>
        /// <returns>True if period reached maximum</returns>
        public bool HasReachedMaxPeriod(int currentIndex, int startBarIndex, int maxPeriod)
        {
            if (maxPeriod <= 0) return false;
            
            int naturalPeriod = Math.Max(1, currentIndex - startBarIndex + 1);
            return naturalPeriod >= maxPeriod;
        }
    }
}
