using System;

namespace cAlgo
{
    /// <summary>
    /// Factory for creating regression calculator instances
    /// </summary>
    public static class RegressionFactory
    {
        /// <summary>
        /// Creates a regression calculator instance based on the specified type
        /// </summary>
        /// <param name="type">Regression type to create</param>
        /// <param name="period">Period for regression calculations</param>
        /// <param name="degree">Degree for polynomial regression (ignored for other types)</param>
        /// <returns>An IRegressionCalculator implementation</returns>
        public static IRegressionCalculator CreateRegression(RegressionType type, int period, int degree = 2)
        {
            return type switch
            {
                RegressionType.Linear => new LinearRegression(period),
                RegressionType.Logarithmic => new LogarithmicRegression(period),
                RegressionType.Exponential => new ExponentialRegression(period),
                RegressionType.Weighted => new WeightedRegression(period),
                RegressionType.Polynomial => new PolynomialRegression(period, degree),
                RegressionType.Moving => new MovingRegression(period),
                RegressionType.ExponentialMoving => new ExponentialMovingRegression(period, 0.3),
                RegressionType.LOWESS => new LOWESSRegression(period, 0.3),
                _ => throw new ArgumentException("Unsupported regression type")
            };
        }
    }
}
