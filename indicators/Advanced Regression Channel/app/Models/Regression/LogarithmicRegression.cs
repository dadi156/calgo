using System;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Logarithmic regression implementation (y = a + b*ln(x)) with numeric stability improvements
    /// </summary>
    public class LogarithmicRegression : BaseRegression
    {
        public LogarithmicRegression(int period) : base(period) { }

        public override (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Handle empty arrays or invalid inputs
            if (n < 2)
                return (new double[] { 0, 0 }, 0);
                
            try
            {
                // Calculate with overflow protection
                return CalculateProtected(x, y);
            }
            catch (Exception)
            {
                // Fallback to a more conservative calculation
                return CalculateFallback(x, y);
            }
        }

        private (double[] coefficients, double standardDeviation) CalculateProtected(double[] x, double[] y)
        {
            int n = x.Length;
            double sumLnX = 0, sumY = 0, sumYLnX = 0, sumLnX2 = 0;

            for (int i = 0; i < n; i++)
            {
                // Add small value to handle x=0 and prevent negative values
                double lnX = Math.Log(Math.Max(x[i], 1e-10) + 1); 
                sumLnX += lnX;
                sumY += y[i];
                sumYLnX += y[i] * lnX;
                sumLnX2 += lnX * lnX;
                
                // Check for overflow
                if (double.IsInfinity(sumLnX) || double.IsInfinity(sumY) || 
                    double.IsInfinity(sumYLnX) || double.IsInfinity(sumLnX2))
                {
                    throw new OverflowException("Calculation overflow");
                }
            }

            double denominator = (n * sumLnX2 - sumLnX * sumLnX);
            if (Math.Abs(denominator) < 1e-10)
            {
                // Near-zero denominator, use flat line
                double avgY = sumY / n;
                return (new double[] { avgY, 0 }, 0.0001);
            }

            double slope = (n * sumYLnX - sumLnX * sumY) / denominator;
            double intercept = (sumY - slope * sumLnX) / n;
            
            // Check for valid values
            if (double.IsInfinity(slope) || double.IsNaN(slope) ||
                double.IsInfinity(intercept) || double.IsNaN(intercept))
            {
                throw new OverflowException("Invalid calculation result");
            }

            double[] coefficients = new double[] { intercept, slope };
            double standardDeviation = CalculateStandardDeviationSafe(x, y, coefficients);

            return (coefficients, standardDeviation);
        }

        private (double[] coefficients, double standardDeviation) CalculateFallback(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Find min/max for better normalization
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            
            for (int i = 0; i < n; i++)
            {
                minX = Math.Min(minX, x[i]);
                maxX = Math.Max(maxX, x[i]);
                minY = Math.Min(minY, y[i]);
                maxY = Math.Max(maxY, y[i]);
            }
            
            // Avoid division by zero
            double rangeX = Math.Max(maxX - minX, 0.0001);
            double rangeY = Math.Max(maxY - minY, 0.0001);
            
            // Use normalized values between 0-1
            double[] normX = new double[n];
            double[] normY = new double[n];
            
            for (int i = 0; i < n; i++)
            {
                normX[i] = (x[i] - minX) / rangeX;
                normY[i] = (y[i] - minY) / rangeY;
            }
            
            // Transform x values for logarithmic regression
            double[] lnX = new double[n];
            for (int i = 0; i < n; i++)
            {
                lnX[i] = Math.Log(Math.Max(normX[i], 1e-10) + 1);
            }
            
            // Calculate with normalized values
            double sumLnX = 0, sumY = 0, sumYLnX = 0, sumLnX2 = 0;
            
            for (int i = 0; i < n; i++)
            {
                sumLnX += lnX[i];
                sumY += normY[i];
                sumYLnX += normY[i] * lnX[i];
                sumLnX2 += lnX[i] * lnX[i];
            }
            
            // Calculate coefficients
            double denominator = (n * sumLnX2 - sumLnX * sumLnX);
            double normSlope, normIntercept;
            
            if (Math.Abs(denominator) < 1e-10)
            {
                // Use flat line at average y
                normSlope = 0;
                normIntercept = sumY / n;
            }
            else
            {
                normSlope = (n * sumYLnX - sumLnX * sumY) / denominator;
                normIntercept = (sumY - normSlope * sumLnX) / n;
            }
            
            // Denormalize coefficients
            double slope = normSlope * rangeY;
            double intercept = (normIntercept * rangeY + minY);
            
            // Create coefficients array
            double[] coefficients = new double[] { intercept, slope };
            
            // Use a simple standard deviation calculation
            double sumSquaredErrors = 0;
            
            for (int i = 0; i < n; i++)
            {
                double predicted = EvaluateRegression(coefficients, x[i]);
                double error = y[i] - predicted;
                sumSquaredErrors += error * error;
            }
            
            double standardDeviation = Math.Sqrt(sumSquaredErrors / n);
            
            // If still invalid, use a very simple approximation
            if (double.IsNaN(standardDeviation) || double.IsInfinity(standardDeviation))
            {
                standardDeviation = rangeY * 0.1; // Use 10% of y range as std dev
            }
            
            return (coefficients, standardDeviation);
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
            // Protect against invalid x values
            double safeX = Math.Max(x, 1e-10) + 1;
            return coefficients[0] + coefficients[1] * Math.Log(safeX);
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
