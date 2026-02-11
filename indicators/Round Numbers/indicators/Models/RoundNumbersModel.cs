using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class RoundNumbersModel
    {
        private readonly Symbol _symbol;
        private readonly List<double> _priceLevels;

        public IReadOnlyList<double> PriceLevels => _priceLevels;

        public RoundNumbersModel(Symbol symbol)
        {
            _symbol = symbol;
            _priceLevels = new List<double>();
        }

        public void CalculatePriceLevels(double currentPrice, int multiples, int numberOfLevels)
        {
            _priceLevels.Clear();

            double pipValue = _symbol.PipSize * multiples;

            // Find the closest round price level to current price
            double lowerRoundPrice = Math.Floor(currentPrice / pipValue) * pipValue;
            double upperRoundPrice = Math.Ceiling(currentPrice / pipValue) * pipValue;

            // Determine which one is closer
            double startingPrice;
            if (Math.Abs(currentPrice - lowerRoundPrice) <= Math.Abs(currentPrice - upperRoundPrice))
                startingPrice = lowerRoundPrice;
            else
                startingPrice = upperRoundPrice;

            // Calculate how many levels below and above
            int levelsBelow = numberOfLevels / 2;
            int levelsAbove = numberOfLevels - levelsBelow;

            // Create price levels
            for (int i = -levelsBelow; i < levelsAbove; i++)
            {
                double price = startingPrice + (i * pipValue);
                _priceLevels.Add(price);
            }
        }
    }
}
