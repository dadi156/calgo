using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class MetricsView<TData> : IMetricsPanel
    {
        private readonly Chart _chart;
        private readonly MetricsTableBuilder _tableBuilder;
        private readonly IToggleConfiguration _toggleConfig;
        private StackPanel _mainPanel;
        private MetricsToggleManager _toggleManager;

        public MetricsView(Chart chart, TableConfiguration tableConfig, IToggleConfiguration toggleConfig = null)
        {
            _chart = chart;
            _tableBuilder = new MetricsTableBuilder(tableConfig);
            _toggleConfig = toggleConfig;
        }

        public void UpdateDisplay(IMetricsDataProvider<TData> dataProvider, 
            IMetricsRowRenderer<TData> rowRenderer, PanelPosition position, Thickness margin, 
            ToggleButtonsPosition toggleButtonsPosition = ToggleButtonsPosition.BottomRight)
        {
            if (position == PanelPosition.None)
            {
                Hide();
                return;
            }

            if (dataProvider.ShouldShowWarning(out string warningMessage))
            {
                ShowWarning(dataProvider.GetPanelTitle(), warningMessage, position, margin);
                UpdateToggles(toggleButtonsPosition);
                return;
            }

            if (!dataProvider.HasData)
            {
                Hide();
                return;
            }

            ShowData(dataProvider, rowRenderer, position, margin);
            UpdateToggles(toggleButtonsPosition);
        }

        public void Show(PanelPosition position, Thickness margin)
        {
            EnsurePanelExists(position);
        }

        public void Hide()
        {
            if (_mainPanel != null)
            {
                _chart.RemoveControl(_mainPanel);
                _mainPanel = null;
            }
            _toggleManager?.Hide();
        }

        public void Update()
        {
            // For future periodic updates if needed
        }

        /// <summary>
        /// Show or hide the panel while keeping toggle buttons visible
        /// </summary>
        public void SetPanelVisibility(bool isVisible)
        {
            if (_mainPanel != null)
            {
                _mainPanel.IsVisible = isVisible;
            }
        }

        private void ShowData(IMetricsDataProvider<TData> dataProvider, 
            IMetricsRowRenderer<TData> rowRenderer, PanelPosition position, Thickness margin)
        {
            GridCellStyles.CreateStyles();

            var data = dataProvider.GetSortedData();
            int totalRows = 3 + data.Count;

            var grid = _tableBuilder.CreateTable(totalRows, margin);

            int currentRow = 0;
            currentRow = _tableBuilder.AddTableTitle(grid, dataProvider.GetPanelTitle(), currentRow);
            currentRow = _tableBuilder.AddMainHeaderRow(grid, currentRow);
            currentRow = _tableBuilder.AddSubHeaderRow(grid, currentRow);

            foreach (var item in data)
            {
                rowRenderer.RenderRow(grid, item, currentRow);
                currentRow++;
            }

            EnsurePanelExists(position);
            ClearPanelChildren();
            _mainPanel.AddChild(grid);
        }

        private void ShowWarning(string title, string warningMessage, PanelPosition position, Thickness margin)
        {
            GridCellStyles.CreateStyles();

            var grid = new Grid(2, 1)
            {
                ShowGridLines = false,
                Margin = margin
            };

            grid.Columns[0].SetWidthInPixels(290);

            GridCellBuilder.AddTitleCell(grid, 0, title, 1);

            var warningBlock = new TextBlock
            {
                Text = warningMessage,
                Style = GridCellStyles.NegativeStyle,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(5, 8, 5, 8),
                Margin = new Thickness(0, 0, 1, 1)
            };
            grid.AddChild(warningBlock, 1, 0);

            EnsurePanelExists(position);
            ClearPanelChildren();
            _mainPanel.AddChild(grid);
        }

        private void UpdateToggles(ToggleButtonsPosition toggleButtonsPosition)
        {
            if (_toggleConfig != null && toggleButtonsPosition != ToggleButtonsPosition.None)
            {
                if (_toggleManager == null)
                {
                    _toggleManager = new MetricsToggleManager(_chart, _toggleConfig, 
                        () => Update());
                }

                _toggleManager.Show(toggleButtonsPosition);
            }
            else
            {
                _toggleManager?.Hide();
            }
        }

        private void EnsurePanelExists(PanelPosition position)
        {
            if (_mainPanel == null)
            {
                _mainPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = GridCellBuilder.GetHorizontalAlignment(position),
                    VerticalAlignment = GridCellBuilder.GetVerticalAlignment(position)
                };
                _chart.AddControl(_mainPanel);
            }
            else
            {
                _mainPanel.HorizontalAlignment = GridCellBuilder.GetHorizontalAlignment(position);
                _mainPanel.VerticalAlignment = GridCellBuilder.GetVerticalAlignment(position);
            }
        }

        private void ClearPanelChildren()
        {
            if (_mainPanel != null)
            {
                foreach (var child in System.Linq.Enumerable.ToArray(_mainPanel.Children))
                {
                    _mainPanel.RemoveChild(child);
                }
            }
        }
    }
}
