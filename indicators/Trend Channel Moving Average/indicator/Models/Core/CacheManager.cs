using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Cache Manager for TrendChannelMovingAverage indicator
    /// </summary>
    public class CacheManager
    {
        private readonly FastCache<DateTime, CachedValues> _baseTimeframeCache;
        private const int MAX_CACHE_SIZE = 200;

        private int _lastCalculatedIndex = -1;
        private int _cacheHits = 0;
        private int _cacheMisses = 0;

        public CacheManager()
        {
            _baseTimeframeCache = new FastCache<DateTime, CachedValues>(MAX_CACHE_SIZE);
        }

        /// <summary>
        /// Fast cache lookup
        /// </summary>
        public bool TryGetCachedValues(DateTime baseTime, out CachedValues values)
        {
            bool found = _baseTimeframeCache.TryGet(baseTime, out values);

            if (found)
                _cacheHits++;
            else
                _cacheMisses++;

            return found;
        }

        /// <summary>
        /// Store values in cache
        /// </summary>
        public void StoreValues(DateTime baseTime, CachedValues values)
        {
            if (!values.IsValid())
                return;

            _baseTimeframeCache.Set(baseTime, values);
        }

        /// <summary>
        /// Check if calculation is needed
        /// </summary>
        public bool NeedsCalculation(int index, int totalBars)
        {
            bool isFormingBar = (index == totalBars - 1);
            return index > _lastCalculatedIndex || isFormingBar;
        }

        /// <summary>
        /// Update last calculated index
        /// </summary>
        public void UpdateLastCalculatedIndex(int index, int totalBars)
        {
            bool isFormingBar = (index == totalBars - 1);
            if (!isFormingBar)
                _lastCalculatedIndex = index;
        }

        /// <summary>
        /// Get calculation range
        /// </summary>
        public (int startIndex, int endIndex) GetCalculationRange(int targetIndex)
        {
            int startIndex = Math.Max(0, _lastCalculatedIndex + 1);
            return (startIndex, targetIndex);
        }

        /// <summary>
        /// Clear all cache
        /// </summary>
        public void ClearCache()
        {
            _baseTimeframeCache.Clear();
            _lastCalculatedIndex = -1;
            _cacheHits = 0;
            _cacheMisses = 0;
        }

        /// <summary>
        /// Get cache performance info
        /// </summary>
        public double GetCacheHitRatio()
        {
            int total = _cacheHits + _cacheMisses;
            return total > 0 ? (double)_cacheHits / total : 0.0;
        }
    }

    /// <summary>
    /// Fast Cache implementation
    /// </summary>
    public class FastCache<TKey, TValue> where TKey : IEquatable<TKey>
    {
        private readonly struct CacheItem
        {
            public readonly TKey Key;
            public readonly TValue Value;

            public CacheItem(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        private CacheItem[] _items;
        private readonly int _capacity;
        private int _count;

        public FastCache(int capacity)
        {
            _capacity = capacity;
            _items = new CacheItem[capacity];
            _count = 0;
        }

        /// <summary>
        /// Fast cache lookup
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_items[i].Key.Equals(key))
                {
                    value = _items[i].Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Fast cache store
        /// </summary>
        public void Set(TKey key, TValue value)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_items[i].Key.Equals(key))
                {
                    _items[i] = new CacheItem(key, value);
                    return;
                }
            }

            if (_count < _capacity)
            {
                _items[_count] = new CacheItem(key, value);
                _count++;
            }
            else
            {
                for (int i = 0; i < _count - 1; i++)
                {
                    _items[i] = _items[i + 1];
                }
                _items[_count - 1] = new CacheItem(key, value);
            }
        }

        /// <summary>
        /// Clear all items
        /// </summary>
        public void Clear()
        {
            _count = 0;
        }

        public int Count => _count;
    }

    /// <summary>
    /// CachedValues structure for TrendChannelMovingAverage
    /// </summary>
    public struct CachedValues
    {
        public double High { get; }
        public double Close { get; }
        public double Low { get; }
        public double Open { get; }
        public double Median { get; }
        public TrendDirection Trend { get; }

        // private const double TREND_EPSILON = 1e-10;

        public CachedValues(double high, double close, double low, double open, double median)
        {
            High = high;
            Close = close;
            Low = low;
            Open = open;
            Median = median;
            Trend = TrendDirection.Neutral;
        }

        public CachedValues(double high, double close, double low, double open, double median, TrendDirection trend)
        {
            High = high;
            Close = close;
            Low = low;
            Open = open;
            Median = median;
            Trend = trend;
        }

        /// <summary>
        /// Create new CachedValues with specified trend
        /// </summary>
        public CachedValues WithTrend(TrendDirection trend)
        {
            return new CachedValues(High, Close, Low, Open, Median, trend);
        }

        /// <summary>
        /// Calculate trend based on current vs previous Close MA
        /// </summary>
        public static TrendDirection CalculateTrendFromCloseMA(double currentCloseMA, double previousCloseMA)
        {
            if (double.IsNaN(currentCloseMA) || double.IsNaN(previousCloseMA))
                return TrendDirection.Neutral;

            double difference = currentCloseMA - previousCloseMA;
            double epsilon = GetOptimalEpsilon(currentCloseMA); // NEW LINE

            if (Math.Abs(difference) < epsilon) // CHANGED: use epsilon instead of TREND_EPSILON
            {
                return TrendDirection.Neutral;
            }
            else if (difference > 0)
            {
                return TrendDirection.Uptrend;
            }
            else
            {
                return TrendDirection.Downtrend;
            }
        }

        /// <summary>
        /// Ultra sensitive trend detection method
        /// </summary>
        public static TrendDirection CalculateTrendFromCloseMAUltraSensitive(double currentCloseMA, double previousCloseMA)
        {
            if (double.IsNaN(currentCloseMA) || double.IsNaN(previousCloseMA))
                return TrendDirection.Neutral;

            double relativeDifference = Math.Abs(currentCloseMA - previousCloseMA) / Math.Max(Math.Abs(currentCloseMA), Math.Abs(previousCloseMA));

            double epsilon = GetOptimalEpsilon(currentCloseMA); // NEW LINE
            double RELATIVE_EPSILON = epsilon / currentCloseMA; // CHANGED: calculate relative epsilon

            if (relativeDifference < RELATIVE_EPSILON) // CHANGED: use calculated epsilon
            {
                return TrendDirection.Neutral;
            }
            else if (currentCloseMA > previousCloseMA)
            {
                return TrendDirection.Uptrend;
            }
            else
            {
                return TrendDirection.Downtrend;
            }
        }

        /// <summary>
        /// Check if all values are valid
        /// </summary>
        public bool IsValid()
        {
            return !double.IsNaN(High) && !double.IsNaN(Close) &&
                   !double.IsNaN(Low) && !double.IsNaN(Open);
        }

        /// <summary>
        /// Create invalid cache values
        /// </summary>
        public static CachedValues Invalid()
        {
            return new CachedValues(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
        }

        /// <summary>
        /// Calculate optimal epsilon based on price digits
        /// </summary>
        private static double GetOptimalEpsilon(double currentPrice)
        {
            if (currentPrice <= 0)
                return 1e-5; // Safety value

            // Count digits before decimal point
            int digits = (int)Math.Floor(Math.Log10(currentPrice)) + 1;

            // Your tested values
            switch (digits)
            {
                case 1: return 0.0001;   // Like EURUSD (1.16)
                case 2: return 0.02;     // Like XBRUSD (69.10)  
                case 3: return 0.01;     // Like USDJPY (147.86)
                case 4: return 0.3;      // Like XAUUSD (3349.48)
                case 5: return 3.0;      // Like USTEC (22865.60)
                default:
                    // For very high prices
                    return Math.Pow(10, digits - 5) * 3.0;
            }
        }
    }

    /// <summary>
    /// Trend direction for dynamic colors
    /// </summary>
    public enum TrendDirection
    {
        Uptrend,     // Current Close MA > Previous Close MA
        Downtrend,   // Current Close MA < Previous Close MA  
        Neutral      // Current Close MA ≈ Previous Close MA
    }
}
