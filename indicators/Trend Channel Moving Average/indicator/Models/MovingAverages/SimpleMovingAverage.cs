using cAlgo.API;

namespace cAlgo
{
    /// <summary>
    /// Simple Moving Average (SMA)
    /// </summary>
    public class SimpleMovingAverage : IMovingAverage
    {
        /// <summary>
        /// Calculate SMA using traditional method
        /// </summary>
        public double Calculate(DataSeries prices, int index, int period)
        {
            if (index < period - 1)
                return double.NaN;

            if (index < 0 || index >= prices.Count)
                return double.NaN;

            double sum = 0;
            
            for (int i = 0; i < period; i++)
            {
                int priceIndex = index - i;
                
                if (priceIndex < 0 || priceIndex >= prices.Count)
                    return double.NaN;
                
                sum += prices[priceIndex];
            }

            return sum / period;
        }
    }
}
