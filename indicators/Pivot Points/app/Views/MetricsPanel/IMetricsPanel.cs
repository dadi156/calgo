using cAlgo.API;

namespace cAlgo.Indicators
{
    public interface IMetricsPanel
    {
        void Show(PanelPosition position, Thickness margin);
        void Hide();
        void Update();
    }
}
