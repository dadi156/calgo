using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Double Smoothed Exponential Moving Average (DSEMA)
    /// Smoother than regular EMA
    /// Good for noisy markets
    /// Applies EMA two times
    /// </summary>
    public class DoubleSmoothedExponentialMovingAverage : IMovingAverage
    {
        // Cache for DSEMA values
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _dsemaCache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _firstEmaCache;
        private readonly Dictionary<DataSeries, int> _periodCache;

        public DoubleSmoothedExponentialMovingAverage()
        {
            _dsemaCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _firstEmaCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
        }

        /// <summary>
        /// Calculate DSEMA value
        /// First EMA, then EMA of that result
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period * 2 - 1 || index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_dsemaCache.ContainsKey(prices))
            {
                _dsemaCache[prices] = new Dictionary<int, double>();
                _firstEmaCache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _dsemaCache[prices].Clear();
                _firstEmaCache[prices].Clear();
                _periodCache[prices] = period;
            }

            var dsemaCache = _dsemaCache[prices];
            var firstEmaCache = _firstEmaCache[prices];

            // Check cache first
            if (dsemaCache.ContainsKey(index))
                return dsemaCache[index];

            // Calculate DSEMA
            double dsemaValue = CalculateDSEMA(prices, index, period, firstEmaCache);

            // Store in cache
            dsemaCache[index] = dsemaValue;

            // Clean cache if needed
            if (dsemaCache.Count > 1000)
            {
                CleanCache(dsemaCache, index);
            }

            return dsemaValue;
        }

        /// <summary>
        /// Calculate DSEMA step by step
        /// </summary>
        private double CalculateDSEMA(DataSeries prices, int index, int period, Dictionary<int, double> firstEmaCache)
        {
            try
            {
                // Calculate smoothing factor
                double alpha = 2.0 / (period + 1.0);

                // Step 1: Calculate first EMA
                double firstEma = CalculateEMA(prices, index, period, alpha, firstEmaCache);
                if (double.IsNaN(firstEma))
                    return double.NaN;

                // Step 2: Calculate second EMA (EMA of first EMA)
                // We need to create a virtual data series from first EMA values
                double secondEma = CalculateSecondEMA(index, period, alpha, firstEmaCache);

                return secondEma;
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Calculate first EMA from prices
        /// </summary>
        private double CalculateEMA(DataSeries prices, int index, int period, double alpha, Dictionary<int, double> cache)
        {
            if (cache.ContainsKey(index))
                return cache[index];

            double emaValue;

            if (index < period - 1)
            {
                emaValue = double.NaN;
            }
            else if (index == period - 1)
            {
                // First EMA = SMA
                double sum = 0;
                for (int i = 0; i < period; i++)
                {
                    sum += prices[index - i];
                }
                emaValue = sum / period;
            }
            else
            {
                // EMA formula
                double previousEMA = CalculateEMA(prices, index - 1, period, alpha, cache);
                if (double.IsNaN(previousEMA))
                {
                    emaValue = double.NaN;
                }
                else
                {
                    emaValue = alpha * prices[index] + (1 - alpha) * previousEMA;
                }
            }

            cache[index] = emaValue;
            return emaValue;
        }

        /// <summary>
        /// Calculate second EMA from first EMA values
        /// </summary>
        private double CalculateSecondEMA(int index, int period, double alpha, Dictionary<int, double> firstEmaCache)
        {
            // Check if we have enough first EMA values
            if (index < period * 2 - 1)
                return double.NaN;

            // Get first EMA value
            if (!firstEmaCache.ContainsKey(index))
                return double.NaN;

            double currentFirstEma = firstEmaCache[index];

            if (index == period * 2 - 1)
            {
                // First second EMA = SMA of first EMA values
                double sum = 0;
                int count = 0;
                
                for (int i = 0; i < period; i++)
                {
                    int emaIndex = index - i;
                    if (firstEmaCache.ContainsKey(emaIndex))
                    {
                        sum += firstEmaCache[emaIndex];
                        count++;
                    }
                }
                
                if (count == period)
                    return sum / period;
                else
                    return double.NaN;
            }
            else
            {
                // Get previous second EMA
                double previousSecondEma = CalculateSecondEMA(index - 1, period, alpha, firstEmaCache);
                if (double.IsNaN(previousSecondEma))
                    return double.NaN;

                // Calculate second EMA
                return alpha * currentFirstEma + (1 - alpha) * previousSecondEma;
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
