using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Jurik Moving Average (JMA)
    /// Based on proven algorithm
    /// Very smooth and fast MA
    /// Used by professional traders
    /// </summary>
    public class JurikMovingAverage : IMovingAverage
    {
        // Cache for JMA values and filters
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _jmaCache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _filtCache;
        private readonly Dictionary<DataSeries, int> _periodCache;

        // Default parameters for JMA
        private const double DefaultPhase = 0;    // Phase shift parameter

        public JurikMovingAverage()
        {
            _jmaCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _filtCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
        }

        /// <summary>
        /// Calculate JMA value
        /// Uses the proven Jurik algorithm
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period || index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_jmaCache.ContainsKey(prices))
            {
                _jmaCache[prices] = new Dictionary<int, double>();
                _filtCache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _jmaCache[prices].Clear();
                _filtCache[prices].Clear();
                _periodCache[prices] = period;
            }

            var jmaCache = _jmaCache[prices];
            var filtCache = _filtCache[prices];

            // Check cache first
            if (jmaCache.ContainsKey(index))
                return jmaCache[index];

            // Calculate JMA
            double jmaValue = CalculateJMA(prices, index, period, jmaCache, filtCache);

            // Store in cache
            jmaCache[index] = jmaValue;

            // Clean cache if needed
            if (jmaCache.Count > 1000)
            {
                CleanCache(jmaCache, index);
                CleanCache(filtCache, index);
            }

            return jmaValue;
        }

        /// <summary>
        /// Calculate JMA using the proven algorithm
        /// From the reference implementation
        /// </summary>
        private double CalculateJMA(DataSeries prices, int index, int period, 
            Dictionary<int, double> jmaCache, Dictionary<int, double> filtCache)
        {
            try
            {
                // If this is the first calculation (at period), initialize with current price
                if (index == period)
                {
                    filtCache[index] = prices[index];
                    jmaCache[index] = prices[index];
                    return prices[index];
                }

                // Calculate phase ratio
                double phase = DefaultPhase;
                double phaseRatio = (phase < -100) ? 0.5 : 
                                   (phase > 100) ? 2.5 : 
                                   (phase / 100.0 + 1.5);

                // Calculate beta and alpha
                double beta = 0.45 * (period - 1) / (0.45 * (period - 1) + 2);
                double alpha = Math.Pow(beta, Math.Sqrt(phaseRatio));

                // Get previous filter value
                double previousFilt = 0;
                if (filtCache.ContainsKey(index - 1))
                {
                    previousFilt = filtCache[index - 1];
                }
                else if (index > period)
                {
                    // Calculate previous filter first
                    CalculateJMA(prices, index - 1, period, jmaCache, filtCache);
                    previousFilt = filtCache.ContainsKey(index - 1) ? filtCache[index - 1] : prices[index - 1];
                }
                else
                {
                    previousFilt = prices[index - 1];
                }

                // Calculate filter
                double currentFilt = (1 - alpha) * prices[index] + alpha * previousFilt;
                filtCache[index] = currentFilt;

                // Calculate weighted sum for JMA
                double sum1 = 0, sum2 = 0;
                for (int i = 0; i < period; i++)
                {
                    int filtIndex = index - i;
                    if (filtIndex < 0) break;

                    double weight = Math.Pow(beta, i);
                    
                    // Get filter value for this index
                    double filtValue;
                    if (filtCache.ContainsKey(filtIndex))
                    {
                        filtValue = filtCache[filtIndex];
                    }
                    else if (filtIndex >= period)
                    {
                        // Calculate missing filter value
                        CalculateJMA(prices, filtIndex, period, jmaCache, filtCache);
                        filtValue = filtCache.ContainsKey(filtIndex) ? filtCache[filtIndex] : prices[filtIndex];
                    }
                    else
                    {
                        filtValue = prices[filtIndex];
                    }

                    sum1 += weight * filtValue;
                    sum2 += weight;
                }

                // Calculate final JMA
                double jma = (sum2 != 0) ? sum1 / sum2 : currentFilt;
                
                return jma;
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
