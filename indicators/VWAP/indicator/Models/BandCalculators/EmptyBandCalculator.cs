using System;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Empty band calculator that always returns NaN to hide bands
    /// </summary>
    public class EmptyBandCalculator : IBandCalculator
    {
        public EmptyBandCalculator()
        {
            // Nothing to initialize
        }
        
        public void ProcessBar(int index, double price, double volume, double vwap)
        {
            // Do nothing
        }
        
        public void Reset()
        {
            // Do nothing
        }
        
        public void UpdateParameters(VwapResetPeriod resetPeriod, int pivotDepth, DateTime? anchorPoint)
        {
            // Nothing to update as this calculator always returns NaN
        }
        
        // Return NaN for all band values to hide them
        public double GetUpperBand() => double.NaN;
        public double GetLowerBand() => double.NaN;
        public double GetFibLevel886() => double.NaN;
        public double GetFibLevel764() => double.NaN;
        public double GetFibLevel628() => double.NaN;
        public double GetFibLevel382() => double.NaN;
        public double GetFibLevel236() => double.NaN;
        public double GetFibLevel114() => double.NaN;
    }
}
