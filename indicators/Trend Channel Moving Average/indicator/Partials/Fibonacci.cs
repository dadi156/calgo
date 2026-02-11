using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class TrendChannelMovingAverage : Indicator
    {
        /// <summary>
        /// Set all fibonacci lines to NaN for given index
        /// </summary>
        private void SetAllFibonacciLinesToNaN(int index)
        {
            try
            {
                _fibonacciView?.HideAllLines(index);
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        // ===============================================================
        // PUBLIC FIBONACCI API METHODS
        // ===============================================================

        /// <summary>
        /// Get fibonacci levels for external access
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="displayMode">Display mode (optional, uses current setting if not specified)</param>
        /// <returns>Array of 7 fibonacci level values</returns>
        public double[] GetFibonacciLevels(int index, FibonacciDisplayMode? displayMode = null)
        {
            if (_fibonacciController == null)
                return CreateNaNArray();

            FibonacciDisplayMode mode = displayMode ?? FibonacciDisplayMode;
            return _fibonacciController.GetFibonacciLevels(index, mode);
        }

        /// <summary>
        /// Get specific fibonacci level value
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="levelIndex">Level index (0-6)</param>
        /// <param name="displayMode">Display mode (optional)</param>
        /// <returns>Fibonacci level value</returns>
        public double GetFibonacciLevel(int index, int levelIndex, FibonacciDisplayMode? displayMode = null)
        {
            if (_fibonacciController == null)
                return double.NaN;

            FibonacciDisplayMode mode = displayMode ?? FibonacciDisplayMode;
            return _fibonacciController.GetFibonacciLevel(index, levelIndex, mode);
        }

        /// <summary>
        /// Check if fibonacci calculation is possible
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="displayMode">Display mode (optional)</param>
        /// <returns>True if calculation is possible</returns>
        public bool CanCalculateFibonacci(int index, FibonacciDisplayMode? displayMode = null)
        {
            if (_fibonacciController == null)
                return false;

            FibonacciDisplayMode mode = displayMode ?? FibonacciDisplayMode;
            return _fibonacciController.CanCalculateFibonacci(index, mode);
        }

        /// <summary>
        /// Get current fibonacci display mode
        /// </summary>
        /// <returns>Current fibonacci display mode</returns>
        public FibonacciDisplayMode GetFibonacciDisplayMode()
        {
            return FibonacciDisplayMode;
        }

        /// <summary>
        /// Get fibonacci zone description
        /// </summary>
        /// <param name="displayMode">Display mode (optional)</param>
        /// <returns>Zone description</returns>
        public string GetFibonacciZoneDescription(FibonacciDisplayMode? displayMode = null)
        {
            if (_fibonacciController == null)
                return "No Fibonacci Controller";

            FibonacciDisplayMode mode = displayMode ?? FibonacciDisplayMode;
            return _fibonacciController.GetZoneDescription(mode);
        }

        /// <summary>
        /// Get fibonacci level names
        /// </summary>
        /// <returns>Array of level names</returns>
        public string[] GetFibonacciLevelNames()
        {
            if (_fibonacciController == null)
                return new string[0];

            return _fibonacciController.GetFibonacciLevelNames();
        }

        /// <summary>
        /// Get fibonacci level percentages
        /// </summary>
        /// <returns>Array of percentages</returns>
        public double[] GetFibonacciLevelPercentages()
        {
            if (_fibonacciController == null)
                return new double[0];

            return _fibonacciController.GetFibonacciLevelPercentages();
        }

        /// <summary>
        /// Check if any fibonacci lines are visible at index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <returns>True if any lines are visible</returns>
        public bool AnyFibonacciLinesVisible(int index)
        {
            if (_fibonacciController == null)
                return false;

            return _fibonacciController.AnyLinesVisible(index);
        }

        /// <summary>
        /// Create array filled with NaN values
        /// </summary>
        /// <returns>Array of 9 NaN values</returns>
        private double[] CreateNaNArray()
        {
            double[] result = new double[FibonacciLevels.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = double.NaN;
            }
            return result;
        }
    }
}
