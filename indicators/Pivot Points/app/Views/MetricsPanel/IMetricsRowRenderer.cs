using cAlgo.API;

namespace cAlgo.Indicators
{
    public interface IMetricsRowRenderer<T>
    {
        void RenderRow(Grid grid, T data, int row);
    }
}
