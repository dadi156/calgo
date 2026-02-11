using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Main model class. It uses all helper classes.
    /// Uses factory pattern for MA calculations.
    /// </summary>
    public class MAModel
    {
        private readonly StateManager stateManager;
        private readonly DateTimeHelper dateTimeHelper;
        private readonly PeriodManager periodManager;

        private IMACalculator maCalculator;
        private BandCalculator bandCalculator;
        private MABandVisibility bandVisibility;

        /// <summary>
        /// Create model with all helpers
        /// </summary>
        public MAModel()
        {
            this.stateManager = new StateManager();
            this.dateTimeHelper = new DateTimeHelper();
            this.periodManager = new PeriodManager();

            this.bandCalculator = new BandCalculator(BandRangeLevel.Fib500);
            this.bandVisibility = MABandVisibility.None;
        }

        /// <summary>
        /// Start the model. Parse start date.
        /// </summary>
        public bool Initialize(string startDateTimeString)
        {
            bool success = dateTimeHelper.TryParseDateTime(startDateTimeString, out DateTime startDate);

            stateManager.StartDate = startDate;
            stateManager.IsValidStartDate = success;

            return success;
        }

        /// <summary>
        /// Update band settings
        /// </summary>
        public void UpdateBandSettings(MABandVisibility bandVisibility, BandRangeLevel bandRange)
        {
            this.bandVisibility = bandVisibility;

            if (bandCalculator != null)
            {
                bandCalculator.UpdateBandRange(bandRange);
            }
        }

        /// <summary>
        /// Check if bar time is good for calculation
        /// </summary>
        public bool IsBarValid(DateTime barTime)
        {
            if (!stateManager.IsValidStartDate)
                return false;

            return dateTimeHelper.IsBarTimeValid(barTime, stateManager.StartDate);
        }

        /// <summary>
        /// Find start bar if not found yet
        /// </summary>
        public bool FindStartBar(Bars bars, int currentIndex)
        {
            if (stateManager.StartBarIndexFound)
                return true;

            int startIndex = dateTimeHelper.FindStartBarIndex(bars, stateManager.StartDate, currentIndex);

            if (startIndex >= 0)
            {
                stateManager.StartBarIndex = startIndex;
                stateManager.StartBarIndexFound = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculate moving average value with MaxPeriod and band processing
        /// </summary>
        public double CalculateMA(int index, double currentValue, dynamic customMAType,
            DataSeries source, double previousValue, dynamic indicators, Bars bars, int maxPeriod = 0)
        {
            int period = periodManager.CalculatePeriod(index, stateManager.StartBarIndex, maxPeriod);

            if (maCalculator == null)
            {
                maCalculator = MACalculatorFactory.Create((int)customMAType);
            }

            double maValue = maCalculator.Calculate(currentValue, period, index, previousValue, stateManager);

            if (bandVisibility != MABandVisibility.None && bandCalculator != null)
            {
                double sourceValue = source[index];
                bandCalculator.ProcessSourceValue(sourceValue);
            }

            return maValue;
        }

        /// <summary>
        /// Get Fibonacci band values around current MA
        /// </summary>
        public void GetBandValues(double currentMA, out double upperBand, out double lowerBand,
            out double fibo886, out double fibo764, out double fibo628,
            out double fibo382, out double fibo236, out double fibo114)
        {
            if (bandVisibility == MABandVisibility.None || bandCalculator == null || !bandCalculator.HasData())
            {
                upperBand = double.NaN;
                lowerBand = double.NaN;
                fibo886 = double.NaN;
                fibo764 = double.NaN;
                fibo628 = double.NaN;
                fibo382 = double.NaN;
                fibo236 = double.NaN;
                fibo114 = double.NaN;
                return;
            }

            double tempUpperBand, tempLowerBand;
            double tempFibo886, tempFibo764, tempFibo628;
            double tempFibo382, tempFibo236, tempFibo114;

            bandCalculator.GetBandValues(currentMA,
                out tempUpperBand, out tempLowerBand,
                out tempFibo886, out tempFibo764, out tempFibo628,
                out tempFibo382, out tempFibo236, out tempFibo114);

            switch (bandVisibility)
            {
                case MABandVisibility.Band:
                    upperBand = tempUpperBand;
                    lowerBand = tempLowerBand;
                    fibo886 = tempFibo886;
                    fibo764 = tempFibo764;
                    fibo628 = tempFibo628;
                    fibo382 = tempFibo382;
                    fibo236 = tempFibo236;
                    fibo114 = tempFibo114;
                    break;

                case MABandVisibility.UpperBand:
                    upperBand = tempUpperBand;
                    lowerBand = double.NaN;
                    fibo886 = tempFibo886;
                    fibo764 = tempFibo764;
                    fibo628 = tempFibo628;
                    fibo382 = tempFibo382;
                    fibo236 = double.NaN;
                    fibo114 = double.NaN;
                    break;

                case MABandVisibility.LowerBand:
                    upperBand = double.NaN;
                    lowerBand = tempLowerBand;
                    fibo886 = double.NaN;
                    fibo764 = double.NaN;
                    fibo628 = tempFibo628;
                    fibo382 = tempFibo382;
                    fibo236 = tempFibo236;
                    fibo114 = tempFibo114;
                    break;

                case MABandVisibility.ReversionZone:
                    upperBand = double.NaN;
                    lowerBand = double.NaN;
                    fibo886 = double.NaN;
                    fibo764 = double.NaN;
                    fibo628 = tempFibo628;
                    fibo382 = tempFibo382;
                    fibo236 = double.NaN;
                    fibo114 = double.NaN;
                    break;

                default:
                    upperBand = double.NaN;
                    lowerBand = double.NaN;
                    fibo886 = double.NaN;
                    fibo764 = double.NaN;
                    fibo628 = double.NaN;
                    fibo382 = double.NaN;
                    fibo236 = double.NaN;
                    fibo114 = double.NaN;
                    break;
            }
        }

        /// <summary>
        /// Check if model is ready for calculation
        /// </summary>
        public bool IsReady()
        {
            return stateManager.IsReadyForCalculation();
        }

        /// <summary>
        /// Reset all calculation state
        /// </summary>
        public void Reset()
        {
            stateManager.Reset();

            if (maCalculator != null)
            {
                maCalculator.Reset();
            }

            if (bandCalculator != null)
            {
                bandCalculator.Reset();
            }
        }

        /// <summary>
        /// Change MA type (creates new calculator)
        /// </summary>
        public void ChangeMAType(int newMAType)
        {
            maCalculator = MACalculatorFactory.Create(newMAType);
        }
    }
}
