using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// T3 Moving Average (Triple Exponential Moving Average)
    /// Very smooth MA with less noise
    /// Good for trend following
    /// Uses triple smoothing with volume factor
    /// </summary>
    public class T3MovingAverage : IMovingAverage
    {
        // Cache for T3 values
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _t3Cache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _e1Cache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _e2Cache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _e3Cache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _e4Cache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _e5Cache;
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _e6Cache;
        private readonly Dictionary<DataSeries, int> _periodCache;

        // Default volume factor (makes T3 smoother)
        private const double DefaultVolumeFactor = 0.7;

        public T3MovingAverage()
        {
            _t3Cache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _e1Cache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _e2Cache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _e3Cache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _e4Cache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _e5Cache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _e6Cache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
        }

        /// <summary>
        /// Calculate T3 value
        /// Uses 6 EMA calculations for triple smoothing
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period * 3 || index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize caches for this data series if needed
            InitializeCaches(prices, period);

            var t3Cache = _t3Cache[prices];

            // Check cache first
            if (t3Cache.ContainsKey(index))
                return t3Cache[index];

            // Calculate T3
            double t3Value = CalculateT3(prices, index, period);

            // Store in cache
            t3Cache[index] = t3Value;

            // Clean cache if needed
            if (t3Cache.Count > 1000)
            {
                CleanCache(t3Cache, index);
            }

            return t3Value;
        }

        /// <summary>
        /// Initialize all caches for data series
        /// </summary>
        private void InitializeCaches(DataSeries prices, int period)
        {
            if (!_t3Cache.ContainsKey(prices))
            {
                _t3Cache[prices] = new Dictionary<int, double>();
                _e1Cache[prices] = new Dictionary<int, double>();
                _e2Cache[prices] = new Dictionary<int, double>();
                _e3Cache[prices] = new Dictionary<int, double>();
                _e4Cache[prices] = new Dictionary<int, double>();
                _e5Cache[prices] = new Dictionary<int, double>();
                _e6Cache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _t3Cache[prices].Clear();
                _e1Cache[prices].Clear();
                _e2Cache[prices].Clear();
                _e3Cache[prices].Clear();
                _e4Cache[prices].Clear();
                _e5Cache[prices].Clear();
                _e6Cache[prices].Clear();
                _periodCache[prices] = period;
            }
        }

        /// <summary>
        /// Calculate T3 using triple smoothing
        /// </summary>
        private double CalculateT3(DataSeries prices, int index, int period)
        {
            try
            {
                // Calculate smoothing factor
                double alpha = 2.0 / (period + 1.0);
                double volumeFactor = DefaultVolumeFactor;

                // Calculate coefficients for T3 formula
                double c1 = -volumeFactor * volumeFactor * volumeFactor;
                double c2 = 3 * volumeFactor * volumeFactor + 3 * volumeFactor * volumeFactor * volumeFactor;
                double c3 = -6 * volumeFactor * volumeFactor - 3 * volumeFactor - 3 * volumeFactor * volumeFactor * volumeFactor;
                double c4 = 1 + 3 * volumeFactor + volumeFactor * volumeFactor * volumeFactor + 3 * volumeFactor * volumeFactor;

                // Calculate 6 EMA values (triple smoothing)
                double e1 = CalculateEMA(prices, index, alpha, _e1Cache[prices]);
                if (double.IsNaN(e1)) return double.NaN;

                double e2 = CalculateEMAFromValue(e1, index, alpha, _e2Cache[prices]);
                if (double.IsNaN(e2)) return double.NaN;

                double e3 = CalculateEMAFromValue(e2, index, alpha, _e3Cache[prices]);
                if (double.IsNaN(e3)) return double.NaN;

                double e4 = CalculateEMAFromValue(e3, index, alpha, _e4Cache[prices]);
                if (double.IsNaN(e4)) return double.NaN;

                double e5 = CalculateEMAFromValue(e4, index, alpha, _e5Cache[prices]);
                if (double.IsNaN(e5)) return double.NaN;

                double e6 = CalculateEMAFromValue(e5, index, alpha, _e6Cache[prices]);
                if (double.IsNaN(e6)) return double.NaN;

                // Calculate final T3 value
                double t3 = c1 * e6 + c2 * e5 + c3 * e4 + c4 * e3;

                return t3;
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Calculate EMA from price data
        /// </summary>
        private double CalculateEMA(DataSeries prices, int index, double alpha, Dictionary<int, double> cache)
        {
            if (cache.ContainsKey(index))
                return cache[index];

            double emaValue;

            if (index == 0)
            {
                emaValue = prices[index];
            }
            else
            {
                double previousEMA = CalculateEMA(prices, index - 1, alpha, cache);
                if (double.IsNaN(previousEMA))
                {
                    emaValue = prices[index];
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
        /// Calculate EMA from single value (not price series)
        /// </summary>
        private double CalculateEMAFromValue(double currentValue, int index, double alpha, Dictionary<int, double> cache)
        {
            if (cache.ContainsKey(index))
                return cache[index];

            double emaValue;

            if (index == 0)
            {
                emaValue = currentValue;
            }
            else
            {
                if (cache.ContainsKey(index - 1))
                {
                    double previousEMA = cache[index - 1];
                    emaValue = alpha * currentValue + (1 - alpha) * previousEMA;
                }
                else
                {
                    emaValue = currentValue;
                }
            }

            cache[index] = emaValue;
            return emaValue;
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
