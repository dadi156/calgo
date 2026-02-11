using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// Main Indicator: Orchestrates FVG detection
    /// Multi-Timeframe: Detects on selected TF, displays on current TF
    /// </summary>
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class FairValueGap : Indicator
    {
        #region MVC Components
        
        private FVGController _controller;
        private FVGView _view;
        
        #endregion
        
        #region Initialization
        
        protected override void Initialize()
        {
            // Determine which bars to use for detection
            Bars detectionBars;
            
            // Check if MTF is enabled AND selected timeframe is higher than current timeframe
            bool canUseMTF = EnableMultiTimeframe && IsHigherTimeframe(SelectedTimeframe, TimeFrame);
            
            if (canUseMTF)
            {
                // Use selected timeframe for detection
                detectionBars = MarketData.GetBars(SelectedTimeframe);
            }
            else
            {
                // Use current timeframe for detection
                detectionBars = Bars;
            }
            
            // Initialize Controller with business logic
            // displayBars: Current TF (for mitigation checks and drawing)
            // detectionBars: Selected TF (for FVG detection)
            _controller = new FVGController(
                Bars,              // displayBars (current chart timeframe)
                detectionBars,     // detectionBars (selected timeframe or current)
                Symbol, 
                LookbackPeriod, 
                MinimumSizePips,
                DisplayMode,
                PartialFillThreshold
            );
            
            // Initialize View with drawing capabilities
            _view = new FVGView(
                Chart,
                Bars,              // displayBars (for time-to-index conversion)
                Symbol,            // For label calculations (pips, price format)
                BullishUnfilledColor,
                BullishPartialColor,
                BullishFilledColor,
                BearishUnfilledColor,
                BearishPartialColor,
                BearishFilledColor,
                ShowUnfilled,      // Status filters
                ShowPartial,
                ShowFilled,
                EnableLabels,      // Label controls
                LabelFontFamily,
                LabelFontSize,
                LabelColor,
                canUseMTF,
                SelectedTimeframe,
                MaxFVGsToDisplay,  // Number of FVGs to display
                ExtendFilledFVGs,  // Extend filled FVGs to current bar
                EnableFibonacciLines,  // Fibonacci controls
                FibLinesColor,
                FibLinesStyle,
                EnableFib236,
                EnableFib382,
                EnableFib500,
                EnableFib618,
                EnableFib786
            );
        }
        
        #endregion
        
        #region Calculate
        
        public override void Calculate(int index)
        {
            // Controller: Process FVG detection and updates
            // Detects on selected TF, checks mitigation on current TF
            _controller.ProcessBar(index);
            
            // View: Draw FVGs on chart
            _view.DrawFVGs(_controller.GetFVGList(), index);
        }
        
        #endregion
    }
}
