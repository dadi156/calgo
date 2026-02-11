using System;
using cAlgo.API;

namespace cAlgo
{
    public class ZeroLagEMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _emaData;
        private double[] _zlemaValues;
        private int _lag;
        
        public ZeroLagEMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _emaData = new double[initialSize];
            _zlemaValues = new double[initialSize];
            _lag = (_indicator.Period - 1) / 2;
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
            
            // Ensure arrays have sufficient size
            EnsureArraySize(index);
            
            // Calculate the zero-lag price data
            double currentPrice = _indicator.Source[index];
            double lagPrice = _indicator.Source[index - _lag];
            double zeroLagData = 2 * currentPrice - lagPrice;
            
            // Store the zero-lag data
            _emaData[index] = zeroLagData;
            
            // Calculate ZeroLag
            if (index == _indicator.Period - 1)
            {
                // Initial EMA value (SMA for first period)
                double sum = 0;
                for (int i = 0; i < _indicator.Period; i++)
                {
                    sum += _emaData[index - i];
                }
                _zlemaValues[index] = sum / _indicator.Period;
            }
            else if (index >= _indicator.Period)
            {
                // EMA formula: EMA = (K × (C - P)) + P
                // where C is the current price, P is the previous EMA, and K is the weighting multiplier
                double k = 2.0 / (_indicator.Period + 1);
                _zlemaValues[index] = (k * (_emaData[index] - _zlemaValues[index - 1])) + _zlemaValues[index - 1];
            }
            
            return new MAResult(_zlemaValues[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _emaData.Length)
            {
                // Double the array size
                int newSize = _emaData.Length * 2;
                Array.Resize(ref _emaData, newSize);
                Array.Resize(ref _zlemaValues, newSize);
            }
        }
    }
}
