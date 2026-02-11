namespace cAlgo
{
    /// <summary>
    /// Controller class for TrendChannelMovingAverage indicator
    /// Coordinates main MA lines calculation and display
    /// </summary>
    public class MAHLController
    {
        private readonly MAHLModel _model;
        private readonly MAHLView _view;
        private readonly TrendChannelMovingAverage _indicator;

        public MAHLController(MAHLModel model, MAHLView view, TrendChannelMovingAverage indicator)
        {
            _model = model;
            _view = view;
            _indicator = indicator;
        }

        /// <summary>
        /// Calculate values and update view for main MA lines
        /// </summary>
        public void Calculate(int index)
        {
            // Always calculate the model (for calculations)
            _model.CalculateForIndex(index);
            
            // Check if we should display lines at this index
            if (_indicator.ShouldDisplayLines(index))
            {
                // Get calculated values and update view normally
                var values = _model.GetAllValues(index);
                LineDisplayMode displayMode = _indicator.LineDisplayMode;
                _view.UpdateOutputs(index, values, displayMode);
            }
            else
            {
                // Don't display lines before lines start point
                // Set all lines to NaN for this index
                var emptyValues = CachedValues.Invalid();
                LineDisplayMode displayMode = _indicator.LineDisplayMode;
                _view.UpdateOutputs(index, emptyValues, displayMode);
            }
        }

        /// <summary>
        /// Get calculated values from model (for fibonacci controller access)
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <returns>Calculated MA values</returns>
        public CachedValues GetCalculatedValues(int index)
        {
            return _model.GetAllValues(index);
        }

        /// <summary>
        /// Check if calculation is needed at given index
        /// </summary>
        /// <param name="index">Bar index</param>
        /// <returns>True if calculation is needed</returns>
        public bool NeedsCalculation(int index)
        {
            // This would depend on the model's internal logic
            // For now, we'll assume calculation is always needed
            return true;
        }

        /// <summary>
        /// Force recalculation from specific index
        /// </summary>
        /// <param name="fromIndex">Starting index for recalculation</param>
        public void RecalculateFrom(int fromIndex)
        {
            // Reset model optimizations and recalculate
            _model.ResetOptimizations();
            
            // Recalculate from the specified index
            int barsCount = _indicator.Bars.Count;
            for (int i = fromIndex; i < barsCount; i++)
            {
                Calculate(i);
            }
        }

        /// <summary>
        /// Reset all calculations
        /// </summary>
        public void Reset()
        {
            _model.ResetOptimizations();
        }
    }
}
