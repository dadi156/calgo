using cAlgo.API;

namespace cAlgo.Indicators
{
    public class MetricsTableBuilder
    {
        private readonly TableConfiguration _config;

        public MetricsTableBuilder(TableConfiguration config)
        {
            _config = config;
        }

        public Grid CreateTable(int totalRows, Thickness margin)
        {
            var grid = new Grid(totalRows, _config.TotalColumns)
            {
                ShowGridLines = false,
                Margin = margin
            };

            SetupColumns(grid);
            return grid;
        }

        private void SetupColumns(Grid grid)
        {
            for (int i = 0; i < _config.ColumnWidths.Count; i++)
            {
                grid.Columns[i].SetWidthInPixels(_config.ColumnWidths[i]);
            }
        }

        public int AddTableTitle(Grid grid, string titleText, int row)
        {
            GridCellBuilder.AddTitleCell(grid, row, titleText, _config.TotalColumns);
            return row + 1;
        }

        public int AddMainHeaderRow(Grid grid, int row)
        {
            foreach (var headerGroup in _config.HeaderGroups)
            {
                GridCellBuilder.AddHeaderCell(grid, row, headerGroup.StartColumn, headerGroup.Text, 1, headerGroup.ColumnSpan);
            }
            return row + 1;
        }

        public int AddSubHeaderRow(Grid grid, int row)
        {
            for (int i = 0; i < _config.SubHeaders.Count; i++)
            {
                GridCellBuilder.AddSubHeaderCell(grid, row, i, _config.SubHeaders[i]);
            }
            return row + 1;
        }
    }
}
