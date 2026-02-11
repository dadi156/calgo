using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Volume Weighted Average Price (VWAP) indicator with Fibonacci levels
    /// Main indicator class that implements the MVC pattern
    /// Optimized for performance with lazy calculation and efficient band updates
    /// </summary>
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None, AutoRescale = false)]
    public partial class VWAP : Indicator
    {
        #region Fields

        private VwapController _controller;
        private VwapResetPeriod _previousResetPeriod;
        private VwapBandType _previousBandType;
        private int _previousPivotDepth;
        private string _previousAnchorDateTime;
        private DateTime? _parsedAnchorPoint;
        private TimeFrame _previousTimeFrame;

        // Session configuration tracking
        private int _previousAsianSessionHour;
        private int _previousLondonSessionHour;
        private int _previousNewYorkSessionHour;
        private int _previousTimezoneOffset;

        // Fields for tracking level visibility
        private bool _previousShowFibo114;
        private bool _previousShowFibo236;
        private bool _previousShowFibo382;
        private bool _previousShowFibo628;
        private bool _previousShowFibo764;
        private bool _previousShowFibo886;

        // Fields for tracking group visibility
        private bool _previousShowUpperBand;
        private bool _previousShowLowerBand;

        // Field for error message display
        private ChartStaticText _errorText;
        private ChartStaticText _sessionInfoText;

        // Throttling mechanism for parameter updates
        private int _calculationCount = 0;
        private const int PARAMETER_UPDATE_THROTTLE = 10;

        #endregion

        #region Initialize

        protected override void Initialize()
        {
            // Initialize error and session info text
            _errorText = Chart.DrawStaticText("VWAP_ErrorMessage", "",
                VerticalAlignment.Bottom,
                HorizontalAlignment.Center,
                Color.Red);
            _errorText.FontFamily = "Consolas";
            _errorText.FontSize = 11;

            _sessionInfoText = Chart.DrawStaticText("VWAP_SessionInfo", "",
                VerticalAlignment.Bottom,
                HorizontalAlignment.Right,
                Color.Gray);
            _sessionInfoText.FontFamily = "Consolas";
            _sessionInfoText.FontSize = 9;

            // Update session configuration
            UpdateSessionConfiguration();

            // Parse anchor point if needed
            DateTime? anchorPoint = null;
            if (ResetPeriod == VwapResetPeriod.AnchorPoint)
            {
                anchorPoint = ParseAnchorDateTime(AnchorDateTime);
                _parsedAnchorPoint = anchorPoint;
            }

            // Initialize the MVC components
            _controller = new VwapController(
                Bars,
                Source,
                VWAP,
                UpperBand,
                LowerBand,
                FiboLevel886,
                FiboLevel764,
                FiboLevel628,
                FiboLevel382,
                FiboLevel236,
                FiboLevel114,
                Chart,
                ResetPeriod,
                BandType,
                StdDevMultiplier,
                PivotDepth,
                ShowFibo114,
                ShowFibo236,
                ShowFibo382,
                ShowFibo628,
                ShowFibo764,
                ShowFibo886,
                ShowUpperBand,
                ShowLowerBand,
                anchorPoint);

            // Process historical data
            _controller.Initialize();

            // Store initial parameter values
            _previousResetPeriod = ResetPeriod;
            _previousBandType = BandType;
            _previousPivotDepth = PivotDepth;
            _previousAnchorDateTime = AnchorDateTime;
            _previousTimeFrame = TimeFrame;

            // Store initial session configuration
            _previousAsianSessionHour = AsianSessionHour;
            _previousLondonSessionHour = LondonSessionHour;
            _previousNewYorkSessionHour = NewYorkSessionHour;
            _previousTimezoneOffset = TimezoneOffset;

            // Store initial visibility parameters
            _previousShowFibo114 = ShowFibo114;
            _previousShowFibo236 = ShowFibo236;
            _previousShowFibo382 = ShowFibo382;
            _previousShowFibo628 = ShowFibo628;
            _previousShowFibo764 = ShowFibo764;
            _previousShowFibo886 = ShowFibo886;

            // Store initial group visibility parameters
            _previousShowUpperBand = ShowUpperBand;
            _previousShowLowerBand = ShowLowerBand;

            // Display session information
            UpdateSessionInfoDisplay();
        }

        #endregion

        #region Calculate

        public override void Calculate(int index)
        {
            // Increment calculation counter for throttling
            _calculationCount++;

            // Check if timeframe changed
            bool timeframeChanged = TimeFrame != _previousTimeFrame;
            if (timeframeChanged)
            {
                _controller.ForceFullRecalculation();
                PeriodUtility.ClearCaches();
                _previousTimeFrame = TimeFrame;
                return;
            }

            // Check if parameters changed
            bool checkParameters = (_calculationCount % PARAMETER_UPDATE_THROTTLE == 0);
            bool parametersChanged = false;
            bool visibilityChanged = false;
            bool sessionConfigChanged = false;

            if (checkParameters)
            {
                bool anchorChanged = AnchorDateTime != _previousAnchorDateTime;
                parametersChanged = ResetPeriod != _previousResetPeriod ||
                    BandType != _previousBandType ||
                    PivotDepth != _previousPivotDepth ||
                    anchorChanged;

                // Check session configuration changes
                sessionConfigChanged =
                    AsianSessionHour != _previousAsianSessionHour ||
                    LondonSessionHour != _previousLondonSessionHour ||
                    NewYorkSessionHour != _previousNewYorkSessionHour ||
                    TimezoneOffset != _previousTimezoneOffset;

                // Check visibility parameters
                visibilityChanged =
                    ShowFibo114 != _previousShowFibo114 ||
                    ShowFibo236 != _previousShowFibo236 ||
                    ShowFibo382 != _previousShowFibo382 ||
                    ShowFibo628 != _previousShowFibo628 ||
                    ShowFibo764 != _previousShowFibo764 ||
                    ShowFibo886 != _previousShowFibo886 ||
                    ShowUpperBand != _previousShowUpperBand ||
                    ShowLowerBand != _previousShowLowerBand;

                // Handle session configuration changes
                if (sessionConfigChanged)
                {
                    UpdateSessionConfiguration();
                    _previousAsianSessionHour = AsianSessionHour;
                    _previousLondonSessionHour = LondonSessionHour;
                    _previousNewYorkSessionHour = NewYorkSessionHour;
                    _previousTimezoneOffset = TimezoneOffset;
                    UpdateSessionInfoDisplay();
                    _controller.ForceFullRecalculation();
                }

                // Handle parameter changes
                if (parametersChanged)
                {
                    DateTime? anchorPoint = null;
                    if (ResetPeriod == VwapResetPeriod.AnchorPoint)
                    {
                        if (anchorChanged || _parsedAnchorPoint == null)
                        {
                            anchorPoint = ParseAnchorDateTime(AnchorDateTime);
                            _parsedAnchorPoint = anchorPoint;
                        }
                        else
                        {
                            anchorPoint = _parsedAnchorPoint;
                        }
                    }

                    _controller.UpdateConfiguration(
                        ResetPeriod,
                        BandType,
                        PivotDepth,
                        ShowUpperBand,
                        ShowLowerBand,
                        anchorPoint);

                    _previousResetPeriod = ResetPeriod;
                    _previousBandType = BandType;
                    _previousPivotDepth = PivotDepth;
                    _previousAnchorDateTime = AnchorDateTime;

                    if (ResetPeriod != _previousResetPeriod)
                    {
                        UpdateSessionInfoDisplay();
                    }
                }

                // Handle visibility changes
                if (visibilityChanged)
                {
                    _controller.UpdateFiboLevelVisibility(
                        ShowFibo114,
                        ShowFibo236,
                        ShowFibo382,
                        ShowFibo628,
                        ShowFibo764,
                        ShowFibo886,
                        ShowUpperBand,
                        ShowLowerBand);

                    // Update previous values
                    _previousShowFibo114 = ShowFibo114;
                    _previousShowFibo236 = ShowFibo236;
                    _previousShowFibo382 = ShowFibo382;
                    _previousShowFibo628 = ShowFibo628;
                    _previousShowFibo764 = ShowFibo764;
                    _previousShowFibo886 = ShowFibo886;

                    // Update previous group values
                    _previousShowUpperBand = ShowUpperBand;
                    _previousShowLowerBand = ShowLowerBand;
                }
            }

            // Weekly timeframe adjustments
            if (TimeFrame >= TimeFrame.Weekly && index > 0 && index < Bars.Count)
            {
                if (ResetPeriod == VwapResetPeriod.Daily ||
                    ResetPeriod == VwapResetPeriod.OneHour ||
                    ResetPeriod == VwapResetPeriod.TwoHour ||
                    ResetPeriod == VwapResetPeriod.ThreeHour ||
                    ResetPeriod == VwapResetPeriod.FourHour ||
                    ResetPeriod == VwapResetPeriod.SixHour ||
                    ResetPeriod == VwapResetPeriod.EightHour ||
                    ResetPeriod == VwapResetPeriod.TwelveHour)
                {
                    ResetPeriod = VwapResetPeriod.Weekly;
                    _controller.UpdateConfiguration(
                        ResetPeriod,
                        BandType,
                        PivotDepth,
                        ShowUpperBand,
                        ShowLowerBand,
                        _parsedAnchorPoint);
                    UpdateSessionInfoDisplay();
                }
            }

            // Delegate calculation to controller
            _controller.Calculate(index);
        }

        #endregion
    }
}
