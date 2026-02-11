using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators.App.Models
{
    // Update the PriceLevel class in VolumeProfileModel.cs
    public class PriceLevel
    {
        public double Price { get; set; }           // Lower bound
        public double UpperBound { get; set; }      // Upper bound
        public double Volume { get; set; }          // Total volume at this level
        public double BuyingVolume { get; set; }    // Volume from buying (close > open)
        public double SellingVolume { get; set; }   // Volume from selling (close < open)
        public double VolumeDelta { get; set; }     // Buying - Selling volume
        public bool IsVAH { get; set; }             // Is Value Area High
        public bool IsVAL { get; set; }             // Is Value Area Low

        // New TPO-related properties
        public int TPOCount { get; set; }           // Number of time periods at this price
        public HashSet<string> TPOLetters { get; set; }  // TPO letters for periods that visited this price
        public bool IsIBHigh { get; set; }          // Is Initial Balance High
        public bool IsIBLow { get; set; }           // Is Initial Balance Low

        public PriceLevel()
        {
            // Initialize TPO-related properties
            TPOCount = 0;
            TPOLetters = new HashSet<string>();
        }

        public override string ToString()
        {
            return string.Format("Price: {0} - {1}, Volume: {2}, Delta: {3}, TPO: {4}",
                Price, UpperBound, Volume, VolumeDelta, TPOCount);
        }
    }

    public class VolumeProfileModel
    {
        public List<PriceLevel> PriceLevelsList { get; private set; }
        public List<PriceLevel> ValueArea { get; private set; }
        public PriceLevel PointOfControl { get; private set; }
        public double HighestPrice { get; private set; }
        public double LowestPrice { get; private set; }

        private readonly int _priceLevels;
        private readonly int _valueAreaPercent;

        // TPO Period in minutes
        private int _tpoPeriodMinutes;
        private bool _calculateInitialBalance;
        private int _initialBalancePeriod;
        private double _ibHigh;
        private double _ibLow;

        public VolumeProfileModel(int priceLevels, int valueAreaPercent, int tpoPeriodMinutes = 30,
                        bool calculateInitialBalance = true, int initialBalancePeriod = 1)
        {
            _priceLevels = priceLevels;
            _valueAreaPercent = valueAreaPercent;
            _tpoPeriodMinutes = tpoPeriodMinutes;
            _calculateInitialBalance = calculateInitialBalance;
            _initialBalancePeriod = initialBalancePeriod;
            PriceLevelsList = new List<PriceLevel>();
            ValueArea = new List<PriceLevel>();
            _ibHigh = double.MinValue;
            _ibLow = double.MaxValue;
        }

        public void CalculatePriceRange(Bars bars, int startIndex, int endIndex)
        {
            // Find the highest and lowest prices in the range
            HighestPrice = double.MinValue;
            LowestPrice = double.MaxValue;

            for (int i = startIndex; i <= endIndex; i++)
            {
                HighestPrice = Math.Max(HighestPrice, bars.HighPrices[i]);
                LowestPrice = Math.Min(LowestPrice, bars.LowPrices[i]);
            }

            // Add a small buffer for visualization
            double range = HighestPrice - LowestPrice;
            double buffer = range * 0.05;
            HighestPrice += buffer;
            LowestPrice -= buffer;
        }

        public void CreatePriceLevels()
        {
            // Clear the previous list
            PriceLevelsList.Clear();

            // Calculate the height of each price level
            double priceRange = HighestPrice - LowestPrice;
            double levelHeight = priceRange / _priceLevels;

            // Create price levels from lowest to highest
            for (int i = 0; i < _priceLevels; i++)
            {
                double levelPrice = LowestPrice + (i * levelHeight);
                double upperBound = levelPrice + levelHeight;

                PriceLevelsList.Add(new PriceLevel
                {
                    Price = levelPrice,
                    UpperBound = upperBound,
                    Volume = 0,
                    IsVAH = false,
                    IsVAL = false
                });
            }
        }

        public void ProcessCandles(Bars bars, int startIndex, int endIndex)
        {
            // Reset all volume values before processing
            foreach (var level in PriceLevelsList)
            {
                level.Volume = 0;
                level.BuyingVolume = 0;
                level.SellingVolume = 0;
                level.VolumeDelta = 0;
            }

            // Loop through each candle in the range
            for (int i = startIndex; i <= endIndex; i++)
            {
                double highPrice = bars.HighPrices[i];
                double lowPrice = bars.LowPrices[i];
                double openPrice = bars.OpenPrices[i];
                double closePrice = bars.ClosePrices[i];
                double volume = bars.TickVolumes[i];

                // Determine if this is a buying or selling candle
                bool isBuying = closePrice >= openPrice;

                // Calculate percentage of volume to attribute to buying vs selling
                // A more sophisticated approach based on price action
                double buyVolume, sellVolume;

                if (isBuying)
                {
                    // For bullish candles, calculate buying pressure based on close position within range
                    double range = highPrice - lowPrice;
                    if (range == 0) // Handle flat candles
                    {
                        buyVolume = volume;
                        sellVolume = 0;
                    }
                    else
                    {
                        // More bullish when close is near high
                        double bullishFactor = (closePrice - lowPrice) / range;
                        buyVolume = volume * bullishFactor;
                        sellVolume = volume * (1 - bullishFactor);
                    }
                }
                else
                {
                    // For bearish candles, calculate selling pressure based on close position within range
                    double range = highPrice - lowPrice;
                    if (range == 0) // Handle flat candles
                    {
                        buyVolume = 0;
                        sellVolume = volume;
                    }
                    else
                    {
                        // More bearish when close is near low
                        double bearishFactor = (highPrice - closePrice) / range;
                        sellVolume = volume * bearishFactor;
                        buyVolume = volume * (1 - bearishFactor);
                    }
                }

                // Distribute volume across price levels touched by this candle
                foreach (var level in PriceLevelsList)
                {
                    // If candle touched this price level (single condition covers all cases)
                    if (highPrice >= level.Price && lowPrice <= level.UpperBound)
                    {
                        // Calculate overlap between candle range and level range
                        double overlapLow = Math.Max(lowPrice, level.Price);
                        double overlapHigh = Math.Min(highPrice, level.UpperBound);
                        double overlapRange = overlapHigh - overlapLow;
                        double candleRange = highPrice - lowPrice;

                        // Calculate proportion of candle in this level
                        double proportion = candleRange > 0 ? overlapRange / candleRange : 1.0;

                        // Distribute volume proportionally
                        level.Volume += volume * proportion;
                        level.BuyingVolume += buyVolume * proportion;
                        level.SellingVolume += sellVolume * proportion;
                        level.VolumeDelta = level.BuyingVolume - level.SellingVolume;
                    }
                }
            }
        }

        public void IdentifyPointOfControl()
        {
            double maxVolume = 0;
            PointOfControl = null;

            for (int i = 0; i < PriceLevelsList.Count; i++)
            {
                if (PriceLevelsList[i].Volume > maxVolume)
                {
                    maxVolume = PriceLevelsList[i].Volume;
                    PointOfControl = PriceLevelsList[i];
                }
            }
        }

        public void CalculateValueArea()
        {
            ValueArea.Clear();

            // Calculate total volume with loop instead of LINQ
            double totalVolume = 0;
            for (int i = 0; i < PriceLevelsList.Count; i++)
                totalVolume += PriceLevelsList[i].Volume;

            double targetVolume = totalVolume * _valueAreaPercent / 100.0;

            // Sort copy in-place instead of LINQ
            var sortedLevels = new List<PriceLevel>(PriceLevelsList);
            sortedLevels.Sort((a, b) => b.Volume.CompareTo(a.Volume));

            double accumulatedVolume = 0;

            foreach (var level in sortedLevels)
            {
                ValueArea.Add(level);
                accumulatedVolume += level.Volume;

                if (accumulatedVolume >= targetVolume)
                    break;
            }

            double valueAreaHigh = double.MinValue;
            double valueAreaLow = double.MaxValue;

            foreach (var level in ValueArea)
            {
                valueAreaHigh = Math.Max(valueAreaHigh, level.UpperBound);
                valueAreaLow = Math.Min(valueAreaLow, level.Price);
            }

            if (ValueArea.Count > 0)
            {
                double levelHeight = (HighestPrice - LowestPrice) / _priceLevels;
                double tolerance = levelHeight * 0.01;

                foreach (var level in PriceLevelsList)
                {
                    if (Math.Abs(level.Price - valueAreaLow) < tolerance)
                        level.IsVAL = true;

                    if (Math.Abs(level.UpperBound - valueAreaHigh) < tolerance)
                        level.IsVAH = true;
                }
            }
        }

        #region TPO

        // Process TPO data for the given bars
        public void ProcessTPO(Bars bars, int startIndex, int endIndex)
        {
            // Skip if no price levels defined
            if (PriceLevelsList.Count == 0)
                return;

            // Create TPO time periods
            var timePeriods = CreateTPOPeriods(bars, startIndex, endIndex);

            // Reset TPO data
            foreach (var level in PriceLevelsList)
            {
                level.TPOCount = 0;
                level.TPOLetters.Clear();
                level.IsIBHigh = false;
                level.IsIBLow = false;
            }

            // Calculate initial balance if enabled
            if (_calculateInitialBalance)
            {
                CalculateInitialBalance(bars, startIndex, endIndex);
            }

            // Process each bar
            for (int i = startIndex; i <= endIndex; i++)
            {
                double highPrice = bars.HighPrices[i];
                double lowPrice = bars.LowPrices[i];
                DateTime barTime = bars.OpenTimes[i];

                // Find the TPO letter for this bar's time
                string tpoLetter = GetTPOLetter(barTime, timePeriods);

                if (string.IsNullOrEmpty(tpoLetter))
                    continue;

                // Mark TPO for each price level the bar touched
                foreach (var level in PriceLevelsList)
                {
                    if (highPrice >= level.Price && lowPrice <= level.UpperBound)
                    {
                        // Count this as a TPO hit
                        level.TPOCount++;

                        // Add the TPO letter if not already added
                        if (!level.TPOLetters.Contains(tpoLetter))
                        {
                            level.TPOLetters.Add(tpoLetter);
                        }
                    }
                }
            }

            // Mark Initial Balance levels
            if (_calculateInitialBalance)
            {
                MarkInitialBalanceLevels();
            }
        }

        // Create TPO time periods for the given bar range
        private Dictionary<DateTime, string> CreateTPOPeriods(Bars bars, int startIndex, int endIndex)
        {
            if (startIndex > endIndex || startIndex < 0 || endIndex >= bars.Count)
                return new Dictionary<DateTime, string>();

            // Pre-calculate capacity to avoid dictionary resizing
            DateTime startTimeEst = bars.OpenTimes[startIndex];
            DateTime endTimeEst = bars.OpenTimes[endIndex];
            int estimatedPeriods = (int)((endTimeEst - startTimeEst).TotalMinutes / _tpoPeriodMinutes) + 1;
            var timePeriods = new Dictionary<DateTime, string>(Math.Max(1, estimatedPeriods));

            // Get the start time rounded down to nearest TPO period
            DateTime startTime = bars.OpenTimes[startIndex];
            startTime = RoundDownToTPOPeriod(startTime);

            // Get the end time
            DateTime endTime = bars.OpenTimes[endIndex];

            // Create time periods
            DateTime currentTime = startTime;
            char currentLetter = 'A';

            while (currentTime <= endTime)
            {
                // Add time period with the current letter
                timePeriods[currentTime] = currentLetter.ToString();

                // Move to next period
                currentTime = currentTime.AddMinutes(_tpoPeriodMinutes);

                // Move to next letter (A-Z, wraps back to A)
                if (currentLetter == 'Z')
                    currentLetter = 'A'; // Reset to A if we reach Z
                else
                    currentLetter++;
            }

            return timePeriods;
        }

        // Get the TPO letter for a given bar time
        private string GetTPOLetter(DateTime barTime, Dictionary<DateTime, string> timePeriods)
        {
            // Round down to the nearest TPO period
            DateTime periodStart = RoundDownToTPOPeriod(barTime);

            // Get the letter for this period
            return timePeriods.TryGetValue(periodStart, out string letter) ? letter : string.Empty;
        }

        // Round a time down to the nearest TPO period start
        private DateTime RoundDownToTPOPeriod(DateTime time)
        {
            int minutesInPeriod = time.Minute % _tpoPeriodMinutes;
            return time.AddMinutes(-minutesInPeriod).AddSeconds(-time.Second);
        }

        // Calculate Initial Balance (IB) high and low
        private void CalculateInitialBalance(Bars bars, int startIndex, int endIndex)
        {
            // Reset IB values
            _ibHigh = double.MinValue;
            _ibLow = double.MaxValue;

            if (startIndex > endIndex || startIndex < 0 || endIndex >= bars.Count)
                return;

            // Get the start time
            DateTime startTime = bars.OpenTimes[startIndex];

            // Calculate the end of Initial Balance period
            DateTime ibEndTime = startTime.AddHours(_initialBalancePeriod);

            // Find high and low within IB period
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (bars.OpenTimes[i] > ibEndTime)
                    break;

                _ibHigh = Math.Max(_ibHigh, bars.HighPrices[i]);
                _ibLow = Math.Min(_ibLow, bars.LowPrices[i]);
            }
        }

        // Mark price levels that correspond to Initial Balance high and low
        private void MarkInitialBalanceLevels()
        {
            if (_ibHigh == double.MinValue || _ibLow == double.MaxValue)
                return;

            foreach (var level in PriceLevelsList)
            {
                // Mark IB High
                if (_ibHigh >= level.Price && _ibHigh <= level.UpperBound)
                {
                    level.IsIBHigh = true;
                }

                // Mark IB Low
                if (_ibLow >= level.Price && _ibLow <= level.UpperBound)
                {
                    level.IsIBLow = true;
                }
            }
        }

        // Get sorted price levels by TPO count (for identifying TPO-based POC and Value Area)
        public List<PriceLevel> GetTPOSortedLevels()
        {
            return PriceLevelsList.OrderByDescending(level => level.TPOCount).ToList();
        }

        #endregion
    }
}
