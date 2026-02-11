// RegimeView - Main view coordinator
using cAlgo.API;

namespace cAlgo.Indicators
{
    // Main view - coordinates output display
    public class RegimeView
    {
        private readonly OutputSeriesManager _outputManager;

        public RegimeView(IndicatorDataSeries bullTrail, IndicatorDataSeries bearTrail)
        {
            // Create output manager
            _outputManager = new OutputSeriesManager(bullTrail, bearTrail);
        }

        // Update values on chart
        public void UpdateValues(int index, RegimeResult result)
        {
            _outputManager.UpdateOutputLines(index, result);
        }
    }
}
