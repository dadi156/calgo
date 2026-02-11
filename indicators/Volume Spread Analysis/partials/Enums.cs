using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Enums

        public enum VolumeLevel { Low, BelowAvg, AboveAvg, High, UltraHigh }
        public enum SpreadLevel { Narrow, Normal, Wide }
        public enum CloseZone { Low, Middle, High }
        public enum VSAPattern { None, NoDemand, NoSupply, ClimaxBuying, ClimaxSelling, AbsorptionBuying, AbsorptionSelling, ENRBullish, ENRBearish }
        private enum OutputType { Bullish, Bearish, ClimaxBuying, ClimaxSelling, NoDemand, NoSupply, AbsorptionBuying, AbsorptionSelling, ENRBullish, ENRBearish }

        #endregion
    }
}
