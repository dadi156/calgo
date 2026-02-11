namespace cAlgo.Indicators
{
    /// <summary>
    /// Factory for creating appropriate pivot point calculator
    /// </summary>
    public class PivotPointCalculatorFactory
    {
        /// <summary>
        /// Creates a pivot point calculator based on the specified type
        /// </summary>
        /// <param name="type">Type of pivot points to calculate</param>
        /// <returns>The appropriate calculator instance</returns>
        public static IPivotPointCalculator CreateCalculator(PivotPointType type)
        {
            switch (type)
            {
                case PivotPointType.Standard:
                    return new StandardPivotCalculator();
                case PivotPointType.Fibonacci:
                    return new FibonacciPivotCalculator();
                case PivotPointType.Woodie:
                    return new WoodiePivotCalculator();
                case PivotPointType.Camarilla:
                    return new CamarillaPivotCalculator();
                case PivotPointType.DeMark:
                    return new DeMarkPivotCalculator();
                default:
                    return new StandardPivotCalculator(); // Default to Standard
            }
        }
    }
}
