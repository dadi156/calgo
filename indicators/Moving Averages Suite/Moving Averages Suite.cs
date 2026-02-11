using cAlgo.API;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MovingAveragesSuite : Indicator
    {
        #region Common Parameters
        
        [Parameter("Type", Group = "Moving Average", DefaultValue = CustomMAType.Exponential)]
        public CustomMAType MAType { get; set; }
        
        [Parameter("Source", Group = "Moving Average", DefaultValue = "Close")]
        public DataSeries Source { get; set; }
        
        [Parameter("Period", Group = "Moving Average", DefaultValue = 14, MinValue = 1)]
        public int Period { get; set; }
        
        #endregion
        
        #region Arnaud Legoux Parameters
        
        [Parameter("Offset", Group = "Arnaud Legoux", DefaultValue = 0.85, MinValue = 0, MaxValue = 1)]
        public double Offset { get; set; }
        
        [Parameter("Sigma", Group = "Arnaud Legoux", DefaultValue = 6, MinValue = 1, MaxValue = 10)]
        public double Sigma { get; set; }
        
        #endregion
        
        #region MESA Adaptive Parameters
        
        [Parameter("Fast Limit", Group = "MESA Adaptive", DefaultValue = 0.5, MinValue = 0.01, MaxValue = 0.99)]
        public double FastLimit { get; set; }
        
        [Parameter("Slow Limit", Group = "MESA Adaptive", DefaultValue = 0.05, MinValue = 0.01, MaxValue = 0.99)]
        public double SlowLimit { get; set; }
        
        #endregion
        
        #region Jurik Parameters
        
        [Parameter("Phase", Group = "Jurik", DefaultValue = 0)]
        public double Phase { get; set; }
        
        #endregion
        
        #region Regularized Exponential Parameters
        
        [Parameter("Lambda", Group = "Regularized Exponential", DefaultValue = 0.5, MinValue = 0.001, MaxValue = 10)]
        public double Lambda { get; set; }
        
        #endregion
        
        #region Ehlers Instantaneous Parameters
        
        [Parameter("Alpha", Group = "Ehlers Instantaneous", DefaultValue = 0.07, MinValue = 0.01, MaxValue = 0.5)]
        public double Alpha { get; set; }
        
        #endregion
        
        // Output - the calculated moving average
        [Output("General MA", LineColor = "DodgerBlue", PlotType = PlotType.Line, Thickness = 1)]
        public IndicatorDataSeries MA_Result { get; set; }
        
        // Output for MESAAdaptive's FAMA (only contains values when MESAAdaptive is selected)
        [Output("MESAAdaptive's FAMA", LineColor = "Red", PlotType = PlotType.Line, Thickness = 1)]
        public IndicatorDataSeries FAMA_Result { get; set; }
        
        private MovingAveragesController _controller;
        
        protected override void Initialize()
        {
            // Initialize controller which will handle the logic
            _controller = new MovingAveragesController(this, MA_Result, FAMA_Result);
        }
        
        public override void Calculate(int index)
        {
            // Delegate calculation to the controller
            _controller.Calculate(index);
        }
    }
}
