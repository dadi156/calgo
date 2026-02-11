using cAlgo.API;

namespace cAlgo.Indicators
{
    public class OutputSeriesManager
    {
        // Store 7 output lines now (added Median)
        private readonly IndicatorDataSeries _highLine;
        private readonly IndicatorDataSeries _lowLine;
        private readonly IndicatorDataSeries _closeLine;
        private readonly IndicatorDataSeries _openLine;
        private readonly IndicatorDataSeries _medianLine;         // NEW: Median line
        private readonly IndicatorDataSeries _lowerReversionZone;  // Was Fib382
        private readonly IndicatorDataSeries _upperReversionZone;  // Was Fib618

        public OutputSeriesManager(IndicatorDataSeries highLine, IndicatorDataSeries lowLine, 
                                 IndicatorDataSeries closeLine, IndicatorDataSeries openLine,
                                 IndicatorDataSeries medianLine,
                                 IndicatorDataSeries lowerReversionZone, IndicatorDataSeries upperReversionZone)
        {
            _highLine = highLine;
            _lowLine = lowLine;
            _closeLine = closeLine;
            _openLine = openLine;
            _medianLine = medianLine;  // NEW
            _lowerReversionZone = lowerReversionZone;
            _upperReversionZone = upperReversionZone;
        }

        // Update all 7 output lines with new values
        public void UpdateOutputLines(int index, MAResult result)
        {
            // Update High Line
            if (ValidationHelper.IsValidValue(result.HighMA))
                _highLine[index] = result.HighMA;
            else
                HandleInvalidValue(index, _highLine);

            // Update Low Line  
            if (ValidationHelper.IsValidValue(result.LowMA))
                _lowLine[index] = result.LowMA;
            else
                HandleInvalidValue(index, _lowLine);

            // Update Close Line
            if (ValidationHelper.IsValidValue(result.CloseMA))
                _closeLine[index] = result.CloseMA;
            else
                HandleInvalidValue(index, _closeLine);

            // Update Open Line
            if (ValidationHelper.IsValidValue(result.OpenMA))
                _openLine[index] = result.OpenMA;
            else
                HandleInvalidValue(index, _openLine);

            // NEW: Update Median Line
            if (ValidationHelper.IsValidValue(result.MedianMA))
                _medianLine[index] = result.MedianMA;
            else
                HandleInvalidValue(index, _medianLine);

            // Update Lower Reversion Zone (was Fib382)
            if (ValidationHelper.IsValidValue(result.Fib382MA))
                _lowerReversionZone[index] = result.Fib382MA;
            else
                HandleInvalidValue(index, _lowerReversionZone);

            // Update Upper Reversion Zone (was Fib618)
            if (ValidationHelper.IsValidValue(result.Fib618MA))
                _upperReversionZone[index] = result.Fib618MA;
            else
                HandleInvalidValue(index, _upperReversionZone);
        }

        // Hide all output lines (when using trendlines)
        public void HideOutputLines(int index)
        {
            _highLine[index] = double.NaN;
            _lowLine[index] = double.NaN;
            _closeLine[index] = double.NaN;
            _openLine[index] = double.NaN;
            _medianLine[index] = double.NaN;  // NEW
            _lowerReversionZone[index] = double.NaN;
            _upperReversionZone[index] = double.NaN;
        }

        // Fix bad values
        private void HandleInvalidValue(int index, IndicatorDataSeries series)
        {
            if (index > 0)
            {
                series[index] = series[index - 1];  // Use previous value
            }
            else
            {
                series[index] = 0;  // Use zero for first bar
            }
        }
    }
}
