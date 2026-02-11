using System.Collections.Generic;

namespace cAlgo.Indicators
{
    public class TableConfiguration
    {
        public List<int> ColumnWidths { get; set; }
        public List<HeaderGroup> HeaderGroups { get; set; }
        public List<string> SubHeaders { get; set; }

        public int TotalColumns => ColumnWidths.Count;

        public TableConfiguration()
        {
            ColumnWidths = new List<int>();
            HeaderGroups = new List<HeaderGroup>();
            SubHeaders = new List<string>();
        }
    }

    public class HeaderGroup
    {
        public string Text { get; set; }
        public int StartColumn { get; set; }
        public int ColumnSpan { get; set; }

        public HeaderGroup(string text, int startColumn, int columnSpan)
        {
            Text = text;
            StartColumn = startColumn;
            ColumnSpan = columnSpan;
        }
    }
}
