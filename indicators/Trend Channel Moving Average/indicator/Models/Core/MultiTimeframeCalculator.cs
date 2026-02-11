using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Multi-timeframe Calculator for TrendChannelMovingAverage indicator
    /// </summary>
    public class MultiTimeframeCalculator
    {
        private readonly IMovingAverage _movingAverage;
        private readonly DataSeriesHelper _dataSeriesHelper;
        private readonly ConfigurationManager _config;
        private readonly CacheManager _cacheManager;
        private readonly TrendChannelMovingAverage _indicator;

        public MultiTimeframeCalculator(ConfigurationManager config, CacheManager cacheManager, TrendChannelMovingAverage indicator = null)
        {
            _config = config;
            _cacheManager = cacheManager;
            _indicator = indicator;
            
            _movingAverage = MovingAverageFactory.Create(config.MAType);
            _dataSeriesHelper = new DataSeriesHelper();
        }

        /// <summary>
        /// Calculate multi-timeframe values with fixed period
        /// </summary>
        public CachedValues Calculate(int currentIndex)
        {
            return Calculate(currentIndex, _config.Period);
        }

        /// <summary>
        /// Calculate multi-timeframe values with custom effective period
        /// </summary>
        public CachedValues Calculate(int currentIndex, int effectivePeriod)
        {
            try
            {
                DateTime currentTime = _config.CurrentBars.OpenTimes[currentIndex];
                int baseIndex = FindBaseTimeframeIndex(currentTime);

                if (baseIndex < 0)
                    return CachedValues.Invalid();

                int baseEffectivePeriod = CalculateBaseTimeframeEffectivePeriod(baseIndex);
                var currentValues = GetCachedBaseValues(baseIndex, baseEffectivePeriod);
                if (!currentValues.IsValid())
                    return CachedValues.Invalid();

                int nextBaseIndex = baseIndex + 1;
                bool isLastBaseBar = (nextBaseIndex >= _config.BaseTimeframeBars.Count);

                if (isLastBaseBar && baseIndex > 0)
                {
                    int prevBaseEffectivePeriod = CalculateBaseTimeframeEffectivePeriod(baseIndex - 1);
                    var previousValues = GetCachedBaseValues(baseIndex - 1, prevBaseEffectivePeriod);
                    
                    if (previousValues.IsValid())
                    {
                        DateTime currentBaseTime = _config.BaseTimeframeBars.OpenTimes[baseIndex];
                        double timeFactor = CalculateTimeFactor(currentTime, currentBaseTime);
                        var slopes = CalculateSlopes(currentValues, previousValues);
                        return ExtendValues(currentValues, slopes, timeFactor);
                    }

                    return currentValues;
                }

                if (!isLastBaseBar)
                {
                    int nextBaseEffectivePeriod = CalculateBaseTimeframeEffectivePeriod(nextBaseIndex);
                    var nextValues = GetCachedBaseValues(nextBaseIndex, nextBaseEffectivePeriod);
                    
                    if (nextValues.IsValid())
                    {
                        DateTime currentBaseTime = _config.BaseTimeframeBars.OpenTimes[baseIndex];
                        DateTime nextBaseTime = _config.BaseTimeframeBars.OpenTimes[nextBaseIndex];

                        double factor = InterpolationHelper.CalculateInterpolationFactor(
                            currentTime, currentBaseTime, nextBaseTime);

                        return InterpolateValues(currentValues, nextValues, factor);
                    }
                }

                return currentValues;
            }
            catch
            {
                return CachedValues.Invalid();
            }
        }

        /// <summary>
        /// Calculate effective period for BASE timeframe bars
        /// </summary>
        private int CalculateBaseTimeframeEffectivePeriod(int baseIndex)
        {
            if (_indicator == null || _indicator.GetPeriodCalculationType() != PeriodCalculationType.AnchorDate)
            {
                return _config.Period;
            }

            try
            {
                DateTime anchorDate = _indicator.GetAnchorDate();
                DateTime baseBarTime = _config.BaseTimeframeBars.OpenTimes[baseIndex];
                
                if (baseBarTime < anchorDate)
                    return 1;

                int firstValidBaseIndex = FindFirstValidBaseTimeframeIndex(anchorDate);
                
                if (firstValidBaseIndex < 0 || baseIndex < firstValidBaseIndex)
                    return 1;

                int expandingPeriod = baseIndex - firstValidBaseIndex + 1;
                return Math.Max(1, expandingPeriod);
            }
            catch
            {
                return _config.Period;
            }
        }

        /// <summary>
        /// Find first valid base timeframe bar after anchor date
        /// </summary>
        private int FindFirstValidBaseTimeframeIndex(DateTime anchorDate)
        {
            if (_config.BaseTimeframeBars == null)
                return -1;

            for (int i = 0; i < _config.BaseTimeframeBars.Count; i++)
            {
                if (_config.BaseTimeframeBars.OpenTimes[i] >= anchorDate)
                    return i;
            }
            
            return -1;
        }

        /// <summary>
        /// Calculate movement slopes for each line
        /// </summary>
        private ValueSlopes CalculateSlopes(CachedValues current, CachedValues previous)
        {
            return new ValueSlopes
            {
                HighSlope = current.High - previous.High,
                CloseSlope = current.Close - previous.Close,
                LowSlope = current.Low - previous.Low,
                OpenSlope = current.Open - previous.Open,
                MedianSlope = current.Median - previous.Median
            };
        }

        /// <summary>
        /// Extend values using movement speed
        /// </summary>
        private CachedValues ExtendValues(CachedValues current, ValueSlopes slopes, double timeFactor)
        {
            return new CachedValues(
                current.High + (slopes.HighSlope * timeFactor),
                current.Close + (slopes.CloseSlope * timeFactor),
                current.Low + (slopes.LowSlope * timeFactor),
                current.Open + (slopes.OpenSlope * timeFactor),
                current.Median + (slopes.MedianSlope * timeFactor)
            );
        }

        /// <summary>
        /// Interpolate between two sets of values
        /// </summary>
        private CachedValues InterpolateValues(CachedValues current, CachedValues next, double factor)
        {
            return new CachedValues(
                InterpolationHelper.InterpolateValue(current.High, next.High, factor),
                InterpolationHelper.InterpolateValue(current.Close, next.Close, factor),
                InterpolationHelper.InterpolateValue(current.Low, next.Low, factor),
                InterpolationHelper.InterpolateValue(current.Open, next.Open, factor),
                InterpolationHelper.InterpolateValue(current.Median, next.Median, factor)
            );
        }

        /// <summary>
        /// Calculate time factor for extrapolation
        /// </summary>
        private double CalculateTimeFactor(DateTime currentTime, DateTime baseTime)
        {
            TimeSpan baseTimeframeDuration = GetBaseTimeframeDuration();
            double baseMinutes = baseTimeframeDuration.TotalMinutes;

            if (baseMinutes <= 0)
                return 0;

            double passedMinutes = (currentTime - baseTime).TotalMinutes;
            return Math.Max(0, Math.Min(1.5, passedMinutes / baseMinutes));
        }

        /// <summary>
        /// Get base timeframe duration
        /// </summary>
        private TimeSpan GetBaseTimeframeDuration()
        {
            if (_config.BaseTimeframeBars.Count < 2)
                return TimeSpan.FromMinutes(60);

            int sampleSize = Math.Min(3, _config.BaseTimeframeBars.Count - 1);
            double totalMinutes = 0;

            for (int i = 0; i < sampleSize; i++)
            {
                int idx = _config.BaseTimeframeBars.Count - 1 - i;
                double minutes = (_config.BaseTimeframeBars.OpenTimes[idx] - _config.BaseTimeframeBars.OpenTimes[idx - 1]).TotalMinutes;
                totalMinutes += minutes;
            }

            return TimeSpan.FromMinutes(totalMinutes / sampleSize);
        }

        /// <summary>
        /// Find base timeframe index
        /// </summary>
        private int FindBaseTimeframeIndex(DateTime currentTime)
        {
            if (_config.BaseTimeframeBars == null) 
                return -1;

            for (int i = _config.BaseTimeframeBars.Count - 1; i >= 0; i--)
            {
                DateTime baseBarTime = _config.BaseTimeframeBars.OpenTimes[i];
                
                if (currentTime >= baseBarTime)
                {
                    return i;
                }
            }
            
            return -1;
        }

        /// <summary>
        /// Get cached base timeframe values
        /// </summary>
        private CachedValues GetCachedBaseValues(int baseIndex, int effectivePeriod)
        {
            DateTime baseTime = _config.BaseTimeframeBars.OpenTimes[baseIndex];

            bool useCache = (_indicator == null || _indicator.GetPeriodCalculationType() != PeriodCalculationType.AnchorDate);
            
            CachedValues cachedValue = CachedValues.Invalid();
            bool foundInCache = false;
            
            if (useCache)
            {
                foundInCache = _cacheManager.TryGetCachedValues(baseTime, out cachedValue);
                if (foundInCache)
                    return cachedValue;
            }

            DataSeries baseHighPrices = _config.BaseTimeframeBars.HighPrices;
            DataSeries baseLowPrices = _config.BaseTimeframeBars.LowPrices;
            DataSeries baseOpenPrices = _config.BaseTimeframeBars.OpenPrices;
            
            // For multi-timeframe, we need to get the correct source from base timeframe
            // We'll use ClosePrices as default since Source is from current timeframe
            DataSeries baseSourcePrices = _config.BaseTimeframeBars.ClosePrices;

            double baseHigh = _movingAverage.Calculate(baseHighPrices, baseIndex, effectivePeriod);
            double baseSource = _movingAverage.Calculate(baseSourcePrices, baseIndex, effectivePeriod);
            double baseLow = _movingAverage.Calculate(baseLowPrices, baseIndex, effectivePeriod);
            double baseOpen = _movingAverage.Calculate(baseOpenPrices, baseIndex, effectivePeriod);

            double baseMedian = double.NaN;
            if (!double.IsNaN(baseHigh) && !double.IsNaN(baseSource) && !double.IsNaN(baseLow) && !double.IsNaN(baseOpen))
            {
                baseMedian = (baseHigh + baseSource + baseLow + baseOpen) / 4.0;
            }

            var result = new CachedValues(baseHigh, baseSource, baseLow, baseOpen, baseMedian);

            if (useCache && result.IsValid())
            {
                _cacheManager.StoreValues(baseTime, result);
            }

            return result;
        }

        /// <summary>
        /// Structure for value movement speeds
        /// </summary>
        private struct ValueSlopes
        {
            public double HighSlope { get; set; }
            public double CloseSlope { get; set; }
            public double LowSlope { get; set; }
            public double OpenSlope { get; set; }
            public double MedianSlope { get; set; }
        }
    }
}
