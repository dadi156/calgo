using System;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Exponential moving regression implementation (applies EMA smoothing before regression)
    /// with optimized calculations and error handling
    /// </summary>
    public class ExponentialMovingRegression : BaseRegression
    {
        private readonly double _alpha;

        public ExponentialMovingRegression(int period, double alpha = 0.3) : base(period)
        {
            _alpha = Math.Max(0.01, Math.Min(0.99, alpha)); // Limit alpha to reasonable range
        }

        public override (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Handle empty arrays or insufficient data
            if (n < 2)
                return (new double[] { 0, 0 }, 0);
                
            try
            {
                // Calculate with overflow protection
                return CalculateWithProtection(x, y);
            }
            catch (Exception)
            {
                // Fallback to a more conservative calculation
                return CalculateWithFallback(x, y);
            }
        }

        private (double[] coefficients, double standardDeviation) CalculateWithProtection(double[] x, double[] y)
        {
            int primN = x.Length;
            
            // First smooth the y values
            double[] primSmoothedY = new double[primN];
            primSmoothedY[0] = y[0];

            for (int i = 1; i < primN; i++)
            {
                primSmoothedY[i] = _alpha * y[i] + (1 - _alpha) * primSmoothedY[i - 1];
                
                // Check for overflow/NaN
                if (double.IsInfinity(primSmoothedY[i]) || double.IsNaN(primSmoothedY[i]))
                {
                    throw new OverflowException("Smoothing calculation overflow");
                }
            }

            // Then calculate linear regression on the last _period points
            double primSumX = 0, primSumY = 0, primSumXY = 0, primSumX2 = 0;

            int primStartIdx = Math.Max(0, primN - _period);
            int primWindowSize = primN - primStartIdx;

            for (int i = primStartIdx; i < primN; i++)
            {
                primSumX += x[i];
                primSumY += primSmoothedY[i];
                primSumXY += x[i] * primSmoothedY[i];
                primSumX2 += x[i] * x[i];
                
                // Check for overflow
                if (double.IsInfinity(primSumX) || double.IsInfinity(primSumY) || 
                    double.IsInfinity(primSumXY) || double.IsInfinity(primSumX2))
                {
                    throw new OverflowException("Calculation overflow");
                }
            }

            double primDenom = (primWindowSize * primSumX2 - primSumX * primSumX);
            if (Math.Abs(primDenom) < 1e-10)
            {
                // Return flat line at last smoothed price
                double[] primFlatResult = new double[] { primSmoothedY[primN - 1], 0 };
                return (primFlatResult, 0.0001);
            }

            double primSlope = (primWindowSize * primSumXY - primSumX * primSumY) / primDenom;
            double primIntercept = (primSumY - primSlope * primSumX) / primWindowSize;
            
            // Check for valid coefficients
            if (double.IsInfinity(primSlope) || double.IsNaN(primSlope) ||
                double.IsInfinity(primIntercept) || double.IsNaN(primIntercept))
            {
                throw new OverflowException("Invalid calculation result");
            }

            double[] primResult = new double[] { primIntercept, primSlope };
            double primStdDev = ComputeSmoothedStandardDeviation(x, primSmoothedY, primResult, primStartIdx);

            return (primResult, primStdDev);
        }

        private (double[] coefficients, double standardDeviation) CalculateWithFallback(double[] x, double[] y)
        {
            int altN = x.Length;
            
            // Use only last _period values, or all if less
            int altStartIdx = Math.Max(0, altN - _period);
            int altWindowSize = altN - altStartIdx;
            
            // Create a less aggressive smoothing for fallback
            double altAlpha = Math.Min(_alpha, 0.2);
            
            // Apply simpler smoothing
            double[] altSmoothedY = new double[altN];
            if (altN > 0) altSmoothedY[0] = y[0];
            
            for (int i = 1; i < altN; i++)
            {
                // Use a simple moving average if EMA fails
                if (i < 5)
                {
                    double altSum = 0;
                    for (int j = 0; j <= i; j++)
                    {
                        altSum += y[j];
                    }
                    altSmoothedY[i] = altSum / (i + 1);
                }
                else
                {
                    // Try EMA with fallback alpha
                    try
                    {
                        altSmoothedY[i] = altAlpha * y[i] + (1 - altAlpha) * altSmoothedY[i - 1];
                        
                        // Check for validity
                        if (double.IsInfinity(altSmoothedY[i]) || double.IsNaN(altSmoothedY[i]))
                        {
                            // Use SMA instead
                            double altSum = 0;
                            for (int j = Math.Max(0, i - 4); j <= i; j++)
                            {
                                altSum += y[j];
                            }
                            altSmoothedY[i] = altSum / Math.Min(5, i + 1);
                        }
                    }
                    catch
                    {
                        // Use last valid value
                        altSmoothedY[i] = altSmoothedY[i - 1];
                    }
                }
            }
            
            // Extract window data
            double[] altWindowX = new double[altWindowSize];
            double[] altWindowY = new double[altWindowSize];
            
            for (int i = 0; i < altWindowSize; i++)
            {
                altWindowX[i] = x[altStartIdx + i];
                altWindowY[i] = altSmoothedY[altStartIdx + i];
            }
            
            // Find min/max for better normalization
            double altMinX = double.MaxValue;
            double altMaxX = double.MinValue;
            double altMinY = double.MaxValue;
            double altMaxY = double.MinValue;
            
            for (int i = 0; i < altWindowSize; i++)
            {
                altMinX = Math.Min(altMinX, altWindowX[i]);
                altMaxX = Math.Max(altMaxX, altWindowX[i]);
                altMinY = Math.Min(altMinY, altWindowY[i]);
                altMaxY = Math.Max(altMaxY, altWindowY[i]);
            }
            
            // Avoid division by zero
            double altRangeX = Math.Max(altMaxX - altMinX, 0.0001);
            double altRangeY = Math.Max(altMaxY - altMinY, 0.0001);
            
            // Normalize values
            double[] altNormX = new double[altWindowSize];
            double[] altNormY = new double[altWindowSize];
            
            for (int i = 0; i < altWindowSize; i++)
            {
                altNormX[i] = (altWindowX[i] - altMinX) / altRangeX;
                altNormY[i] = (altWindowY[i] - altMinY) / altRangeY;
            }
            
            // Calculate with normalized values
            double altSumX = 0, altSumY = 0, altSumXY = 0, altSumX2 = 0;
            
            for (int i = 0; i < altWindowSize; i++)
            {
                altSumX += altNormX[i];
                altSumY += altNormY[i];
                altSumXY += altNormX[i] * altNormY[i];
                altSumX2 += altNormX[i] * altNormX[i];
            }
            
            // Calculate coefficients
            double altDenom = (altWindowSize * altSumX2 - altSumX * altSumX);
            double altNormSlope, altNormIntercept;
            
            if (Math.Abs(altDenom) < 1e-10 || altWindowSize < 2)
            {
                // Use flat line at average smoothed y
                altNormSlope = 0;
                altNormIntercept = altWindowSize > 0 ? altSumY / altWindowSize : 0;
            }
            else
            {
                altNormSlope = (altWindowSize * altSumXY - altSumX * altSumY) / altDenom;
                altNormIntercept = (altSumY - altNormSlope * altSumX) / altWindowSize;
            }
            
            // Denormalize coefficients
            double altSlope = altNormSlope * (altRangeY / altRangeX);
            double altIntercept = (altNormIntercept * altRangeY + altMinY) - altSlope * altMinX;
            
            // Calculate standard deviation
            double altSumSquaredErrors = 0;
            
            for (int i = 0; i < altWindowSize; i++)
            {
                double altPredicted = altIntercept + altSlope * altWindowX[i];
                double altError = altWindowY[i] - altPredicted;
                altSumSquaredErrors += altError * altError;
            }
            
            double altStdDev = Math.Sqrt(altSumSquaredErrors / altWindowSize);
            
            // If invalid, use simple range-based approximation
            if (double.IsNaN(altStdDev) || double.IsInfinity(altStdDev))
            {
                altStdDev = altRangeY * 0.1; // 10% of range as std dev
            }
            
            return (new double[] { altIntercept, altSlope }, altStdDev);
        }
        
        /// <summary>
        /// Calculates standard deviation using smoothed values
        /// </summary>
        private double ComputeSmoothedStandardDeviation(double[] x, double[] smoothedY, double[] coeffs, int startIdx)
        {
            try
            {
                double devSumErrors = 0;
                int devN = x.Length;
                int devWindowSize = devN - startIdx;
                
                for (int i = startIdx; i < devN; i++)
                {
                    double devPredicted = EvaluateRegression(coeffs, x[i]);
                    double devError = smoothedY[i] - devPredicted;
                    
                    // Check for overflow
                    if (double.IsInfinity(devError) || double.IsNaN(devError))
                    {
                        throw new OverflowException("Error calculation overflow");
                    }
                    
                    devSumErrors += devError * devError;
                    
                    // Check for overflow
                    if (double.IsInfinity(devSumErrors))
                    {
                        throw new OverflowException("Sum of squared errors overflow");
                    }
                }
                
                return Math.Sqrt(devSumErrors / devWindowSize);
            }
            catch (Exception)
            {
                // Fallback to a simpler calculation
                double devSum = 0;
                double devMin = double.MaxValue;
                double devMax = double.MinValue;
                
                // Use only window data
                for (int i = startIdx; i < smoothedY.Length; i++)
                {
                    devMin = Math.Min(devMin, smoothedY[i]);
                    devMax = Math.Max(devMax, smoothedY[i]);
                    devSum += smoothedY[i];
                }
                
                // Use a percentage of the range as std dev
                double devRange = devMax - devMin;
                if (devRange <= 0) devRange = 1;
                
                return devRange * 0.1; // 10% of range
            }
        }

        public override double EvaluateRegression(double[] coefficients, double x)
        {
            try
            {
                return coefficients[0] + coefficients[1] * x;
            }
            catch (Exception)
            {
                // Fallback to a safer evaluation
                return coefficients.Length > 0 ? coefficients[0] : 0;
            }
        }
        
        /// <summary>
        /// Override the standard deviation calculation to use smoothed values
        /// </summary>
        protected override double CalculateStandardDeviation(double[] x, double[] y, double[] coefficients)
        {
            // Must calculate smoothed values again to match the ones used in calculation
            double[] calcSmoothedY = new double[y.Length];
            if (y.Length > 0) calcSmoothedY[0] = y[0];
            
            for (int i = 1; i < y.Length; i++)
            {
                calcSmoothedY[i] = _alpha * y[i] + (1 - _alpha) * calcSmoothedY[i - 1];
            }
            
            int calcStartIdx = Math.Max(0, x.Length - _period);
            return ComputeSmoothedStandardDeviation(x, calcSmoothedY, coefficients, calcStartIdx);
        }
    }
}
