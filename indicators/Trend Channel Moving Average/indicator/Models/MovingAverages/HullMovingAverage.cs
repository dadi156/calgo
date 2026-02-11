using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Hull Moving Average (HMA)
    /// Very smooth MA with less lag
    /// Good for trend following
    /// </summary>
    public class HullMovingAverage : IMovingAverage
    {
        // Helper for weighted moving average
        private readonly WeightedMovingAverage _wma;
        
        // Cache for HMA values
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _hmaCache;
        private readonly Dictionary<DataSeries, int> _periodCache;

        public HullMovingAverage()
        {
            _wma = new WeightedMovingAverage();
            _hmaCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
        }

        /// <summary>
        /// Calculate HMA value
        /// Uses formula: WMA(2*WMA(n/2) - WMA(n), sqrt(n))
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < 0 || index >= prices.Count)
                return double.NaN;

            // Need at least period bars
            if (index < period - 1)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_hmaCache.ContainsKey(prices))
            {
                _hmaCache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _hmaCache[prices].Clear();
                _periodCache[prices] = period;
            }

            var cache = _hmaCache[prices];

            // Check cache first
            if (cache.ContainsKey(index))
                return cache[index];

            // Calculate HMA
            double hmaValue = CalculateHMA(prices, index, period);

            // Store in cache
            cache[index] = hmaValue;

            // Clean cache if needed
            if (cache.Count > 1000)
            {
                CleanCache(cache, index);
            }

            return hmaValue;
        }

        /// <summary>
        /// Calculate HMA using the formula
        /// </summary>
        private double CalculateHMA(DataSeries prices, int index, int period)
        {
            try
            {
                // Step 1: Calculate WMA with half period
                int halfPeriod = period / 2;
                double wma1 = _wma.Calculate(prices, index, halfPeriod);
                
                if (double.IsNaN(wma1))
                    return double.NaN;

                // Step 2: Calculate WMA with full period
                double wma2 = _wma.Calculate(prices, index, period);
                
                if (double.IsNaN(wma2))
                    return double.NaN;

                // Step 3: Calculate 2*WMA(n/2) - WMA(n)
                double[] tempValues = new double[period];
                for (int i = 0; i < period; i++)
                {
                    int priceIndex = index - i;
                    if (priceIndex < 0)
                        return double.NaN;

                    // Get WMA values for this index
                    double tempWma1 = _wma.Calculate(prices, priceIndex, halfPeriod);
                    double tempWma2 = _wma.Calculate(prices, priceIndex, period);
                    
                    if (double.IsNaN(tempWma1) || double.IsNaN(tempWma2))
                        return double.NaN;

                    tempValues[period - 1 - i] = 2 * tempWma1 - tempWma2;
                }

                // Step 4: Calculate WMA of the temp values with sqrt(period)
                int sqrtPeriod = (int)Math.Round(Math.Sqrt(period));
                sqrtPeriod = Math.Max(1, sqrtPeriod);
                
                if (sqrtPeriod > tempValues.Length)
                    sqrtPeriod = tempValues.Length;

                return CalculateWMAFromArray(tempValues, sqrtPeriod);
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Calculate WMA from array of values
        /// </summary>
        private double CalculateWMAFromArray(double[] values, int period)
        {
            if (values.Length < period)
                return double.NaN;

            double sum = 0;
            double weightSum = 0;

            // Take last 'period' values
            int startIndex = values.Length - period;
            
            for (int i = 0; i < period; i++)
            {
                double weight = i + 1; // Weight increases with newer values
                sum += values[startIndex + i] * weight;
                weightSum += weight;
            }

            if (weightSum == 0)
                return double.NaN;

            return sum / weightSum;
        }

        /// <summary>
        /// Clean old cache values
        /// </summary>
        private void CleanCache(Dictionary<int, double> cache, int currentIndex)
        {
            var keysToRemove = new List<int>();
            
            foreach (var key in cache.Keys)
            {
                if (key < currentIndex - 500)
                    keysToRemove.Add(key);
            }
            
            foreach (var key in keysToRemove)
            {
                cache.Remove(key);
            }
        }
    }

    /// <summary>
    /// Helper class for Weighted Moving Average
    /// Used by Hull Moving Average
    /// </summary>
    internal class WeightedMovingAverage
    {
        public double Calculate(DataSeries prices, int index, int period)
        {
            if (index < period - 1 || index < 0 || index >= prices.Count)
                return double.NaN;

            double sum = 0;
            double weightSum = 0;

            for (int i = 0; i < period; i++)
            {
                int priceIndex = index - i;
                if (priceIndex < 0)
                    return double.NaN;

                double weight = period - i; // Newer prices get higher weight
                sum += prices[priceIndex] * weight;
                weightSum += weight;
            }

            if (weightSum == 0)
                return double.NaN;

            return sum / weightSum;
        }
    }
}
