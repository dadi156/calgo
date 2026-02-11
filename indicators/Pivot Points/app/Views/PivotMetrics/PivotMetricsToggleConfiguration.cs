using System.Collections.Generic;

namespace cAlgo.Indicators
{
    public class PivotMetricsToggleConfiguration : IToggleConfiguration
    {
        private readonly PivotMetricsColumnVisibility _visibility;
        private readonly PivotMetricsView _view;

        public PivotMetricsToggleConfiguration(PivotMetricsColumnVisibility visibility, PivotMetricsView view)
        {
            _visibility = visibility;
            _view = view;
        }

        public List<ToggleButton> GetButtons()
        {
            return new List<ToggleButton>
            {
                // Panel visibility toggle
                new ToggleButton("panel", "☶", _view.IsPanelVisible),
                
                // Column visibility toggles
                new ToggleButton("conviction", "Con", _visibility.ShowConviction),
                new ToggleButton("wasted", "Was", _visibility.ShowWastedEffort),
                new ToggleButton("absorption", "Abs", _visibility.ShowAbsorption),
                new ToggleButton("efficiency", "Eff", _visibility.ShowEfficiency),
                new ToggleButton("dominance", "Dom", _visibility.ShowDominance),
                new ToggleButton("pressure", "Prs", _visibility.ShowPressure),
                new ToggleButton("volume", "Vol", _visibility.ShowVolume),
                new ToggleButton("bars", "Bar", _visibility.ShowBars)
            };
        }

        public void OnToggleChanged(string buttonId, bool isEnabled)
        {
            // Handle panel visibility toggle - no full refresh needed
            if (buttonId == "panel")
            {
                _view.TogglePanelVisibility();
                return; // Don't call RefreshDisplay, just toggle visibility
            }
            
            // Handle column visibility toggles - these require full refresh to rebuild table
            switch (buttonId)
            {
                case "bars":
                    _visibility.ShowBars = isEnabled;
                    break;
                case "volume":
                    _visibility.ShowVolume = isEnabled;
                    break;
                case "pressure":
                    _visibility.ShowPressure = isEnabled;
                    break;
                case "dominance":
                    _visibility.ShowDominance = isEnabled;
                    break;
                case "efficiency":
                    _visibility.ShowEfficiency = isEnabled;
                    break;
                case "absorption":
                    _visibility.ShowAbsorption = isEnabled;
                    break;
                case "wasted":
                    _visibility.ShowWastedEffort = isEnabled;
                    break;
                case "conviction":
                    _visibility.ShowConviction = isEnabled;
                    break;
            }

            _view.RefreshDisplay();
        }
    }
}
