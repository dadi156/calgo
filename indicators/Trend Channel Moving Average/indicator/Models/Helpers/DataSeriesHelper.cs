using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Helper class - now simplified to work with Source parameter
    /// </summary>
    public class DataSeriesHelper
    {
        /// <summary>
        /// Get the source data series directly
        /// No need for type conversion - Source is already DataSeries
        /// </summary>
        public DataSeries GetSourceSeries(DataSeries source)
        {
            return source;
        }
    }
}
