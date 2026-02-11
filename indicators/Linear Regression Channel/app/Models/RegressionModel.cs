using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class RegressionModel
    {
        private List<OHLC> _priceData;
        private double _slope;
        private double _intercept;

        private double _upperChannelWidth;
        private double _lowerChannelWidth;
        private double _channelWidth;

        private PriceType _priceType = PriceType.Median;
        private DeviationMethod _deviationMethod = DeviationMethod.Average;
        private bool _historicalBarsOnly = true;
        private double _stdDevMultiplier = 2.0;
        private double _atrMultiplier = 1.5;

        // Deviation calculators
        private Dictionary<DeviationMethod, IDeviationCalculator> _calculators;

        public RegressionModel()
        {
            _priceData = new List<OHLC>();
            InitializeCalculators();
        }

        private void InitializeCalculators()
        {
            _calculators = new Dictionary<DeviationMethod, IDeviationCalculator>
            {
                { DeviationMethod.Average, new AverageDeviationCalculator() },
                { DeviationMethod.Maximum, new MaximumDeviationCalculator() },
                { DeviationMethod.Independent, new IndependentDeviationCalculator() },
                { DeviationMethod.StandardDeviation, new StandardDeviationCalculator() },
                { DeviationMethod.ATR, new ATRDeviationCalculator() },
                { DeviationMethod.WeightedLinear, new WeightedLinearDeviationCalculator() }
            };
        }

        public void SetPriceData(List<OHLC> priceData)
        {
            _priceData = priceData;
        }

        public void SetPriceType(PriceType priceType)
        {
            _priceType = priceType;
        }

        public void SetDeviationMethod(DeviationMethod deviationMethod)
        {
            _deviationMethod = deviationMethod;
        }

        public void SetHistoricalBarsOnly(bool historicalBarsOnly)
        {
            _historicalBarsOnly = historicalBarsOnly;
        }

        public void SetStdDevMultiplier(double multiplier)
        {
            _stdDevMultiplier = multiplier;
        }

        public void SetATRMultiplier(double multiplier)
        {
            _atrMultiplier = multiplier;
        }

        public void CalculateRegression()
        {
            if (_priceData == null || _priceData.Count < 2)
                return;

            // Sort data by time (oldest first for x-axis sequential numbering)
            var sortedData = _priceData.OrderBy(p => p.Time).ToList();

            // Create X (time periods) and Y (price) arrays for regression
            int n = sortedData.Count;
            double[] x = Enumerable.Range(0, n).Select(i => (double)i).ToArray();

            // Select price based on price type
            double[] y;
            switch (_priceType)
            {
                case PriceType.Open:
                    y = sortedData.Select(p => p.Open).ToArray();
                    break;
                case PriceType.High:
                    y = sortedData.Select(p => p.High).ToArray();
                    break;
                case PriceType.Low:
                    y = sortedData.Select(p => p.Low).ToArray();
                    break;
                case PriceType.Close:
                    y = sortedData.Select(p => p.Close).ToArray();
                    break;
                case PriceType.Median:
                    y = sortedData.Select(p => (p.High + p.Low) / 2).ToArray();
                    break;
                case PriceType.Typical:
                    y = sortedData.Select(p => (p.High + p.Low + p.Close) / 3).ToArray();
                    break;
                default:
                    y = sortedData.Select(p => (p.High + p.Low) / 2).ToArray();
                    break;
            }

            // Calculate linear regression coefficients
            CalculateLinearRegression(x, y, out _slope, out _intercept);

            // Calculate channel width using selected calculator
            if (_calculators.ContainsKey(_deviationMethod))
            {
                var calculator = _calculators[_deviationMethod];

                // Apply multiplier for StandardDeviation method
                if (_deviationMethod == DeviationMethod.StandardDeviation && calculator is StandardDeviationCalculator stdCalc)
                {
                    stdCalc.SetMultiplier(_stdDevMultiplier);
                }

                // Apply multiplier for ATR method
                if (_deviationMethod == DeviationMethod.ATR && calculator is ATRDeviationCalculator atrCalc)
                {
                    atrCalc.SetMultiplier(_atrMultiplier);
                }

                calculator.Calculate(
                    _priceData,
                    x,
                    y,
                    _slope,
                    _intercept,
                    out _upperChannelWidth,
                    out _lowerChannelWidth);

                // For backward compatibility
                _channelWidth = Math.Max(_upperChannelWidth, _lowerChannelWidth);
            }
            else
            {
                // Fallback to Average if method not found
                _calculators[DeviationMethod.Average].Calculate(
                    _priceData,
                    x,
                    y,
                    _slope,
                    _intercept,
                    out _upperChannelWidth,
                    out _lowerChannelWidth);

                _channelWidth = Math.Max(_upperChannelWidth, _lowerChannelWidth);
            }
        }

        private void CalculateLinearRegression(double[] x, double[] y, out double slope, out double intercept)
        {
            int n = x.Length;

            // Calculate means
            double meanX = x.Average();
            double meanY = y.Average();

            // Calculate intermediate sums
            double sumXY = 0;
            double sumXX = 0;

            for (int i = 0; i < n; i++)
            {
                sumXY += (x[i] - meanX) * (y[i] - meanY);
                sumXX += Math.Pow(x[i] - meanX, 2);
            }

            // Calculate slope and intercept
            slope = sumXY / sumXX;
            intercept = meanY - slope * meanX;
        }

        public RegressionChannelData GetRegressionChannelData()
        {
            if (_priceData == null || _priceData.Count < 2)
                return null;

            // Sort data by time (oldest first)
            var sortedData = _priceData.OrderBy(p => p.Time).ToList();
            int n = sortedData.Count;

            // Calculate regression line points
            DateTime startTime = sortedData.First().Time;
            DateTime endTime = sortedData.Last().Time;
            DateTime dataStartTime = sortedData.First().Time;
            DateTime dataEndTime = sortedData.Last().Time;

            // Calculate typical time difference between bars to extend properly
            if (sortedData.Count >= 2)
            {
                // Get the average time difference between the last few bars
                TimeSpan typicalDiff = new TimeSpan();
                int samplesToUse = Math.Min(3, sortedData.Count - 1);
                for (int i = 0; i < samplesToUse; i++)
                {
                    typicalDiff += sortedData[sortedData.Count - 1 - i].Time - sortedData[sortedData.Count - 2 - i].Time;
                }
                typicalDiff = new TimeSpan(typicalDiff.Ticks / samplesToUse);

                // Extension logic based on HistoricalBarsOnly setting
                if (_historicalBarsOnly)
                {
                    endTime = endTime.Add(typicalDiff).Add(typicalDiff);
                }
                else
                {
                    endTime = endTime.Add(typicalDiff);
                }
            }

            // Calculate y-values for regression points
            double startY = _intercept;
            double endY = _slope * (n - 1) + _intercept;

            // Calculate upper and lower channel points using separate widths
            double upperStartY = startY + _upperChannelWidth;
            double upperEndY = endY + _upperChannelWidth;

            double lowerStartY = startY - _lowerChannelWidth;
            double lowerEndY = endY - _lowerChannelWidth;

            // Return all calculated data
            return new RegressionChannelData
            {
                Slope = _slope,
                Intercept = _intercept,
                ChannelWidth = _channelWidth,
                UpperChannelWidth = _upperChannelWidth,
                LowerChannelWidth = _lowerChannelWidth,
                DeviationMethod = _deviationMethod,
                MidLineStart = new ChartPoint { Time = startTime, Price = startY },
                MidLineEnd = new ChartPoint { Time = endTime, Price = endY },
                UpperLineStart = new ChartPoint { Time = startTime, Price = upperStartY },
                UpperLineEnd = new ChartPoint { Time = endTime, Price = upperEndY },
                LowerLineStart = new ChartPoint { Time = startTime, Price = lowerStartY },
                LowerLineEnd = new ChartPoint { Time = endTime, Price = lowerEndY },
                DataStartTime = dataStartTime,
                DataEndTime = dataEndTime,
                BarCount = n
            };
        }
    }

    public class RegressionChannelData
    {
        public double Slope { get; set; }
        public double Intercept { get; set; }
        public double ChannelWidth { get; set; }
        public double UpperChannelWidth { get; set; }
        public double LowerChannelWidth { get; set; }
        public DeviationMethod DeviationMethod { get; set; }
        public ChartPoint MidLineStart { get; set; }
        public ChartPoint MidLineEnd { get; set; }
        public ChartPoint UpperLineStart { get; set; }
        public ChartPoint UpperLineEnd { get; set; }
        public ChartPoint LowerLineStart { get; set; }
        public ChartPoint LowerLineEnd { get; set; }
        public DateTime DataStartTime { get; set; }
        public DateTime DataEndTime { get; set; }
        public int BarCount { get; set; }
    }

    public class ChartPoint
    {
        public DateTime Time { get; set; }
        public double Price { get; set; }
    }
}
