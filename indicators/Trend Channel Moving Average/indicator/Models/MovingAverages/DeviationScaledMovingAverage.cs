using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Enhanced Deviation Scaled Moving Average (DSMA) by John Ehlers
    /// Improved version with noise reduction for short periods
    /// </summary>
    public class DeviationScaledMovingAverage : IMovingAverage
    {
        // SuperSmoother filter coefficients
        private double _a1, _b1, _c1, _c2, _c3;
        private bool _coefficientsCalculated = false;
        
        // Filter arrays for each price series
        private readonly System.Collections.Generic.Dictionary<string, double[]> _filtArrays 
            = new System.Collections.Generic.Dictionary<string, double[]>();
        private readonly System.Collections.Generic.Dictionary<string, double[]> _dsmaArrays 
            = new System.Collections.Generic.Dictionary<string, double[]>();
        private readonly System.Collections.Generic.Dictionary<string, double[]> _zerosArrays 
            = new System.Collections.Generic.Dictionary<string, double[]>();
        
        private const int MAX_BARS = 10000; // Maximum bars to store

        /// <summary>
        /// Calculate Enhanced DSMA with noise reduction for short periods
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            if (index < 0 || period < 3)
                return double.NaN;

            // Create unique key for this price series
            string seriesKey = prices.GetHashCode().ToString();
            
            // Initialize arrays if needed
            if (!_filtArrays.ContainsKey(seriesKey))
            {
                _filtArrays[seriesKey] = new double[MAX_BARS];
                _dsmaArrays[seriesKey] = new double[MAX_BARS];
                _zerosArrays[seriesKey] = new double[MAX_BARS];
                
                // Initialize arrays to zero
                for (int i = 0; i < MAX_BARS; i++)
                {
                    _filtArrays[seriesKey][i] = 0;
                    _dsmaArrays[seriesKey][i] = 0;
                    _zerosArrays[seriesKey][i] = 0;
                }
            }

            var filtArray = _filtArrays[seriesKey];
            var dsmaArray = _dsmaArrays[seriesKey];
            var zerosArray = _zerosArrays[seriesKey];

            // Calculate SuperSmoother coefficients (only once)
            if (!_coefficientsCalculated)
            {
                CalculateEnhancedSuperSmootherCoefficients(period);
                _coefficientsCalculated = true;
            }

            // Handle first few bars
            if (index < 3)
            {
                zerosArray[index] = 0;
                filtArray[index] = 0;
                dsmaArray[index] = prices[index];
                return dsmaArray[index];
            }

            // Step 1: Zeros Oscillator (Close - Close[2])
            zerosArray[index] = prices[index] - prices[index - 2];

            // Step 2: Enhanced SuperSmoother Filter
            double filt = _c1 * (zerosArray[index] + zerosArray[index - 1]) / 2 + 
                         _c2 * filtArray[index - 1] + 
                         _c3 * filtArray[index - 2];

            // Fix NaN values
            if (double.IsNaN(filt) || double.IsInfinity(filt))
            {
                filt = 0;
            }

            filtArray[index] = filt;

            // Step 3: Calculate DSMA
            if (index >= period + 2)
            {
                // Use different calculation based on period
                if (period < 20)
                {
                    // ENHANCED CALCULATION for short periods (≤ 20)
                    
                    // ENHANCEMENT 1: Use minimum RMS period for stability
                    int rmsPeriod = Math.Max(period, 15); // Minimum 15 bars for stable RMS
                    double rms = CalculateRMS(index, rmsPeriod, filtArray);
                    
                    double scaledFilt = 0;
                    if (rms > 0.000001)
                    {
                        scaledFilt = filt / rms;
                    }

                    // ENHANCEMENT 2: Adaptive multiplier based on period
                    double multiplier = 3.5;

                    // Calculate adaptive alpha
                    double alpha = Math.Abs(scaledFilt) * multiplier / period;
                    
                    // ENHANCEMENT 3: Adaptive alpha limits
                    double minAlpha = 0.005, maxAlpha = 0.6;
                    
                    alpha = Math.Max(minAlpha, Math.Min(maxAlpha, alpha));

                    // Calculate DSMA
                    dsmaArray[index] = alpha * prices[index] + 
                                      (1 - alpha) * dsmaArray[index - 1];
                }
                else
                {
                    // ORIGINAL CALCULATION for long periods (> 20)
                    // Same as John Ehlers formula - no changes
                    
                    double rms = CalculateRMS(index, period, filtArray);
                    double scaledFilt = 0;
                    
                    if (rms > 0.000001)
                    {
                        scaledFilt = filt / rms;
                    }

                    // Original alpha calculation
                    double alpha = Math.Abs(scaledFilt) * 5.0 / period;
                    alpha = Math.Max(0.001, Math.Min(0.999, alpha)); // Original limits

                    // Original DSMA calculation
                    dsmaArray[index] = alpha * prices[index] + 
                                      (1 - alpha) * dsmaArray[index - 1];
                }
            }
            else
            {
                // Use simple EMA for early bars
                double simpleAlpha = 2.0 / (period + 1);
                dsmaArray[index] = simpleAlpha * prices[index] + 
                                  (1 - simpleAlpha) * dsmaArray[index - 1];
            }

            // Final safety check
            if (double.IsNaN(dsmaArray[index]) || double.IsInfinity(dsmaArray[index]))
            {
                dsmaArray[index] = prices[index];
            }
            
            return dsmaArray[index];
        }

        /// <summary>
        /// Calculate RMS with validation
        /// </summary>
        private double CalculateRMS(int index, int period, double[] filtArray)
        {
            double sumSquares = 0;
            int validPoints = 0;
            
            for (int i = 0; i < period; i++)
            {
                int lookbackIndex = index - i;
                if (lookbackIndex >= 0 && !double.IsNaN(filtArray[lookbackIndex]))
                {
                    sumSquares += filtArray[lookbackIndex] * filtArray[lookbackIndex];
                    validPoints++;
                }
            }
            
            return validPoints > 0 ? Math.Sqrt(sumSquares / validPoints) : 0;
        }

        /// <summary>
        /// Calculate SuperSmoother coefficients
        /// Enhanced only for short periods (≤ 20)
        /// </summary>
        private void CalculateEnhancedSuperSmootherCoefficients(int period)
        {
            double criticalPeriod;
            
            if (period < 20)
            {
                // ENHANCEMENT: Minimum critical period for better smoothing (short periods only)
                criticalPeriod = Math.Max(0.5 * period, 8); // Minimum 8 bars critical period
            }
            else
            {
                // ORIGINAL: Standard calculation for long periods
                criticalPeriod = 0.5 * period;
            }
            
            _a1 = Math.Exp(-1.414 * Math.PI / criticalPeriod);
            _b1 = 2 * _a1 * Math.Cos(1.414 * Math.PI / criticalPeriod);
            _c2 = _b1;
            _c3 = -_a1 * _a1;
            _c1 = 1 - _c2 - _c3;
        }

        /// <summary>
        /// Reset internal state
        /// </summary>
        public void Reset()
        {
            _coefficientsCalculated = false;
            _filtArrays.Clear();
            _dsmaArrays.Clear();
            _zerosArrays.Clear();
        }
    }
}
