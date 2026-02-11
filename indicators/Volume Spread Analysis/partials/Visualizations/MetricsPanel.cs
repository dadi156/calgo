using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        private void DrawMetricsPanel()
        {
            var grid = new Grid(4, 2)
            {
                // BackgroundColor = Color.FromArgb(200, 30, 30, 30),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = "5, 25, 0, 0"
            };

            // Labels
            grid.AddChild(CreateCell("Volume:", Color.White, false, true), 0, 0);
            grid.AddChild(CreateCell("Spread:", Color.White, false, true), 1, 0);
            grid.AddChild(CreateCell("Efficiency:", Color.White, false, true), 2, 0);
            grid.AddChild(CreateCell("Pattern:", Color.White, false, true), 3, 0);

            // Values (will be updated)
            _volText = CreateCell("-", Color.White);
            _spreadText = CreateCell("-", Color.White);
            _efficiencyText = CreateCell("-", Color.White);
            _patternText = CreateCell("-", Color.White);

            grid.AddChild(_volText, 0, 1);
            grid.AddChild(_spreadText, 1, 1);
            grid.AddChild(_efficiencyText, 2, 1);
            grid.AddChild(_patternText, 3, 1);

            IndicatorArea.AddControl(grid);
        }

        private void UpdateMetricsPanel(double volumeRatio, VolumeLevel volLevel, double spreadRank, SpreadLevel spreadLevel, double efficiency, VSAPattern pattern)
        {
            _volText.Text = $"{volumeRatio:F2}x ({volLevel})";
            _volText.ForegroundColor = GetVolumeColor(volLevel);

            _spreadText.Text = $"{spreadRank:F0}% ({spreadLevel})";
            _spreadText.ForegroundColor = GetSpreadColor(spreadLevel);

            string effSign = efficiency >= 0 ? "+" : "";
            _efficiencyText.Text = $"{effSign}{efficiency:F2}";
            _efficiencyText.ForegroundColor = GetEfficiencyColor(efficiency);

            _patternText.Text = pattern == VSAPattern.None ? "-" : pattern.ToString();
            _patternText.ForegroundColor = GetPatternColor(pattern);
        }
    }
}
