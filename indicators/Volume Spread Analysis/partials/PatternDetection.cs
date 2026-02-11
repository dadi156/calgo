using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Pattern Detection

        private VSAPattern DetectPattern(VolumeLevel vol, SpreadLevel spread, CloseZone close, double efficiency, bool uptrend, bool downtrend)
        {
            bool isHighVolume = vol == VolumeLevel.High || vol == VolumeLevel.UltraHigh;
            bool isLowVolume = vol == VolumeLevel.Low || vol == VolumeLevel.BelowAvg;
            bool isLowEfficiency = Math.Abs(efficiency) < EfficiencyThreshold;

            // Climax Buying: Wide spread, Ultra high volume, Close high, after uptrend
            if (spread == SpreadLevel.Wide && vol == VolumeLevel.UltraHigh && close == CloseZone.High && uptrend)
                return VSAPattern.ClimaxBuying;

            // Climax Selling: Wide spread, Ultra high volume, Close low, after downtrend
            if (spread == SpreadLevel.Wide && vol == VolumeLevel.UltraHigh && close == CloseZone.Low && downtrend)
                return VSAPattern.ClimaxSelling;

            // Absorption Buying: Wide spread, High volume, positive efficiency (buyers winning), after downtrend
            if (spread == SpreadLevel.Wide && isHighVolume && efficiency >= EfficiencyThreshold && downtrend)
                return VSAPattern.AbsorptionBuying;

            // Absorption Selling: Wide spread, High volume, negative efficiency (sellers winning), after uptrend
            if (spread == SpreadLevel.Wide && isHighVolume && efficiency <= -EfficiencyThreshold && uptrend)
                return VSAPattern.AbsorptionSelling;

            // ENR Bullish: Wide spread, High volume, low efficiency (no winner), in downtrend
            if (spread == SpreadLevel.Wide && isHighVolume && isLowEfficiency && downtrend)
                return VSAPattern.ENRBullish;

            // ENR Bearish: Wide spread, High volume, low efficiency (no winner), in uptrend
            if (spread == SpreadLevel.Wide && isHighVolume && isLowEfficiency && uptrend)
                return VSAPattern.ENRBearish;

            // No Demand: Narrow spread, Low volume, Close middle/low, in uptrend
            if (spread == SpreadLevel.Narrow && isLowVolume && (close == CloseZone.Low || close == CloseZone.Middle) && uptrend)
                return VSAPattern.NoDemand;

            // No Supply: Narrow spread, Low volume, Close middle/high, in downtrend
            if (spread == SpreadLevel.Narrow && isLowVolume && (close == CloseZone.High || close == CloseZone.Middle) && downtrend)
                return VSAPattern.NoSupply;

            return VSAPattern.None;
        }

        #endregion
    }
}
