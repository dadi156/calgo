using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Zero Lag Exponential Moving Average (ZLEMA)
    /// Faster response than regular EMA
    /// Removes lag by adding momentum
    /// Good for quick signals
    /// </summary>
    public class ZeroLagExponentialMovingAverage : IMovingAverage
    {
        // Cache for ZLEMA values
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _zlemaCache;
        private readonly Dictionary<DataSeries, int> _periodCache;

        public ZeroLagExponentialMovingAverage()
        {
            _zlemaCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
        }

        /// <summary>
        /// Calculate ZLEMA value
        /// Uses price + momentum to remove lag
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period || index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_zlemaCache.ContainsKey(prices))
            {
                _zlemaCache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _zlemaCache[prices].Clear();
                _periodCache[prices] = period;
            }

            var zlemaCache = _zlemaCache[prices];

            // Check cache first
            if (zlemaCache.ContainsKey(index))
                return zlemaCache[index];

            // Calculate ZLEMA
            double zlemaValue = CalculateZLEMA(prices, index, period);

            // Store in cache
            zlemaCache[index] = zlemaValue;

            // Clean cache if needed
            if (zlemaCache.Count > 1000)
            {
                CleanCache(zlemaCache, index);
            }

            return zlemaValue;
        }

        /// <summary>
        /// Calculate ZLEMA step by step
        /// </summary>
        private double CalculateZLEMA(DataSeries prices, int index, int period)
        {
            try
            {
                // Step 1: Calculate lag amount
                int lag = (period - 1) / 2;

                // Step 2: Calculate momentum adjusted price
                double adjustedPrice = CalculateAdjustedPrice(prices, index, lag);
                if (double.IsNaN(adjustedPrice))
                    return double.NaN;

                // Step 3: Calculate smoothing factor
                double alpha = 2.0 / (period + 1.0);

                // Step 4: Calculate ZLEMA
                if (index == period)
                {
                    // First ZLEMA = simple average of adjusted prices
                    double sum = 0;
                    int count = 0;
                    
                    for (int i = 0; i <= lag && index - i >= 0; i++)
                    {
                        double tempAdjusted = CalculateAdjustedPrice(prices, index - i, lag);
                        if (!double.IsNaN(tempAdjusted))
                        {
                            sum += tempAdjusted;
                            count++;
                        }
                    }
                    
                    if (count > 0)
                        return sum / count;
                    else
                        return adjustedPrice;
                }
                else
                {
                    // ZLEMA formula: alpha * adjusted_price + (1 - alpha) * previous_ZLEMA
                    double previousZLEMA = CalculateZLEMA(prices, index - 1, period);
                    if (double.IsNaN(previousZLEMA))
                        return adjustedPrice;

                    return alpha * adjustedPrice + (1 - alpha) * previousZLEMA;
                }
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Calculate price with momentum adjustment
        /// This removes the lag from price data
        /// </summary>
        private double CalculateAdjustedPrice(DataSeries prices, int index, int lag)
        {
            // Check bounds
            if (index < lag || index >= prices.Count)
                return prices[index];

            try
            {
                // Current price
                double currentPrice = prices[index];
                
                // Price from lag periods ago
                double lagPrice = prices[index - lag];

                // Momentum = current price - old price
                double momentum = currentPrice - lagPrice;

                // Adjusted price = current price + momentum
                // This moves the price forward to remove lag
                double adjustedPrice = currentPrice + momentum;

                return adjustedPrice;
            }
            catch
            {
                return prices[index];
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
