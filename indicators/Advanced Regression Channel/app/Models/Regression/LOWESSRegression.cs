using System;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// LOWESS (Locally Weighted Scatterplot Smoothing) regression implementation with robust error handling
    /// </summary>
    public class LOWESSRegression : BaseRegression
    {
        private readonly double _bandwidth;
        private readonly int _robustIterations;
        private readonly int _maxIterations; // Safety limit for iterations

        public LOWESSRegression(int period, double bandwidth = 0.3, int robustIterations = 2)
            : base(period)
        {
            // Validate parameters and provide safe defaults
            _bandwidth = Math.Max(0.1, Math.Min(bandwidth, 1.0));
            _robustIterations = Math.Max(1, Math.Min(robustIterations, 5));
            _maxIterations = 10; // Hard limit to prevent excessive calculation
        }

        public override (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y)
        {
            int n = x.Length;

            // Handle empty arrays or insufficient data
            if (n < 3) // LOWESS needs at least 3 points for meaningful calculation
                return (new double[n > 0 ? n : 1], 0);

            try
            {
                // Calculate with overflow protection and iteration limits
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
            double[] smoothedValues = new double[n];
            double[] weights = new double[n];
            double[] robustWeights = new double[n];

            // Initialize robust weights
            Array.Fill(robustWeights, 1.0);

            // Validate input data
            for (int i = 0; i < n; i++)
            {
                if (double.IsInfinity(x[i]) || double.IsNaN(x[i]) ||
                    double.IsInfinity(y[i]) || double.IsNaN(y[i]))
                {
                    throw new ArgumentException("Input data contains invalid values");
                }
            }

            // Robust regression iterations with safety limits
            int actualIterations = Math.Min(_robustIterations, _maxIterations);

            for (int iter = 0; iter < actualIterations; iter++)
            {
                for (int i = 0; i < n; i++)
                {
                    try
                    {
                        // Compute local weights
                        double effectiveBandwidth = GetEffectiveBandwidth(x, i);

                        for (int j = 0; j < n; j++)
                        {
                            // Calculate tri-cube weight function with overflow protection
                            double distance = Math.Abs(x[j] - x[i]) / Math.Max(effectiveBandwidth, 1e-10);

                            if (distance >= 1)
                            {
                                weights[j] = 0;
                            }
                            else
                            {
                                // Use safer calculation for tri-cube weight
                                double temp = 1 - Math.Pow(distance, 3);
                                weights[j] = Math.Pow(Math.Max(0, temp), 3);
                            }

                            // Apply robust weights from previous iteration
                            weights[j] *= robustWeights[j];

                            // Check for invalid weights
                            if (double.IsInfinity(weights[j]) || double.IsNaN(weights[j]))
                            {
                                weights[j] = 0;
                            }
                        }

                        // Calculate weighted average for this point
                        smoothedValues[i] = CalculateWeightedAverage(x, y, weights, x[i]);

                        // Check if result is valid
                        if (double.IsInfinity(smoothedValues[i]) || double.IsNaN(smoothedValues[i]))
                        {
                            // Use original value if weighted average fails
                            smoothedValues[i] = y[i];
                        }
                    }
                    catch (Exception)
                    {
                        // For any calculation error, use original value
                        smoothedValues[i] = y[i];
                    }
                }

                // Update robust weights based on residuals for next iteration
                if (iter < actualIterations - 1)
                {
                    try
                    {
                        UpdateRobustWeights(y, smoothedValues, robustWeights);
                    }
                    catch (Exception)
                    {
                        // If updating weights fails, just use equal weights and stop iterations
                        Array.Fill(robustWeights, 1.0);
                        break;
                    }
                }
            }

            // Calculate standard deviation safely
            double standardDeviation = CalculateStandardDeviationSafe(x, y, smoothedValues);

            // Return smoothed values as coefficients
            return (smoothedValues, standardDeviation);
        }

        private (double[] coefficients, double standardDeviation) CalculateFallback(double[] x, double[] y)
        {
            int n = x.Length;

            // Use a simplified moving average as fallback
            double[] smoothedValues = new double[n];
            int windowSize = Math.Min(5, n);

            if (n <= 1)
            {
                // For single point, just return it
                if (n == 1) smoothedValues[0] = y[0];
                return (smoothedValues, 0.0001);
            }

            // Calculate moving average with smaller window
            for (int i = 0; i < n; i++)
            {
                int windowStart = Math.Max(0, i - windowSize / 2);
                int windowEnd = Math.Min(n - 1, i + windowSize / 2);
                int count = windowEnd - windowStart + 1;

                double sum = 0;
                for (int j = windowStart; j <= windowEnd; j++)
                {
                    sum += y[j];
                }

                smoothedValues[i] = sum / count;
            }

            // Calculate simple standard deviation
            double sumSquaredErrors = 0;
            for (int i = 0; i < n; i++)
            {
                double error = y[i] - smoothedValues[i];
                sumSquaredErrors += error * error;
            }

            double standardDeviation = Math.Sqrt(sumSquaredErrors / n);

            // If invalid, use range-based approximation
            if (double.IsNaN(standardDeviation) || double.IsInfinity(standardDeviation))
            {
                double min = y.Min();
                double max = y.Max();
                double range = max - min;
                standardDeviation = Math.Max(range * 0.1, 0.0001);
            }

            return (smoothedValues, standardDeviation);
        }

        private double GetEffectiveBandwidth(double[] x, int i)
        {
            try
            {
                // Adapt bandwidth near boundaries to avoid edge effects
                int n = x.Length;
                double range = x[n - 1] - x[0];

                // Prevent division by zero
                if (Math.Abs(range) < 1e-10)
                    return Math.Max(_bandwidth, 0.1);

                double distance = Math.Min(x[n - 1] - x[i], x[i] - x[0]);
                return Math.Max(_bandwidth * range, distance * 2);
            }
            catch (Exception)
            {
                // Return a safe default if calculation fails
                return Math.Max(_bandwidth, 0.1);
            }
        }

        private double CalculateWeightedAverage(double[] x, double[] y, double[] weights, double xi)
        {
            try
            {
                double sumWY = 0, sumW = 0;
                for (int i = 0; i < x.Length; i++)
                {
                    if (weights[i] > 0)
                    {
                        double weight = weights[i];
                        sumWY += weight * y[i];
                        sumW += weight;

                        // Check for overflow
                        if (double.IsInfinity(sumWY) || double.IsInfinity(sumW))
                            throw new OverflowException("Weighted average calculation overflow");
                    }
                }

                // Avoid division by zero
                if (sumW <= 1e-10)
                    return CalculateFallbackValue(y);

                double result = sumWY / sumW;

                // Validate result
                if (double.IsInfinity(result) || double.IsNaN(result))
                    return CalculateFallbackValue(y);

                return result;
            }
            catch (Exception)
            {
                return CalculateFallbackValue(y);
            }
        }

        private double CalculateFallbackValue(double[] y)
        {
            // Simple fallback value when weighted calculation fails
            if (y.Length == 0)
                return 0;

            try
            {
                // Try median (use copy to avoid mutation)
                double[] sorted = new double[y.Length];
                Array.Copy(y, sorted, y.Length);
                Array.Sort(sorted);
                return sorted[sorted.Length / 2];
            }
            catch (Exception)
            {
                // If sorting fails, use simple average
                double sum = 0;
                int count = 0;

                foreach (var val in y)
                {
                    if (!double.IsInfinity(val) && !double.IsNaN(val))
                    {
                        sum += val;
                        count++;
                    }
                }

                return count > 0 ? sum / count : 0;
            }
        }

        private void UpdateRobustWeights(double[] y, double[] fitted, double[] robustWeights)
        {
            try
            {
                // Calculate residuals
                double[] residuals = new double[y.Length];
                for (int i = 0; i < y.Length; i++)
                {
                    residuals[i] = Math.Abs(y[i] - fitted[i]);
                }

                // Calculate median absolute deviation
                double mad = CalculateMedian(residuals);

                // Increase MAD to avoid over-sensitivity to small residuals
                mad = Math.Max(mad * 6.0, 1e-10);

                // Update robust weights using bisquare function
                for (int i = 0; i < y.Length; i++)
                {
                    double u = residuals[i] / mad;

                    if (u >= 1)
                    {
                        robustWeights[i] = 0;
                    }
                    else
                    {
                        // Calculate with overflow protection
                        double temp = 1 - u * u;
                        robustWeights[i] = temp * temp;
                    }

                    // Ensure valid weight
                    if (double.IsNaN(robustWeights[i]) || double.IsInfinity(robustWeights[i]))
                    {
                        robustWeights[i] = 0;
                    }
                }
            }
            catch (Exception)
            {
                // If robust weight calculation fails, use equal weights
                Array.Fill(robustWeights, 1.0);
            }
        }

        private double CalculateMedian(double[] values)
        {
            int n = values.Length;

            if (n == 0) return 0;
            if (n == 1) return values[0];

            try
            {
                // Create copy to avoid modifying original data
                double[] sorted = new double[n];
                Array.Copy(values, sorted, n);

                // Check for invalid values
                for (int i = 0; i < n; i++)
                {
                    if (double.IsInfinity(sorted[i]) || double.IsNaN(sorted[i]))
                    {
                        sorted[i] = 0; // Replace invalid values
                    }
                }

                Array.Sort(sorted);

                if (n % 2 == 0)
                    return (sorted[n / 2 - 1] + sorted[n / 2]) / 2;
                else
                    return sorted[n / 2];
            }
            catch (Exception)
            {
                // If sorting fails, calculate simple average (don't mutate original)
                double sum = 0;
                int count = 0;

                foreach (var val in values)
                {
                    if (!double.IsInfinity(val) && !double.IsNaN(val))
                    {
                        sum += val;
                        count++;
                    }
                }

                return count > 0 ? sum / count : 0;
            }
        }

        private double CalculateStandardDeviationSafe(double[] x, double[] y, double[] smoothedValues)
        {
            try
            {
                double sumSquaredErrors = 0;
                int n = x.Length;
                int validCount = 0;

                for (int i = 0; i < n; i++)
                {
                    double error = y[i] - smoothedValues[i];

                    // Check for valid error
                    if (!double.IsInfinity(error) && !double.IsNaN(error))
                    {
                        sumSquaredErrors += error * error;
                        validCount++;
                    }
                }

                // Avoid division by zero
                if (validCount == 0)
                    return 0.0001;

                return Math.Sqrt(sumSquaredErrors / validCount);
            }
            catch (Exception)
            {
                // Fallback to a range-based estimation
                double min = double.MaxValue;
                double max = double.MinValue;
                int count = 0;

                for (int i = 0; i < y.Length; i++)
                {
                    if (!double.IsInfinity(y[i]) && !double.IsNaN(y[i]))
                    {
                        min = Math.Min(min, y[i]);
                        max = Math.Max(max, y[i]);
                        count++;
                    }
                }

                double range = count > 0 ? max - min : 1;
                if (range <= 0) range = 1;

                return range * 0.1; // 10% of range
            }
        }

        public override double EvaluateRegression(double[] coefficients, double x)
        {
            // The coefficients array contains the smoothed values at each input x
            try
            {
                // Find the closest index for interpolation
                int n = coefficients.Length;

                if (n == 0) return 0;
                if (n == 1) return coefficients[0];

                // Find two closest points for linear interpolation
                int idx1 = 0;
                double minDist = double.MaxValue;

                for (int i = 0; i < n; i++)
                {
                    double dist = Math.Abs(i / (double)(n - 1) - x);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        idx1 = i;
                    }
                }

                int idx2;
                if (idx1 == 0)
                    idx2 = 1;
                else if (idx1 == n - 1)
                    idx2 = n - 2;
                else
                    idx2 = idx1 + (x > idx1 / (double)(n - 1) ? 1 : -1);

                // Perform linear interpolation
                double x1 = idx1 / (double)(n - 1);
                double x2 = idx2 / (double)(n - 1);
                double y1 = coefficients[idx1];
                double y2 = coefficients[idx2];

                // Check if x falls on a point
                if (Math.Abs(x - x1) < 1e-10)
                    return y1;

                // Linear interpolation formula
                double slope = (y2 - y1) / (x2 - x1);
                return y1 + slope * (x - x1);
            }
            catch (Exception)
            {
                // Default to returning the first or middle coefficient
                if (coefficients.Length == 0)
                    return 0;

                int midIdx = coefficients.Length / 2;
                return coefficients[midIdx];
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
