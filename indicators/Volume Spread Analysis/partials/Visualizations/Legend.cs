using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        private void DrawLegend()
        {
            // Define all colors here for easy modification
            var headerColor = Color.White;
            var labelColor = Color.White;
            var descColor = Color.Gray;

            var legendData = new[]
            {
                new { Label = "Bullish", Color = BullishOutput.LineOutput.Color, Desc = "Close >= 0.5, no pattern" },
                new { Label = "Bearish", Color = BearishOutput.LineOutput.Color, Desc = "Close < 0.5, no pattern" },
                new { Label = "Climax Buying", Color = ClimaxBuyingOutput.LineOutput.Color, Desc = "Wide, ultra vol, close high, uptrend" },
                new { Label = "Climax Selling", Color = ClimaxSellingOutput.LineOutput.Color, Desc = "Wide, ultra vol, close low, downtrend" },
                new { Label = "No Demand", Color = NoDemandOutput.LineOutput.Color, Desc = "Narrow, low vol, close mid/low, uptrend" },
                new { Label = "No Supply", Color = NoSupplyOutput.LineOutput.Color, Desc = "Narrow, low vol, close mid/high, downtrend" },
                new { Label = "Absorption Buying", Color = AbsorptionBuyingOutput.LineOutput.Color, Desc = "Wide, high vol, +efficiency, downtrend" },
                new { Label = "Absorption Selling", Color = AbsorptionSellingOutput.LineOutput.Color, Desc = "Wide, high vol, -efficiency, uptrend" },
                new { Label = "ENR Bullish", Color = ENRBullishOutput.LineOutput.Color, Desc = "Wide, high vol, low efficiency, downtrend" },
                new { Label = "ENR Bearish", Color = ENRBearishOutput.LineOutput.Color, Desc = "Wide, high vol, low efficiency, uptrend" }
            };

            var grid = new Grid(11, 3)
            {
                BackgroundColor = Color.FromArgb(200, 30, 30, 30),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = 5
            };

            // Header row
            /*
            grid.AddChild(CreateCell("", headerColor, true), 0, 0);
            grid.AddChild(CreateCell("Output", headerColor, true), 0, 1);
            grid.AddChild(CreateCell("Pattern", headerColor, true), 0, 2);
            */

            // Data rows
            for (int i = 0; i < legendData.Length; i++)
            {
                int row = i + 1;
                grid.AddChild(CreateCell("⬤", legendData[i].Color), row, 0);
                grid.AddChild(CreateCell(legendData[i].Label, labelColor), row, 1);
                grid.AddChild(CreateCell(legendData[i].Desc, descColor), row, 2);
            }

            IndicatorArea.AddControl(grid);
        }
    }
}
