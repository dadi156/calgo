using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class MAModel
    {
        // MA manager handles all OHLC lines
        private readonly MultiMAManager _maManager;
        private readonly MAParameters _parameters;

        // One smoothing manager for both SMA and EMA
        private readonly SmoothingManager _smoothingManager;

        public MAModel(int arraySize, MAParameters parameters)
        {
            _parameters = parameters;

            // Create MA manager (creates the selected MA type)
            _maManager = new MultiMAManager(parameters, arraySize);

            // Create smoothing manager for SMA or EMA only
            if (_parameters.MAType == MATypes.Simple)
            {
                // SMA uses SMA smoothing
                _smoothingManager = new SmoothingManager(
                    parameters.GeneralMASmoothPeriod,
                    arraySize,
                    SmoothingType.SMA);
            }
            else if (_parameters.MAType == MATypes.Exponential)
            {
                // EMA uses EMA smoothing
                _smoothingManager = new SmoothingManager(
                    parameters.GeneralMASmoothPeriod,
                    arraySize,
                    SmoothingType.EMA);
            }
            // Note: Wilder, DSMA, SuperSmoother, and Hull do NOT use additional smoothing
        }

        // Calculate all 6 MA lines - with appropriate smoothing
        public MAResult Calculate(int index, Bars bars)
        {
            // Get original MA result
            MAResult originalResult = _maManager.Calculate(index, bars);

            // Apply smoothing if using SMA or EMA
            if (_smoothingManager != null)
            {
                return _smoothingManager.SmoothMAResult(index, originalResult);
            }
            else
            {
                // No smoothing for DSMA and SuperSmoother
                return originalResult;
            }
        }

        public int GetPeriod()
        {
            return _parameters.GetPeriod();
        }

        // Set first values for all 4 MA
        public void InitializeFirstValues(double firstHigh, double firstLow,
                                        double firstClose, double firstOpen)
        {
            _maManager.InitializeFirstValues(firstHigh, firstLow, firstClose, firstOpen);
        }
    }
}
