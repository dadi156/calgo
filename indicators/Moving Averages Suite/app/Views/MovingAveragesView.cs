using cAlgo.API;

namespace cAlgo
{
    public class MovingAveragesView
    {
        private readonly IndicatorDataSeries _maOutput;
        private readonly IndicatorDataSeries _famaOutput;
        
        public MovingAveragesView(IndicatorDataSeries maOutput, IndicatorDataSeries famaOutput)
        {
            _maOutput = maOutput;
            _famaOutput = famaOutput;
        }
        
        public void Update(int index, MAResult result, CustomMAType maType)
        {
            // Update the MA output
            _maOutput[index] = result.MA;
            
            // Update FAMA output if available (only for MESAAdaptive)
            if (result.FAMA.HasValue && maType == CustomMAType.MESAAdaptive)
            {
                _famaOutput[index] = result.FAMA.Value;
            }
            else
            {
                // Set FAMA to NaN for non-MESAAdaptive indicators to hide the line
                _famaOutput[index] = double.NaN;
            }
        }
        
        public void ConfigureOutputVisibility(CustomMAType maType)
        {
            // For non-MESAAdaptive indicators, we'll set all previous FAMA values to Double.NaN
            // This effectively hides the line on the chart
            if (maType != CustomMAType.MESAAdaptive)
            {
                for (int i = 0; i < _famaOutput.Count; i++)
                {
                    _famaOutput[i] = double.NaN;
                }
            }
        }
    }
}
