namespace cAlgo.Indicators
{
    /// <summary>
    /// Controller for pivot points - coordinates between model and view
    /// </summary>
    public class PivotPointsController
    {
        private PivotPointsModel _model;
        private PivotPointsView _view;

        public PivotPointsController(PivotPointsModel model, PivotPointsView view)
        {
            _model = model;
            _view = view;
        }

        /// <summary>
        /// Updates the view with data from the model
        /// </summary>
        public void UpdateView()
        {
            // Clear existing pivot lines
            _view.ClearLines();

            // Get the data to display
            var pivotData = _model.GetPivotData();
            var periodsToDisplay = _model.GetPeriodsToDisplay();

            // No data to display
            if (periodsToDisplay == null || periodsToDisplay.Count == 0)
                return;

            // Draw pivot points for each period that should be displayed
            foreach (var period in periodsToDisplay)
            {
                _view.DrawPivotPointsForPeriod(
                    period.PivotData,
                    period.StartTime,
                    period.EndTime,
                    period.PeriodName);
            }
        }
    }
}
