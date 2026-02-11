namespace cAlgo.Indicators
{
    /// <summary>
    /// Fibonacci pivot point calculator
    /// </summary>
    public class FibonacciPivotCalculator : IPivotPointCalculator
    {
        public PivotPointsData Calculate(double high, double low, double close, double open, int levelsToShow)
        {
            // Calculate pivot point
            double pivot = (high + low + close) / 3;
            double range = high - low;
            
            // Calculate support and resistance levels using Fibonacci ratios
            // Standard Fibonacci levels
            double r1 = pivot + 0.382 * range;
            double s1 = pivot - 0.382 * range;
            double r2 = pivot + 0.618 * range;
            double s2 = pivot - 0.618 * range;
            double r3 = pivot + 1.000 * range;
            double s3 = pivot - 1.000 * range;
            
            // Extended Fibonacci levels
            double r4 = pivot + 1.382 * range;
            double s4 = pivot - 1.382 * range;
            double r5 = pivot + 1.618 * range;
            double s5 = pivot - 1.618 * range;
            double r6 = pivot + 2.000 * range;
            double s6 = pivot - 2.000 * range;
            
            return new PivotPointsData
            {
                PivotLevel = pivot,
                ResistanceLevels = new double[] { r1, r2, r3, r4, r5, r6 },
                SupportLevels = new double[] { s1, s2, s3, s4, s5, s6 },
                LevelsToShow = levelsToShow,
                PivotType = PivotPointType.Fibonacci
            };
        }
        
        public string GetName()
        {
            return "Fibonacci";
        }
    }
}
