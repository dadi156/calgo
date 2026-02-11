// RegimeDetector - Detects regime changes
namespace cAlgo.Indicators
{
    // Detects regime changes based on price crossing trails
    public class RegimeDetector
    {
        // State variables
        private int _regime;
        private int _bullCount;
        private int _bearCount;

        public RegimeDetector()
        {
            _regime = 0;      // Start neutral
            _bullCount = 0;
            _bearCount = 0;
        }

        // Detect regime based on price and trails
        public int DetectRegime(double closePrice, double trailLong, double trailShort, int confirmBars)
        {
            // Check conditions
            bool aboveShort = closePrice > trailShort;
            bool belowLong = closePrice < trailLong;

            // Update counters
            if (aboveShort)
            {
                _bullCount++;
            }
            else
            {
                _bullCount = 0;
            }

            if (belowLong)
            {
                _bearCount++;
            }
            else
            {
                _bearCount = 0;
            }

            // Update regime with confirmation
            if (_regime == 0) // Neutral
            {
                if (_bullCount >= confirmBars)
                {
                    _regime = 1;  // Switch to Bull
                }
                else if (_bearCount >= confirmBars)
                {
                    _regime = -1; // Switch to Bear
                }
            }
            else if (_regime == 1) // Bull
            {
                if (_bearCount >= confirmBars)
                {
                    _regime = -1; // Flip to Bear
                }
            }
            else if (_regime == -1) // Bear
            {
                if (_bullCount >= confirmBars)
                {
                    _regime = 1;  // Flip to Bull
                }
            }

            return _regime;
        }

        // Get current regime (for external queries)
        public int GetCurrentRegime()
        {
            return _regime;
        }
    }
}
