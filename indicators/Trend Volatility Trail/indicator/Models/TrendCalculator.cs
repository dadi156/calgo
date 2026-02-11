// TrendCalculator - Calculates trend boost
using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    // Calculates trend boost based on direction steps
    public class TrendCalculator
    {
        private double[] _dirStep;
        private int _arraySize;

        public TrendCalculator(int arraySize)
        {
            _arraySize = arraySize;
            _dirStep = new double[arraySize];
        }

        // Calculate direction step at current index
        // Returns 1.0 if upward slope, -1.0 if downward slope
        public double CalculateDirectionStep(int index, double currentBasis, double previousBasis)
        {
            try
            {
                // Resize array if needed
                if (index >= _arraySize)
                {
                    int newSize = Math.Max(index + 1000, _arraySize * 2);
                    Array.Resize(ref _dirStep, newSize);
                    _arraySize = newSize;
                }

                // For first bar, return 0
                if (index == 0)
                {
                    _dirStep[index] = 0.0;
                    return 0.0;
                }

                // Calculate slope
                double slope = currentBasis - previousBasis;

                // Direction: +1 for up, -1 for down
                _dirStep[index] = slope >= 0.0 ? 1.0 : -1.0;

                return _dirStep[index];
            }
            catch (Exception)
            {
                // If error, return 0
                _dirStep[index] = 0.0;
                return 0.0;
            }
        }

        // Get direction step at index (for MA calculation)
        public double GetDirectionStep(int index)
        {
            if (index >= 0 && index < _dirStep.Length)
            {
                return _dirStep[index];
            }
            return 0.0;
        }

        // Calculate trend boost
        // trendBoost = 1.0 + trendImpact * |trendMemory|
        public double CalculateTrendBoost(double trendMemory, double trendImpact)
        {
            try
            {
                // Validate trend memory
                if (!ValidationHelper.IsValidValue(trendMemory))
                {
                    return 1.0;
                }

                // Calculate boost
                double trendBoost = 1.0 + trendImpact * Math.Abs(trendMemory);

                // Validate result
                if (!ValidationHelper.IsValidValue(trendBoost))
                {
                    return 1.0;
                }

                return trendBoost;
            }
            catch (Exception)
            {
                // If error, return neutral value
                return 1.0;
            }
        }
    }
}
