using System;

namespace cAlgo
{
    /// <summary>
    /// Controller for Fibonacci levels component
    /// Coordinates between calculator and view, integrates with main TrendChannelMovingAverage model
    /// </summary>
    public class FibonacciLevelsController
    {
        private readonly FibonacciLevelsCalculator _calculator;
        private readonly FibonacciLevelsView _view;
        private readonly MAHLModel _mahlModel;
        private readonly TrendChannelMovingAverage _indicator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="calculator">Fibonacci levels calculator</param>
        /// <param name="view">Fibonacci levels view</param>
        /// <param name="mahlModel">Main TrendChannelMovingAverage model for getting MA values</param>
        /// <param name="indicator">Main TrendChannelMovingAverage indicator for settings</param>
        public FibonacciLevelsController(FibonacciLevelsCalculator calculator, FibonacciLevelsView view, 
                                        MAHLModel mahlModel, TrendChannelMovingAverage indicator)
        {
            _calculator = calculator;
            _view = view;
            _mahlModel = mahlModel;
            _indicator = indicator;
        }

        /// <summary>
        /// Calculate and update fibonacci levels for given index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="displayMode">Fibonacci display mode</param>
        public void Calculate(int index, FibonacciDisplayMode displayMode)
        {
            try
            {
                // Check if we should calculate fibonacci levels
                if (displayMode == FibonacciDisplayMode.None)
                {
                    _view.HideAllLines(index);
                    return;
                }

                // Check if we should display lines at this index (respect anchor date and lines start point)
                if (!_indicator.ShouldDisplayLines(index))
                {
                    _view.HideAllLines(index);
                    return;
                }

                // Get MA values from the main model
                var maValues = _mahlModel.GetAllValues(index);
                
                if (!maValues.IsValid())
                {
                    _view.HideAllLines(index);
                    return;
                }

                // Calculate fibonacci levels for the specified zone
                double[] fibLevels = _calculator.CalculateFibonacciLevels(
                    maValues.Low, 
                    maValues.Median, 
                    maValues.High, 
                    displayMode);

                // Update view with calculated levels
                _view.UpdateOutputs(index, fibLevels, displayMode);
            }
            catch (Exception)
            {
                // Hide all lines on error
                _view.HideAllLines(index);
            }
        }

        /// <summary>
        /// Calculate fibonacci levels without updating view (for external access)
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="displayMode">Fibonacci display mode</param>
        /// <returns>Array of fibonacci level values</returns>
        public double[] GetFibonacciLevels(int index, FibonacciDisplayMode displayMode)
        {
            try
            {
                if (displayMode == FibonacciDisplayMode.None)
                    return CreateNaNArray();

                if (!_indicator.ShouldDisplayLines(index))
                    return CreateNaNArray();

                var maValues = _mahlModel.GetAllValues(index);
                
                if (!maValues.IsValid())
                    return CreateNaNArray();

                return _calculator.CalculateFibonacciLevels(
                    maValues.Low, 
                    maValues.Median, 
                    maValues.High, 
                    displayMode);
            }
            catch (Exception)
            {
                return CreateNaNArray();
            }
        }

        /// <summary>
        /// Get specific fibonacci level value
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="levelIndex">Fibonacci level index (0-6)</param>
        /// <param name="displayMode">Fibonacci display mode</param>
        /// <returns>Fibonacci level value</returns>
        public double GetFibonacciLevel(int index, int levelIndex, FibonacciDisplayMode displayMode)
        {
            try
            {
                if (levelIndex < 0 || levelIndex >= FibonacciLevels.Count)
                    return double.NaN;

                double[] levels = GetFibonacciLevels(index, displayMode);
                return levels[levelIndex];
            }
            catch (Exception)
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Check if fibonacci calculation is possible at given index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <param name="displayMode">Fibonacci display mode</param>
        /// <returns>True if calculation is possible</returns>
        public bool CanCalculateFibonacci(int index, FibonacciDisplayMode displayMode)
        {
            try
            {
                if (displayMode == FibonacciDisplayMode.None)
                    return false;

                if (!_indicator.ShouldDisplayLines(index))
                    return false;

                var maValues = _mahlModel.GetAllValues(index);
                
                if (!maValues.IsValid())
                    return false;

                return _calculator.CanCalculateFibonacci(
                    maValues.Low, 
                    maValues.Median, 
                    maValues.High, 
                    displayMode);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get zone description for current display mode
        /// </summary>
        /// <param name="displayMode">Display mode</param>
        /// <returns>Zone description</returns>
        public string GetZoneDescription(FibonacciDisplayMode displayMode)
        {
            return _calculator.GetZoneDescription(displayMode);
        }

        /// <summary>
        /// Get fibonacci level names
        /// </summary>
        /// <returns>Array of level names</returns>
        public string[] GetFibonacciLevelNames()
        {
            return _view.GetFibonacciLevelNames();
        }

        /// <summary>
        /// Get fibonacci level percentages
        /// </summary>
        /// <returns>Array of percentages (0.0 to 1.0)</returns>
        public double[] GetFibonacciLevelPercentages()
        {
            return (double[])FibonacciLevels.Percentages.Clone();
        }

        /// <summary>
        /// Check if any fibonacci lines are visible at given index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <returns>True if at least one line is visible</returns>
        public bool AnyLinesVisible(int index)
        {
            return _view.AnyLinesVisible(index);
        }

        /// <summary>
        /// Get count of visible fibonacci lines at given index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <returns>Number of visible lines</returns>
        public int GetVisibleLinesCount(int index)
        {
            return _view.GetVisibleLinesCount(index);
        }

        /// <summary>
        /// Get specific fibonacci level value from view
        /// </summary>
        /// <param name="barIndex">Bar index</param>
        /// <param name="levelIndex">Level index (0-6)</param>
        /// <returns>Level value from view</returns>
        public double GetFibonacciLevelFromView(int barIndex, int levelIndex)
        {
            return _view.GetFibonacciLevelValue(barIndex, levelIndex);
        }

        /// <summary>
        /// Create array filled with NaN values
        /// </summary>
        /// <returns>Array of 7 NaN values</returns>
        private double[] CreateNaNArray()
        {
            double[] result = new double[FibonacciLevels.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = double.NaN;
            }
            return result;
        }

        /// <summary>
        /// Reset all fibonacci calculations (called when main indicator resets)
        /// </summary>
        public void Reset()
        {
            // Currently no state to reset in calculator or view
            // This method is here for future extensibility
        }
    }
}
