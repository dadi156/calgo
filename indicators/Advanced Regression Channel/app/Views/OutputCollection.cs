using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Manages a collection of output data series for the channel
    /// </summary>
    public class OutputCollection
    {
        private readonly IndicatorDataSeries[] _allSeries;

        /// <summary>
        /// 100% Fibonacci level (upper boundary)
        /// </summary>
        public IndicatorDataSeries Fib100 { get; }

        /// <summary>
        /// 88.6% Fibonacci level
        /// </summary>
        public IndicatorDataSeries Level886 { get; }

        /// <summary>
        /// 76.4% Fibonacci level
        /// </summary>
        public IndicatorDataSeries Fib764 { get; }

        /// <summary>
        /// 61.8% Fibonacci level
        /// </summary>
        public IndicatorDataSeries Fib618 { get; }

        /// <summary>
        /// 50% Fibonacci level (middle line)
        /// </summary>
        public IndicatorDataSeries Fib50 { get; }

        /// <summary>
        /// 38.2% Fibonacci level
        /// </summary>
        public IndicatorDataSeries Fib382 { get; }

        /// <summary>
        /// 23.6% Fibonacci level
        /// </summary>
        public IndicatorDataSeries Fib236 { get; }

        /// <summary>
        /// 11.4% Fibonacci level
        /// </summary>
        public IndicatorDataSeries Level114 { get; }

        /// <summary>
        /// 0% Fibonacci level (lower boundary)
        /// </summary>
        public IndicatorDataSeries Fib0 { get; }

        /// <summary>
        /// Constructor that takes all indicator data series
        /// </summary>
        public OutputCollection(
            IndicatorDataSeries fib100,
            IndicatorDataSeries level886,
            IndicatorDataSeries fib764,
            IndicatorDataSeries fib618,
            IndicatorDataSeries fib50,
            IndicatorDataSeries fib382,
            IndicatorDataSeries fib236,
            IndicatorDataSeries level114,
            IndicatorDataSeries fib0)
        {
            Fib100 = fib100;
            Level886 = level886;
            Fib764 = fib764;
            Fib618 = fib618;
            Fib50 = fib50;
            Fib382 = fib382;
            Fib236 = fib236;
            Level114 = level114;
            Fib0 = fib0;

            // Cache array once
            _allSeries = new[]
            {
                Fib100,
                Level886,
                Fib764,
                Fib618,
                Fib50,
                Fib382,
                Fib236,
                Level114,
                Fib0
            };
        }

        /// <summary>
        /// Get all output series as an array
        /// </summary>
        public IndicatorDataSeries[] GetAllSeries()
        {
            return _allSeries;
        }

        /// <summary>
        /// Clear values for a specific index
        /// </summary>
        public void ClearValues(int index)
        {
            Fib100[index] = double.NaN;
            Level886[index] = double.NaN;
            Fib764[index] = double.NaN;
            Fib618[index] = double.NaN;
            Fib50[index] = double.NaN;
            Fib382[index] = double.NaN;
            Fib236[index] = double.NaN;
            Level114[index] = double.NaN;
            Fib0[index] = double.NaN;
        }
    }
}
