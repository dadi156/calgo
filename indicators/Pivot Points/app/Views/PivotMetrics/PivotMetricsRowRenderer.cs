using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class PivotMetricsRowRenderer : IMetricsRowRenderer<PivotMetricsData>
    {
        private readonly Symbol _symbol;
        private readonly Color _highlightColor;
        private readonly PivotMetricsColumnVisibility _visibility;

        public PivotMetricsRowRenderer(Symbol symbol, Color highlightColor, PivotMetricsColumnVisibility visibility)
        {
            _symbol = symbol;
            _highlightColor = highlightColor;
            _visibility = visibility;
        }

        public void RenderRow(Grid grid, PivotMetricsData data, int row)
        {
            var level = data.Level;
            var pressure = data.Pressure;
            var isActive = data.IsActive;
            int col = 0;

            // Column 0: Level
            if (isActive)
                GridCellBuilder.AddValueCell(grid, row, col++, level.ToString("F" + _symbol.Digits), CreateHighlightStyle(_highlightColor));
            else
                GridCellBuilder.AddValueCell(grid, row, col++, level.ToString("F" + _symbol.Digits), GridCellStyles.LabelStyle);

            // Bars group - Color code the winning side
            if (_visibility.ShowBars)
            {
                bool bullishBarsWin = pressure.BullishBars > pressure.BearishBars;
                AddCell(grid, row, col++, pressure.BullishBars.ToString(), isActive, bullishBarsWin, false);
                AddCell(grid, row, col++, pressure.BearishBars.ToString(), isActive, false, !bullishBarsWin);

                string barsDeltaText = $"{(pressure.BarsDelta > 0 ? "+" : "")}{pressure.BarsDelta} ({pressure.BarsDeltaPercentage:F0}%)";
                AddCell(grid, row, col++, barsDeltaText, isActive);

                AddCell(grid, row, col++, pressure.TotalBars.ToString(), isActive);
            }

            // Volume group - Color code the winning side
            if (_visibility.ShowVolume)
            {
                bool bullishVolumeWins = pressure.BullishVolume > pressure.BearishVolume;
                AddCell(grid, row, col++, PivotMetricsFormatter.FormatVolume(pressure.BullishVolume), isActive, bullishVolumeWins, false);
                AddCell(grid, row, col++, PivotMetricsFormatter.FormatVolume(pressure.BearishVolume), isActive, false, !bullishVolumeWins);

                string volumeDeltaText = PivotMetricsFormatter.FormatVolumeDelta(pressure.VolumeDelta, pressure.VolumeDeltaPercentage);
                AddCell(grid, row, col++, volumeDeltaText, isActive);

                AddCell(grid, row, col++, PivotMetricsFormatter.FormatVolume(pressure.TotalVolume), isActive);
            }

            // Pressure group - Color code the winning side
            if (_visibility.ShowPressure)
            {
                bool buyPressureWins = pressure.BuyPressure > pressure.SellPressure;
                AddCell(grid, row, col++, MetricsFormatter.FormatLargeNumber(pressure.BuyPressure), isActive, buyPressureWins, false);
                AddCell(grid, row, col++, MetricsFormatter.FormatLargeNumber(pressure.SellPressure), isActive, false, !buyPressureWins);

                string pressureDeltaText = PivotMetricsFormatter.FormatPressureDelta(pressure.Delta, pressure.DeltaPercentage);
                AddCell(grid, row, col++, pressureDeltaText, isActive);

                AddCell(grid, row, col++, MetricsFormatter.FormatLargeNumber(pressure.TotalPressure), isActive);
            }

            // Dominance group
            if (_visibility.ShowDominance)
            {
                AddCell(grid, row, col++, pressure.BarsDominance, isActive);
                AddCell(grid, row, col++, pressure.VolumeDominance, isActive);
                AddCell(grid, row, col++, pressure.PressureDominance, isActive);
            }

            // Efficiency group - Color code the winning side
            if (_visibility.ShowEfficiency)
            {
                bool buyEfficiencyWins = pressure.BuyEfficiency > pressure.SellEfficiency;
                AddCell(grid, row, col++, pressure.BuyEfficiency.ToString("F2"), isActive, buyEfficiencyWins, false);
                AddCell(grid, row, col++, pressure.SellEfficiency.ToString("F2"), isActive, false, !buyEfficiencyWins);
                AddCell(grid, row, col++, pressure.TotalEfficiency.ToString("F2"), isActive);
            }

            // Absorption group - Color code the winning side
            if (_visibility.ShowAbsorption)
            {
                bool buyAbsorptionWins = pressure.BuyAbsorption > pressure.SellAbsorption;
                AddCell(grid, row, col++, pressure.BuyAbsorption.ToString("F2"), isActive, buyAbsorptionWins, false);
                AddCell(grid, row, col++, pressure.SellAbsorption.ToString("F2"), isActive, false, !buyAbsorptionWins);
                AddCell(grid, row, col++, pressure.TotalAbsorption.ToString("F2"), isActive);
            }

            // Wasted Effort group - Color code the LOWER side (less waste is better)
            if (_visibility.ShowWastedEffort)
            {
                bool buyWastedLower = pressure.BuyWastedEffort < pressure.SellWastedEffort;
                AddCell(grid, row, col++, pressure.BuyWastedEffort.ToString("F2") + "%", isActive, buyWastedLower, false);
                AddCell(grid, row, col++, pressure.SellWastedEffort.ToString("F2") + "%", isActive, false, !buyWastedLower);
                AddCell(grid, row, col++, pressure.TotalWastedEffort.ToString("F2") + "%", isActive);
            }

            // Conviction group - Color code the winning side
            if (_visibility.ShowConviction)
            {
                bool buyConvictionWins = pressure.BuyConviction > pressure.SellConviction;
                AddCell(grid, row, col++, pressure.BuyConviction.ToString("F2"), isActive, buyConvictionWins, false);
                AddCell(grid, row, col++, pressure.SellConviction.ToString("F2"), isActive, false, !buyConvictionWins);
                string convictionText = MetricsFormatter.FormatWithSign(pressure.TotalConviction, "F2");
                AddCell(grid, row, col++, convictionText, isActive);
            }
        }

        private void AddCell(Grid grid, int row, int col, string text, bool isActive, bool isPositive = false, bool isNegative = false)
        {
            if (isActive)
                GridCellBuilder.AddValueCell(grid, row, col, text, CreateHighlightStyle(_highlightColor, isPositive, isNegative));
            else
                GridCellBuilder.AddValueCell(grid, row, col, text, isPositive, isNegative);
        }

        private Style CreateHighlightStyle(Color highlightColor, bool isPositive = false, bool isNegative = false)
        {
            var style = new Style();
            style.Set(ControlProperty.FontFamily, "Bahnschrift");
            style.Set(ControlProperty.FontSize, 12);
            style.Set(ControlProperty.BackgroundColor, highlightColor);

            if (isPositive)
                style.Set(ControlProperty.ForegroundColor, Color.FromHex("FF20B2AA"));
            else if (isNegative)
                style.Set(ControlProperty.ForegroundColor, Color.FromHex("FFFF7F50"));
            else
                style.Set(ControlProperty.ForegroundColor, Color.FromHex("CCB9B9B9"));

            return style;
        }
    }
}
