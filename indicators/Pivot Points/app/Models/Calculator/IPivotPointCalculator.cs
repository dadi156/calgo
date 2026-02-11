namespace cAlgo.Indicators
{
    /// <summary>
    /// Interface for all pivot point calculators
    /// </summary>
    public interface IPivotPointCalculator
    {
        /// <summary>
        /// Calculates pivot points based on OHLC values
        /// </summary>
        /// <param name="high">High price</param>
        /// <param name="low">Low price</param>
        /// <param name="close">Close price</param>
        /// <param name="open">Open price</param>
        /// <param name="levelsToShow">Number of S/R levels to calculate</param>
        /// <returns>Calculated pivot points data</returns>
        PivotPointsData Calculate(double high, double low, double close, double open, int levelsToShow);
        
        /// <summary>
        /// Gets the name of the pivot point type
        /// </summary>
        /// <returns>Name of the pivot point type</returns>
        string GetName();
    }
}
