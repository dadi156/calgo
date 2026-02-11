using cAlgo.API;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None, AutoRescale = false)]

    [Cloud("Fib 62.8%", "Fib 38.2%", Opacity = 0.25)]

    public partial class AnchoredMovingAverage : Indicator
    {
        
        // MVC components
        private MAController controller;
        private MAModel model;
        private MAView view;

        protected override void Initialize()
        {
            // Create all MVC parts
            CreateMVCComponents();
            
            // Get anchor datetime (dynamic or manual)
            string anchorDateTime = GetFinalAnchorDateTimeString();
            
            // Start controller with calculated anchor datetime
            controller.Initialize(anchorDateTime);
            
            // Set initial band settings (UPDATED: BandRange instead of PivotDepth)
            controller.UpdateBandSettings(BandVisibility, BandRange);
        }

        public override void Calculate(int index)
        {
            // Let controller do all work (UPDATED: BandRange instead of PivotDepth)
            controller.Calculate(index, Bars, Source, MaType, Indicators, BandVisibility, BandRange, MaxPeriod);
        }
        
        /// <summary>
        /// Create all MVC parts (pass band series to view)
        /// </summary>
        private void CreateMVCComponents()
        {
            // Create Model
            model = new MAModel();
            
            // Create View with MA series and all band series
            view = new MAView(
                Result,                 // MA result
                UpperBand,              // Upper band (100%)
                LowerBand,              // Lower band (0%)
                FiboLevel886,           // 88.6% level
                FiboLevel764,           // 76.4% level
                FiboLevel628,           // 62.8% level
                FiboLevel382,           // 38.2% level
                FiboLevel236,           // 23.6% level
                FiboLevel114            // 11.4% level
            );
            
            // Create Controller
            controller = new MAController(model, view);
        }
    }
}
