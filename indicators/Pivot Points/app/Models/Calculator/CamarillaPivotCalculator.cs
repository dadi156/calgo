namespace cAlgo.Indicators
{
    /// <summary>
    /// Camarilla pivot point calculator
    /// </summary>
    public class CamarillaPivotCalculator : IPivotPointCalculator
    {
        public PivotPointsData Calculate(double high, double low, double close, double open, int levelsToShow)
        {
            // Camarilla uses close for the pivot
            double pivot = (high + low + close) / 3;
            double range = high - low;
            
            // Camarilla has specific formulas for S/R levels
            double r1 = close + range * 1.1 / 12.0;
            double s1 = close - range * 1.1 / 12.0;
            double r2 = close + range * 1.1 / 6.0;
            double s2 = close - range * 1.1 / 6.0;
            double r3 = close + range * 1.1 / 4.0;
            double s3 = close - range * 1.1 / 4.0;
            
            // Extended Camarilla levels using trading literature standard
            double r4 = close + range * 1.1 / 2.0;
            double s4 = close - range * 1.1 / 2.0;
            double r5 = close + range * 1.1;
            double s5 = close - range * 1.1;
            double r6 = close + range * 1.1 * 1.168;
            double s6 = close - range * 1.1 * 1.168;
            
            return new PivotPointsData
            {
                PivotLevel = pivot,
                ResistanceLevels = new double[] { r1, r2, r3, r4, r5, r6 },
                SupportLevels = new double[] { s1, s2, s3, s4, s5, s6 },
                LevelsToShow = levelsToShow,
                PivotType = PivotPointType.Camarilla
            };
        }
        
        public string GetName()
        {
            return "Camarilla";
        }
    }
}
