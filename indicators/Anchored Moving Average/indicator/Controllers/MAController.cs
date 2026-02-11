using System;
using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Control all work. Connect Model and View.
    /// </summary>
    public class MAController
    {
        private readonly MAModel model;
        private readonly MAView view;
        
        /// <summary>
        /// Create controller with model and view
        /// </summary>
        public MAController(MAModel model, MAView view)
        {
            this.model = model;
            this.view = view;
        }
        
        /// <summary>
        /// Start controller. Set up everything.
        /// </summary>
        public bool Initialize(string startDateTimeString)
        {
            return model.Initialize(startDateTimeString);
        }
        
        /// <summary>
        /// Update band settings from parameters
        /// </summary>
        public void UpdateBandSettings(MABandVisibility bandVisibility, BandRangeLevel bandRange)
        {
            model.UpdateBandSettings(bandVisibility, bandRange);
        }
        
        /// <summary>
        /// Do calculation for one bar
        /// </summary>
        public void Calculate(int index, Bars bars, DataSeries source, dynamic maType, dynamic indicators,
            MABandVisibility bandVisibility, BandRangeLevel bandRange, int maxPeriod = 0)
        {
            if (!view.CanWriteResult())
                return;
                
            DateTime currentBarTime = bars.OpenTimes[index];
            
            if (!model.IsBarValid(currentBarTime))
            {
                view.SetInvalidValue(index);
                view.SetInvalidBandValues(index);
                return;
            }

            if (!model.FindStartBar(bars, index))
            {
                view.SetInvalidValue(index);
                view.SetInvalidBandValues(index);
                return;
            }

            if (!model.IsReady())
            {
                view.SetInvalidValue(index);
                view.SetInvalidBandValues(index);
                return;
            }

            model.UpdateBandSettings(bandVisibility, bandRange);

            double currentValue = source[index];
            double previousValue = view.GetPreviousValue(index);
            double maValue = model.CalculateMA(index, currentValue, maType, source, previousValue, indicators, bars, maxPeriod);

            view.SetValue(index, maValue);
            
            if (bandVisibility != MABandVisibility.None)
            {
                model.GetBandValues(maValue, 
                    out double upperBand, out double lowerBand,
                    out double fibo886, out double fibo764, out double fibo628,
                    out double fibo382, out double fibo236, out double fibo114);
                
                view.SetBandValues(index, upperBand, lowerBand, 
                    fibo886, fibo764, fibo628, fibo382, fibo236, fibo114);
            }
            else
            {
                view.SetInvalidBandValues(index);
            }
        }
        
        /// <summary>
        /// Reset everything
        /// </summary>
        public void Reset()
        {
            model.Reset();
        }
    }
}
