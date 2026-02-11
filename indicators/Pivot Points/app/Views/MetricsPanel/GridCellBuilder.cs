using cAlgo.API;

namespace cAlgo.Indicators
{
    public static class GridCellBuilder
    {
        #region Title and Headers

        public static void AddTitleCell(Grid grid, int row, string titleText, int columnSpan)
        {
            var titleBlock = new TextBlock
            {
                Text = titleText,
                Style = GridCellStyles.TitleStyle,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(3, 5, 3, 5),
                Margin = new Thickness(0, 0, 1, 1)
            };
            grid.AddChild(titleBlock, row, 0, 1, columnSpan);
        }

        public static void AddHeaderCell(Grid grid, int row, int col, string text, int rowSpan = 1, int colSpan = 1)
        {
            var header = new TextBlock
            {
                Text = text,
                Style = GridCellStyles.TableHeaderStyle,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(3, 5, 3, 5),
                Margin = new Thickness(0, 0, 1, 1)
            };
            grid.AddChild(header, row, col, rowSpan, colSpan);
        }

        public static void AddSubHeaderCell(Grid grid, int row, int col, string text)
        {
            var header = new TextBlock
            {
                Text = text,
                Style = GridCellStyles.TableHeaderStyle,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(3, 5, 3, 5),
                Margin = new Thickness(0, 0, 1, 1)
            };
            grid.AddChild(header, row, col);
        }

        #endregion

        #region Simple Value Cells

        public static void AddValueCell(Grid grid, int row, int col, string text, bool isPositive, bool isNegative)
        {
            var block = new TextBlock
            {
                Text = text,
                Style = GridCellStyles.GetValueStyle(isPositive, isNegative),
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(1, 5, 1, 5),
                Margin = new Thickness(0, 0, 1, 1)
            };
            grid.AddChild(block, row, col);
        }

        public static void AddValueCell(Grid grid, int row, int col, string text, Style customStyle)
        {
            var block = new TextBlock
            {
                Text = text,
                Style = customStyle,
                TextAlignment = TextAlignment.Center,
                Padding = new Thickness(1, 5, 1, 5),
                Margin = new Thickness(0, 0, 1, 1)
            };
            grid.AddChild(block, row, col);
        }

        #endregion

        #region Position Helpers

        public static HorizontalAlignment GetHorizontalAlignment(PanelPosition position)
        {
            switch (position)
            {
                case PanelPosition.TopLeft:
                case PanelPosition.BottomLeft:
                    return HorizontalAlignment.Left;
                case PanelPosition.TopRight:
                case PanelPosition.BottomRight:
                    return HorizontalAlignment.Right;
                default:
                    return HorizontalAlignment.Right;
            }
        }

        public static VerticalAlignment GetVerticalAlignment(PanelPosition position)
        {
            switch (position)
            {
                case PanelPosition.TopLeft:
                case PanelPosition.TopRight:
                    return VerticalAlignment.Top;
                case PanelPosition.BottomLeft:
                case PanelPosition.BottomRight:
                    return VerticalAlignment.Bottom;
                default:
                    return VerticalAlignment.Top;
            }
        }

        #endregion
    }
}
