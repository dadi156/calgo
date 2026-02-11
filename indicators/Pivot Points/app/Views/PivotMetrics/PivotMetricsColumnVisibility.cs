namespace cAlgo.Indicators
{
    public class PivotMetricsColumnVisibility
    {
        public bool ShowBars { get; set; }
        public bool ShowVolume { get; set; }
        public bool ShowPressure { get; set; }
        public bool ShowDominance { get; set; }
        public bool ShowEfficiency { get; set; }
        public bool ShowAbsorption { get; set; }
        public bool ShowWastedEffort { get; set; }
        public bool ShowConviction { get; set; }

        public const int LevelColumnCount = 1;
        public const int BarsColumnCount = 4;
        public const int VolumeColumnCount = 4;
        public const int PressureColumnCount = 4;
        public const int DominanceColumnCount = 3;
        public const int EfficiencyColumnCount = 3;
        public const int AbsorptionColumnCount = 3;
        public const int WastedColumnCount = 3;
        public const int ConvictionColumnCount = 3;

        public PivotMetricsColumnVisibility()
        {
            ShowBars = false;
            ShowVolume = true;
            ShowPressure = true;
            ShowDominance = false;
            ShowEfficiency = true;
            ShowAbsorption = true;
            ShowWastedEffort = true;
            ShowConviction = true;
        }

        public int GetTotalVisibleColumns()
        {
            int count = LevelColumnCount;

            if (ShowBars) count += BarsColumnCount;
            if (ShowVolume) count += VolumeColumnCount;
            if (ShowPressure) count += PressureColumnCount;
            if (ShowDominance) count += DominanceColumnCount;
            if (ShowEfficiency) count += EfficiencyColumnCount;
            if (ShowAbsorption) count += AbsorptionColumnCount;
            if (ShowWastedEffort) count += WastedColumnCount;
            if (ShowConviction) count += ConvictionColumnCount;

            return count;
        }

        public void ApplyFromString(string columnsString)
        {
            if (string.IsNullOrWhiteSpace(columnsString))
                return;

            // Reset all to false first
            ShowBars = false;
            ShowVolume = false;
            ShowPressure = false;
            ShowDominance = false;
            ShowEfficiency = false;
            ShowAbsorption = false;
            ShowWastedEffort = false;
            ShowConviction = false;

            // Parse and apply
            var columns = columnsString.Split(',');
            foreach (var col in columns)
            {
                var normalized = col.Trim().ToLowerInvariant();
                switch (normalized)
                {
                    case "bar":
                    case "bars":
                        ShowBars = true;
                        break;
                    case "vol":
                    case "volume":
                        ShowVolume = true;
                        break;
                    case "prs":
                    case "pressure":
                        ShowPressure = true;
                        break;
                    case "dom":
                    case "dominance":
                        ShowDominance = true;
                        break;
                    case "eff":
                    case "efficiency":
                        ShowEfficiency = true;
                        break;
                    case "abs":
                    case "absorption":
                        ShowAbsorption = true;
                        break;
                    case "was":
                    case "wasted":
                        ShowWastedEffort = true;
                        break;
                    case "con":
                    case "conviction":
                        ShowConviction = true;
                        break;
                        // Unknown values are silently ignored as requested
                }
            }
        }
    }
}
