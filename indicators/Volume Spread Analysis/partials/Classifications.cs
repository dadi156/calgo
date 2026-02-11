using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Classifications

        private VolumeLevel ClassifyVolume(double ratio)
        {
            if (ratio >= VolumeUltraRatio) return VolumeLevel.UltraHigh;
            if (ratio >= VolumeHighRatio) return VolumeLevel.High;
            if (ratio >= 1.0) return VolumeLevel.AboveAvg;
            if (ratio >= VolumeLowRatio) return VolumeLevel.BelowAvg;
            return VolumeLevel.Low;
        }

        private SpreadLevel ClassifySpread(double percentile)
        {
            if (percentile >= SpreadWideThreshold) return SpreadLevel.Wide;
            if (percentile <= SpreadNarrowThreshold) return SpreadLevel.Narrow;
            return SpreadLevel.Normal;
        }

        private CloseZone ClassifyCloseZone(double closeLocation)
        {
            if (closeLocation > 0.67) return CloseZone.High;
            if (closeLocation < 0.33) return CloseZone.Low;
            return CloseZone.Middle;
        }

        private bool IsUptrend(int index)
        {
            if (index < TrendBars) return false;
            return Bars.ClosePrices[index] > Bars.ClosePrices[index - TrendBars];
        }

        private bool IsDowntrend(int index)
        {
            if (index < TrendBars) return false;
            return Bars.ClosePrices[index] < Bars.ClosePrices[index - TrendBars];
        }

        #endregion
    }
}
