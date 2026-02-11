using System;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Exponential regression implementation (y = a * e^(bx)) with robust error handling
    /// </summary>
    public class ExponentialRegression : BaseRegression
    {
        public ExponentialRegression(int period) : base(period) { }

        public override (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Handle empty arrays
            if (n < 2)
                return (new double[] { 1.0, 0 }, 0);
                
            try
            {
                return CalculateProtected(x, y);
            }
            catch (Exception)
            {
                return CalculateFallback(x, y);
            }
        }

        private (double[] coefficients, double standardDeviation) CalculateProtected(double[] x, double[] y)
        {
            int n = x.Length;
            double sumX = 0, sumLnY = 0, sumXLnY = 0, sumX2 = 0;
            
            // First validate if exponential model is applicable by checking for non-positive y values
            bool allPositive = true;
            for (int i = 0; i < n; i++)
            {
                if (y[i] <= 0)
                {
                    allPositive = false;
                    break;
                }
            }
            
            if (!allPositive)
                throw new ArgumentException("Exponential regression requires positive y values");

            for (int i = 0; i < n; i++)
            {
                // Use epsilon to prevent log(0)
                double lnY = Math.Log(Math.Max(y[i], double.Epsilon));
                sumX += x[i];
                sumLnY += lnY;
                sumXLnY += x[i] * lnY;
                sumX2 += x[i] * x[i];
                
                // Check for potential overflow
                if (double.IsInfinity(sumX) || double.IsInfinity(sumLnY) || 
                    double.IsInfinity(sumXLnY) || double.IsInfinity(sumX2))
                {
                    throw new OverflowException("Calculation overflow");
                }
            }

            double denominator = (n * sumX2 - sumX * sumX);
            if (Math.Abs(denominator) < 1e-10)
            {
                // Use geometric mean for flat line
                double lnYAvg = sumLnY / n;
                double coeffA = Math.Exp(lnYAvg);
                return (new double[] { coeffA, 0 }, 0.0001);
            }

            double bCoeff = (n * sumXLnY - sumX * sumLnY) / denominator;
            double aCoeff = Math.Exp((sumLnY - bCoeff * sumX) / n);
            
            // Check for valid coefficients
            if (double.IsInfinity(aCoeff) || double.IsNaN(aCoeff) ||
                double.IsInfinity(bCoeff) || double.IsNaN(bCoeff))
            {
                throw new OverflowException("Invalid calculation result");
            }

            double[] coeffResults = new[] { aCoeff, bCoeff };
            double standardDeviation = CalculateStandardDeviationSafe(x, y, coeffResults);

            return (coeffResults, standardDeviation);
        }
        
        private (double[] coefficients, double standardDeviation) CalculateFallback(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Substitute non-positive values with small positive values
            double[] adjustedY = new double[n];
            for (int i = 0; i < n; i++)
            {
                adjustedY[i] = Math.Max(y[i], double.Epsilon);
            }
            
            // Find min/max for better normalization
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            
            for (int i = 0; i < n; i++)
            {
                minX = Math.Min(minX, x[i]);
                maxX = Math.Max(maxX, x[i]);
                minY = Math.Min(minY, adjustedY[i]);
                maxY = Math.Max(maxY, adjustedY[i]);
            }
            
            // Avoid division by zero
            double rangeX = Math.Max(maxX - minX, 0.0001);
            double rangeY = Math.Max(maxY - minY, 0.0001);
            
            // Use normalized values 
            double[] normX = new double[n];
            double[] normY = new double[n];
            
            for (int i = 0; i < n; i++)
            {
                normX[i] = (x[i] - minX) / rangeX;
                normY[i] = adjustedY[i] / maxY; // Normalize to [0,1] range
            }
            
            // Transform y values for linear regression
            double[] lnY = new double[n];
            for (int i = 0; i < n; i++)
            {
                lnY[i] = Math.Log(Math.Max(normY[i], double.Epsilon));
            }
            
            // Calculate with normalized values
            double sumX = 0, sumLnY = 0, sumXLnY = 0, sumX2 = 0;
            
            for (int i = 0; i < n; i++)
            {
                sumX += normX[i];
                sumLnY += lnY[i];
                sumXLnY += normX[i] * lnY[i];
                sumX2 += normX[i] * normX[i];
            }
            
            // Calculate coefficients
            double denominator = (n * sumX2 - sumX * sumX);
            double normB, normA;
            
            if (Math.Abs(denominator) < 1e-10)
            {
                // Use flat line at geometric mean
                normB = 0;
                normA = Math.Exp(sumLnY / n);
            }
            else
            {
                normB = (n * sumXLnY - sumX * sumLnY) / denominator;
                normA = Math.Exp((sumLnY - normB * sumX) / n);
            }
            
            // Denormalize coefficients
            double bValue = normB / rangeX;
            double aValue = normA * maxY;
            
            // Create coefficients array with limits to prevent extreme values
            double[] coeffResults = new double[] { 
                Math.Min(aValue, maxY * 10),
                Math.Max(Math.Min(bValue, 10), -10) // Limit b to [-10, 10] range
            };
            
            // Calculate standard deviation safely
            double sumSquaredErrors = 0;
            for (int i = 0; i < n; i++)
            {
                double predicted = EvaluateRegression(coeffResults, x[i]);
                double error = y[i] - predicted;
                sumSquaredErrors += error * error;
            }
            
            double standardDeviation = Math.Sqrt(sumSquaredErrors / n);
            
            // If still invalid, use a simple approximation
            if (double.IsNaN(standardDeviation) || double.IsInfinity(standardDeviation))
            {
                standardDeviation = rangeY * 0.1; // Use 10% of y range as std dev
            }
            
            return (coeffResults, standardDeviation);
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
                    
                    // Cap prediction to avoid extreme values
                    predicted = Math.Min(predicted, y.Max() * 10);
                    
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
                double result = coefficients[0] * Math.Exp(coefficients[1] * x);
                
                // Cap the result to prevent overflow
                if (double.IsInfinity(result) || double.IsNaN(result))
                {
                    throw new OverflowException("Exponential evaluation overflow");
                }
                
                return result;
            }
            catch (Exception)
            {
                // Fallback to a simpler evaluation
                return coefficients[0] * (1 + coefficients[1] * x);
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
