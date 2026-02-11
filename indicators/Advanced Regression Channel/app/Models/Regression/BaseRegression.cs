using System;

namespace cAlgo
{
    /// <summary>
    /// Base abstract class for regression implementations
    /// </summary>
    public abstract class BaseRegression : IRegressionCalculator
    {
        protected readonly int _period;

        protected BaseRegression(int period)
        {
            _period = period;
        }

        /// <summary>
        /// Calculates regression coefficients and standard deviation
        /// </summary>
        public abstract (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y);
        
        /// <summary>
        /// Evaluates the regression at a specific x value
        /// </summary>
        public abstract double EvaluateRegression(double[] coefficients, double x);

        /// <summary>
        /// Calculates standard deviation of residuals
        /// </summary>
        protected virtual double CalculateStandardDeviation(double[] x, double[] y, double[] coefficients)
        {
            double sumSquaredErrors = 0;
            int n = x.Length;

            for (int i = 0; i < n; i++)
            {
                double predicted = EvaluateRegression(coefficients, x[i]);
                double error = y[i] - predicted;
                sumSquaredErrors += error * error;
            }

            return Math.Sqrt(sumSquaredErrors / n);
        }
    }
}
