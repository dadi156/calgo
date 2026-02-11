namespace cAlgo
{
    public class MovingAveragesModel
    {
        private readonly MovingAveragesSuite _indicator;
        private readonly MAFactory _maFactory;
        private MAInterface _currentMA;
        private CustomMAType _lastMAType;
        
        public MovingAveragesModel(MovingAveragesSuite indicator)
        {
            _indicator = indicator;
            _maFactory = new MAFactory();
            _lastMAType = CustomMAType.MESAAdaptive; // Default value
        }
        
        public MAResult Calculate(int index, CustomMAType maType)
        {
            // If MA type has changed or this is the first calculation, create the appropriate MA
            if (_currentMA == null || _lastMAType != maType)
            {
                _currentMA = _maFactory.CreateMA(maType, _indicator);
                _lastMAType = maType;
            }
            
            // Calculate and return the result
            return _currentMA.Calculate(index);
        }
    }
    
    // Result object to hold both MA and FAMA values (for MESAAdaptive)
    public class MAResult
    {
        public double MA { get; set; }
        public double? FAMA { get; set; }
        
        public MAResult(double ma, double? fama = null)
        {
            MA = ma;
            FAMA = fama;
        }
    }
}
