using System;
using cAlgo.API;

namespace cAlgo
{
    public class TriangularHullMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        
        public TriangularHullMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Nothing specific to initialize
        }
        
        public MAResult Calculate(int index)
        {
            // If not enough data to calculate, return current price
            if (index < _indicator.Period)
            {
                if (index >= 0)
                {
                    return new MAResult(_indicator.Source[index]);
                }
                return new MAResult(0);
            }
            
            // Calculate TriangularHull using component WMAs
            int len1 = Math.Max(1, _indicator.Period / 3);
            int len2 = Math.Max(1, _indicator.Period / 2);
            
            // Calculate the component WMAs
            double wma1 = CalculateWMA(_indicator.Source, index, len1);
            double wma2 = CalculateWMA(_indicator.Source, index, len2);
            double wma3 = CalculateWMA(_indicator.Source, index, _indicator.Period);
            
            // Calculate intermediate value: 3*WMA(length/3) - WMA(length/2) - WMA(length)
            double intermediate = (wma1 * 3) - wma2 - wma3;
            
            // Apply final WMA to the intermediate value
            double[] tempValues = new double[_indicator.Period];
            for (int i = 0; i < _indicator.Period; i++)
            {
                int pastIndex = index - i;
                
                if (pastIndex < _indicator.Period)
                {
                    tempValues[i] = _indicator.Source[pastIndex];
                }
                else
                {
                    double pastWma1 = CalculateWMA(_indicator.Source, pastIndex, len1);
                    double pastWma2 = CalculateWMA(_indicator.Source, pastIndex, len2);
                    double pastWma3 = CalculateWMA(_indicator.Source, pastIndex, _indicator.Period);
                    
                    tempValues[i] = (pastWma1 * 3) - pastWma2 - pastWma3;
                }
            }
            
            // Calculate final WMA on the intermediate values
            double thma = CalculateWMAFromArray(tempValues, _indicator.Period);
            
            return new MAResult(thma);
        }
        
        // Weighted Moving Average calculation
        private double CalculateWMA(DataSeries series, int index, int period)
        {
            if (period <= 0)
                period = 1;
                
            if (index < period)
                return series[index];
                
            double sumWeightedValues = 0;
            double sumWeights = 0;
            
            for (int i = 0; i < period; i++)
            {
                int weight = period - i;
                sumWeightedValues += series[index - i] * weight;
                sumWeights += weight;
            }
            
            return sumWeightedValues / sumWeights;
        }
        
        // Helper method to calculate WMA from an array
        private double CalculateWMAFromArray(double[] values, int period)
        {
            double sumWeightedValues = 0;
            double sumWeights = 0;
            
            for (int i = 0; i < period; i++)
            {
                int weight = period - i;
                sumWeightedValues += values[i] * weight;
                sumWeights += weight;
            }
            
            return sumWeightedValues / sumWeights;
        }
    }
}
