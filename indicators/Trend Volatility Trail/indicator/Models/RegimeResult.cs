// RegimeResult - Stores calculation result
namespace cAlgo.Indicators
{
    // Result of regime trail calculation
    public class RegimeResult
    {
        public double TrailLong { get; }
        public double TrailShort { get; }
        public int Regime { get; }

        public RegimeResult(double trailLong, double trailShort, int regime)
        {
            TrailLong = trailLong;
            TrailShort = trailShort;
            Regime = regime;
        }
    }
}
