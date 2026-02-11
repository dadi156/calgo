using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Kaufman Adaptive Moving Average (KAMA)
    /// Changes speed based on market noise
    /// Fast when trend is strong, slow when market is noisy
    /// </summary>
    public class KaufmanAdaptiveMovingAverage : IMovingAverage
    {
        // Cache for KAMA values
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _kamaCache;
        private readonly Dictionary<DataSeries, int> _periodCache;

        // Default parameters
        private const int FastSC = 2;   // Fast smoothing constant
        private const int SlowSC = 30;  // Slow smoothing constant

        public KaufmanAdaptiveMovingAverage()
        {
            _kamaCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
        }

        /// <summary>
        /// Calculate KAMA value
        /// Uses efficiency ratio to adapt speed
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period || index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_kamaCache.ContainsKey(prices))
            {
                _kamaCache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _kamaCache[prices].Clear();
                _periodCache[prices] = period;
            }

            var cache = _kamaCache[prices];

            // Check cache first
            if (cache.ContainsKey(index))
                return cache[index];

            double kamaValue;

            if (index == period)
            {
                // First KAMA value = simple average
                double sum = 0;
                for (int i = 0; i < period; i++)
                {
                    sum += prices[index - i];
                }
                kamaValue = sum / period;
            }
            else
            {
                // Calculate KAMA using previous value
                double previousKAMA = Calculate(prices, index - 1, period);
                if (double.IsNaN(previousKAMA))
                {
                    kamaValue = double.NaN;
                }
                else
                {
                    kamaValue = CalculateKAMA(prices, index, period, previousKAMA);
                }
            }

            // Store in cache
            cache[index] = kamaValue;

            // Clean cache if needed
            if (cache.Count > 1000)
            {
                CleanCache(cache, index);
            }

            return kamaValue;
        }

        /// <summary>
        /// Calculate KAMA using efficiency ratio
        /// </summary>
        private double CalculateKAMA(DataSeries prices, int index, int period, double previousKAMA)
        {
            try
            {
                // Step 1: Calculate efficiency ratio
                double efficiencyRatio = CalculateEfficiencyRatio(prices, index, period);
                
                if (double.IsNaN(efficiencyRatio))
                    return double.NaN;

                // Step 2: Calculate smoothing constant
                double fastSC = 2.0 / (FastSC + 1.0);
                double slowSC = 2.0 / (SlowSC + 1.0);
                double smoothingConstant = Math.Pow(efficiencyRatio * (fastSC - slowSC) + slowSC, 2);

                // Step 3: Calculate KAMA
                double currentPrice = prices[index];
                double kama = previousKAMA + smoothingConstant * (currentPrice - previousKAMA);

                return kama;
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Calculate efficiency ratio
        /// Measures how much price moved vs how noisy it was
        /// </summary>
        private double CalculateEfficiencyRatio(DataSeries prices, int index, int period)
        {
            if (index < period)
                return double.NaN;

            // Direction = total price change over period
            double direction = Math.Abs(prices[index] - prices[index - period]);

            // Volatility = sum of all price changes in period
            double volatility = 0;
            for (int i = 1; i <= period; i++)
            {
                int currentIndex = index - i + 1;
                int previousIndex = index - i;
                
                if (currentIndex < 0 || previousIndex < 0)
                    return double.NaN;

                volatility += Math.Abs(prices[currentIndex] - prices[previousIndex]);
            }

            // Avoid division by zero
            if (volatility == 0)
                return 0;

            // Efficiency ratio = direction / volatility
            return direction / volatility;
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
}
