namespace cAlgo
{
    /// <summary>
    /// Calculates channel levels including Fibonacci levels
    /// </summary>
    public class ChannelLevelCalculator
    {
        /// <summary>
        /// Fibonacci levels to calculate
        /// </summary>
        private readonly double[] _fibonacciLevels = { 1.0, 0.886, 0.764, 0.618, 0.5, 0.382, 0.236, 0.114, 0.0 };

        /// <summary>
        /// Calculates all channel levels based on middle value and offset
        /// </summary>
        /// <param name="middleValue">Middle value of the channel (regression line)</param>
        /// <param name="channelOffset">Channel width/2</param>
        /// <returns>Array of price levels corresponding to each Fibonacci level</returns>
        public double[] CalculateLevels(double middleValue, double channelOffset)
        {
            double upperValue = middleValue + channelOffset;
            double lowerValue = middleValue - channelOffset;
            double range = upperValue - lowerValue;

            var levels = new double[_fibonacciLevels.Length];
            for (int i = 0; i < _fibonacciLevels.Length; i++)
            {
                levels[i] = lowerValue + (range * _fibonacciLevels[i]);
            }

            return levels;
        }
        
        /// <summary>
        /// Gets the array of Fibonacci level percentages used by this calculator
        /// </summary>
        public double[] GetFibonacciLevels()
        {
            return (double[])_fibonacciLevels.Clone();
        }
    }
}
