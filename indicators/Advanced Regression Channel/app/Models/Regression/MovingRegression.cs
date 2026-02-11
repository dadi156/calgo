using System;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Moving regression implementation (uses only the latest window of data) with optimized calculations
    /// </summary>
    public class MovingRegression : BaseRegression
    {
        public MovingRegression(int period) : base(period) { }

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
            
            // Calculate simple linear regression for the latest window
            double primSumX = 0, primSumY = 0, primSumXY = 0, primSumX2 = 0;

            // Use only the last _period points
            int primStartIdx = Math.Max(0, primN - _period);
            int primWindowSize = primN - primStartIdx;

            for (int i = primStartIdx; i < primN; i++)
            {
                primSumX += x[i];
                primSumY += y[i];
                primSumXY += x[i] * y[i];
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
                // Near-zero denominator, use flat line at last price
                double[] primResultFlat = new double[] { y[primN - 1], 0 };
                return (primResultFlat, 0.0001);
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
            double primStdDev = ComputeWindowStandardDeviation(x, y, primResult, primStartIdx);

            return (primResult, primStdDev);
        }

        private (double[] coefficients, double standardDeviation) CalculateWithFallback(double[] x, double[] y)
        {
            int altN = x.Length;
            
            // Use only last _period values, or all if less
            int altStartIdx = Math.Max(0, altN - _period);
            int altWindowSize = altN - altStartIdx;
            
            // Extract window data
            double[] altWindowX = new double[altWindowSize];
            double[] altWindowY = new double[altWindowSize];
            
            for (int i = 0; i < altWindowSize; i++)
            {
                altWindowX[i] = x[altStartIdx + i];
                altWindowY[i] = y[altStartIdx + i];
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
                // Use flat line at average y
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
            
            // Use window standard deviation
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
                altStdDev = altRangeY * 0.1; // 10% of y range as std dev
            }
            
            return (new double[] { altIntercept, altSlope }, altStdDev);
        }
        
        /// <summary>
        /// Calculates standard deviation for just the window
        /// </summary>
        private double ComputeWindowStandardDeviation(double[] x, double[] y, double[] coeffs, int startIdx)
        {
            try
            {
                double winSumErrors = 0;
                int winN = x.Length;
                int winSize = winN - startIdx;
                
                for (int i = startIdx; i < winN; i++)
                {
                    double winPredicted = EvaluateRegression(coeffs, x[i]);
                    double winError = y[i] - winPredicted;
                    
                    // Check for overflow
                    if (double.IsInfinity(winError) || double.IsNaN(winError))
                    {
                        throw new OverflowException("Error calculation overflow");
                    }
                    
                    winSumErrors += winError * winError;
                    
                    // Check for overflow
                    if (double.IsInfinity(winSumErrors))
                    {
                        throw new OverflowException("Sum of squared errors overflow");
                    }
                }
                
                return Math.Sqrt(winSumErrors / winSize);
            }
            catch (Exception)
            {
                // Fallback to a simpler calculation
                double winSum = 0;
                double winMin = double.MaxValue;
                double winMax = double.MinValue;
                
                // Use only window data
                for (int i = startIdx; i < y.Length; i++)
                {
                    winMin = Math.Min(winMin, y[i]);
                    winMax = Math.Max(winMax, y[i]);
                    winSum += y[i];
                }
                
                // Use a percentage of the range as std dev
                double winRange = winMax - winMin;
                if (winRange <= 0) winRange = 1;
                
                return winRange * 0.1; // 10% of range
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
        /// Override the standard deviation calculation to use only window data
        /// </summary>
        protected override double CalculateStandardDeviation(double[] x, double[] y, double[] coefficients)
        {
            int startIdx = Math.Max(0, x.Length - _period);
            return ComputeWindowStandardDeviation(x, y, coefficients, startIdx);
        }
    }
}
