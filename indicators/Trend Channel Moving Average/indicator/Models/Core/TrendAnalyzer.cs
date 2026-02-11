using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Trend Calculator for TrendChannelMovingAverage indicator
    /// Uses averaging for better trend detection
    /// </summary>
    public class TrendAnalyzer
    {
        // Store calculated Close MA values for trend calculation
        private readonly Dictionary<int, double> _calculatedCloseMAs = new Dictionary<int, double>();
        
        // Store trend averaging period
        private int _trendAveragingPeriod;

        /// <summary>
        /// Constructor to set averaging period
        /// </summary>
        public TrendAnalyzer(int trendAveragingPeriod = 5)
        {
            _trendAveragingPeriod = Math.Max(1, trendAveragingPeriod);
        }

        /// <summary>
        /// Update trend averaging period
        /// </summary>
        public void SetTrendAveragingPeriod(int period)
        {
            _trendAveragingPeriod = Math.Max(1, period);
        }

        /// <summary>
        /// Store Close MA value for specific index
        /// </summary>
        public void StoreCloseMA(int index, double closeMA)
        {
            _calculatedCloseMAs[index] = closeMA;
        }

        /// <summary>
        /// Calculate trend using averaging method
        /// Compare current Close MA vs Average of last N bars
        /// </summary>
        public TrendDirection CalculateTrend(int currentIndex, double currentCloseMA)
        {
            try
            {
                // For very first bars, default to neutral
                if (currentIndex < _trendAveragingPeriod)
                {
                    return TrendDirection.Neutral;
                }

                // Calculate average of last N bars
                double averageMA = CalculateAverageCloseMA(currentIndex);
                
                if (double.IsNaN(averageMA))
                {
                    return TrendDirection.Neutral;
                }

                // Compare current vs average
                return CachedValues.CalculateTrendFromCloseMA(currentCloseMA, averageMA);
            }
            catch (Exception)
            {
                return TrendDirection.Neutral;
            }
        }

        /// <summary>
        /// Calculate average of last N Close MA values
        /// </summary>
        private double CalculateAverageCloseMA(int currentIndex)
        {
            try
            {
                double sum = 0;
                int count = 0;

                // Get last N bars (not including current)
                for (int i = 1; i <= _trendAveragingPeriod; i++)
                {
                    int lookbackIndex = currentIndex - i;
                    
                    if (_calculatedCloseMAs.ContainsKey(lookbackIndex))
                    {
                        double closeMA = _calculatedCloseMAs[lookbackIndex];
                        if (!double.IsNaN(closeMA))
                        {
                            sum += closeMA;
                            count++;
                        }
                    }
                }

                // Return average if we have enough data
                if (count >= _trendAveragingPeriod / 2) // At least half of the required bars
                {
                    return sum / count;
                }
                else
                {
                    return double.NaN;
                }
            }
            catch (Exception)
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Calculate trend direction with ultra sensitive detection
        /// </summary>
        public TrendDirection CalculateTrendUltraSensitive(int currentIndex, double currentCloseMA)
        {
            try
            {
                if (currentIndex < _trendAveragingPeriod)
                {
                    return TrendDirection.Neutral;
                }

                double averageMA = CalculateAverageCloseMA(currentIndex);
                
                if (double.IsNaN(averageMA))
                {
                    return TrendDirection.Neutral;
                }

                return CachedValues.CalculateTrendFromCloseMAUltraSensitive(currentCloseMA, averageMA);
            }
            catch (Exception)
            {
                return TrendDirection.Neutral;
            }
        }

        /// <summary>
        /// Get stored Close MA value for specific index
        /// </summary>
        public double GetStoredCloseMA(int index)
        {
            if (_calculatedCloseMAs.ContainsKey(index))
            {
                return _calculatedCloseMAs[index];
            }
            return double.NaN;
        }

        /// <summary>
        /// Check if Close MA value exists for index
        /// </summary>
        public bool HasStoredCloseMA(int index)
        {
            return _calculatedCloseMAs.ContainsKey(index);
        }

        /// <summary>
        /// Get trend direction as string for display
        /// </summary>
        public string GetTrendDirectionAsString(TrendDirection trend)
        {
            switch (trend)
            {
                case TrendDirection.Uptrend: return "Uptrend";
                case TrendDirection.Downtrend: return "Downtrend";
                case TrendDirection.Neutral: return "Neutral";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Clear all stored Close MA values
        /// Used when resetting optimizations
        /// </summary>
        public void Clear()
        {
            _calculatedCloseMAs.Clear();
        }

        /// <summary>
        /// Get count of stored Close MA values
        /// </summary>
        public int GetStoredCount()
        {
            return _calculatedCloseMAs.Count;
        }

        /// <summary>
        /// Remove old stored values to prevent memory growth
        /// Keeps only recent values
        /// </summary>
        public void CleanOldValues(int keepRecentCount = 1000)
        {
            if (_calculatedCloseMAs.Count <= keepRecentCount)
                return;

            try
            {
                // Find the highest index
                int maxIndex = -1;
                foreach (var key in _calculatedCloseMAs.Keys)
                {
                    if (key > maxIndex)
                        maxIndex = key;
                }

                // Remove values older than (maxIndex - keepRecentCount)
                int cutoffIndex = maxIndex - keepRecentCount;
                var keysToRemove = new List<int>();

                foreach (var key in _calculatedCloseMAs.Keys)
                {
                    if (key < cutoffIndex)
                        keysToRemove.Add(key);
                }

                foreach (var key in keysToRemove)
                {
                    _calculatedCloseMAs.Remove(key);
                }
            }
            catch (Exception)
            {
                // Silent error handling
            }
        }

        /// <summary>
        /// Get current trend averaging period
        /// </summary>
        public int GetTrendAveragingPeriod()
        {
            return _trendAveragingPeriod;
        }
    }
}
