using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    // Enum to choose smoothing type
    public enum SmoothingType
    {
        SMA,    // Simple Moving Average smoothing
        EMA     // Exponential Moving Average smoothing
    }

    public class SmoothingManager
    {
        // Arrays to store OHLC + Median values
        private double[] _highValues;
        private double[] _lowValues;
        private double[] _closeValues;
        private double[] _openValues;
        private double[] _medianValues;  // NEW

        // Arrays for smoothed values
        private double[] _smoothedHigh;
        private double[] _smoothedLow;
        private double[] _smoothedClose;
        private double[] _smoothedOpen;
        private double[] _smoothedMedian;  // NEW

        private readonly int _smoothPeriod;
        private readonly SmoothingType _smoothingType;
        private readonly double _emaAlpha; // Only used for EMA
        private int _arraySize;

        public SmoothingManager(int smoothPeriod, int arraySize, SmoothingType smoothingType)
        {
            _smoothPeriod = smoothPeriod;
            _arraySize = arraySize;
            _smoothingType = smoothingType;
            
            // Calculate EMA alpha if using EMA smoothing
            if (_smoothingType == SmoothingType.EMA)
            {
                _emaAlpha = 2.0 / (_smoothPeriod + 1);
            }

            // Create arrays for OHLC + Median
            _highValues = new double[arraySize];
            _lowValues = new double[arraySize];
            _closeValues = new double[arraySize];
            _openValues = new double[arraySize];
            _medianValues = new double[arraySize];  // NEW
            
            // Create arrays for smoothed values
            _smoothedHigh = new double[arraySize];
            _smoothedLow = new double[arraySize];
            _smoothedClose = new double[arraySize];
            _smoothedOpen = new double[arraySize];
            _smoothedMedian = new double[arraySize];  // NEW

            // Initialize arrays with NaN
            InitializeArrays();
        }

        // Smooth OHLC + Median values and calculate Fibonacci
        public MAResult SmoothMAResult(int index, MAResult originalResult)
        {
            // Store original OHLC + Median values first
            StoreValues(index, originalResult);

            // Calculate smoothed OHLC + Median values (SMA or EMA based on type)
            double smoothedHigh = CalculateSmoothedValue(index, _highValues, _smoothedHigh);
            double smoothedLow = CalculateSmoothedValue(index, _lowValues, _smoothedLow);
            double smoothedClose = CalculateSmoothedValue(index, _closeValues, _smoothedClose);
            double smoothedOpen = CalculateSmoothedValue(index, _openValues, _smoothedOpen);
            double smoothedMedian = CalculateSmoothedValue(index, _medianValues, _smoothedMedian);  // NEW

            // Calculate 2 Fibonacci levels using helper
            var (fib618, fib382) = CalculationHelper.CalculateFibonacciLevels(smoothedHigh, smoothedLow);

            // Return new MAResult with smoothed values
            return new MAResult(smoothedHigh, smoothedLow, smoothedClose, smoothedOpen, 
                               smoothedMedian, fib618, fib382, originalResult.IsNewMTFBar);
        }

        // Store OHLC + Median values
        private void StoreValues(int index, MAResult result)
        {
            // Make sure arrays are big enough
            if (index >= _arraySize)
            {
                ResizeArrays(Math.Max(index + 1000, _arraySize * 2));
            }

            // Store OHLC + Median values
            _highValues[index] = result.HighMA;
            _lowValues[index] = result.LowMA;
            _closeValues[index] = result.CloseMA;
            _openValues[index] = result.OpenMA;
            _medianValues[index] = result.MedianMA;  // NEW
        }

        // Calculate smoothed value - uses SMA or EMA based on type
        private double CalculateSmoothedValue(int index, double[] values, double[] smoothed)
        {
            try
            {
                // Choose smoothing method
                if (_smoothingType == SmoothingType.SMA)
                {
                    // SMA smoothing
                    return CalculateSMASmoothing(index, values);
                }
                else // SmoothingType.EMA
                {
                    // EMA smoothing
                    return CalculateEMASmoothing(index, values, smoothed);
                }
            }
            catch (Exception)
            {
                // If error, return original value
                return values[index];
            }
        }

        // SMA smoothing - average of last X values
        private double CalculateSMASmoothing(int index, double[] values)
        {
            // Need minimum bars for smoothing
            if (index < _smoothPeriod - 1)
            {
                // For first few bars, return original value
                return values[index];
            }

            // Calculate Simple Moving Average of MA values
            double sum = 0;
            int validCount = 0;

            for (int i = 0; i < _smoothPeriod; i++)
            {
                int lookbackIndex = index - i;
                if (lookbackIndex >= 0 && lookbackIndex < values.Length)
                {
                    double value = values[lookbackIndex];
                    if (ValidationHelper.IsValidValue(value))
                    {
                        sum += value;
                        validCount++;
                    }
                }
            }

            // Calculate average
            if (validCount > 0)
            {
                return sum / validCount;
            }
            else
            {
                // If no valid values, return original
                return values[index];
            }
        }

        // EMA smoothing - weighted average
        private double CalculateEMASmoothing(int index, double[] values, double[] smoothed)
        {
            // For first bar, use original value
            if (index == 0)
            {
                smoothed[index] = values[index];
                return smoothed[index];
            }

            // EMA formula: smoothed = alpha * value + (1 - alpha) * previous_smoothed
            smoothed[index] = _emaAlpha * values[index] + (1 - _emaAlpha) * smoothed[index - 1];

            // Fix NaN values
            if (!ValidationHelper.IsValidValue(smoothed[index]))
            {
                smoothed[index] = values[index];
            }

            return smoothed[index];
        }

        // Initialize arrays with NaN
        private void InitializeArrays()
        {
            for (int i = 0; i < _arraySize; i++)
            {
                _highValues[i] = double.NaN;
                _lowValues[i] = double.NaN;
                _closeValues[i] = double.NaN;
                _openValues[i] = double.NaN;
                _medianValues[i] = double.NaN;  // NEW
                
                _smoothedHigh[i] = double.NaN;
                _smoothedLow[i] = double.NaN;
                _smoothedClose[i] = double.NaN;
                _smoothedOpen[i] = double.NaN;
                _smoothedMedian[i] = double.NaN;  // NEW
            }
        }

        // Resize arrays when needed
        private void ResizeArrays(int newSize)
        {
            Array.Resize(ref _highValues, newSize);
            Array.Resize(ref _lowValues, newSize);
            Array.Resize(ref _closeValues, newSize);
            Array.Resize(ref _openValues, newSize);
            Array.Resize(ref _medianValues, newSize);  // NEW
            
            Array.Resize(ref _smoothedHigh, newSize);
            Array.Resize(ref _smoothedLow, newSize);
            Array.Resize(ref _smoothedClose, newSize);
            Array.Resize(ref _smoothedOpen, newSize);
            Array.Resize(ref _smoothedMedian, newSize);  // NEW

            // Initialize new elements with NaN
            for (int i = _arraySize; i < newSize; i++)
            {
                _highValues[i] = double.NaN;
                _lowValues[i] = double.NaN;
                _closeValues[i] = double.NaN;
                _openValues[i] = double.NaN;
                _medianValues[i] = double.NaN;  // NEW
                
                _smoothedHigh[i] = double.NaN;
                _smoothedLow[i] = double.NaN;
                _smoothedClose[i] = double.NaN;
                _smoothedOpen[i] = double.NaN;
                _smoothedMedian[i] = double.NaN;  // NEW
            }

            _arraySize = newSize;
        }
    }
}
