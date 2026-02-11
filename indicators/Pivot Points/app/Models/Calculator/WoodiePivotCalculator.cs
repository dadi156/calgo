namespace cAlgo.Indicators
{
    /// <summary>
    /// Woodie pivot point calculator
    /// </summary>
    public class WoodiePivotCalculator : IPivotPointCalculator
    {
        public PivotPointsData Calculate(double high, double low, double close, double open, int levelsToShow)
        {
            // Woodie uses a modified pivot formula that weights the close price more heavily
            double pivot = (high + low + 2 * close) / 4;
            double range = high - low;
            
            // Calculate support and resistance levels
            double r1 = 2 * pivot - low;
            double s1 = 2 * pivot - high;
            double r2 = pivot + range;
            double s2 = pivot - range;
            double r3 = high + 2 * (pivot - low);
            double s3 = low - 2 * (high - pivot);
            
            // Extended Woodie levels (following the same pattern as standard)
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
                PivotType = PivotPointType.Woodie
            };
        }
        
        public string GetName()
        {
            return "Woodie";
        }
    }
}
