using cAlgo.API;

namespace cAlgo
{
    public class MovingAveragesController
    {
        private readonly MovingAveragesSuite _indicator;
        private readonly MovingAveragesModel _model;
        private readonly MovingAveragesView _view;
        
        public MovingAveragesController(MovingAveragesSuite indicator, 
                                        IndicatorDataSeries maOutput,
                                        IndicatorDataSeries famaOutput)
        {
            _indicator = indicator;
            _view = new MovingAveragesView(maOutput, famaOutput);
            _model = new MovingAveragesModel(_indicator);
        }
        
        public void Calculate(int index)
        {
            // Get selected MA type
            CustomMAType selectedMAType = _indicator.MAType;
            
            // Get calculation results from model
            var result = _model.Calculate(index, selectedMAType);
            
            // Update view with results
            _view.Update(index, result, selectedMAType);
            
            // Show/hide FAMA output based on MA type
            if (index == 0)
            {
                _view.ConfigureOutputVisibility(selectedMAType);
            }
        }
    }
}
