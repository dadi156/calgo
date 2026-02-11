using System;

namespace cAlgo.Indicators
{
    /// <summary>
    /// DeMark pivot point calculator
    /// </summary>
    public class DeMarkPivotCalculator : IPivotPointCalculator
    {
        public PivotPointsData Calculate(double high, double low, double close, double open, int levelsToShow)
        {
            // DeMark's pivot formula depends on the close relative to the open
            double x;
            if (close < open)
                x = high + 2 * low + close;
            else if (close > open)
                x = 2 * high + low + close;
            else // close == open
                x = high + low + 2 * close;
                
            double pivot = x / 4;
            
            // DeMark only has one set of S/R levels
            double r1 = x / 2 - low;
            double s1 = x / 2 - high;
            
            // For consistency with extended arrays, duplicate the first one
            // Note: DeMark fundamentally only has 1 level
            return new PivotPointsData
            {
                PivotLevel = pivot,
                ResistanceLevels = new double[] { r1, r1, r1, r1, r1, r1 },
                SupportLevels = new double[] { s1, s1, s1, s1, s1, s1 },
                LevelsToShow = Math.Min(1, levelsToShow), // DeMark only has 1 level
                PivotType = PivotPointType.DeMark
            };
        }
        
        public string GetName()
        {
            return "DeMark";
        }
    }
}
