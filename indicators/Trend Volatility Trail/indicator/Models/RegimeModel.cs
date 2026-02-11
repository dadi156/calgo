// RegimeModel - Main calculation model
using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    // Main model - coordinates all calculators
    public class RegimeModel
    {
        private readonly RegimeParameters _parameters;
        
        // Calculator objects
        private readonly VolatilityCalculator _volatilityCalculator;
        private readonly TrendCalculator _trendCalculator;
        private readonly TrailCalculator _trailCalculator;
        private readonly RegimeDetector _regimeDetector;

        // Built-in indicators (managed by controller)
        private MovingAverage _ma;
        private AverageTrueRange _atr;
        private MovingAverage _atrAvg;
        private MovingAverage _trendMemory;

        public RegimeModel(int arraySize, RegimeParameters parameters)
        {
            _parameters = parameters;

            // Create calculator objects
            _volatilityCalculator = new VolatilityCalculator();
            _trendCalculator = new TrendCalculator(arraySize);
            _trailCalculator = new TrailCalculator(arraySize);
            _regimeDetector = new RegimeDetector();
        }

        // Set built-in indicators (called by controller)
        public void SetIndicators(MovingAverage ma, AverageTrueRange atr, 
                                 MovingAverage atrAvg, MovingAverage trendMemory)
        {
            _ma = ma;
            _atr = atr;
            _atrAvg = atrAvg;
            _trendMemory = trendMemory;
        }

        // Get direction step data series (for trend memory MA)
        public double GetDirectionStep(int index)
        {
            return _trendCalculator.GetDirectionStep(index);
        }

        // Main calculation
        public RegimeResult Calculate(int index)
        {
            try
            {
                // Get values from built-in indicators
                double basis = _ma.Result[index];
                double atr = _atr.Result[index];
                double atrAvg = _atrAvg.Result[index];
                double trendMemory = _trendMemory.Result[index];

                // 1. Calculate direction step (for trend memory)
                double previousBasis = index > 0 ? _ma.Result[index - 1] : basis;
                _trendCalculator.CalculateDirectionStep(index, basis, previousBasis);

                // 2. Calculate volatility stretch
                double volStretch = _volatilityCalculator.CalculateVolatilityStretch(
                    atr, atrAvg, _parameters.VolPower);

                // 3. Calculate trend boost
                double trendBoost = _trendCalculator.CalculateTrendBoost(
                    trendMemory, _parameters.TrendImpact);

                // 4. Calculate final multiplier (clamped)
                double multRaw = _parameters.BaseMultiplier * volStretch * trendBoost;
                double multFinal = Math.Max(_parameters.MultMin, 
                                           Math.Min(multRaw, _parameters.MultMax));

                // 5. Get current regime before calculating trails
                int currentRegime = _regimeDetector.GetCurrentRegime();

                // 6. Calculate trails (bands calculated internally)
                var (trailLong, trailShort) = _trailCalculator.CalculateTrails(
                    index, basis, atr, multFinal, currentRegime);

                // 7. Detect regime (may update regime based on price crossing trails)
                // Need close price from controller
                double closePrice = 0; // Will be set by controller
                int regime = _regimeDetector.DetectRegime(
                    closePrice, trailLong, trailShort, _parameters.ConfirmBars);

                // Return result
                return new RegimeResult(trailLong, trailShort, regime);
            }
            catch (Exception)
            {
                // If error, return safe default
                return new RegimeResult(0, 0, 0);
            }
        }

        // Calculate with close price (called by controller)
        public RegimeResult CalculateWithPrice(int index, double closePrice)
        {
            try
            {
                // Get values from built-in indicators
                double basis = _ma.Result[index];
                double atr = _atr.Result[index];
                double atrAvg = _atrAvg.Result[index];
                double trendMemory = _trendMemory.Result[index];

                // 1. Calculate direction step
                double previousBasis = index > 0 ? _ma.Result[index - 1] : basis;
                _trendCalculator.CalculateDirectionStep(index, basis, previousBasis);

                // 2. Calculate volatility stretch
                double volStretch = _volatilityCalculator.CalculateVolatilityStretch(
                    atr, atrAvg, _parameters.VolPower);

                // 3. Calculate trend boost
                double trendBoost = _trendCalculator.CalculateTrendBoost(
                    trendMemory, _parameters.TrendImpact);

                // 4. Calculate final multiplier (clamped)
                double multRaw = _parameters.BaseMultiplier * volStretch * trendBoost;
                double multFinal = Math.Max(_parameters.MultMin, 
                                           Math.Min(multRaw, _parameters.MultMax));

                // 5. Get current regime
                int currentRegime = _regimeDetector.GetCurrentRegime();

                // 6. Calculate trails
                var (trailLong, trailShort) = _trailCalculator.CalculateTrails(
                    index, basis, atr, multFinal, currentRegime);

                // 7. Detect regime with close price
                int regime = _regimeDetector.DetectRegime(
                    closePrice, trailLong, trailShort, _parameters.ConfirmBars);

                return new RegimeResult(trailLong, trailShort, regime);
            }
            catch (Exception)
            {
                // If error, return safe default
                return new RegimeResult(0, 0, 0);
            }
        }
    }
}
