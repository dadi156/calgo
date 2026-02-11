// Parameters - Separated from main indicator
using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class TrendVolatilityTrail : Indicator
    {
        [Parameter("Type", DefaultValue = MovingAverageType.Exponential, Group = "Moving Average")]
        public MovingAverageType MaType { get; set; }

        [Parameter("Period", DefaultValue = 8, MinValue = 2, Group = "Moving Average")]
        public int MaLength { get; set; }

        [Parameter("Calculation", DefaultValue = MovingAverageType.Simple, Group = "Average True Range")]
        public MovingAverageType AtrMaType { get; set; }

        [Parameter("Period", DefaultValue = 14, MinValue = 1, Group = "Average True Range")]
        public int AtrLength { get; set; }

        [Parameter("Multiplier", DefaultValue = 2.0, MinValue = 0.1, Step = 0.1, Group = "Average True Range")]
        public double BaseMultiplier { get; set; }

        [Parameter("Calculation", DefaultValue = MovingAverageType.Simple, Group = "Volatility")]
        public MovingAverageType VolAvgMaType { get; set; }

        [Parameter("Period", DefaultValue = 20, MinValue = 2, Group = "Volatility")]
        public int VolLookback { get; set; }

        [Parameter("Stretch Sensitivity", DefaultValue = 1.0, MinValue = 0.1, Step = 0.05, Group = "Volatility")]
        public double VolPower { get; set; }

        [Parameter("Calculation", DefaultValue = MovingAverageType.Exponential, Group = "Trend")]
        public MovingAverageType TrendMaType { get; set; }

        [Parameter("Period", DefaultValue = 25, MinValue = 2, Group = "Trend")]
        public int TrendLookback { get; set; }

        [Parameter("Impact", DefaultValue = 0.4, MinValue = 0.0, MaxValue = 1.0, Step = 0.05, Group = "Trend")]
        public double TrendImpact { get; set; }

        [Parameter("Min Effective Multiplier", DefaultValue = 1.0, MinValue = 0.1, Step = 0.1, Group = "Flip Logic")]
        public double MultMin { get; set; }

        [Parameter("Max Effective Multiplier", DefaultValue = 4.0, MinValue = 0.5, Step = 0.1, Group = "Flip Logic")]
        public double MultMax { get; set; }

        [Parameter("Bars Needed To Flip", DefaultValue = 1, MinValue = 1, Group = "Flip Logic")]
        public int ConfirmBars { get; set; }
    }

    // Parameters class for passing settings to model
    public class RegimeParameters
    {
        // Moving Average settings
        public MovingAverageType MaType { get; }
        public int MaLength { get; }

        // ATR settings
        public MovingAverageType AtrMaType { get; }
        public int AtrLength { get; }
        public double BaseMultiplier { get; }

        // Volatility settings
        public MovingAverageType VolAvgMaType { get; }
        public int VolLookback { get; }
        public double VolPower { get; }

        // Trend settings
        public MovingAverageType TrendMaType { get; }
        public int TrendLookback { get; }
        public double TrendImpact { get; }

        // Flip logic settings
        public double MultMin { get; }
        public double MultMax { get; }
        public int ConfirmBars { get; }

        public RegimeParameters(
            MovingAverageType maType, int maLength,
            MovingAverageType atrMaType, int atrLength, double baseMultiplier,
            MovingAverageType volAvgMaType, int volLookback, double volPower,
            MovingAverageType trendMaType, int trendLookback, double trendImpact,
            double multMin, double multMax, int confirmBars)
        {
            MaType = maType;
            MaLength = maLength;
            AtrMaType = atrMaType;
            AtrLength = atrLength;
            BaseMultiplier = baseMultiplier;
            VolAvgMaType = volAvgMaType;
            VolLookback = volLookback;
            VolPower = volPower;
            TrendMaType = trendMaType;
            TrendLookback = trendLookback;
            TrendImpact = trendImpact;
            MultMin = multMin;
            MultMax = multMax;
            ConfirmBars = confirmBars;
        }
    }
}
