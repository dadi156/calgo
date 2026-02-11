using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// VIDYA Moving Average (Variable Index Dynamic Average)
    /// Based on proven algorithm
    /// Changes speed based on market movement
    /// Fast when market moves, slow when market is quiet
    /// </summary>
    public class VIDYAMovingAverage : IMovingAverage
    {
        // Cache for VIDYA values
        private readonly Dictionary<DataSeries, Dictionary<int, double>> _vidyaCache;
        private readonly Dictionary<DataSeries, int> _periodCache;
        private readonly Dictionary<DataSeries, bool> _initializedCache;

        // Default sigma value - controls sensitivity
        private const double DefaultSigma = 0.3629;

        public VIDYAMovingAverage()
        {
            _vidyaCache = new Dictionary<DataSeries, Dictionary<int, double>>();
            _periodCache = new Dictionary<DataSeries, int>();
            _initializedCache = new Dictionary<DataSeries, bool>();
        }

        /// <summary>
        /// Calculate VIDYA value
        /// Uses CMO (momentum) to change speed
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            // Check if we have enough data
            if (index < period || index < 0 || index >= prices.Count)
                return double.NaN;

            // Initialize cache for this data series if needed
            if (!_vidyaCache.ContainsKey(prices))
            {
                _vidyaCache[prices] = new Dictionary<int, double>();
                _periodCache[prices] = period;
                _initializedCache[prices] = false;
            }

            // Check if period changed
            if (_periodCache[prices] != period)
            {
                _vidyaCache[prices].Clear();
                _periodCache[prices] = period;
                _initializedCache[prices] = false;
            }

            var vidyaCache = _vidyaCache[prices];

            // Check cache first
            if (vidyaCache.ContainsKey(index))
                return vidyaCache[index];

            // Calculate VIDYA
            double vidyaValue = CalculateVIDYA(prices, index, period, vidyaCache);

            // Store in cache
            vidyaCache[index] = vidyaValue;

            // Clean cache if needed
            if (vidyaCache.Count > 1000)
            {
                CleanCache(vidyaCache, index);
            }

            return vidyaValue;
        }

        /// <summary>
        /// Calculate VIDYA using proven algorithm
        /// From the reference implementation
        /// </summary>
        private double CalculateVIDYA(DataSeries prices, int index, int period, Dictionary<int, double> vidyaCache)
        {
            try
            {
                // Check if this is the first calculation
                if (index == period || !_initializedCache[prices])
                {
                    // First VIDYA = simple average
                    double sum = 0;
                    for (int i = 0; i < period; i++)
                    {
                        sum += prices[index - i];
                    }
                    double firstVidya = sum / period;
                    _initializedCache[prices] = true;
                    return firstVidya;
                }

                // Get previous VIDYA value
                double previousVidya;
                if (vidyaCache.ContainsKey(index - 1))
                {
                    previousVidya = vidyaCache[index - 1];
                }
                else
                {
                    // Calculate previous VIDYA first
                    previousVidya = CalculateVIDYA(prices, index - 1, period, vidyaCache);
                }

                // Calculate CMO (Chande Momentum Oscillator)
                double sumUp = 0;
                double sumDown = 0;

                for (int i = 1; i <= period; i++)
                {
                    int currentIdx = index - i + 1;
                    int previousIdx = index - i;
                    
                    if (currentIdx < 0 || previousIdx < 0) continue;

                    double change = prices[currentIdx] - prices[previousIdx];

                    if (change > 0)
                        sumUp += change;
                    else
                        sumDown += Math.Abs(change);
                }

                // Calculate CMO value
                double cmo = 0;
                if (sumUp + sumDown != 0)
                    cmo = Math.Abs((sumUp - sumDown) / (sumUp + sumDown));

                // Calculate smoothing factor k using sigma and CMO
                double sigma = DefaultSigma; // Sensitivity parameter
                double k = sigma * cmo;

                // Calculate VIDYA using the formula: 
                // VIDYA = k × Price + (1 - k) × Previous_VIDYA
                double currentPrice = prices[index];
                double vidya = k * currentPrice + (1 - k) * previousVidya;

                return vidya;
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Clean old cache values
        /// </summary>
        private void CleanCache(Dictionary<int, double> cache, int currentIndex)
        {
            var keysToRemove = new List<int>();
            
            foreach (var key in cache.Keys)
            {
                if (key < currentIndex - 500)
                    keysToRemove.Add(key);
            }
            
            foreach (var key in keysToRemove)
            {
                cache.Remove(key);
            }
        }
    }
}
