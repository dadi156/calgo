using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class EMASmoothingManager
    {
        // Arrays to store OHLC + Median values
        private double[] _highValues;
        private double[] _lowValues;
        private double[] _closeValues;
        private double[] _openValues;
        private double[] _medianValues;  // NEW: Median values

        // EMA smoothed values
        private double[] _smoothedHigh;
        private double[] _smoothedLow;
        private double[] _smoothedClose;
        private double[] _smoothedOpen;
        private double[] _smoothedMedian;  // NEW: Smoothed median

        private readonly int _smoothPeriod;
        private readonly double _alpha; // EMA smoothing factor
        private int _arraySize;

        public EMASmoothingManager(int smoothPeriod, int arraySize)
        {
            _smoothPeriod = smoothPeriod;
            _arraySize = arraySize;
            
            // Calculate EMA alpha: 2 / (period + 1)
            _alpha = 2.0 / (_smoothPeriod + 1);

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

        // Smooth OHLC + Median values using EMA and calculate Fibonacci
        public MAResult SmoothMAResult(int index, MAResult originalResult)
        {
            // Store original OHLC + Median values first
            StoreOHLCValues(index, originalResult);

            // Calculate EMA smoothed OHLC + Median values
            double smoothedHigh = CalculateEMASmoothedValue(index, _highValues, _smoothedHigh);
            double smoothedLow = CalculateEMASmoothedValue(index, _lowValues, _smoothedLow);
            double smoothedClose = CalculateEMASmoothedValue(index, _closeValues, _smoothedClose);
            double smoothedOpen = CalculateEMASmoothedValue(index, _openValues, _smoothedOpen);
            double smoothedMedian = CalculateEMASmoothedValue(index, _medianValues, _smoothedMedian);  // NEW

            // Calculate 2 Fibonacci levels using helper
            var (fib618, fib382) = CalculationHelper.CalculateFibonacciLevels(smoothedHigh, smoothedLow);

            // Return new MAResult with EMA smoothed values (now includes median)
            return new MAResult(smoothedHigh, smoothedLow, smoothedClose, smoothedOpen, 
                               smoothedMedian, fib618, fib382, originalResult.IsNewMTFBar);
        }

        // Store OHLC + Median values
        private void StoreOHLCValues(int index, MAResult result)
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

        // Calculate EMA smoothed value
        private double CalculateEMASmoothedValue(int index, double[] values, double[] smoothed)
        {
            try
            {
                // For first bar, use original value
                if (index == 0)
                {
                    smoothed[index] = values[index];
                    return smoothed[index];
                }

                // EMA formula: smoothed = alpha * value + (1 - alpha) * previous_smoothed
                smoothed[index] = _alpha * values[index] + (1 - _alpha) * smoothed[index - 1];

                // Fix NaN values
                if (!ValidationHelper.IsValidValue(smoothed[index]))
                {
                    smoothed[index] = values[index];
                }

                return smoothed[index];
            }
            catch (Exception)
            {
                // If error, return original value
                return values[index];
            }
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
