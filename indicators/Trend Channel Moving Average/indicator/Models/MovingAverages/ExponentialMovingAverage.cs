using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// OPTIMIZED Exponential Moving Average (EMA)
    /// - No more recursive calls (much faster)
    /// - Better cache system 
    /// - Faster for repeated calculations
    /// Simple language for easy understanding
    /// </summary>
    public class ExponentialMovingAverage : IMovingAverage
    {
        // OPTIMIZATION: Simple cache - just remember last few values
        private readonly Dictionary<DataSeries, EMAState> _emaStates;

        public ExponentialMovingAverage()
        {
            _emaStates = new Dictionary<DataSeries, EMAState>();
        }

        /// <summary>
        /// OPTIMIZATION: EMA state - stores current info
        /// This helps avoid recursive calls
        /// </summary>
        private class EMAState
        {
            public double LastEMA { get; set; } = double.NaN;
            public int LastIndex { get; set; } = -1;
            public int Period { get; set; } = -1;
            public double Alpha { get; set; } = 0;
            public bool IsInitialized { get; set; } = false;
        }

        /// <summary>
        /// OPTIMIZED: Calculate EMA faster
        /// Old way: Used recursive calls (slow)
        /// New way: Remember last value and build from there (fast)
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < 0 || index >= prices.Count)
                return double.NaN;

            // Get or create state for this price series
            if (!_emaStates.ContainsKey(prices))
            {
                _emaStates[prices] = new EMAState();
            }

            var state = _emaStates[prices];

            // OPTIMIZATION: Check if period changed - reset if needed
            if (state.Period != period)
            {
                ResetState(state, period);
            }

            // OPTIMIZATION: Calculate based on what we need
            if (index < period - 1)
            {
                // Not enough data yet
                return double.NaN;
            }
            else if (index == period - 1)
            {
                // First EMA = simple average
                return CalculateFirstEMA(prices, index, period, state);
            }
            else if (state.LastIndex == index - 1 && state.IsInitialized)
            {
                // OPTIMIZATION: Fast path - we have previous value
                return CalculateNextEMA(prices, index, state);
            }
            else
            {
                // OPTIMIZATION: Need to build up from known point
                return CalculateFromScratch(prices, index, period, state);
            }
        }

        /// <summary>
        /// Reset state for new period
        /// </summary>
        private void ResetState(EMAState state, int period)
        {
            state.Period = period;
            state.Alpha = 2.0 / (period + 1.0);
            state.LastEMA = double.NaN;
            state.LastIndex = -1;
            state.IsInitialized = false;
        }

        /// <summary>
        /// Calculate first EMA value (simple average)
        /// </summary>
        private double CalculateFirstEMA(DataSeries prices, int index, int period, EMAState state)
        {
            double sum = 0;
            for (int i = 0; i < period; i++)
            {
                sum += prices[index - i];
            }
            
            double firstEMA = sum / period;
            
            // Save state
            state.LastEMA = firstEMA;
            state.LastIndex = index;
            state.IsInitialized = true;
            
            return firstEMA;
        }

        /// <summary>
        /// OPTIMIZATION: Calculate next EMA (fast path)
        /// This is the most common case and should be very fast
        /// </summary>
        private double CalculateNextEMA(DataSeries prices, int index, EMAState state)
        {
            double currentPrice = prices[index];
            double newEMA = state.Alpha * currentPrice + (1 - state.Alpha) * state.LastEMA;
            
            // Update state
            state.LastEMA = newEMA;
            state.LastIndex = index;
            
            return newEMA;
        }

        /// <summary>
        /// OPTIMIZATION: Calculate from scratch when needed
        /// Still faster than old recursive method
        /// </summary>
        private double CalculateFromScratch(DataSeries prices, int index, int period, EMAState state)
        {
            // Find where to start
            int startIndex = period - 1;
            
            // Calculate first EMA
            double sum = 0;
            for (int i = 0; i < period; i++)
            {
                sum += prices[startIndex - i];
            }
            double ema = sum / period;
            
            // Build up to target index
            for (int i = startIndex + 1; i <= index; i++)
            {
                double currentPrice = prices[i];
                ema = state.Alpha * currentPrice + (1 - state.Alpha) * ema;
            }
            
            // Save state
            state.LastEMA = ema;
            state.LastIndex = index;
            state.IsInitialized = true;
            
            return ema;
        }
    }
}
