using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Calculates Fibonacci levels based on previous period's high/low price range
    /// </summary>
    public class HighLowBandCalculator : BaseBandCalculator
    {
        // Store the band width from previous period, not absolute values
        private double _highLowBandWidth;
        
        // Current period tracking
        private double _currentPeriodHigh;
        private double _currentPeriodLow;
        
        public HighLowBandCalculator(Bars bars, VwapResetPeriod resetPeriod, DateTime? anchorPoint = null)
            : base(bars, resetPeriod, anchorPoint)
        {
            // Initialize with default values
            _highLowBandWidth = 0;
            _currentPeriodHigh = double.MinValue;
            _currentPeriodLow = double.MaxValue;
        }
        
        protected override void OnPeriodChange(int index)
        {
            // Calculate and save the band width from the previous period
            if (_currentPeriodHigh > _currentPeriodLow)
            {
                _highLowBandWidth = _currentPeriodHigh - _currentPeriodLow;
                HasCompletedOnePeriod = true;
            }
            
            // Reset current period tracking
            _currentPeriodHigh = Bars.HighPrices[index];
            _currentPeriodLow = Bars.LowPrices[index];
        }
        
        protected override void ProcessBarInPeriod(int index, double price, double volume, bool isNewPeriod)
        {
            // Update current period high and low
            _currentPeriodHigh = Math.Max(_currentPeriodHigh, Bars.HighPrices[index]);
            _currentPeriodLow = Math.Min(_currentPeriodLow, Bars.LowPrices[index]);
            
            // If we haven't completed a full period yet, use current period as the band width
            if (!HasCompletedOnePeriod && _currentPeriodHigh > _currentPeriodLow)
            {
                _highLowBandWidth = _currentPeriodHigh - _currentPeriodLow;
            }
        }
        
        public override void Reset()
        {
            // Reset period tracking
            _highLowBandWidth = 0;
            _currentPeriodHigh = double.MinValue;
            _currentPeriodLow = double.MaxValue;
            HasCompletedOnePeriod = false;
        }
        
        // Calculate band values on-demand based on the current VWAP and stored band width
        public override double GetUpperBand() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap + halfBandWidth;
        }
        
        public override double GetLowerBand() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap - halfBandWidth;
        }
        
        public override double GetFibLevel886() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap + (0.772 * halfBandWidth);
        }
        
        public override double GetFibLevel764() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap + (0.528 * halfBandWidth);
        }
        
        public override double GetFibLevel628() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap + (0.256 * halfBandWidth);
        }
        
        public override double GetFibLevel382() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap - (0.236 * halfBandWidth);
        }
        
        public override double GetFibLevel236() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap - (0.528 * halfBandWidth);
        }
        
        public override double GetFibLevel114() 
        {
            if (_highLowBandWidth <= 0)
                return CurrentVwap;
                
            double halfBandWidth = _highLowBandWidth / 2;
            return CurrentVwap - (0.772 * halfBandWidth);
        }
    }
}
