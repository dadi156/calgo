using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Handle output and display
    /// </summary>
    public class MAView
    {
        private readonly IndicatorDataSeries resultSeries;
        
        private readonly IndicatorDataSeries upperBandSeries;
        private readonly IndicatorDataSeries lowerBandSeries;
        private readonly IndicatorDataSeries fibo886Series;
        private readonly IndicatorDataSeries fibo764Series;
        private readonly IndicatorDataSeries fibo628Series;
        private readonly IndicatorDataSeries fibo382Series;
        private readonly IndicatorDataSeries fibo236Series;
        private readonly IndicatorDataSeries fibo114Series;
        
        /// <summary>
        /// Create view with result series and band series
        /// </summary>
        public MAView(IndicatorDataSeries resultSeries, 
            IndicatorDataSeries upperBandSeries = null, IndicatorDataSeries lowerBandSeries = null,
            IndicatorDataSeries fibo886Series = null, IndicatorDataSeries fibo764Series = null, 
            IndicatorDataSeries fibo628Series = null, IndicatorDataSeries fibo382Series = null,
            IndicatorDataSeries fibo236Series = null, IndicatorDataSeries fibo114Series = null)
        {
            this.resultSeries = resultSeries;
            
            this.upperBandSeries = upperBandSeries;
            this.lowerBandSeries = lowerBandSeries;
            this.fibo886Series = fibo886Series;
            this.fibo764Series = fibo764Series;
            this.fibo628Series = fibo628Series;
            this.fibo382Series = fibo382Series;
            this.fibo236Series = fibo236Series;
            this.fibo114Series = fibo114Series;
        }
        
        /// <summary>
        /// Set MA value at index
        /// </summary>
        public void SetValue(int index, double value)
        {
            if (resultSeries != null)
            {
                resultSeries[index] = value;
            }
        }
        
        /// <summary>
        /// Set invalid MA value (NaN) at index
        /// </summary>
        public void SetInvalidValue(int index)
        {
            if (resultSeries != null)
            {
                resultSeries[index] = double.NaN;
            }
        }
        
        /// <summary>
        /// Set all band values at index
        /// </summary>
        public void SetBandValues(int index, double upperBand, double lowerBand,
            double fibo886, double fibo764, double fibo628,
            double fibo382, double fibo236, double fibo114)
        {
            if (upperBandSeries != null) upperBandSeries[index] = upperBand;
            if (lowerBandSeries != null) lowerBandSeries[index] = lowerBand;
            if (fibo886Series != null) fibo886Series[index] = fibo886;
            if (fibo764Series != null) fibo764Series[index] = fibo764;
            if (fibo628Series != null) fibo628Series[index] = fibo628;
            if (fibo382Series != null) fibo382Series[index] = fibo382;
            if (fibo236Series != null) fibo236Series[index] = fibo236;
            if (fibo114Series != null) fibo114Series[index] = fibo114;
        }
        
        /// <summary>
        /// Set all band values to NaN (hide lines)
        /// </summary>
        public void SetInvalidBandValues(int index)
        {
            if (upperBandSeries != null) upperBandSeries[index] = double.NaN;
            if (lowerBandSeries != null) lowerBandSeries[index] = double.NaN;
            if (fibo886Series != null) fibo886Series[index] = double.NaN;
            if (fibo764Series != null) fibo764Series[index] = double.NaN;
            if (fibo628Series != null) fibo628Series[index] = double.NaN;
            if (fibo382Series != null) fibo382Series[index] = double.NaN;
            if (fibo236Series != null) fibo236Series[index] = double.NaN;
            if (fibo114Series != null) fibo114Series[index] = double.NaN;
        }
        
        /// <summary>
        /// Check if we can write to result
        /// </summary>
        public bool CanWriteResult()
        {
            return resultSeries != null;
        }
        
        /// <summary>
        /// Get previous MA value for calculation
        /// </summary>
        public double GetPreviousValue(int index)
        {
            if (resultSeries == null || index <= 0)
            {
                return double.NaN;
            }
            
            return resultSeries[index - 1];
        }
    }
}
