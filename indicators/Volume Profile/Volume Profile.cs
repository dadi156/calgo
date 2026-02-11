using cAlgo.API;
using cAlgo.Indicators.App.Controllers;
using cAlgo.Indicators.App.Models;
using cAlgo.Indicators.App.Views;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class VolumeProfile : Indicator
    {
        // MVC components
        private VolumeProfileController _controller;

        protected override void Initialize()
        {
            // Create model with TPO parameters
            var volumeProfileModel = new VolumeProfileModel(
                PriceLevels,
                ValueAreaPercent,
                TPOPeriodMinutes,
                CalculateInitialBalance,
                InitialBalancePeriod
            );

            // Create view with all parameters
            var volumeProfileView = new VolumeProfileView(
                Chart,
                PositiveTextColor,
                NegativeTextColor,
                ShowLevelInfo,
                ShowAllLevels,
                InfoTextPosition,
                FontFamily,
                FontSize,
                PipMargin,
                PositiveDeltaColor,
                NegativeDeltaColor,
                TPOTextColor,
                ShowVolTPO,
                FullWidthBars
            );

            // Create controller with all parameters
            _controller = new VolumeProfileController(
                volumeProfileModel,
                volumeProfileView,
                Bars,
                LookbackPeriods,
                UseDateTimeProfiles,
                StartDateTimeProfiles,
                EndDateTimeProfiles,
                TimezoneOffsetHours,
                EnableTPO
            );

            // Pass lookback periods to view for width calculation
            volumeProfileView.SetLookbackPeriods(LookbackPeriods);
        }

        public override void Calculate(int index)
        {
            // Only calculate on the last bar
            if (index < Bars.Count - 1)
                return;

            // Delegate to controller
            _controller.Calculate(index);
        }
    }

    public enum TextPosition
    {
        Left,
        Right
    }
}
