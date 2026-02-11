using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Laguerre Filter Moving Average
    /// Very smooth MA with good response
    /// Uses 4 filter components
    /// Great for trend following
    /// </summary>
    public class LaguerreFilterMovingAverage : IMovingAverage
    {
        // Cache for Laguerre values and filter components
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _laguerreCache;
        private readonly Dictionary<DataSeries, Dictionary<int, LaguerreState>> _stateCache;
        private readonly Dictionary<DataSeries, int> _periodCache;
        private readonly Dictionary<DataSeries, double> _gammaCache;

        public LaguerreFilterMovingAverage()
        {
            _laguerreCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _stateCache = new Dictionary<DataSeries, Dictionary<int, LaguerreState>>();
            _periodCache = new Dictionary<DataSeries, int>();
            _gammaCache = new Dictionary<DataSeries, double>();
        }

        /// <summary>
        /// Calculate Laguerre Filter MA value
        /// Uses 4 filter components like reference code
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_laguerreCache.ContainsKey(prices))
            {
                _laguerreCache[prices] = new Dictionary<int, double>();
                _stateCache[prices] = new Dictionary<int, LaguerreState>();
                _periodCache[prices] = period;
                
                // Calculate gamma from period (like reference code)
                double gamma = Math.Max(0.1, Math.Min(0.9, 1.0 - (3.0 / period)));
                _gammaCache[prices] = gamma;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _laguerreCache[prices].Clear();
                _stateCache[prices].Clear();
                _periodCache[prices] = period;
                
                // Recalculate gamma
                double gamma = Math.Max(0.1, Math.Min(0.9, 1.0 - (3.0 / period)));
                _gammaCache[prices] = gamma;
            }

            var laguerreCache = _laguerreCache[prices];
            var stateCache = _stateCache[prices];
            double currentGamma = _gammaCache[prices];

            // Check cache first
            if (laguerreCache.ContainsKey(index))
                return laguerreCache[index];

            // Calculate Laguerre Filter
            double laguerreValue = CalculateLaguerre(prices, index, currentGamma, stateCache);

            // Store in cache
            laguerreCache[index] = laguerreValue;

            // Clean cache if needed
            if (laguerreCache.Count > 1000)
            {
                CleanCache(laguerreCache, index);
                CleanStateCache(stateCache, index);
            }

            return laguerreValue;
        }

        /// <summary>
        /// Calculate Laguerre Filter using reference algorithm
        /// </summary>
        private double CalculateLaguerre(DataSeries prices, int index, double gamma, Dictionary<int, LaguerreState> stateCache)
        {
            try
            {
                LaguerreState currentState;

                if (index == 0)
                {
                    // First bar - initialize all components with current price
                    double price = prices[index];
                    currentState = new LaguerreState
                    {
                        L0 = price,
                        L1 = price,
                        L2 = price,
                        L3 = price
                    };
                }
                else
                {
                    // Get previous state
                    if (!stateCache.ContainsKey(index - 1))
                    {
                        // Calculate previous state first
                        CalculateLaguerre(prices, index - 1, gamma, stateCache);
                    }

                    var previousState = stateCache[index - 1];
                    currentState = CalculateNextLaguerreState(prices, index, gamma, previousState);
                }

                // Store state
                stateCache[index] = currentState;

                // Calculate final Laguerre value (average of all 4 components)
                double laguerre = (currentState.L0 + currentState.L1 + currentState.L2 + currentState.L3) / 4.0;

                return laguerre;
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Calculate next Laguerre state from previous state
        /// Uses exact formulas from reference code
        /// </summary>
        private LaguerreState CalculateNextLaguerreState(DataSeries prices, int index, double gamma, LaguerreState previousState)
        {
            var newState = new LaguerreState();

            double currentPrice = prices[index];

            // Calculate filter components using reference formulas
            // L0 = (1 - gamma) * price + gamma * L0[previous]
            newState.L0 = (1 - gamma) * currentPrice + gamma * previousState.L0;

            // L1 = -gamma * L0 + L0[previous] + gamma * L1[previous]  
            newState.L1 = -gamma * newState.L0 + previousState.L0 + gamma * previousState.L1;

            // L2 = -gamma * L1 + L1[previous] + gamma * L2[previous]
            newState.L2 = -gamma * newState.L1 + previousState.L1 + gamma * previousState.L2;

            // L3 = -gamma * L2 + L2[previous] + gamma * L3[previous]
            newState.L3 = -gamma * newState.L2 + previousState.L2 + gamma * previousState.L3;

            return newState;
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

        /// <summary>
        /// Clean old state cache values
        /// </summary>
        private void CleanStateCache(Dictionary<int, LaguerreState> cache, int currentIndex)
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

        /// <summary>
        /// State for Laguerre Filter calculation
        /// Stores 4 filter components like reference code
        /// </summary>
        private class LaguerreState
        {
            public double L0 { get; set; }
            public double L1 { get; set; }
            public double L2 { get; set; }
            public double L3 { get; set; }
        }
    }
}
