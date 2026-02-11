using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Calculates Fibonacci levels based on Fibonacci pivot points from previous period
    /// </summary>
    public class FibonacciPivotBandCalculator : BaseBandCalculator
    {
        private int _pivotDepth;
        
        // Previous complete period's data
        private double _previousPeriodHigh;
        private double _previousPeriodLow;
        private double _previousPeriodClose;
        
        // Current period tracking
        private double _currentPeriodHigh;
        private double _currentPeriodLow;
        private double _currentPeriodClose;
        
        // Calculated band width based on previous period data
        private double _calculatedBandWidth;
        
        public FibonacciPivotBandCalculator(Bars bars, VwapResetPeriod resetPeriod, int pivotDepth, DateTime? anchorPoint = null)
            : base(bars, resetPeriod, anchorPoint)
        {
            _pivotDepth = Math.Max(1, Math.Min(3, pivotDepth)); // Constrain between 1-3
            
            // Initialize with default values
            _previousPeriodHigh = 0;
            _previousPeriodLow = 0;
            _previousPeriodClose = 0;
            
            _currentPeriodHigh = double.MinValue;
            _currentPeriodLow = double.MaxValue;
            _currentPeriodClose = 0;
            
            _calculatedBandWidth = 0;
        }
        
        public override void UpdateParameters(VwapResetPeriod resetPeriod, int pivotDepth, DateTime? anchorPoint)
        {
            bool pivotDepthChanged = _pivotDepth != pivotDepth;
            
            // First call the base implementation to handle period parameters
            base.UpdateParameters(resetPeriod, pivotDepth, anchorPoint);
            
            // Then handle pivot depth specifically
            if (pivotDepthChanged)
            {
                _pivotDepth = Math.Max(1, Math.Min(3, pivotDepth)); // Constrain between 1-3
                
                // Recalculate band width if we have data
                if (_previousPeriodHigh > _previousPeriodLow)
                {
                    CalculateBandWidth();
                }
            }
        }
        
        protected override void OnPeriodChange(int index)
        {
            // When a period completes, save its data as the previous period
            if (_currentPeriodHigh > _currentPeriodLow)
            {
                _previousPeriodHigh = _currentPeriodHigh;
                _previousPeriodLow = _currentPeriodLow;
                _previousPeriodClose = _currentPeriodClose;
                
                // Calculate the band width based on the now-completed previous period
                CalculateBandWidth();
                
                HasCompletedOnePeriod = true;
            }
            
            // Reset current period tracking for the new period
            _currentPeriodHigh = Bars.HighPrices[index];
            _currentPeriodLow = Bars.LowPrices[index];
            _currentPeriodClose = Bars.ClosePrices[index];
        }
        
        protected override void ProcessBarInPeriod(int index, double price, double volume, bool isNewPeriod)
        {
            // Update current period high, low, and close
            _currentPeriodHigh = Math.Max(_currentPeriodHigh, Bars.HighPrices[index]);
            _currentPeriodLow = Math.Min(_currentPeriodLow, Bars.LowPrices[index]);
            _currentPeriodClose = Bars.ClosePrices[index];
            
            // If we haven't completed a full period yet, use current period as the reference
            if (!HasCompletedOnePeriod && _currentPeriodHigh > _currentPeriodLow)
            {
                _previousPeriodHigh = _currentPeriodHigh;
                _previousPeriodLow = _currentPeriodLow;
                _previousPeriodClose = _currentPeriodClose;
                
                CalculateBandWidth();
            }
        }
        
        /// <summary>
        /// Calculate the band width based on previous period's data using Fibonacci pivot points logic
        /// </summary>
        private void CalculateBandWidth()
        {
            // Calculate the pivot point (PP)
            double pivotPoint = (_previousPeriodHigh + _previousPeriodLow + _previousPeriodClose) / 3;
            
            // Calculate the full band width from the previous period
            double fullRange = _previousPeriodHigh - _previousPeriodLow;
            
            // Determine the band width factor based on pivot depth
            switch (_pivotDepth)
            {
                case 1:
                    _calculatedBandWidth = 0.382 * fullRange; // 38.2% of range for level 1
                    break;
                case 2:
                    _calculatedBandWidth = 0.618 * fullRange; // 61.8% of range for level 2
                    break;
                case 3:
                    _calculatedBandWidth = 1.000 * fullRange; // 100% of range for level 3
                    break;
                default:
                    _calculatedBandWidth = 0.382 * fullRange; // Default to level 1
                    break;
            }
        }
        
        public override void Reset()
        {
            // Reset all period tracking
            _previousPeriodHigh = 0;
            _previousPeriodLow = 0;
            _previousPeriodClose = 0;
            
            _currentPeriodHigh = double.MinValue;
            _currentPeriodLow = double.MaxValue;
            _currentPeriodClose = 0;
            
            _calculatedBandWidth = 0;
            HasCompletedOnePeriod = false;
        }
        
        // Calculate band values on-demand based on VWAP and calculated band width
        public override double GetUpperBand() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap + _calculatedBandWidth;
        }
        
        public override double GetLowerBand() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap - _calculatedBandWidth;
        }
        
        public override double GetFibLevel886() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap + (0.772 * _calculatedBandWidth);
        }
        
        public override double GetFibLevel764() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap + (0.528 * _calculatedBandWidth);
        }
        
        public override double GetFibLevel628() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap + (0.256 * _calculatedBandWidth);
        }
        
        public override double GetFibLevel382() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap - (0.256 * _calculatedBandWidth);
        }
        
        public override double GetFibLevel236() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap - (0.528 * _calculatedBandWidth);
        }
        
        public override double GetFibLevel114() 
        {
            if (_calculatedBandWidth <= 0)
                return CurrentVwap;
                
            return CurrentVwap - (0.772 * _calculatedBandWidth);
        }
    }
}
