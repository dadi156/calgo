namespace cAlgo
{
    /// <summary>
    /// Interface for regression calculators
    /// </summary>
    public interface IRegressionCalculator
    {
        /// <summary>
        /// Calculates regression coefficients and standard deviation
        /// </summary>
        /// <param name="x">X values (typically normalized time indices)</param>
        /// <param name="y">Y values (typically price data)</param>
        /// <returns>Tuple containing coefficients array and standard deviation</returns>
        (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y);
        
        /// <summary>
        /// Evaluates the regression at a specific x value
        /// </summary>
        /// <param name="coefficients">Coefficients from Calculate method</param>
        /// <param name="x">X value to evaluate</param>
        /// <returns>Regression value at point x</returns>
        double EvaluateRegression(double[] coefficients, double x);
    }
}
