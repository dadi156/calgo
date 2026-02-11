namespace cAlgo.Indicators
{
    public static class PivotMetricsTableConfiguration
    {
        public static TableConfiguration Create(PivotMetricsColumnVisibility visibility)
        {
            var config = new TableConfiguration();
            int colIndex = 0;

            // Level column
            config.ColumnWidths.Add(80);
            config.HeaderGroups.Add(new HeaderGroup("Levels", colIndex, 1));
            config.SubHeaders.Add("");
            colIndex++;

            // Bars group
            if (visibility.ShowBars)
            {
                config.ColumnWidths.AddRange(new[] { 55, 55, 75, 55 });
                config.HeaderGroups.Add(new HeaderGroup("Bars", colIndex, 4));
                config.SubHeaders.AddRange(new[] { "Bull", "Bear", "∆", "Total" });
                colIndex += 4;
            }

            // Volume group
            if (visibility.ShowVolume)
            {
                config.ColumnWidths.AddRange(new[] { 55, 55, 75, 55 });
                config.HeaderGroups.Add(new HeaderGroup("Volume", colIndex, 4));
                config.SubHeaders.AddRange(new[] { "Buy", "Sell", "∆", "Total" });
                colIndex += 4;
            }

            // Pressure group
            if (visibility.ShowPressure)
            {
                config.ColumnWidths.AddRange(new[] { 55, 55, 75, 55 });
                config.HeaderGroups.Add(new HeaderGroup("Pressure", colIndex, 4));
                config.SubHeaders.AddRange(new[] { "Buy", "Sell", "∆", "Total" });
                colIndex += 4;
            }

            // Dominance group
            if (visibility.ShowDominance)
            {
                config.ColumnWidths.AddRange(new[] { 70, 70, 70 });
                config.HeaderGroups.Add(new HeaderGroup("Dominance", colIndex, 3));
                config.SubHeaders.AddRange(new[] { "Bars", "Volume", "Pressure" });
                colIndex += 3;
            }

            // Efficiency group
            if (visibility.ShowEfficiency)
            {
                config.ColumnWidths.AddRange(new[] { 55, 55, 55 });
                config.HeaderGroups.Add(new HeaderGroup("Efficiency", colIndex, 3));
                config.SubHeaders.AddRange(new[] { "Buy", "Sell", "Total" });
                colIndex += 3;
            }

            // Absorption group
            if (visibility.ShowAbsorption)
            {
                config.ColumnWidths.AddRange(new[] { 55, 55, 55 });
                config.HeaderGroups.Add(new HeaderGroup("Absorption", colIndex, 3));
                config.SubHeaders.AddRange(new[] { "Buy", "Sell", "Total" });
                colIndex += 3;
            }

            // Wasted Effort group
            if (visibility.ShowWastedEffort)
            {
                config.ColumnWidths.AddRange(new[] { 55, 55, 55 });
                config.HeaderGroups.Add(new HeaderGroup("Wasted Effort", colIndex, 3));
                config.SubHeaders.AddRange(new[] { "Buy", "Sell", "Total" });
                colIndex += 3;
            }

            // Conviction group
            if (visibility.ShowConviction)
            {
                config.ColumnWidths.AddRange(new[] { 55, 55, 55 });
                config.HeaderGroups.Add(new HeaderGroup("Conviction", colIndex, 3));
                config.SubHeaders.AddRange(new[] { "Buy", "Sell", "Total" });
            }

            return config;
        }
    }
}
