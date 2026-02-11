using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class PivotMetricsView
    {
        private readonly Chart _chart;
        private readonly Symbol _symbol;
        private readonly PivotMetricsColumnVisibility _visibility;
        private MetricsView<PivotMetricsData> _view;

        // Cached state for refresh
        private MetricsModel _lastModel;
        private string _lastPeriodName;
        private int _lastBarCount;
        private PanelPosition _lastPosition;
        private Thickness _lastMargin;
        private Color _lastHighlightColor;
        private ToggleButtonsPosition _lastToggleButtonsPosition;

        // Panel visibility state
        private bool _isPanelVisible = true;

        public PivotMetricsView(Chart chart, Symbol symbol, string defaultColumns)
        {
            _chart = chart;
            _symbol = symbol;
            _visibility = new PivotMetricsColumnVisibility();
            _visibility.ApplyFromString(defaultColumns); // Apply user's defaults

            var toggleConfig = new PivotMetricsToggleConfiguration(_visibility, this);
            var tableConfig = PivotMetricsTableConfiguration.Create(_visibility);
            _view = new MetricsView<PivotMetricsData>(chart, tableConfig, toggleConfig);
        }

        public void UpdateDisplay(MetricsModel model, string periodName, int barCount,
            PanelPosition position, Thickness margin, Color highlightColor, ToggleButtonsPosition toggleButtonsPosition)
        {
            // Cache state for refresh
            _lastModel = model;
            _lastPeriodName = periodName;
            _lastBarCount = barCount;
            _lastPosition = position;
            _lastMargin = margin;
            _lastHighlightColor = highlightColor;
            _lastToggleButtonsPosition = toggleButtonsPosition;

            var dataProvider = new PivotMetricsDataProvider(model, periodName, barCount);
            var rowRenderer = new PivotMetricsRowRenderer(_symbol, highlightColor, _visibility);

            _view.UpdateDisplay(dataProvider, rowRenderer, position, margin, toggleButtonsPosition);

            // Apply panel visibility state after display update
            _view.SetPanelVisibility(_isPanelVisible);
        }

        public void RefreshDisplay()
        {
            // Hide old view first to clean up
            _view.Hide();

            // Rebuild table config with new visibility settings
            var tableConfig = PivotMetricsTableConfiguration.Create(_visibility);
            var toggleConfig = new PivotMetricsToggleConfiguration(_visibility, this);
            _view = new MetricsView<PivotMetricsData>(_chart, tableConfig, toggleConfig);

            // Re-render with cached state
            if (_lastModel != null)
            {
                UpdateDisplay(_lastModel, _lastPeriodName, _lastBarCount,
                    _lastPosition, _lastMargin, _lastHighlightColor, _lastToggleButtonsPosition);
            }
        }

        /// <summary>
        /// Toggle panel visibility while keeping toggle buttons visible
        /// </summary>
        public void TogglePanelVisibility()
        {
            _isPanelVisible = !_isPanelVisible;

            // Update the panel visibility through the framework
            if (_view != null)
            {
                _view.SetPanelVisibility(_isPanelVisible);
            }
        }

        /// <summary>
        /// Gets whether the panel is currently visible
        /// </summary>
        public bool IsPanelVisible => _isPanelVisible;
    }
}
