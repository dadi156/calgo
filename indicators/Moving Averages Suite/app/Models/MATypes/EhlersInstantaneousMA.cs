using System;
using cAlgo.API;

namespace cAlgo
{
    public class EhlersInstantaneousMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _trendline;
        private double[] _smooth;
        private double[] _cycle;
        private double[] _detrender;
        private double[] _q1;
        private double[] _i1;
        private int _lastPeriod;
        private bool _initialized;
        
        public EhlersInstantaneousMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _trendline = new double[initialSize];
            _smooth = new double[initialSize];
            _cycle = new double[initialSize];
            _detrender = new double[initialSize];
            _q1 = new double[initialSize];
            _i1 = new double[initialSize];
            _lastPeriod = 0;
            _initialized = false;
        }
        
        public MAResult Calculate(int index)
        {
            // If not enough data to calculate, return current price
            if (index < 7)
            {
                if (index >= 0)
                {
                    return new MAResult(_indicator.Source[index]);
                }
                return new MAResult(0);
            }
            
            // Ensure arrays have sufficient size
            EnsureArraySize(index);
            
            // If period changed, we need to reinitialize
            if (_lastPeriod != _indicator.Period)
            {
                _initialized = false;
                _lastPeriod = _indicator.Period;
            }
            
            // Setup phase variables for the filter
            double a1 = 0.0, a2 = 0.0;
            double coef1 = 0.0, coef2 = 0.0, coef3 = 0.0;
            
            // Cycle period defaulting to input period or 10 if auto-detect
            double cyclePeriod = _indicator.Period;
            
            // Calculate filter coefficients based on period
            a1 = Math.Exp(-1.414 * Math.PI / cyclePeriod);
            a2 = 2.0 * a1 * Math.Cos(1.414 * Math.PI / cyclePeriod);
            coef2 = a2;
            coef3 = -a1 * a1;
            coef1 = 1.0 - coef2 - coef3;
            
            // For first run, initialize the arrays with starting values
            if (!_initialized && index >= 7)
            {
                for (int i = 0; i < 7; i++)
                {
                    _trendline[i] = _indicator.Source[i];
                    _smooth[i] = _indicator.Source[i];
                    _detrender[i] = 0;
                    _q1[i] = 0;
                    _i1[i] = 0;
                    _cycle[i] = 0;
                }
                _initialized = true;
            }
            
            // Apply Super Smoother Filter to price
            _smooth[index] = coef1 * (_indicator.Source[index] + _indicator.Source[index-1]) / 2.0 + 
                             coef2 * _smooth[index-1] + 
                             coef3 * _smooth[index-2];
            
            // Calculate Hilbert Transform components
            _detrender[index] = (0.0962 * _smooth[index] + 
                                0.5769 * _smooth[index - 2] - 
                                0.5769 * _smooth[index - 4] - 
                                0.0962 * _smooth[index - 6]) * (0.075 * cyclePeriod + 0.54);
                    
            // Get in-phase and quadrature components
            _q1[index] = (0.0962 * _detrender[index] + 
                         0.5769 * _detrender[index - 2] - 
                         0.5769 * _detrender[index - 4] - 
                         0.0962 * _detrender[index - 6]) * (0.075 * cyclePeriod + 0.54);
            
            _i1[index] = _detrender[index - 3];
            
            // Calculate the dominant cycle
            _cycle[index] = 0.2 * (_i1[index] * _i1[index] + _q1[index] * _q1[index]) + 0.8 * _cycle[index - 1];
            
            // Calculate instantaneous trendline with adjusted alpha
            _trendline[index] = _indicator.Alpha * _indicator.Source[index] + (1.0 - _indicator.Alpha) * _trendline[index - 1];
            
            return new MAResult(_trendline[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _trendline.Length)
            {
                // Double the array size
                int newSize = _trendline.Length * 2;
                Array.Resize(ref _trendline, newSize);
                Array.Resize(ref _smooth, newSize);
                Array.Resize(ref _cycle, newSize);
                Array.Resize(ref _detrender, newSize);
                Array.Resize(ref _q1, newSize);
                Array.Resize(ref _i1, newSize);
            }
        }
    }
}
