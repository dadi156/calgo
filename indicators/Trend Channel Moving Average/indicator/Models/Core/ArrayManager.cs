using System;

namespace cAlgo
{
    /// <summary>
    /// Array Manager for TrendChannelMovingAverage indicator
    /// Stores and manages calculated values efficiently
    /// </summary>
    public class ArrayManager
    {
        private CompactValues[] _values;
        private int _capacity;
        private const int INITIAL_SIZE = 1000;
        private const int GROWTH_SIZE = 500;

        private static readonly CompactValues _cachedInvalidValue = CompactValues.Invalid();

        public ArrayManager(int barsCount)
        {
            _capacity = Math.Max(barsCount + GROWTH_SIZE, INITIAL_SIZE);
            _values = new CompactValues[_capacity];
            InitializeArray();
        }

        /// <summary>
        /// Compact structure for storing values
        /// </summary>
        private struct CompactValues
        {
            public double High;
            public double Close;
            public double Low;
            public double Open;
            public double Median;
            public TrendDirection Trend;

            public static CompactValues Invalid()
            {
                return new CompactValues
                {
                    High = double.NaN,
                    Close = double.NaN,
                    Low = double.NaN,
                    Open = double.NaN,
                    Median = double.NaN,
                    Trend = TrendDirection.Neutral
                };
            }
        }

        private void InitializeArray()
        {
            for (int i = 0; i < _capacity; i++)
            {
                _values[i] = _cachedInvalidValue;
            }
        }

        /// <summary>
        /// Ensure array capacity
        /// </summary>
        private void EnsureCapacity(int index)
        {
            if (index >= _capacity)
            {
                int newCapacity = Math.Max(index + GROWTH_SIZE, _capacity * 2);
                Array.Resize(ref _values, newCapacity);
                
                for (int i = _capacity; i < newCapacity; i++)
                {
                    _values[i] = _cachedInvalidValue;
                }
                
                _capacity = newCapacity;
            }
        }

        /// <summary>
        /// Store all values including trend
        /// </summary>
        public void StoreValues(int index, CachedValues values)
        {
            if (index < 0) return;
            
            EnsureCapacity(index);

            _values[index] = new CompactValues
            {
                High = values.High,
                Close = values.Close,
                Low = values.Low,
                Open = values.Open,
                Median = values.Median,
                Trend = values.Trend
            };
        }

        /// <summary>
        /// Get all values including trend
        /// </summary>
        public CachedValues GetValues(int index)
        {
            if (!IsValidIndex(index))
                return CachedValues.Invalid();

            var compact = _values[index];
            return new CachedValues(compact.High, compact.Close, compact.Low, compact.Open, compact.Median, compact.Trend);
        }

        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < _capacity;
        }

        // Fast get methods for individual lines
        public double GetHighLine(int index) => IsValidIndex(index) ? _values[index].High : double.NaN;
        public double GetCloseLine(int index) => IsValidIndex(index) ? _values[index].Close : double.NaN;
        public double GetLowLine(int index) => IsValidIndex(index) ? _values[index].Low : double.NaN;
        public double GetOpenLine(int index) => IsValidIndex(index) ? _values[index].Open : double.NaN;
        public double GetMedianLine(int index) => IsValidIndex(index) ? _values[index].Median : double.NaN;

        /// <summary>
        /// Get trend direction for index
        /// </summary>
        public TrendDirection GetTrend(int index)
        {
            return IsValidIndex(index) ? _values[index].Trend : TrendDirection.Neutral;
        }

        /// <summary>
        /// Get MA values with trend with single bounds check
        /// </summary>
        public bool TryGetMAValues(int index, out double high, out double close, out double low, out double open, out double median, out TrendDirection trend)
        {
            if (IsValidIndex(index))
            {
                var compact = _values[index];
                high = compact.High;
                close = compact.Close;
                low = compact.Low;
                open = compact.Open;
                median = compact.Median;
                trend = compact.Trend;
                return true;
            }
            
            high = close = low = open = median = double.NaN;
            trend = TrendDirection.Neutral;
            return false;
        }

        /// <summary>
        /// Bulk get for multiple indices
        /// </summary>
        public void BulkGetValues(int startIndex, int count, CachedValues[] results)
        {
            if (results == null || startIndex < 0) return;
            
            int endIndex = Math.Min(startIndex + count, _capacity);
            int resultIndex = 0;
            
            for (int i = startIndex; i < endIndex && resultIndex < results.Length; i++)
            {
                if (IsValidIndex(i))
                {
                    results[resultIndex] = GetValues(i);
                }
                else
                {
                    results[resultIndex] = CachedValues.Invalid();
                }
                resultIndex++;
            }
        }
    }
}
