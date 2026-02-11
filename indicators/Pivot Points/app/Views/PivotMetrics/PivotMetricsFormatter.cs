using System;

namespace cAlgo.Indicators
{
    public static class PivotMetricsFormatter
    {
        public static string FormatVolume(long volume)
        {
            if (volume >= 1000000)
                return $"{volume / 1000000.0:F1}m";
            if (volume >= 1000)
                return $"{volume / 1000.0:F1}k";
            return volume.ToString("N0");
        }

        public static string FormatVolumeDelta(long delta, double percentage)
        {
            string sign = delta > 0 ? "+" : "";
            if (Math.Abs(delta) >= 1000000)
                return $"{sign}{delta / 1000000.0:F1}m ({percentage:F0}%)";
            if (Math.Abs(delta) >= 1000)
                return $"{sign}{delta / 1000.0:F1}k ({percentage:F0}%)";
            return $"{sign}{delta:N0} ({percentage:F0}%)";
        }

        public static string FormatPressureDelta(double delta, double percentage)
        {
            if (Math.Abs(delta) >= 1000000)
                return $"{(delta > 0 ? "+" : "")}{delta / 1000000:F1}m ({percentage:F0}%)";
            if (Math.Abs(delta) >= 1000)
                return $"{(delta > 0 ? "+" : "")}{delta / 1000:F1}k ({percentage:F0}%)";
            return $"{MetricsFormatter.FormatWithSign(delta, "F1")} ({percentage:F0}%)";
        }
    }
}
