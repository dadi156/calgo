using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Base class for band calculators with common functionality
    /// </summary>
    public abstract class BaseBandCalculator : IBandCalculator
    {
        protected readonly Bars Bars;
        protected VwapResetPeriod ResetPeriod;
        protected DateTime? AnchorPoint;
        
        // Current VWAP value (updated with each call to ProcessBar)
        protected double CurrentVwap;
        
        // Period tracking
        protected DateTime CurrentPeriodStart;
        protected bool HasCompletedOnePeriod;
        
        protected BaseBandCalculator(Bars bars, VwapResetPeriod resetPeriod, DateTime? anchorPoint = null)
        {
            Bars = bars;
            ResetPeriod = resetPeriod;
            AnchorPoint = anchorPoint;
            
            // Initialize period tracking
            CurrentPeriodStart = PeriodUtility.GetPeriodStartTime(bars.OpenTimes[0], resetPeriod, anchorPoint, bars);
            HasCompletedOnePeriod = false;
            CurrentVwap = 0;
        }
        
        /// <summary>
        /// Process a single bar
        /// </summary>
        public virtual void ProcessBar(int index, double price, double volume, double vwap)
        {
            // Update current VWAP - essential for positioning bands
            CurrentVwap = vwap;
            
            DateTime currentBarTime = Bars.OpenTimes[index];
            bool isNewPeriod = PeriodUtility.IsDifferentPeriod(currentBarTime, CurrentPeriodStart, ResetPeriod, AnchorPoint, Bars);
            
            if (isNewPeriod)
            {
                // Handle the period change
                OnPeriodChange(index);
                
                // Update current period start time
                CurrentPeriodStart = PeriodUtility.GetPeriodStartTime(currentBarTime, ResetPeriod, AnchorPoint, Bars);
            }
            
            // Process the current bar
            ProcessBarInPeriod(index, price, volume, isNewPeriod);
        }
        
        /// <summary>
        /// Update calculator parameters without creating a new instance
        /// </summary>
        public virtual void UpdateParameters(VwapResetPeriod resetPeriod, int pivotDepth, DateTime? anchorPoint)
        {
            bool parametersChanged = ResetPeriod != resetPeriod || 
                                     ((AnchorPoint.HasValue != anchorPoint.HasValue) ||
                                      (AnchorPoint.HasValue && anchorPoint.HasValue && AnchorPoint.Value != anchorPoint.Value));
            
            // Update parameters
            ResetPeriod = resetPeriod;
            AnchorPoint = anchorPoint;
            
            if (parametersChanged)
            {
                // Reset the calculation state when period parameters change
                Reset();
                
                // Initialize period tracking with new parameters
                if (Bars.Count > 0)
                {
                    CurrentPeriodStart = PeriodUtility.GetPeriodStartTime(Bars.OpenTimes[0], resetPeriod, anchorPoint, Bars);
                }
            }
        }
        
        /// <summary>
        /// Called when a period change is detected
        /// </summary>
        protected abstract void OnPeriodChange(int index);
        
        /// <summary>
        /// Process a bar within the current period
        /// </summary>
        protected abstract void ProcessBarInPeriod(int index, double price, double volume, bool isNewPeriod);
        
        /// <summary>
        /// Reset calculation state
        /// </summary>
        public abstract void Reset();
        
        /// <summary>
        /// Get the upper band value (100% Fibonacci level)
        /// </summary>
        public abstract double GetUpperBand();
        
        /// <summary>
        /// Get the lower band value (0% Fibonacci level)
        /// </summary>
        public abstract double GetLowerBand();
        
        /// <summary>
        /// Get the 88.6% Fibonacci level
        /// </summary>
        public abstract double GetFibLevel886();
        
        /// <summary>
        /// Get the 76.4% Fibonacci level
        /// </summary>
        public abstract double GetFibLevel764();
        
        /// <summary>
        /// Get the 62.8% Fibonacci level
        /// </summary>
        public abstract double GetFibLevel628();
        
        /// <summary>
        /// Get the 38.2% Fibonacci level
        /// </summary>
        public abstract double GetFibLevel382();
        
        /// <summary>
        /// Get the 23.6% Fibonacci level
        /// </summary>
        public abstract double GetFibLevel236();
        
        /// <summary>
        /// Get the 11.4% Fibonacci level
        /// </summary>
        public abstract double GetFibLevel114();
    }
}
