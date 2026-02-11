using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Interface for all moving average types
    /// </summary>
    public interface IMovingAverage
    {
        /// <summary>
        /// Calculate moving average value
        /// </summary>
        double Calculate(DataSeries prices, int index, int period);
    }

    /// <summary>
    /// Factory to create moving average types
    /// </summary>
    public static class MovingAverageFactory
    {
        private static readonly Dictionary<MAType, IMovingAverage> _maCache = new Dictionary<MAType, IMovingAverage>();
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Get moving average calculator
        /// </summary>
        public static IMovingAverage Create(MAType type)
        {
            if (_maCache.ContainsKey(type))
                return _maCache[type];

            lock (_lockObject)
            {
                if (_maCache.ContainsKey(type))
                    return _maCache[type];

                IMovingAverage ma = CreateNewMA(type);
                _maCache[type] = ma;
                return ma;
            }
        }

        /// <summary>
        /// Create new MA instance
        /// </summary>
        private static IMovingAverage CreateNewMA(MAType type)
        {
            switch (type)
            {
                case MAType.ArnaudLegouxMA:
                    return new ArnaudLegouxMovingAverage();

                case MAType.ExponentialMA:
                    return new ExponentialMovingAverage();

                case MAType.DoubleSmoothedEMA:
                    return new DoubleSmoothedExponentialMovingAverage();

                case MAType.HullMA:
                    return new HullMovingAverage();

                case MAType.JurikMA:
                    return new JurikMovingAverage();

                case MAType.KaufmanAdaptiveMA:
                    return new KaufmanAdaptiveMovingAverage();

                case MAType.LaguerreMA:
                    return new LaguerreFilterMovingAverage();

                case MAType.McGinleyDynamic:
                    return new McGinleyDynamicMovingAverage();

                case MAType.SimpleMA:
                    return new SimpleMovingAverage();

                case MAType.T3:
                    return new T3MovingAverage();

                case MAType.VIDYA:
                    return new VIDYAMovingAverage();

                case MAType.ZeroLagEMA:
                    return new ZeroLagExponentialMovingAverage();

                case MAType.DeviationScaledMA:
                    return new DeviationScaledMovingAverage();

                case MAType.SuperSmootherMA:
                    return new SuperSmootherMovingAverage();

                case MAType.UltimateSmootherMA:
                    return new UltimateSmootherMovingAverage();

                default:
                    return new SimpleMovingAverage();
            }
        }

        /// <summary>
        /// Clear cache if needed
        /// </summary>
        public static void ClearCache()
        {
            lock (_lockObject)
            {
                _maCache.Clear();
            }
        }

        /// <summary>
        /// Get cache info for debugging
        /// </summary>
        public static int GetCacheSize()
        {
            return _maCache.Count;
        }
    }
}
