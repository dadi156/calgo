using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo
{
    public class FibonacciWeightedMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private List<long> _fibonacciWeights;
        
        public FibonacciWeightedMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
            _fibonacciWeights = new List<long>();
        }
        
        public void Initialize()
        {
            // Generate Fibonacci sequence for the given period
            GenerateFibonacciWeights();
        }
        
        public MAResult Calculate(int index)
        {
            int period = _indicator.Period;
            
            // Ensure we have enough bars
            if (index < period - 1)
                return new MAResult(double.NaN);
                
            double sum = 0;
            long weightSum = 0;
            
            // Calculate weighted sum using Fibonacci weights
            for (int i = 0; i < period; i++)
            {
                int reverseIdx = period - 1 - i; // To give more weight to recent values
                long weight = _fibonacciWeights[i];
                
                sum += _indicator.Source[index - reverseIdx] * weight;
                weightSum += weight;
            }
            
            // Return the weighted average
            return new MAResult(sum / weightSum);
        }
        
        private void GenerateFibonacciWeights()
        {
            _fibonacciWeights.Clear();
            
            int period = _indicator.Period;
            
            // Start with classic Fibonacci sequence
            // First two Fibonacci numbers
            _fibonacciWeights.Add(1);
            if (period > 1)
                _fibonacciWeights.Add(1);
            
            // Generate the rest of the sequence up to the required period
            for (int i = 2; i < period; i++)
            {
                _fibonacciWeights.Add(_fibonacciWeights[i-1] + _fibonacciWeights[i-2]);
            }
            
            // For very large periods, check if we've exceeded long's capacity
            // and normalize the weights if needed
            if (period > 75)
            {
                NormalizeWeights();
            }
        }
        
        private void NormalizeWeights()
        {
            // This method would be called for extremely large periods
            // to prevent potential long overflow
            
            // Find the smallest non-zero weight
            long minWeight = long.MaxValue;
            foreach (long weight in _fibonacciWeights)
            {
                if (weight > 0 && weight < minWeight)
                    minWeight = weight;
            }
            
            // If we have very large weights, scale them down
            if (minWeight > 1)
            {
                for (int i = 0; i < _fibonacciWeights.Count; i++)
                {
                    // Ensure we don't lose the smallest weights
                    _fibonacciWeights[i] = Math.Max(1, _fibonacciWeights[i] / minWeight);
                }
            }
        }
    }
}
