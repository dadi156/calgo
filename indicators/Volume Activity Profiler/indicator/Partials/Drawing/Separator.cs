using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeActivityProfiler : Indicator
    {
        private void DrawSeparator(int offset, DateTime barOpenTime)
        {
            if (!ShowSeparator)
                return;

            string name = $"separator_{offset}";

            var line = Chart.DrawVerticalLine(name, barOpenTime, SeparatorColor);
            line.LineStyle = SeparatorStyle;
            line.Thickness = SeparatorThickness;
            line.IsInteractive = false;
        }
    }
}
