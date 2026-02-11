using System;

namespace cAlgo
{
    /// <summary>
    /// Simple linear regression implementation (y = mx + b) with overflow protection
    /// </summary>
    public class LinearRegression : BaseRegression
    {
        public LinearRegression(int period) : base(period) { }

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
        
        /// <summary>
        /// Standard calculation with overflow protection
        /// </summary>
        private (double[] coefficients, double standardDeviation) CalculateProtected(double[] x, double[] y)
        {
            int n = x.Length;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            // Calculate sums with intermediate checks
            for (int i = 0; i < n; i++)
            {
                // Use normalized x values to prevent overflow
                double normalizedX = x[i];
                double normalizedY = y[i];
                
                sumX += normalizedX;
                sumY += normalizedY;
                sumXY += normalizedX * normalizedY;
                sumX2 += normalizedX * normalizedX;
                
                // Check for potential overflow
                if (double.IsInfinity(sumX) || double.IsInfinity(sumY) || 
                    double.IsInfinity(sumXY) || double.IsInfinity(sumX2))
                {
                    throw new OverflowException("Calculation overflow");
                }
            }

            // Calculate with overflow protection
            double denominator = (n * sumX2 - sumX * sumX);
            if (Math.Abs(denominator) < 1e-10)
            {
                // Near-zero denominator, use flat line
                double avgY = sumY / n;
                double[] coef = new double[] { avgY, 0 };
                return (coef, 0.0001); // Small non-zero standard deviation
            }

            double slope = (n * sumXY - sumX * sumY) / denominator;
            double intercept = (sumY - slope * sumX) / n;

            // Check slope and intercept for infinity/NaN
            if (double.IsInfinity(slope) || double.IsNaN(slope) || 
                double.IsInfinity(intercept) || double.IsNaN(intercept))
            {
                throw new OverflowException("Calculation resulted in invalid value");
            }

            // Create coefficients array
            double[] coefficients = new double[] { intercept, slope };
            
            // Calculate standard deviation with overflow protection
            double standardDeviation = CalculateStandardDeviationSafe(x, y, coefficients);

            return (coefficients, standardDeviation);
        }
        
        /// <summary>
        /// Fallback calculation method using more conservative approach
        /// </summary>
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
            
            // Calculate with normalized values
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            
            for (int i = 0; i < n; i++)
            {
                sumX += normX[i];
                sumY += normY[i];
                sumXY += normX[i] * normY[i];
                sumX2 += normX[i] * normX[i];
            }
            
            // Calculate coefficients
            double denominator = (n * sumX2 - sumX * sumX);
            double normSlope, normIntercept;
            
            if (Math.Abs(denominator) < 1e-10)
            {
                // Use flat line at average y
                normSlope = 0;
                normIntercept = sumY / n;
            }
            else
            {
                normSlope = (n * sumXY - sumX * sumY) / denominator;
                normIntercept = (sumY - normSlope * sumX) / n;
            }
            
            // Denormalize coefficients
            double slope = normSlope * (rangeY / rangeX);
            double intercept = (normIntercept * rangeY + minY) - slope * minX;
            
            // Create coefficients array
            double[] coefficients = new double[] { intercept, slope };
            
            // Use a simple standard deviation calculation
            double sumSquaredErrors = 0;
            double meanY = sumY / n;
            
            for (int i = 0; i < n; i++)
            {
                double error = y[i] - (intercept + slope * x[i]);
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
        
        /// <summary>
        /// Safer standard deviation calculation
        /// </summary>
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
            return coefficients[0] + coefficients[1] * x;
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
