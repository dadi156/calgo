using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// View component for Fibonacci levels
    /// Manages the output data series for fibonacci lines (reuses same 9 outputs for all zones)
    /// </summary>
    public class FibonacciLevelsView
    {
        // Single set of fibonacci output data series (reused for all zones)
        private readonly IndicatorDataSeries _fib0000;
        private readonly IndicatorDataSeries _fib1140;
        private readonly IndicatorDataSeries _fib2360;
        private readonly IndicatorDataSeries _fib3820;
        private readonly IndicatorDataSeries _fib5000;
        private readonly IndicatorDataSeries _fib6180;
        private readonly IndicatorDataSeries _fib7860;
        private readonly IndicatorDataSeries _fib8860;
        private readonly IndicatorDataSeries _fib10000;

        private readonly IndicatorDataSeries[] _fibOutputs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fib0000">0.00% fibonacci level output</param>
        /// <param name="fib1140">11.40% fibonacci level output</param>
        /// <param name="fib2360">23.60% fibonacci level output</param>
        /// <param name="fib3820">38.20% fibonacci level output</param>
        /// <param name="fib5000">50.00% fibonacci level output</param>
        /// <param name="fib6180">61.80% fibonacci level output</param>
        /// <param name="fib7860">78.60% fibonacci level output</param>
        /// <param name="fib8860">88.60% fibonacci level output</param>
        /// <param name="fib10000">100.00% fibonacci level output</param>
        public FibonacciLevelsView(IndicatorDataSeries fib0000, IndicatorDataSeries fib1140, IndicatorDataSeries fib2360,
                                  IndicatorDataSeries fib3820, IndicatorDataSeries fib5000, IndicatorDataSeries fib6180,
                                  IndicatorDataSeries fib7860, IndicatorDataSeries fib8860, IndicatorDataSeries fib10000)
        {
            // Fibonacci levels (reused for all zones)
            _fib0000 = fib0000;
            _fib1140 = fib1140;
            _fib2360 = fib2360;
            _fib3820 = fib3820;
            _fib5000 = fib5000;
            _fib6180 = fib6180;
            _fib7860 = fib7860;
            _fib8860 = fib8860;
            _fib10000 = fib10000;

            // Create array for easier iteration
            _fibOutputs = new IndicatorDataSeries[]
            {
                _fib0000, _fib1140, _fib2360, _fib3820, _fib5000, _fib6180, _fib7860, _fib8860, _fib10000
            };
        }

        /// <summary>
        /// Update fibonacci levels outputs (reuses same 9 outputs for all zones)
        /// MODIFIED: Now shows ALL levels including 0% and 100% for main zones
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="fibonacciLevels">Array of 9 fibonacci level values</param>
        /// <param name="displayMode">Display mode (determines which levels to show)</param>
        public void UpdateOutputs(int index, double[] fibonacciLevels, FibonacciDisplayMode displayMode)
        {
            try
            {
                // Validate input
                if (fibonacciLevels == null || fibonacciLevels.Length != FibonacciLevels.Count)
                {
                    SetAllLinesToNaN(index);
                    return;
                }

                // CHANGED: Now show ALL levels for all zones (including main zones)
                // This allows users to see 0% and 100% levels without changing Line Display to "Channel"
                for (int i = 0; i < FibonacciLevels.Count && i < _fibOutputs.Length; i++)
                {
                    _fibOutputs[i][index] = fibonacciLevels[i];
                }
            }
            catch (Exception)
            {
                // Set all to NaN on error
                SetAllLinesToNaN(index);
            }
        }

        /// <summary>
        /// Update fibonacci levels outputs (backward compatibility)
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="fibonacciLevels">Array of 9 fibonacci level values</param>
        public void UpdateOutputs(int index, double[] fibonacciLevels)
        {
            UpdateOutputs(index, fibonacciLevels, FibonacciDisplayMode.LowHighFibonacciLines);
        }

        /// <summary>
        /// Set all fibonacci lines to NaN at given index
        /// </summary>
        /// <param name="index">Bar index</param>
        public void SetAllLinesToNaN(int index)
        {
            try
            {
                // Set fibonacci outputs to NaN
                for (int i = 0; i < _fibOutputs.Length; i++)
                {
                    _fibOutputs[i][index] = double.NaN;
                }
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        /// <summary>
        /// Hide all fibonacci lines at given index (same as SetAllLinesToNaN)
        /// </summary>
        /// <param name="index">Bar index</param>
        public void HideAllLines(int index)
        {
            SetAllLinesToNaN(index);
        }

        /// <summary>
        /// Get specific fibonacci level output series by index
        /// </summary>
        /// <param name="levelIndex">Index of fibonacci level (0-6)</param>
        /// <returns>Output series or null if invalid index</returns>
        public IndicatorDataSeries GetFibonacciLevelOutput(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < _fibOutputs.Length)
                return _fibOutputs[levelIndex];
            
            return null;
        }

        /// <summary>
        /// Get fibonacci level value at specific index and level
        /// </summary>
        /// <param name="barIndex">Bar index</param>
        /// <param name="levelIndex">Fibonacci level index (0-6)</param>
        /// <returns>Fibonacci level value or NaN if invalid</returns>
        public double GetFibonacciLevelValue(int barIndex, int levelIndex)
        {
            try
            {
                if (levelIndex >= 0 && levelIndex < _fibOutputs.Length)
                {
                    return _fibOutputs[levelIndex][barIndex];
                }
                return double.NaN;
            }
            catch (Exception)
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Get all fibonacci level names for display
        /// </summary>
        /// <returns>Array of level names</returns>
        public string[] GetFibonacciLevelNames()
        {
            return (string[])FibonacciLevels.Names.Clone();
        }

        /// <summary>
        /// Get number of fibonacci levels
        /// </summary>
        /// <returns>Number of levels (always 7)</returns>
        public int GetFibonacciLevelCount()
        {
            return FibonacciLevels.Count;
        }

        /// <summary>
        /// Check if any fibonacci lines are visible at given index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <returns>True if at least one line has a valid value</returns>
        public bool AnyLinesVisible(int index)
        {
            try
            {
                // Check fibonacci outputs
                for (int i = 0; i < _fibOutputs.Length; i++)
                {
                    if (!double.IsNaN(_fibOutputs[i][index]))
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get count of visible lines at given index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <returns>Number of visible lines (0-9)</returns>
        public int GetVisibleLinesCount(int index)
        {
            try
            {
                int count = 0;

                // Count fibonacci outputs
                for (int i = 0; i < _fibOutputs.Length; i++)
                {
                    if (!double.IsNaN(_fibOutputs[i][index]))
                        count++;
                }

                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
