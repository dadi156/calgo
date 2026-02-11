using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Ehlers Ultimate Smoother - EXACT FORMULA
    /// Safe implementation - no crashes
    /// </summary>
    public class UltimateSmootherMovingAverage : IMovingAverage
    {
        // Safe storage for each price series
        private readonly Dictionary<DataSeries, UltimateSmootherState> _states;

        public UltimateSmootherMovingAverage()
        {
            _states = new Dictionary<DataSeries, UltimateSmootherState>();
        }

        /// <summary>
        /// State storage for Ultimate Smoother
        /// </summary>
        private class UltimateSmootherState
        {
            public List<double> USValues { get; set; }
            public double C1 { get; set; }
            public double C2 { get; set; }
            public double C3 { get; set; }
            public int Period { get; set; }
            public int LastIndex { get; set; }

            public UltimateSmootherState()
            {
                USValues = new List<double>();
                LastIndex = -1;
            }
        }

        /// <summary>
        /// Main calculation - safe, no recursion
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Basic checks
            if (index < 0 || period < 1 || index >= prices.Count)
                return double.NaN;

            // Simple cases
            if (period == 1) return prices[index];
            if (index < 4) return prices[index]; // Ehlers requirement: CurrentBar >= 4

            // Get or create state
            var state = GetOrCreateState(prices, period);

            // Make sure we have space
            EnsureCapacity(state, index + 1);

            // Calculate missing values
            CalculateSequentially(prices, state, index);

            return state.USValues[index];
        }

        /// <summary>
        /// Get or create state safely
        /// </summary>
        private UltimateSmootherState GetOrCreateState(DataSeries prices, int period)
        {
            if (!_states.ContainsKey(prices))
            {
                _states[prices] = new UltimateSmootherState();
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
        /// Calculate Ultimate Smoother coefficients - EXACT EHLERS FORMULA
        /// </summary>
        private void CalculateCoefficients(UltimateSmootherState state, int period)
        {
            // Same as SuperSmoother for a1, b1, c2, c3
            double a1 = Math.Exp(-1.414 * Math.PI / period);
            
            // Fix angle conversion (degrees to radians)
            double angleInDegrees = 1.414 * 180.0 / period;
            double angleInRadians = angleInDegrees * Math.PI / 180.0;
            double b1 = 2.0 * a1 * Math.Cos(angleInRadians);

            state.C2 = b1;              // c2 = b1
            state.C3 = -a1 * a1;        // c3 = -a1*a1
            
            // ULTIMATE SMOOTHER DIFFERENCE: Different c1 calculation
            state.C1 = (1.0 + state.C2 - state.C3) / 4.0;  // c1 = (1 + c2 - c3) / 4

            // Safety check - prevent bad coefficients
            if (double.IsNaN(state.C1) || double.IsNaN(state.C2) || double.IsNaN(state.C3))
            {
                // Fallback values
                state.C1 = 0.25;
                state.C2 = 0.5;
                state.C3 = 0.25;
            }
        }

        /// <summary>
        /// Make sure list has enough space
        /// </summary>
        private void EnsureCapacity(UltimateSmootherState state, int neededSize)
        {
            while (state.USValues.Count < neededSize)
            {
                state.USValues.Add(double.NaN);
            }
        }

        /// <summary>
        /// Calculate step by step - NO RECURSION
        /// This prevents crashes
        /// </summary>
        private void CalculateSequentially(DataSeries prices, UltimateSmootherState state, int targetIndex)
        {
            // Start from where we left off
            int startIndex = Math.Max(0, state.LastIndex + 1);

            // Calculate each value in order
            for (int i = startIndex; i <= targetIndex; i++)
            {
                double result;

                if (i < 4)
                {
                    // Ehlers requirement: If CurrentBar < 4 Then US = Price
                    result = prices[i];
                }
                else
                {
                    // ULTIMATE SMOOTHER MAIN FORMULA:
                    // US = (1 - c1)*Price + (2*c1 - c2)*Price[1] - (c1 + c3)*Price[2] + c2*US[1] + c3*US[2]
                    
                    double term1 = (1.0 - state.C1) * prices[i];
                    double term2 = (2.0 * state.C1 - state.C2) * prices[i - 1];
                    double term3 = -(state.C1 + state.C3) * prices[i - 2];
                    double term4 = state.C2 * state.USValues[i - 1];
                    double term5 = state.C3 * state.USValues[i - 2];
                    
                    result = term1 + term2 + term3 + term4 + term5;

                    // Safety check
                    if (double.IsNaN(result) || double.IsInfinity(result))
                    {
                        result = prices[i]; // Use current price if calculation fails
                    }
                }

                state.USValues[i] = result;
            }

            state.LastIndex = targetIndex;
        }

        /// <summary>
        /// Reset all data
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
        /// Clean memory if lists get too big
        /// </summary>
        public void CleanupMemory()
        {
            foreach (var state in _states.Values)
            {
                // Keep only last 5000 values
                if (state.USValues.Count > 5000)
                {
                    int keepCount = 3000;
                    int removeCount = state.USValues.Count - keepCount;
                    
                    state.USValues.RemoveRange(0, removeCount);
                    state.LastIndex = Math.Max(-1, state.LastIndex - removeCount);
                }
            }
        }
    }
}
