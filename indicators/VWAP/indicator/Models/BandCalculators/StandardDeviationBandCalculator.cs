using System;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Calculates Fibonacci levels based on standard deviation from VWAP
    /// </summary>
    public class StandardDeviationBandCalculator : IBandCalculator
    {
        private double _stdDevMultiplier;
        private double _sumSquaredDistances;
        private double _cumulativeVolume;
        
        private double _upperBand;
        private double _lowerBand;
        private double _fibLevel886;
        private double _fibLevel764;
        private double _fibLevel628;
        private double _fibLevel382;
        private double _fibLevel236;
        private double _fibLevel114;
        
        public StandardDeviationBandCalculator(double stdDevMultiplier)
        {
            _stdDevMultiplier = stdDevMultiplier;
            Reset();
        }
        
        public void ProcessBar(int index, double price, double volume, double vwap)
        {
            if (volume == 0)
                return;
                
            // Update sum of squared distances for standard deviation
            _sumSquaredDistances += volume * Math.Pow(price - vwap, 2);
            _cumulativeVolume += volume;
            
            // Calculate standard deviation
            double variance = _cumulativeVolume > 0 ? _sumSquaredDistances / _cumulativeVolume : 0;
            double stdDev = Math.Sqrt(variance);
            
            // Use standard deviation as the band width factor for Fibonacci levels
            double bandFactor = stdDev * _stdDevMultiplier;
            
            // Calculate Fibonacci levels with VWAP as the 50% level
            _upperBand = vwap + bandFactor;            // 100% level
            _lowerBand = vwap - bandFactor;            // 0% level
            
            // Additional Fibonacci levels
            _fibLevel886 = vwap + (0.772 * bandFactor);  // 88.6% level
            _fibLevel764 = vwap + (0.528 * bandFactor);  // 76.4% level
            _fibLevel628 = vwap + (0.256 * bandFactor);  // 62.8% level
            _fibLevel382 = vwap - (0.256 * bandFactor);  // 38.2% level
            _fibLevel236 = vwap - (0.528 * bandFactor);  // 23.6% level
            _fibLevel114 = vwap - (0.772 * bandFactor);  // 11.4% level
        }
        
        public void Reset()
        {
            _sumSquaredDistances = 0;
            _cumulativeVolume = 0;
            
            _upperBand = 0;
            _lowerBand = 0;
            _fibLevel886 = 0;
            _fibLevel764 = 0;
            _fibLevel628 = 0;
            _fibLevel382 = 0;
            _fibLevel236 = 0;
            _fibLevel114 = 0;
        }
        
        public void UpdateParameters(VwapResetPeriod resetPeriod, int pivotDepth, DateTime? anchorPoint)
        {
            // This calculator doesn't use these parameters, so we don't need to do anything
        }
        
        /// <summary>
        /// Update the standard deviation multiplier
        /// </summary>
        public void UpdateStdDevMultiplier(double stdDevMultiplier)
        {
            _stdDevMultiplier = stdDevMultiplier;
        }
        
        public double GetUpperBand() => _upperBand;
        public double GetLowerBand() => _lowerBand;
        public double GetFibLevel886() => _fibLevel886;
        public double GetFibLevel764() => _fibLevel764;
        public double GetFibLevel628() => _fibLevel628;
        public double GetFibLevel382() => _fibLevel382;
        public double GetFibLevel236() => _fibLevel236;
        public double GetFibLevel114() => _fibLevel114;
    }
}
