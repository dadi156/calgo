using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Simple Moving Average Calculator
    /// Supports MaxPeriod limit
    /// </summary>
    public class SMACalculator : IMACalculator
    {
        private List<double> allValues;
        
        /// <summary>
        /// Calculate Simple MA value
        /// </summary>
        public double Calculate(double currentValue, int period, int index, double previousValue, StateManager stateManager)
        {
            if (stateManager.FirstValidBar)
            {
                allValues = new List<double>();
                allValues.Add(currentValue);
                stateManager.LastCalculatedIndex = index;
                stateManager.FirstValidBar = false;
                return currentValue;
            }

            if (index == stateManager.LastCalculatedIndex)
            {
                if (allValues.Count > 0)
                {
                    allValues[allValues.Count - 1] = currentValue;
                }
            }
            else
            {
                allValues.Add(currentValue);
                stateManager.LastCalculatedIndex = index;
            }

            if (allValues.Count > period)
            {
                int itemsToRemove = allValues.Count - period;
                allValues.RemoveRange(0, itemsToRemove);
            }

            return allValues.Average();
        }
        
        /// <summary>
        /// Reset SMA state
        /// </summary>
        public void Reset()
        {
            if (allValues != null)
            {
                allValues.Clear();
            }
            allValues = null;
        }
        
        /// <summary>
        /// Get calculator name
        /// </summary>
        public string GetName()
        {
            return "Simple Moving Average";
        }
    }
}
