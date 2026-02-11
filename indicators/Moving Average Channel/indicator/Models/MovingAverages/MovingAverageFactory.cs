using cAlgo.API;

namespace cAlgo.Indicators
{
    // Factory to create MA objects based on MA Type
    public static class MovingAverageFactory
    {
        // Create MA based on selected type
        public static IMovingAverage CreateMA(MAParameters parameters, int arraySize)
        {
            switch (parameters.MAType)
            {
                case MATypes.Simple:
                    // Simple MA (smoothed with SMA)
                    return new SimpleMACalculation(parameters.GeneralMAPeriod, arraySize);

                case MATypes.Exponential:
                    // Exponential MA (smoothed with EMA)
                    return new EMACalculation(parameters.GeneralMAPeriod, arraySize);

                case MATypes.Wilder:
                    // Wilder's Smoothed MA (no additional smoothing)
                    return new WilderMACalculation(parameters.GeneralMAPeriod, arraySize);

                case MATypes.DeviationScaled:
                    // Deviation Scaled MA
                    return new DSMACalculation(parameters.DSMAPeriod, arraySize);

                case MATypes.SuperSmoother:
                    // SuperSmoother MA
                    return new SuperSmootherCalculation(parameters.SuperSmootherPeriod, arraySize);

                case MATypes.Hull:
                    // Hull MA
                    return new HullMACalculation(parameters.HullMAPeriod, arraySize);

                default:
                    // Default to Simple MA
                    return new SimpleMACalculation(parameters.GeneralMAPeriod, arraySize);
            }
        }
    }

    // Multi MA Manager - manages all 4 OHLC lines
    public class MultiMAManager
    {
        // 4 MA objects - one for each price
        private IMovingAverage _highMA;
        private IMovingAverage _lowMA;
        private IMovingAverage _closeMA;
        private IMovingAverage _openMA;

        private readonly MAParameters _parameters;

        public MultiMAManager(MAParameters parameters, int arraySize)
        {
            _parameters = parameters;

            // Create 4 MA objects of the selected type
            _highMA = MovingAverageFactory.CreateMA(parameters, arraySize);
            _lowMA = MovingAverageFactory.CreateMA(parameters, arraySize);
            _closeMA = MovingAverageFactory.CreateMA(parameters, arraySize);
            _openMA = MovingAverageFactory.CreateMA(parameters, arraySize);
        }

        // Calculate all 7 MA lines (4 OHLC + Median + 2 Fibonacci)
        public MAResult Calculate(int index, Bars bars)
        {
            // Check if inputs are good
            if (bars == null || index < 0 || index >= bars.Count)
                return new MAResult(double.NaN, double.NaN, double.NaN, double.NaN,
                                   double.NaN, double.NaN, double.NaN);

            // Calculate each MA line
            double highMA = _highMA.Calculate(index, bars.HighPrices);
            double lowMA = _lowMA.Calculate(index, bars.LowPrices);
            double closeMA = _closeMA.Calculate(index, bars.ClosePrices);
            double openMA = _openMA.Calculate(index, bars.OpenPrices);

            // NEW: Calculate median (50% between High and Low)
            double medianMA = CalculationHelper.CalculateMedian(highMA, lowMA);

            // Calculate only 2 Fibonacci levels using helper
            var (fib618, fib382) = CalculationHelper.CalculateFibonacciLevels(highMA, lowMA);

            return new MAResult(highMA, lowMA, closeMA, openMA, medianMA, fib618, fib382);
        }

        // Set first values for all 4 MA
        public void InitializeFirstValues(double firstHigh, double firstLow,
                                        double firstClose, double firstOpen)
        {
            _highMA.Initialize(firstHigh);
            _lowMA.Initialize(firstLow);
            _closeMA.Initialize(firstClose);
            _openMA.Initialize(firstOpen);
        }
    }
}
