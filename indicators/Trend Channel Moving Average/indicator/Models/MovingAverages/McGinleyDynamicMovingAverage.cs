using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// McGinley Dynamic Moving Average
    /// Auto-adjusts to price speed
    /// No parameters to set
    /// Very smooth lines
    /// </summary>
    public class McGinleyDynamicMovingAverage : IMovingAverage
    {
        // Cache for McGinley values
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _mcginleyCache;
        private readonly Dictionary<DataSeries, int> _periodCache;
        private readonly Dictionary<DataSeries, bool> _initializedCache;

        public McGinleyDynamicMovingAverage()
        {
            _mcginleyCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
            _initializedCache = new Dictionary<DataSeries, bool>();
        }

        /// <summary>
        /// Calculate McGinley Dynamic value
        /// Uses exact formula from reference code
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period || index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_mcginleyCache.ContainsKey(prices))
            {
                _mcginleyCache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
                _initializedCache[prices] = false;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _mcginleyCache[prices].Clear();
                _periodCache[prices] = period;
                _initializedCache[prices] = false;
            }

            var mcginleyCache = _mcginleyCache[prices];

            // Check cache first
            if (mcginleyCache.ContainsKey(index))
                return mcginleyCache[index];

            // Calculate McGinley Dynamic
            double mcginleyValue = CalculateMcGinley(prices, index, period, mcginleyCache);

            // Store in cache
            mcginleyCache[index] = mcginleyValue;

            // Clean cache if needed
            if (mcginleyCache.Count > 1000)
            {
                CleanCache(mcginleyCache, index);
            }

            return mcginleyValue;
        }

        /// <summary>
        /// Calculate McGinley Dynamic using reference algorithm
        /// </summary>
        private double CalculateMcGinley(DataSeries prices, int index, int period, Dictionary<int, double> mcginleyCache)
        {
            try
            {
                // First calculation - use simple average like reference code
                if (index == period || !_initializedCache[prices])
                {
                    double sum = 0;
                    for (int i = 0; i < period; i++)
                    {
                        sum += prices[index - i];
                    }
                    double firstMcGinley = sum / period;
                    _initializedCache[prices] = true;
                    return firstMcGinley;
                }

                // Get previous McGinley value
                double previousMcGinley;
                if (mcginleyCache.ContainsKey(index - 1))
                {
                    previousMcGinley = mcginleyCache[index - 1];
                }
                else
                {
                    // Calculate previous value first
                    previousMcGinley = CalculateMcGinley(prices, index - 1, period, mcginleyCache);
                }

                // Current price
                double currentPrice = prices[index];

                // McGinley Dynamic formula from reference:
                // MD = MD_previous + (Price - MD_previous) / (N * (Price / MD_previous)^4)
                double ratio = currentPrice / previousMcGinley;
                double dynamicFactor = period * Math.Pow(ratio, 4);
                
                // Avoid division by zero
                if (dynamicFactor == 0)
                    return previousMcGinley;

                double mcGinley = previousMcGinley + ((currentPrice - previousMcGinley) / dynamicFactor);

                return mcGinley;
            }
            catch
            {
                return double.NaN;
            }
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
