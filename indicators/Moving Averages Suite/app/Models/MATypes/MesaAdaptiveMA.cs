using System;
using cAlgo.API;

namespace cAlgo
{
    public class MesaAdaptiveMA : MAInterface
    {
        private readonly MovingAveragesSuite _indicator;
        private double[] _price;
        private double[] _smooth;
        private double[] _detrender;
        private double[] _i1;
        private double[] _q1;
        private double[] _i2;
        private double[] _q2;
        private double[] _re;
        private double[] _im;
        private double[] _period;
        private double[] _smoothPeriod;
        private double[] _phase;
        private double[] _deltaPhase;
        private double[] _alpha;
        private double[] _mama;
        private double[] _fama;
        private bool _initialized;
        
        // Used for phase calculations
        private double _jI = 0, _jQ = 0;
        
        public MesaAdaptiveMA(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
        }
        
        public void Initialize()
        {
            // Pre-allocate memory for arrays
            int initialSize = 10000;
            _price = new double[initialSize];
            _smooth = new double[initialSize];
            _detrender = new double[initialSize];
            _i1 = new double[initialSize];
            _q1 = new double[initialSize];
            _i2 = new double[initialSize];
            _q2 = new double[initialSize];
            _re = new double[initialSize];
            _im = new double[initialSize];
            _period = new double[initialSize];
            _smoothPeriod = new double[initialSize];
            _phase = new double[initialSize];
            _deltaPhase = new double[initialSize];
            _alpha = new double[initialSize];
            _mama = new double[initialSize];
            _fama = new double[initialSize];
            _initialized = false;
        }
        
        public MAResult Calculate(int index)
        {
            // Skip calculation for the first few bars
            if (index < 7)
            {
                if (index >= 0)
                {
                    return new MAResult(_indicator.Source[index], _indicator.Source[index]);
                }
                return new MAResult(0, 0);
            }
            
            // Ensure arrays have sufficient size
            EnsureArraySize(index);
            
            // Store price
            _price[index] = _indicator.Source[index];
            
            // For first run, initialize the arrays with starting values
            if (!_initialized && index >= 7)
            {
                for (int i = 0; i < 7; i++)
                {
                    _price[i] = _indicator.Source[i];
                    _smooth[i] = _indicator.Source[i];
                    _detrender[i] = 0;
                    _i1[i] = 0;
                    _q1[i] = 0;
                    _i2[i] = 0;
                    _q2[i] = 0;
                    _re[i] = 0;
                    _im[i] = 0;
                    _period[i] = 0;
                    _smoothPeriod[i] = 0;
                    _phase[i] = 0;
                    _deltaPhase[i] = 0;
                    _alpha[i] = 0;
                    _mama[i] = _indicator.Source[i];
                    _fama[i] = _indicator.Source[i];
                }
                _initialized = true;
            }
            
            // Apply Ehlers' algorithm
            
            // Smooth price using 4-period weighted moving average
            _smooth[index] = (4 * _price[index] + 3 * _price[index - 1] + 2 * _price[index - 2] + _price[index - 3]) / 10.0;
            
            // Calculate Hilbert Transform components
            // Detrender is a bandpass filter
            _detrender[index] = (0.0962 * _smooth[index] + 0.5769 * _smooth[index - 2] - 0.5769 * _smooth[index - 4] - 0.0962 * _smooth[index - 6]) * (0.075 * _period[index - 1] + 0.54);
            
            // Compute InPhase and Quadrature components
            _q1[index] = (0.0962 * _detrender[index] + 0.5769 * _detrender[index - 2] - 0.5769 * _detrender[index - 4] - 0.0962 * _detrender[index - 6]) * (0.075 * _period[index - 1] + 0.54);
            _i1[index] = _detrender[index - 3];
            
            // Advance the phase by 90 degrees
            _jI = (0.0962 * _i1[index] + 0.5769 * _i1[index - 2] - 0.5769 * _i1[index - 4] - 0.0962 * _i1[index - 6]) * (0.075 * _period[index - 1] + 0.54);
            _jQ = (0.0962 * _q1[index] + 0.5769 * _q1[index - 2] - 0.5769 * _q1[index - 4] - 0.0962 * _q1[index - 6]) * (0.075 * _period[index - 1] + 0.54);
            
            // Calculate Real and Imaginary components with homodyne discriminator
            _i2[index] = _i1[index] - _jQ;
            _q2[index] = _q1[index] + _jI;
            
            // Calculate period and phase
            _re[index] = 0.2 * (_i2[index] * _i2[index - 1] + _q2[index] * _q2[index - 1]) + 0.8 * _re[index - 1];
            _im[index] = 0.2 * (_i2[index] * _q2[index - 1] - _q2[index] * _i2[index - 1]) + 0.8 * _im[index - 1];
            
            // Avoid division by zero
            if (Math.Abs(_re[index]) > 0.0001)
                _period[index] = 2 * Math.PI / Math.Atan2(_im[index], _re[index]);
            else
                _period[index] = _period[index - 1];
            
            // Limit period to reasonable range
            if (_period[index] > 50)
                _period[index] = 50;
            if (_period[index] < 6)
                _period[index] = 6;
            
            // Smooth the period estimate
            _smoothPeriod[index] = 0.2 * _period[index] + 0.8 * _smoothPeriod[index - 1];
            
            if (_im[index] != 0 && _re[index] != 0)
                _phase[index] = Math.Atan2(_im[index], _re[index]);
            else
                _phase[index] = 0;
            
            // Calculate delta phase
            double deltaPhase = _phase[index] - _phase[index - 1];
            if (deltaPhase < 0)
                deltaPhase += 2 * Math.PI;
            
            // Adjust delta phase for rate of change
            _deltaPhase[index] = 0.2 * deltaPhase + 0.8 * _deltaPhase[index - 1];
            
            // Calculate alpha (adaptation factor)
            if (_smoothPeriod[index] > 0)
                _alpha[index] = _indicator.FastLimit / _smoothPeriod[index] * _deltaPhase[index];
            else
                _alpha[index] = _indicator.FastLimit;
            
            // Limit alpha between slow and fast limits
            if (_alpha[index] < _indicator.SlowLimit)
                _alpha[index] = _indicator.SlowLimit;
            if (_alpha[index] > _indicator.FastLimit)
                _alpha[index] = _indicator.FastLimit;
            
            // Calculate MESAAdaptive and FAMA
            _mama[index] = _alpha[index] * _price[index] + (1 - _alpha[index]) * _mama[index - 1];
            _fama[index] = 0.5 * _alpha[index] * _mama[index] + (1 - 0.5 * _alpha[index]) * _fama[index - 1];
            
            // Return both MESAAdaptive and FAMA values
            return new MAResult(_mama[index], _fama[index]);
        }
        
        private void EnsureArraySize(int index)
        {
            if (index >= _price.Length)
            {
                // Double the array size
                int newSize = _price.Length * 2;
                Array.Resize(ref _price, newSize);
                Array.Resize(ref _smooth, newSize);
                Array.Resize(ref _detrender, newSize);
                Array.Resize(ref _i1, newSize);
                Array.Resize(ref _q1, newSize);
                Array.Resize(ref _i2, newSize);
                Array.Resize(ref _q2, newSize);
                Array.Resize(ref _re, newSize);
                Array.Resize(ref _im, newSize);
                Array.Resize(ref _period, newSize);
                Array.Resize(ref _smoothPeriod, newSize);
                Array.Resize(ref _phase, newSize);
                Array.Resize(ref _deltaPhase, newSize);
                Array.Resize(ref _alpha, newSize);
                Array.Resize(ref _mama, newSize);
                Array.Resize(ref _fama, newSize);
            }
        }
    }
}
