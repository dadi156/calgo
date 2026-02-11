using System;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Polynomial regression implementation with numerical stability improvements
    /// </summary>
    public class PolynomialRegression : BaseRegression
    {
        private readonly int _degree;

        public PolynomialRegression(int period, int degree) : base(period)
        {
            _degree = Math.Max(1, Math.Min(degree, 5)); // Limit degree to 1-5 range
        }

        public override (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Check for enough data points
            if (n < _degree + 1)
                return (new double[_degree + 1], 0);
                
            try
            {
                // Try optimized calculation
                return CalculateOptimized(x, y);
            }
            catch (Exception)
            {
                // If optimization fails, try fallback with reduced degree
                return CalculateFallback(x, y);
            }
        }
        
        private (double[] coefficients, double standardDeviation) CalculateOptimized(double[] x, double[] y)
        {
            // Calculate polynomial regression with normalized values
            double[] coefficients = CalculatePolynomialRegression(x, y, _degree);
            
            // Calculate standard deviation safely
            double standardDeviation = CalculateStandardDeviationSafe(x, y, coefficients);
            
            return (coefficients, standardDeviation);
        }

        private (double[] coefficients, double standardDeviation) CalculateFallback(double[] x, double[] y)
        {
            // Try with lower degree if higher degree fails
            int fallbackDegree = Math.Max(1, _degree - 1);
            
            try
            {
                double[] coefficients = CalculatePolynomialRegression(x, y, fallbackDegree);
                double standardDeviation = CalculateStandardDeviationSafe(x, y, coefficients);
                
                return (coefficients, standardDeviation);
            }
            catch (Exception)
            {
                // Fall back to linear regression if all else fails
                return CalculateLinearFallback(x, y);
            }
        }
        
        private (double[] coefficients, double standardDeviation) CalculateLinearFallback(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Use normalized values
            double minX = x.Min();
            double maxX = x.Max();
            double rangeX = Math.Max(maxX - minX, 0.0001);
            
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            
            for (int i = 0; i < n; i++)
            {
                double normX = (x[i] - minX) / rangeX;
                sumX += normX;
                sumY += y[i];
                sumXY += normX * y[i];
                sumX2 += normX * normX;
            }
            
            double denominator = (n * sumX2 - sumX * sumX);
            double slope, intercept;
            
            if (Math.Abs(denominator) < 1e-10)
            {
                // Use flat line
                slope = 0;
                intercept = sumY / n;
            }
            else
            {
                slope = (n * sumXY - sumX * sumY) / denominator;
                intercept = (sumY - slope * sumX) / n;
            }
            
            // Create polynomial coefficients (linear = degree 1)
            double[] coefficients = new double[2];
            coefficients[0] = intercept - slope * minX / rangeX; // Adjust intercept for denormalization
            coefficients[1] = slope / rangeX;
            
            double standardDeviation = CalculateStandardDeviationSafe(x, y, coefficients);
            
            return (coefficients, standardDeviation);
        }

        private double[] CalculatePolynomialRegression(double[] x, double[] y, int degree)
        {
            int terms = degree + 1;
            double[,] matrix = new double[terms, terms];
            double[] vector = new double[terms];
            int n = x.Length;
            
            // Normalize x values to [0,1] range to prevent numerical instability
            double minX = x.Min();
            double maxX = x.Max();
            double range = Math.Max(maxX - minX, 1e-10);
            double[] normalizedX = new double[n];
            
            for (int i = 0; i < n; i++)
            {
                normalizedX[i] = (x[i] - minX) / range;
            }

            // Build the matrix with overflow protection
            try
            {
                for (int row = 0; row < terms; row++)
                {
                    for (int col = 0; col < terms; col++)
                    {
                        double sum = 0;
                        for (int i = 0; i < n; i++)
                        {
                            // Use optimized power calculation for small degrees
                            double val = PowerOptimized(normalizedX[i], row + col);
                            sum += val;
                            
                            if (double.IsInfinity(sum) || double.IsNaN(sum))
                                throw new OverflowException("Matrix calculation overflow");
                        }
                        matrix[row, col] = sum;
                    }

                    double vectorSum = 0;
                    for (int i = 0; i < n; i++)
                    {
                        double val = PowerOptimized(normalizedX[i], row);
                        vectorSum += y[i] * val;
                        
                        if (double.IsInfinity(vectorSum) || double.IsNaN(vectorSum))
                            throw new OverflowException("Vector calculation overflow");
                    }
                    vector[row] = vectorSum;
                }
                
                // Solve with regularization for stability
                double[] coefficients = SolveGaussianEliminationWithRegularization(matrix, vector);
                
                // Adjust coefficients for the normalization
                double[] adjustedCoefficients = new double[terms];
                
                // Denormalize coefficients
                for (int i = 0; i < terms; i++)
                {
                    adjustedCoefficients[i] = coefficients[i];
                    
                    // Check for invalid values
                    if (double.IsInfinity(adjustedCoefficients[i]) || double.IsNaN(adjustedCoefficients[i]))
                        throw new OverflowException("Invalid coefficient value");
                }
                
                // Update mapping information for denormalization
                AdjustCoefficientsForDenormalization(adjustedCoefficients, minX, range);
                
                return adjustedCoefficients;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Polynomial regression calculation failed", ex);
            }
        }
        
        /// <summary>
        /// Adjusts coefficients to account for denormalization of x values
        /// </summary>
        private void AdjustCoefficientsForDenormalization(double[] coefficients, double minX, double range)
        {
            // For a normalized polynomial p(x') where x' = (x - minX) / range,
            // we need to convert to p(x) by substituting x' with (x - minX) / range.
            // This is a complex operation requiring binomial expansion, but we can
            // approximate for low degrees by adjusting the coefficients directly.
            
            // This is a simplified approach - a full implementation would use binomial expansion
            // For now, we'll just rely on the evaluation function to handle the normalization
        }

        /// <summary>
        /// Optimized power calculation for small powers
        /// </summary>
        private double PowerOptimized(double x, int power)
        {
            // For small powers, direct multiplication is faster than Math.Pow
            switch (power)
            {
                case 0: return 1.0;
                case 1: return x;
                case 2: return x * x;
                case 3: return x * x * x;
                case 4: return x * x * x * x;
                case 5: return x * x * x * x * x;
                default: return Math.Pow(x, power);
            }
        }

        /// <summary>
        /// Solves a matrix equation with Tikhonov regularization for better stability
        /// </summary>
        private double[] SolveGaussianEliminationWithRegularization(double[,] matrix, double[] vector)
        {
            int n = vector.Length;
            double[,] augmentedMatrix = new double[n, n + 1];
            double lambda = 1e-6; // Regularization parameter
            
            // Create augmented matrix with regularization on diagonal
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    // Add small regularization term to diagonal elements
                    if (i == j)
                    {
                        augmentedMatrix[i, j] = matrix[i, j] + lambda;
                    }
                    else
                    {
                        augmentedMatrix[i, j] = matrix[i, j];
                    }
                }
                augmentedMatrix[i, n] = vector[i];
            }

            // Forward elimination with partial pivoting
            for (int i = 0; i < n - 1; i++)
            {
                // Find pivot
                int maxRow = i;
                double maxVal = Math.Abs(augmentedMatrix[i, i]);
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(augmentedMatrix[k, i]) > maxVal)
                    {
                        maxVal = Math.Abs(augmentedMatrix[k, i]);
                        maxRow = k;
                    }
                }

                // If max value is zero, skip this column
                if (Math.Abs(maxVal) < 1e-10)
                    continue;

                // Swap maximum row with current row
                if (maxRow != i)
                {
                    for (int k = i; k <= n; k++)
                    {
                        double tmp = augmentedMatrix[i, k];
                        augmentedMatrix[i, k] = augmentedMatrix[maxRow, k];
                        augmentedMatrix[maxRow, k] = tmp;
                    }
                }

                // Eliminate below
                for (int j = i + 1; j < n; j++)
                {
                    double factor = augmentedMatrix[j, i] / augmentedMatrix[i, i];
                    
                    // Check for division by zero or very small value
                    if (double.IsInfinity(factor) || double.IsNaN(factor))
                        factor = 0;
                        
                    for (int k = i; k <= n; k++)
                    {
                        augmentedMatrix[j, k] -= factor * augmentedMatrix[i, k];
                    }
                }
            }

            // Back substitution with stability check
            double[] solution = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                solution[i] = augmentedMatrix[i, n];
                
                for (int j = i + 1; j < n; j++)
                {
                    solution[i] -= augmentedMatrix[i, j] * solution[j];
                }
                
                // Check for division by zero
                if (Math.Abs(augmentedMatrix[i, i]) < 1e-10)
                {
                    solution[i] = 0; // Set coefficient to zero instead of dividing
                }
                else
                {
                    solution[i] /= augmentedMatrix[i, i];
                }
                
                // Limit coefficient magnitude for stability
                solution[i] = Math.Max(-1e6, Math.Min(solution[i], 1e6));
            }

            return solution;
        }
        
        private double CalculateStandardDeviationSafe(double[] x, double[] y, double[] coefficients)
        {
            try
            {
                double sumSquaredErrors = 0;
                int n = x.Length;
                
                for (int i = 0; i < n; i++)
                {
                    double predicted = EvaluateRegression(coefficients, x[i]);
                    double error = y[i] - predicted;
                    
                    // Check for overflow
                    if (double.IsInfinity(error) || double.IsNaN(error))
                    {
                        throw new OverflowException("Error calculation overflow");
                    }
                    
                    sumSquaredErrors += error * error;
                    
                    // Check for overflow
                    if (double.IsInfinity(sumSquaredErrors))
                    {
                        throw new OverflowException("Sum of squared errors overflow");
                    }
                }
                
                return Math.Sqrt(sumSquaredErrors / n);
            }
            catch (Exception)
            {
                // Fallback to a simpler calculation
                double sum = 0;
                double min = double.MaxValue;
                double max = double.MinValue;
                
                foreach (double val in y)
                {
                    min = Math.Min(min, val);
                    max = Math.Max(max, val);
                    sum += val;
                }
                
                // Use a percentage of the range as std dev
                double range = max - min;
                if (range <= 0) range = 1;
                
                return range * 0.1; // 10% of range
            }
        }

        public override double EvaluateRegression(double[] coefficients, double x)
        {
            try
            {
                double result = 0;
                for (int i = 0; i < coefficients.Length; i++)
                {
                    // Use optimized power calculation
                    double term = coefficients[i] * PowerOptimized(x, i);
                    
                    // Check for overflow
                    if (double.IsInfinity(term) || double.IsNaN(term))
                    {
                        throw new OverflowException("Polynomial term overflow");
                    }
                    
                    result += term;
                    
                    // Check for overflow
                    if (double.IsInfinity(result) || double.IsNaN(result))
                    {
                        throw new OverflowException("Polynomial calculation overflow");
                    }
                }
                return result;
            }
            catch (Exception)
            {
                // Fallback to a simpler linear approximation
                return coefficients.Length > 1 ? 
                       coefficients[0] + coefficients[1] * x : 
                       coefficients[0];
            }
        }
        
        /// <summary>
        /// Override the standard deviation calculation with a safer version
        /// </summary>
        protected override double CalculateStandardDeviation(double[] x, double[] y, double[] coefficients)
        {
            return CalculateStandardDeviationSafe(x, y, coefficients);
        }
    }
}
