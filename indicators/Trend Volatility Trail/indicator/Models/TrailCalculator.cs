// TrailCalculator - Calculates trail values
using System;

namespace cAlgo.Indicators
{
    // Calculates trail long and trail short values
    public class TrailCalculator
    {
        private double[] _trailLong;
        private double[] _trailShort;
        private int _arraySize;

        public TrailCalculator(int arraySize)
        {
            _arraySize = arraySize;
            _trailLong = new double[arraySize];
            _trailShort = new double[arraySize];
        }

        // Calculate trails based on bands and regime
        public (double trailLong, double trailShort) CalculateTrails(
            int index,
            double basis,
            double atr,
            double multiplier,
            int regime)
        {
            try
            {
                // Resize arrays if needed
                if (index >= _arraySize)
                {
                    int newSize = Math.Max(index + 1000, _arraySize * 2);
                    Array.Resize(ref _trailLong, newSize);
                    Array.Resize(ref _trailShort, newSize);
                    _arraySize = newSize;
                }

                // Calculate bands internally (not exposed)
                double bandTop = basis + multiplier * atr;
                double bandBot = basis - multiplier * atr;

                // Initialize trails on first calculation
                if (index == 0 || regime == 0 && index > 0 && (_trailLong[index - 1] == 0.0 || double.IsNaN(_trailLong[index - 1])))
                {
                    _trailLong[index] = bandBot;
                    _trailShort[index] = bandTop;
                    return (_trailLong[index], _trailShort[index]);
                }

                // Get previous trails
                double prevTrailLong = index > 0 ? _trailLong[index - 1] : bandBot;
                double prevTrailShort = index > 0 ? _trailShort[index - 1] : bandTop;

                // Update trails based on regime
                if (regime == 1) // Bull mode
                {
                    _trailLong[index] = Math.Max(bandBot, prevTrailLong);
                    _trailShort[index] = bandTop;
                }
                else if (regime == -1) // Bear mode
                {
                    _trailShort[index] = Math.Min(bandTop, prevTrailShort);
                    _trailLong[index] = bandBot;
                }
                else // Neutral mode
                {
                    _trailLong[index] = bandBot;
                    _trailShort[index] = bandTop;
                }

                // Validate results
                if (!ValidationHelper.IsValidValue(_trailLong[index]))
                {
                    _trailLong[index] = bandBot;
                }

                if (!ValidationHelper.IsValidValue(_trailShort[index]))
                {
                    _trailShort[index] = bandTop;
                }

                return (_trailLong[index], _trailShort[index]);
            }
            catch (Exception)
            {
                // If error, return band values
                double bandTop = basis + multiplier * atr;
                double bandBot = basis - multiplier * atr;
                _trailLong[index] = bandBot;
                _trailShort[index] = bandTop;
                return (_trailLong[index], _trailShort[index]);
            }
        }

        // Get trail values at index
        public (double trailLong, double trailShort) GetTrails(int index)
        {
            if (index >= 0 && index < _trailLong.Length)
            {
                return (_trailLong[index], _trailShort[index]);
            }
            return (0.0, 0.0);
        }
    }
}
