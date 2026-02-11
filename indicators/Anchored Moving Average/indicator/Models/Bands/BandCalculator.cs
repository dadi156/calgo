using System;

namespace cAlgo
{
    /// <summary>
    /// Calculate Fibonacci bands for Growing MA
    /// Tracks Source values to determine band width
    /// </summary>
    public class BandCalculator
    {
        private BandRangeLevel _bandRange;

        private double _sourceMax;
        private double _sourceMin;
        private double _calculatedBandWidth;
        private bool _hasData;

        /// <summary>
        /// Create band calculator
        /// </summary>
        public BandCalculator(BandRangeLevel bandRange)
        {
            _bandRange = bandRange;
            Reset();
        }

        /// <summary>
        /// Process new source value
        /// </summary>
        public void ProcessSourceValue(double sourceValue)
        {
            if (double.IsNaN(sourceValue) || double.IsInfinity(sourceValue))
                return;

            if (!_hasData)
            {
                _sourceMax = sourceValue;
                _sourceMin = sourceValue;
                _hasData = true;
            }
            else
            {
                _sourceMax = Math.Max(_sourceMax, sourceValue);
                _sourceMin = Math.Min(_sourceMin, sourceValue);
            }

            CalculateBandWidth();
        }

        /// <summary>
        /// Calculate band width using source value range
        /// </summary>
        private void CalculateBandWidth()
        {
            if (!_hasData)
            {
                _calculatedBandWidth = 0;
                return;
            }

            double fullRange = _sourceMax - _sourceMin;

            if (fullRange <= 0.0000001)
            {
                _calculatedBandWidth = 0;
                return;
            }

            switch (_bandRange)
            {
                case BandRangeLevel.Fib114:
                    _calculatedBandWidth = 0.114 * fullRange;
                    break;

                case BandRangeLevel.Fib236:
                    _calculatedBandWidth = 0.236 * fullRange;
                    break;

                case BandRangeLevel.Fib382:
                    _calculatedBandWidth = 0.382 * fullRange;
                    break;

                case BandRangeLevel.Fib500:
                    _calculatedBandWidth = 0.500 * fullRange;
                    break;

                case BandRangeLevel.Fib618:
                    _calculatedBandWidth = 0.618 * fullRange;
                    break;

                case BandRangeLevel.Fib764:
                    _calculatedBandWidth = 0.764 * fullRange;
                    break;

                case BandRangeLevel.Fib886:
                    _calculatedBandWidth = 0.886 * fullRange;
                    break;

                case BandRangeLevel.Fib1000:
                    _calculatedBandWidth = 1.000 * fullRange;
                    break;

                default:
                    _calculatedBandWidth = 0.618 * fullRange;
                    break;
            }
        }

        /// <summary>
        /// Update band range level
        /// </summary>
        public void UpdateBandRange(BandRangeLevel bandRange)
        {
            _bandRange = bandRange;

            if (_hasData)
            {
                CalculateBandWidth();
            }
        }

        /// <summary>
        /// Get band values around MA
        /// </summary>
        public void GetBandValues(double currentMA, out double upperBand, out double lowerBand,
            out double fibo886, out double fibo764, out double fibo628,
            out double fibo382, out double fibo236, out double fibo114)
        {
            if (_calculatedBandWidth <= 0)
            {
                upperBand = currentMA;
                lowerBand = currentMA;
                fibo886 = currentMA;
                fibo764 = currentMA;
                fibo628 = currentMA;
                fibo382 = currentMA;
                fibo236 = currentMA;
                fibo114 = currentMA;
                return;
            }

            upperBand = currentMA + _calculatedBandWidth;
            lowerBand = currentMA - _calculatedBandWidth;

            fibo886 = currentMA + (0.772 * _calculatedBandWidth);
            fibo764 = currentMA + (0.528 * _calculatedBandWidth);
            fibo628 = currentMA + (0.256 * _calculatedBandWidth);
            fibo382 = currentMA - (0.256 * _calculatedBandWidth);
            fibo236 = currentMA - (0.528 * _calculatedBandWidth);
            fibo114 = currentMA - (0.772 * _calculatedBandWidth);
        }

        /// <summary>
        /// Reset calculator state
        /// </summary>
        public void Reset()
        {
            _sourceMax = 0;
            _sourceMin = 0;
            _calculatedBandWidth = 0;
            _hasData = false;
        }

        /// <summary>
        /// Check if calculator has enough data
        /// </summary>
        public bool HasData()
        {
            return _hasData;
        }
    }
}
