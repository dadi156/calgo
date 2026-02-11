using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Holds the calculated channel data for a specific bar
    /// </summary>
    public class ChannelData
    {
        /// <summary>
        /// Array of Fibonacci level values 
        /// [1.0, 0.886, 0.764, 0.618, 0.5, 0.382, 0.236, 0.114, 0.0]
        /// </summary>
        public double[] FibonacciLevels { get; }

        /// <summary>
        /// Dictionary containing Fibonacci levels for each bar in the calculation window
        /// Key: bar index, Value: array of Fibonacci levels for that bar
        /// </summary>
        public Dictionary<int, double[]> WindowLevels { get; }

        /// <summary>
        /// Middle value of the regression (50% level)
        /// </summary>
        public double MiddleValue => FibonacciLevels[4]; // 50% level

        /// <summary>
        /// Upper value of the regression channel (100% level)
        /// </summary>
        public double UpperValue => FibonacciLevels[0]; // 100% level

        /// <summary>
        /// Lower value of the regression channel (0% level) 
        /// </summary>
        public double LowerValue => FibonacciLevels[8]; // 0% level

        /// <summary>
        /// Channel offset (half of channel height)
        /// </summary>
        public double ChannelOffset { get; }

        /// <summary>
        /// Index of the bar this data corresponds to
        /// </summary>
        public int BarIndex { get; }

        /// <summary>
        /// Raw regression coefficients
        /// </summary>
        public double[] RegressionCoefficients { get; }

        /// <summary>
        /// Standard deviation used for channel calculation
        /// </summary>
        public double StandardDeviation { get; }

        public ChannelData(int barIndex, double[] fibonacciLevels, Dictionary<int, double[]> windowLevels,
                          double channelOffset, double[] regressionCoefficients, double standardDeviation)
        {
            BarIndex = barIndex;
            FibonacciLevels = fibonacciLevels;
            WindowLevels = windowLevels ?? new Dictionary<int, double[]>();
            ChannelOffset = channelOffset;
            RegressionCoefficients = regressionCoefficients;
            StandardDeviation = standardDeviation;
        }
    }
}
