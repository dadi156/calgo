using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// SAFE SuperSmoother - No recursion, no crashes
    /// Fixed all system error causes
    /// </summary>
    public class SuperSmootherMovingAverage : IMovingAverage
    {
        // Safe storage - grows as needed
        private readonly Dictionary<DataSeries, FilterState> _states;

        public SuperSmootherMovingAverage()
        {
            _states = new Dictionary<DataSeries, FilterState>();
        }

        /// <summary>
        /// Safe state storage
        /// </summary>
        private class FilterState
        {
            public List<double> FilterValues { get; set; }
            public double C1 { get; set; }
            public double C2 { get; set; }
            public double C3 { get; set; }
            public int Period { get; set; }
            public int LastIndex { get; set; }

            public FilterState()
            {
                FilterValues = new List<double>();
                LastIndex = -1;
            }
        }

        /// <summary>
        /// Main calculation - SAFE, no recursion
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Basic checks
            if (index < 0 || period < 1 || index >= prices.Count)
                return double.NaN;

            // Handle simple cases
            if (period == 1) return prices[index];
            if (index == 0) return prices[0];
            if (index == 1) return (prices[0] + prices[1]) / 2.0;

            // Get or create state
            var state = GetOrCreateState(prices, period);

            // Make sure we have enough space
            EnsureCapacity(state, index + 1);

            // Calculate missing values - NO RECURSION!
            CalculateSequentially(prices, state, index);

            return state.FilterValues[index];
        }

        /// <summary>
        /// Get or create state safely
        /// </summary>
        private FilterState GetOrCreateState(DataSeries prices, int period)
        {
            if (!_states.ContainsKey(prices))
            {
                _states[prices] = new FilterState();
            }

            var state = _states[prices];

            // Update coefficients if period changed
            if (state.Period != period)
            {
                CalculateCoefficients(state, period);
                state.Period = period;
                state.LastIndex = -1; // Recalculate everything
            }

            return state;
        }

        /// <summary>
        /// Calculate coefficients - CORRECTED formula
        /// </summary>
        private void CalculateCoefficients(FilterState state, int period)
        {
            // Ehlers formula with correct angle conversion
            double a1 = Math.Exp(-1.414 * Math.PI / period);
            
            // Fix: Convert degrees to radians properly
            double angleInDegrees = 1.414 * 180.0 / period;
            double angleInRadians = angleInDegrees * Math.PI / 180.0;
            double b1 = 2.0 * a1 * Math.Cos(angleInRadians);

            state.C2 = b1;
            state.C3 = -a1 * a1;
            state.C1 = 1.0 - state.C2 - state.C3;

            // Safety check - prevent bad coefficients
            if (double.IsNaN(state.C1) || double.IsNaN(state.C2) || double.IsNaN(state.C3))
            {
                // Fallback to simple smoothing
                state.C1 = 0.5;
                state.C2 = 0.3;
                state.C3 = 0.2;
            }
        }

        /// <summary>
        /// Ensure list has enough space
        /// </summary>
        private void EnsureCapacity(FilterState state, int neededSize)
        {
            while (state.FilterValues.Count < neededSize)
            {
                state.FilterValues.Add(double.NaN);
            }
        }

        /// <summary>
        /// Calculate sequentially - NO RECURSION!
        /// This is the key fix that prevents crashes
        /// </summary>
        private void CalculateSequentially(DataSeries prices, FilterState state, int targetIndex)
        {
            // Start from where we left off
            int startIndex = Math.Max(0, state.LastIndex + 1);

            // Calculate each value in order
            for (int i = startIndex; i <= targetIndex; i++)
            {
                double result;

                if (i == 0)
                {
                    result = prices[0];
                }
                else if (i == 1)
                {
                    result = (prices[0] + prices[1]) / 2.0;
                }
                else
                {
                    // SuperSmoother formula
                    double inputPart = state.C1 * (prices[i] + prices[i - 1]) / 2.0;
                    double filter1 = state.FilterValues[i - 1];
                    double filter2 = state.FilterValues[i - 2];
                    
                    result = inputPart + state.C2 * filter1 + state.C3 * filter2;

                    // Safety check
                    if (double.IsNaN(result) || double.IsInfinity(result))
                    {
                        result = prices[i]; // Use current price if calculation fails
                    }
                }

                state.FilterValues[i] = result;
            }

            state.LastIndex = targetIndex;
        }

        /// <summary>
        /// Reset everything
        /// </summary>
        public void Reset()
        {
            _states.Clear();
        }

        /// <summary>
        /// Clear specific price series
        /// </summary>
        public void ClearPriceSeries(DataSeries prices)
        {
            if (_states.ContainsKey(prices))
                _states.Remove(prices);
        }

        /// <summary>
        /// Clean up memory if lists get too big
        /// </summary>
        public void CleanupMemory()
        {
            foreach (var state in _states.Values)
            {
                // Keep only last 5000 values
                if (state.FilterValues.Count > 5000)
                {
                    int keepCount = 3000;
                    int removeCount = state.FilterValues.Count - keepCount;
                    
                    state.FilterValues.RemoveRange(0, removeCount);
                    state.LastIndex = Math.Max(-1, state.LastIndex - removeCount);
                }
            }
        }
    }
}
