namespace cAlgo
{
    /// <summary>
    /// Factory to create MA calculators
    /// Makes the right calculator for each MA type
    /// </summary>
    public static class MACalculatorFactory
    {
        /// <summary>
        /// Create calculator based on MA type
        /// </summary>
        /// <param name="maType">Type of MA (Simple, Exponential, Wilder)</param>
        /// <returns>Calculator for that MA type</returns>
        public static IMACalculator Create(int maType)
        {
            switch (maType)
            {
                case 0: // Arnaud Legoux MA
                    return new ALMACalculator();

                case 1: // Exponential MA
                    return new EMACalculator();

                case 2: // Simple MA
                    return new SMACalculator();

                default:
                    // Default to Simple MA
                    return new SMACalculator();
            }
        }

        /// <summary>
        /// Create calculator with enum type
        /// </summary>
        /// <param name="maType">MA type as enum</param>
        /// <returns>Calculator for that MA type</returns>
        public static IMACalculator Create(CustomMAType maType)
        {
            return Create((int)maType);
        }

        /// <summary>
        /// Get all available MA types
        /// </summary>
        /// <returns>Array of MA type names</returns>
        public static string[] GetAvailableTypes()
        {
            return new string[]
            {
                "Arnaud Legoux Moving Average",
                "Exponential Moving Average",
                "Simple Moving Average"
            };
        }

        /// <summary>
        /// Check if MA type is supported
        /// </summary>
        /// <param name="maType">MA type to check</param>
        /// <returns>True if supported</returns>
        public static bool IsSupported(int maType)
        {
            return maType >= 0 && maType <= 3;
        }
    }
}
