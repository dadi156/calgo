using System.Collections.Generic;

namespace cAlgo.Indicators
{
    public interface IMetricsDataProvider<T>
    {
        List<T> GetSortedData();
        bool HasData { get; }
        string GetPanelTitle();
        bool ShouldShowWarning(out string warningMessage);
    }
}
