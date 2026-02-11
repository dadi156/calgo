using System;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Weighted regression implementation (more recent data gets higher weights) with optimized calculations
    /// </summary>
    public class WeightedRegression : BaseRegression
    {
        public WeightedRegression(int period) : base(period) { }

        public override (double[] coefficients, double standardDeviation) Calculate(double[] x, double[] y)
        {
            int n = x.Length;
            
            // Handle empty arrays or invalid inputs
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
            int mainN = x.Length;
            double mainSumWX = 0, mainSumWY = 0, mainSumWXY = 0, mainSumWX2 = 0, mainSumW = 0;

            for (int i = 0; i < mainN; i++)
            {
                // Exponential weighting - more recent data gets higher weight
                double mainWeight = Math.Exp((double)i / mainN);
                mainSumW += mainWeight;
                mainSumWX += mainWeight * x[i];
                mainSumWY += mainWeight * y[i];
                mainSumWXY += mainWeight * x[i] * y[i];
                mainSumWX2 += mainWeight * x[i] * x[i];
                
                // Check for overflow
                if (double.IsInfinity(mainSumW) || double.IsInfinity(mainSumWX) || 
                    double.IsInfinity(mainSumWY) || double.IsInfinity(mainSumWXY) || 
                    double.IsInfinity(mainSumWX2))
                {
                    throw new OverflowException("Calculation overflow");
                }
            }

            double mainDenom = (mainSumW * mainSumWX2 - mainSumWX * mainSumWX);
            
            double[] mainOutput;
            
            if (Math.Abs(mainDenom) < 1e-10)
            {
                // Near-zero denominator, use weighted average for flat line
                double mainFlat = mainSumWY / mainSumW;
                mainOutput = new double[] { mainFlat, 0 };
                return (mainOutput, 0.0001);
            }

            double mainM = (mainSumW * mainSumWXY - mainSumWX * mainSumWY) / mainDenom;
            double mainB = (mainSumWY - mainM * mainSumWX) / mainSumW;
            
            // Check for valid values
            if (double.IsInfinity(mainM) || double.IsNaN(mainM) ||
                double.IsInfinity(mainB) || double.IsNaN(mainB))
            {
                throw new OverflowException("Invalid calculation result");
            }

            mainOutput = new double[] { mainB, mainM };
            double mainStdDev = ComputeWeightedStandardDeviation(x, y, mainOutput);

            return (mainOutput, mainStdDev);
        }

        private (double[] coefficients, double standardDeviation) CalculateWithFallback(double[] x, double[] y)
        {
            int altN = x.Length;
            
            // Find min/max for better normalization
            double altMinX = double.MaxValue;
            double altMaxX = double.MinValue;
            double altMinY = double.MaxValue;
            double altMaxY = double.MinValue;
            
            for (int i = 0; i < altN; i++)
            {
                altMinX = Math.Min(altMinX, x[i]);
                altMaxX = Math.Max(altMaxX, x[i]);
                altMinY = Math.Min(altMinY, y[i]);
                altMaxY = Math.Max(altMaxY, y[i]);
            }
            
            // Avoid division by zero
            double altRangeX = Math.Max(altMaxX - altMinX, 0.0001);
            double altRangeY = Math.Max(altMaxY - altMinY, 0.0001);
            
            // Use normalized values
            double[] altNormX = new double[altN];
            double[] altNormY = new double[altN];
            
            for (int i = 0; i < altN; i++)
            {
                altNormX[i] = (x[i] - altMinX) / altRangeX;
                altNormY[i] = (y[i] - altMinY) / altRangeY;
            }
            
            // Use simpler weights for fallback
            double[] altWeights = new double[altN];
            double altTotalWeight = 0;
            
            for (int i = 0; i < altN; i++)
            {
                // Linear weights instead of exponential for more stability
                altWeights[i] = 1 + i / (double)altN;
                altTotalWeight += altWeights[i];
            }
            
            // Calculate with normalized values and simpler weights
            double altSumWX = 0, altSumWY = 0, altSumWXY = 0, altSumWX2 = 0;
            
            for (int i = 0; i < altN; i++)
            {
                altSumWX += altWeights[i] * altNormX[i];
                altSumWY += altWeights[i] * altNormY[i];
                altSumWXY += altWeights[i] * altNormX[i] * altNormY[i];
                altSumWX2 += altWeights[i] * altNormX[i] * altNormX[i];
            }
            
            // Calculate coefficients
            double altDenom = (altTotalWeight * altSumWX2 - altSumWX * altSumWX);
            double altNormM, altNormB;
            
            if (Math.Abs(altDenom) < 1e-10)
            {
                // Use weighted average for flat line
                altNormM = 0;
                altNormB = altSumWY / altTotalWeight;
            }
            else
            {
                altNormM = (altTotalWeight * altSumWXY - altSumWX * altSumWY) / altDenom;
                altNormB = (altSumWY - altNormM * altSumWX) / altTotalWeight;
            }
            
            // Denormalize coefficients
            double altM = altNormM * (altRangeY / altRangeX);
            double altB = (altNormB * altRangeY + altMinY) - altM * altMinX;
            
            // Create coefficients array with limits to prevent extreme values
            double[] altOutput = new double[] { 
                altB,
                Math.Max(Math.Min(altM, 1e6), -1e6) // Limit slope to reasonable range
            };
            
            // Calculate standard deviation
            double altSumSquaredErrors = 0;
            double altSumWeights = 0;
            
            for (int i = 0; i < altN; i++)
            {
                double altPredicted = altOutput[0] + altOutput[1] * x[i];
                double altError = y[i] - altPredicted;
                double altWeightedError = altWeights[i] * altError * altError;
                altSumSquaredErrors += altWeightedError;
                altSumWeights += altWeights[i];
            }
            
            double altStdDev = Math.Sqrt(altSumSquaredErrors / altSumWeights);
            
            // If still invalid, use a very simple approximation
            if (double.IsNaN(altStdDev) || double.IsInfinity(altStdDev))
            {
                altStdDev = altRangeY * 0.1; // Use 10% of y range as std dev
            }
            
            return (altOutput, altStdDev);
        }
        
        /// <summary>
        /// Calculates weighted standard deviation of residuals
        /// </summary>
        private double ComputeWeightedStandardDeviation(double[] x, double[] y, double[] coeffs)
        {
            try
            {
                double wSumErrors = 0;
                double wSumWeights = 0;
                int wN = x.Length;

                for (int i = 0; i < wN; i++)
                {
                    double wWeight = Math.Exp((double)i / wN);
                    double wPredicted = EvaluateRegression(coeffs, x[i]);
                    double wError = y[i] - wPredicted;
                    
                    // Check for valid error
                    if (double.IsInfinity(wError) || double.IsNaN(wError))
                    {
                        throw new OverflowException("Error calculation overflow");
                    }
                    
                    wSumErrors += wWeight * wError * wError;
                    wSumWeights += wWeight;
                    
                    // Check for overflow
                    if (double.IsInfinity(wSumErrors) || double.IsInfinity(wSumWeights))
                    {
                        throw new OverflowException("Sum calculation overflow");
                    }
                }

                // Avoid division by zero
                if (wSumWeights < 1e-10)
                    return 0.0001;
                    
                return Math.Sqrt(wSumErrors / wSumWeights);
            }
            catch (Exception)
            {
                // Fallback to unweighted standard deviation
                return ComputeStandardDeviationSafe(x, y, coeffs);
            }
        }
        
        private double ComputeStandardDeviationSafe(double[] x, double[] y, double[] coeffs)
        {
            try
            {
                double sdSumErrors = 0;
                int sdN = x.Length;
                
                for (int i = 0; i < sdN; i++)
                {
                    double sdPredicted = EvaluateRegression(coeffs, x[i]);
                    double sdError = y[i] - sdPredicted;
                    sdSumErrors += sdError * sdError;
                }
                
                return Math.Sqrt(sdSumErrors / sdN);
            }
            catch (Exception)
            {
                // Use range-based estimation
                double sdRange = y.Max() - y.Min();
                if (sdRange <= 0) sdRange = 1;
                return sdRange * 0.1; // 10% of range
            }
        }

        public override double EvaluateRegression(double[] coefficients, double x)
        {
            return coefficients[0] + coefficients[1] * x;
        }
        
        /// <summary>
        /// Override the standard deviation calculation with weighted version
        /// </summary>
        protected override double CalculateStandardDeviation(double[] x, double[] y, double[] coefficients)
        {
            return ComputeWeightedStandardDeviation(x, y, coefficients);
        }
    }
}
