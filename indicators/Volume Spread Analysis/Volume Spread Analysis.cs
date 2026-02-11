using System;
using cAlgo.API;

namespace cAlgo
{
    [Indicator(IsOverlay = false, AutoRescale = true, AccessRights = AccessRights.None, TimeZone = TimeZones.UTC)]
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Private Fields

        private TextBlock _volText;
        private TextBlock _spreadText;
        private TextBlock _efficiencyText;
        private TextBlock _patternText;

        // Pre-allocated buffers for performance
        private double[] _volumeBuffer;
        private double[] _sortBuffer;

        #endregion

        protected override void Initialize()
        {
            // Pre-allocate buffers
            _volumeBuffer = new double[LookbackPeriod];
            _sortBuffer = new double[LookbackPeriod];

            if (ShowLegend)
                DrawLegend();

            if (ShowMetricsPanel)
                DrawMetricsPanel();
        }

        public override void Calculate(int index)
        {
            if (index < LookbackPeriod)
            {
                double earlyVolume = Bars.TickVolumes[index];
                double earlyCloseLocation = CalculateCloseLocation(index);
                AssignOutput(index, earlyVolume, earlyCloseLocation >= 0.5 ? OutputType.Bullish : OutputType.Bearish);
                AverageLine[index] = earlyVolume;
                return;
            }

            double volume = Bars.TickVolumes[index];
            double spread = Bars.HighPrices[index] - Bars.LowPrices[index];
            double closeLocation = CalculateCloseLocation(index);
            double efficiency = CalculateRangeEfficiency(index);

            double avgVolume = CalculateTrimmedMeanVolume(index);
            double volumeRatio = avgVolume > 0 ? volume / avgVolume : 1.0;
            double spreadRank = CalculateSpreadPercentile(index, spread);

            VolumeLevel volLevel = ClassifyVolume(volumeRatio);
            SpreadLevel spreadLevel = ClassifySpread(spreadRank);
            CloseZone closeZone = ClassifyCloseZone(closeLocation);
            bool isUptrend = IsUptrend(index);
            bool isDowntrend = IsDowntrend(index);

            VSAPattern pattern = DetectPattern(volLevel, spreadLevel, closeZone, efficiency, isUptrend, isDowntrend);

            AverageLine[index] = avgVolume;
            OutputType outputType = GetOutputType(pattern, closeLocation);
            AssignOutput(index, volume, outputType);

            if (IsLastBar && ShowMetricsPanel)
                UpdateMetricsPanel(volumeRatio, volLevel, spreadRank, spreadLevel, efficiency, pattern);
        }
    }
}
