using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Visualization

        private OutputType GetOutputType(VSAPattern pattern, double closeLocation)
        {
            switch (pattern)
            {
                case VSAPattern.ClimaxBuying:
                    return OutputType.ClimaxBuying;

                case VSAPattern.ClimaxSelling:
                    return OutputType.ClimaxSelling;

                case VSAPattern.AbsorptionBuying:
                    return OutputType.AbsorptionBuying;

                case VSAPattern.AbsorptionSelling:
                    return OutputType.AbsorptionSelling;

                case VSAPattern.NoDemand:
                    return OutputType.NoDemand;

                case VSAPattern.NoSupply:
                    return OutputType.NoSupply;

                case VSAPattern.ENRBullish:
                    return OutputType.ENRBullish;

                case VSAPattern.ENRBearish:
                    return OutputType.ENRBearish;

                default:
                    return closeLocation >= 0.5 ? OutputType.Bullish : OutputType.Bearish;
            }
        }

        private void AssignOutput(int index, double volume, OutputType type)
        {
            BullishOutput[index] = double.NaN;
            BearishOutput[index] = double.NaN;
            ClimaxBuyingOutput[index] = double.NaN;
            ClimaxSellingOutput[index] = double.NaN;
            NoDemandOutput[index] = double.NaN;
            NoSupplyOutput[index] = double.NaN;
            AbsorptionBuyingOutput[index] = double.NaN;
            AbsorptionSellingOutput[index] = double.NaN;
            ENRBullishOutput[index] = double.NaN;
            ENRBearishOutput[index] = double.NaN;

            switch (type)
            {
                case OutputType.Bullish:
                    BullishOutput[index] = volume;
                    break;
                case OutputType.Bearish:
                    BearishOutput[index] = volume;
                    break;
                case OutputType.ClimaxBuying:
                    ClimaxBuyingOutput[index] = volume;
                    break;
                case OutputType.ClimaxSelling:
                    ClimaxSellingOutput[index] = volume;
                    break;
                case OutputType.NoDemand:
                    NoDemandOutput[index] = volume;
                    break;
                case OutputType.NoSupply:
                    NoSupplyOutput[index] = volume;
                    break;
                case OutputType.AbsorptionBuying:
                    AbsorptionBuyingOutput[index] = volume;
                    break;
                case OutputType.AbsorptionSelling:
                    AbsorptionSellingOutput[index] = volume;
                    break;
                case OutputType.ENRBullish:
                    ENRBullishOutput[index] = volume;
                    break;
                case OutputType.ENRBearish:
                    ENRBearishOutput[index] = volume;
                    break;
            }

            if (ColorChartBars)
                Chart.SetBarColor(index, GetOutputColor(type));
        }

        private TextBlock CreateCell(string text, Color color, bool isHeader = false, bool isStrengthMeter = false)
        {
            return new TextBlock
            {
                Text = text,
                ForegroundColor = color,
                Margin = isHeader ? "6, 3, 6, 3" : !isStrengthMeter ? "4, 2, 4, 2" : "0, 0, 0, 0",
                FontWeight = isHeader ? FontWeight.Bold : FontWeight.Normal,
            };
        }

        private Color GetOutputColor(OutputType type)
        {
            switch (type)
            {
                case OutputType.Bullish: return BullishOutput.LineOutput.Color;
                case OutputType.Bearish: return BearishOutput.LineOutput.Color;
                case OutputType.ClimaxBuying: return ClimaxBuyingOutput.LineOutput.Color;
                case OutputType.ClimaxSelling: return ClimaxSellingOutput.LineOutput.Color;
                case OutputType.NoDemand: return NoDemandOutput.LineOutput.Color;
                case OutputType.NoSupply: return NoSupplyOutput.LineOutput.Color;
                case OutputType.AbsorptionBuying: return AbsorptionBuyingOutput.LineOutput.Color;
                case OutputType.AbsorptionSelling: return AbsorptionSellingOutput.LineOutput.Color;
                case OutputType.ENRBullish: return ENRBullishOutput.LineOutput.Color;
                case OutputType.ENRBearish: return ENRBearishOutput.LineOutput.Color;
                default: return Color.White;
            }
        }

        private Color GetVolumeColor(VolumeLevel level)
        {
            switch (level)
            {
                case VolumeLevel.UltraHigh: return Color.Yellow;
                case VolumeLevel.High: return Color.LimeGreen;
                case VolumeLevel.AboveAvg: return Color.White;
                case VolumeLevel.BelowAvg: return Color.White;
                case VolumeLevel.Low: return Color.DarkGray;
                default: return Color.White;
            }
        }

        private Color GetSpreadColor(SpreadLevel level)
        {
            switch (level)
            {
                case SpreadLevel.Wide: return Color.LimeGreen;
                case SpreadLevel.Normal: return Color.White;
                case SpreadLevel.Narrow: return Color.White;
                default: return Color.White;
            }
        }

        private Color GetEfficiencyColor(double efficiency)
        {
            if (efficiency >= EfficiencyThreshold) return Color.LimeGreen;
            if (efficiency <= -EfficiencyThreshold) return Color.Crimson;
            return Color.Gray;
        }

        private Color GetPatternColor(VSAPattern pattern)
        {
            switch (pattern)
            {
                case VSAPattern.ClimaxBuying: return ClimaxBuyingOutput.LineOutput.Color;
                case VSAPattern.ClimaxSelling: return ClimaxSellingOutput.LineOutput.Color;
                case VSAPattern.AbsorptionBuying: return AbsorptionBuyingOutput.LineOutput.Color;
                case VSAPattern.AbsorptionSelling: return AbsorptionSellingOutput.LineOutput.Color;
                case VSAPattern.NoDemand: return NoDemandOutput.LineOutput.Color;
                case VSAPattern.NoSupply: return NoSupplyOutput.LineOutput.Color;
                case VSAPattern.ENRBullish: return ENRBullishOutput.LineOutput.Color;
                case VSAPattern.ENRBearish: return ENRBearishOutput.LineOutput.Color;
                default: return Color.Gray;
            }
        }

        #endregion
    }
}
