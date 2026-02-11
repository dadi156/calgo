using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Arnaud Legoux Moving Average (ALMA)
    /// Very smooth MA with less lag
    /// Uses Gaussian filter for better smoothing
    /// </summary>
    public class ArnaudLegouxMovingAverage : IMovingAverage
    {
        // Default parameters for ALMA
        private const double DefaultOffset = 0.85;   // Where to focus (0 = start, 1 = end)
        private const double DefaultSigma = 6.0;     // How smooth the filter is

        /// <summary>
        /// Calculate ALMA value
        /// Uses Gaussian weights for smoothing
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period - 1)
                return double.NaN;

            // Check bounds
            if (index < 0 || index >= prices.Count)
                return double.NaN;

            return CalculateALMA(prices, index, period, DefaultOffset, DefaultSigma);
        }

        /// <summary>
        /// Calculate ALMA with custom parameters
        /// </summary>
        public double CalculateWithParameters(DataSeries prices, int index, int period, 
                                            double offset, double sigma)
        {
            if (index < period - 1)
                return double.NaN;

            if (index < 0 || index >= prices.Count)
                return double.NaN;

            return CalculateALMA(prices, index, period, offset, sigma);
        }

        /// <summary>
        /// Main ALMA calculation
        /// </summary>
        private double CalculateALMA(DataSeries prices, int index, int period, 
                                   double offset, double sigma)
        {
            double weightSum = 0;
            double priceSum = 0;

            // Calculate center point
            double m = Math.Floor(offset * (period - 1));
            double s = period / sigma;

            // Calculate weighted sum using Gaussian filter
            for (int i = 0; i < period; i++)
            {
                int priceIndex = index - (period - 1 - i);
                
                // Check bounds
                if (priceIndex < 0 || priceIndex >= prices.Count)
                    continue;

                // Calculate Gaussian weight
                double weight = CalculateGaussianWeight(i, m, s);
                
                // Add to sums
                priceSum += prices[priceIndex] * weight;
                weightSum += weight;
            }

            // Avoid division by zero
            if (weightSum == 0)
                return double.NaN;

            return priceSum / weightSum;
        }

        /// <summary>
        /// Calculate Gaussian weight
        /// This gives smooth weights that focus on certain area
        /// </summary>
        private double CalculateGaussianWeight(double x, double m, double s)
        {
            // Gaussian formula: e^(-((x-m)^2) / (2*s^2))
            double exponent = -((x - m) * (x - m)) / (2 * s * s);
            return Math.Exp(exponent);
        }
    }
}
