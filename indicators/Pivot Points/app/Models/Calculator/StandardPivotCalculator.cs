namespace cAlgo.Indicators
{
    /// <summary>
    /// Standard pivot point calculator
    /// </summary>
    public class StandardPivotCalculator : IPivotPointCalculator
    {
        public PivotPointsData Calculate(double high, double low, double close, double open, int levelsToShow)
        {
            // Calculate pivot point
            double pivot = (high + low + close) / 3;
            double range = high - low;
            
            // Calculate standard support and resistance levels
            double r1 = 2 * pivot - low;
            double s1 = 2 * pivot - high;
            double r2 = pivot + range;
            double s2 = pivot - range;
            double r3 = high + 2 * (pivot - low);
            double s3 = low - 2 * (high - pivot);
            
            // Extended standard levels
            double r4 = r3 + range;
            double s4 = s3 - range;
            double r5 = r3 + 2 * range;
            double s5 = s3 - 2 * range;
            double r6 = r3 + 3 * range;
            double s6 = s3 - 3 * range;
            
            return new PivotPointsData
            {
                PivotLevel = pivot,
                ResistanceLevels = new double[] { r1, r2, r3, r4, r5, r6 },
                SupportLevels = new double[] { s1, s2, s3, s4, s5, s6 },
                LevelsToShow = levelsToShow,
                PivotType = PivotPointType.Standard
            };
        }
        
        public string GetName()
        {
            return "Standard";
        }
    }
}
