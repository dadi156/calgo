using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Current Timeframe Calculator for TrendChannelMovingAverage indicator
    /// </summary>
    public class TimeframeCalculator
    {
        private readonly IMovingAverage _movingAverage;
        private readonly DataSeriesHelper _dataSeriesHelper;
        private readonly ConfigurationManager _config;

        public TimeframeCalculator(ConfigurationManager config)
        {
            _config = config;
            _movingAverage = MovingAverageFactory.Create(config.MAType);
            _dataSeriesHelper = new DataSeriesHelper();
        }

        /// <summary>
        /// Calculate all values for current timeframe with fixed period
        /// </summary>
        public CachedValues Calculate(int index)
        {
            return Calculate(index, _config.Period);
        }

        /// <summary>
        /// Calculate all values with custom effective period
        /// </summary>
        public CachedValues Calculate(int index, int effectivePeriod)
        {
            DataSeries highPrices = _config.CurrentBars.HighPrices;
            DataSeries lowPrices = _config.CurrentBars.LowPrices;
            DataSeries openPrices = _config.CurrentBars.OpenPrices;
            
            // Use the Source parameter directly - no need for conversion
            DataSeries sourcePrices = _config.Source;

            double highMA = _movingAverage.Calculate(highPrices, index, effectivePeriod);
            double sourceMA = _movingAverage.Calculate(sourcePrices, index, effectivePeriod);
            double lowMA = _movingAverage.Calculate(lowPrices, index, effectivePeriod);
            double openMA = _movingAverage.Calculate(openPrices, index, effectivePeriod);

            double medianMA = double.NaN;
            if (!double.IsNaN(highMA) && !double.IsNaN(sourceMA) && !double.IsNaN(lowMA) && !double.IsNaN(openMA))
            {
                medianMA = (highMA + sourceMA + lowMA + openMA) / 4.0;
            }

            // Note: sourceMA is now used for Close line (was closeMA before)
            return new CachedValues(highMA, sourceMA, lowMA, openMA, medianMA);
        }
    }
}
