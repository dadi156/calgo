// OutputSeriesManager - Manages bull and bear trail output lines
using cAlgo.API;

namespace cAlgo.Indicators
{
    // Manages the two output lines (Bull Trail and Bear Trail)
    public class OutputSeriesManager
    {
        private readonly IndicatorDataSeries _bullTrail;
        private readonly IndicatorDataSeries _bearTrail;

        public OutputSeriesManager(IndicatorDataSeries bullTrail, IndicatorDataSeries bearTrail)
        {
            _bullTrail = bullTrail;
            _bearTrail = bearTrail;
        }

        // Update output lines based on regime
        public void UpdateOutputLines(int index, RegimeResult result)
        {
            // Show only the active trail based on regime
            if (result.Regime == 1) // Bull regime
            {
                // Show bull trail (green)
                if (ValidationHelper.IsValidValue(result.TrailLong))
                {
                    _bullTrail[index] = result.TrailLong;
                }
                else
                {
                    _bullTrail[index] = double.NaN;
                }

                // Hide bear trail
                _bearTrail[index] = double.NaN;
            }
            else if (result.Regime == -1) // Bear regime
            {
                // Hide bull trail
                _bullTrail[index] = double.NaN;

                // Show bear trail (red)
                if (ValidationHelper.IsValidValue(result.TrailShort))
                {
                    _bearTrail[index] = result.TrailShort;
                }
                else
                {
                    _bearTrail[index] = double.NaN;
                }
            }
            else // Neutral regime (0)
            {
                // Hide both trails
                _bullTrail[index] = double.NaN;
                _bearTrail[index] = double.NaN;
            }
        }
    }
}
