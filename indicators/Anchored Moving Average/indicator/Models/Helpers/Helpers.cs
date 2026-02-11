using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class AnchoredMovingAverage : Indicator
    {
        /// <summary>
        /// Calculate dynamic anchor point based on pre-defined period
        /// Returns null if Manual is selected
        /// </summary>
        private DateTime? CalculateDynamicAnchorPoint()
        {
            try
            {
                if (AnchorPeriodType == AnchorPointPeriod.Manual)
                    return null;

                DateTime currentTime = Server.Time;

                switch (AnchorPeriodType)
                {
                    case AnchorPointPeriod.Last1Hour:
                        return currentTime.AddHours(-1);
                    case AnchorPointPeriod.Last2Hours:
                        return currentTime.AddHours(-2);
                    case AnchorPointPeriod.Last4Hours:
                        return currentTime.AddHours(-4);
                    case AnchorPointPeriod.Last6Hours:
                        return currentTime.AddHours(-6);
                    case AnchorPointPeriod.Last8Hours:
                        return currentTime.AddHours(-8);
                    case AnchorPointPeriod.Last12Hours:
                        return currentTime.AddHours(-12);
                    case AnchorPointPeriod.LastDay:
                        return currentTime.AddDays(-1);
                    case AnchorPointPeriod.Last2Days:
                        return currentTime.AddDays(-2);
                    case AnchorPointPeriod.Last3Days:
                        return currentTime.AddDays(-3);
                    case AnchorPointPeriod.LastWeek:
                        return currentTime.AddDays(-7);
                    case AnchorPointPeriod.Last2Weeks:
                        return currentTime.AddDays(-14);
                    case AnchorPointPeriod.LastMonth:
                        return currentTime.AddMonths(-1);
                    case AnchorPointPeriod.Last3Months:
                        return currentTime.AddMonths(-3);
                    case AnchorPointPeriod.Last6Months:
                        return currentTime.AddMonths(-6);
                    case AnchorPointPeriod.Last9Months:
                        return currentTime.AddMonths(-9);
                    case AnchorPointPeriod.LastYear:
                        return currentTime.AddYears(-1);
                    case AnchorPointPeriod.Last3Years:
                        return currentTime.AddYears(-3);
                    case AnchorPointPeriod.Last5Years:
                        return currentTime.AddYears(-5);
                    case AnchorPointPeriod.Last10Years:
                        return currentTime.AddYears(-10);
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get the final anchor datetime string to use
        /// Either dynamic (from pre-defined period) or manual (from user input)
        /// </summary>
        private string GetFinalAnchorDateTimeString()
        {
            try
            {
                DateTime? dynamicAnchor = CalculateDynamicAnchorPoint();

                if (dynamicAnchor.HasValue)
                {
                    return dynamicAnchor.Value.ToString("dd/MM/yyyy HH:mm");
                }
                else
                {
                    return StartDateTime;
                }
            }
            catch (Exception)
            {
                return StartDateTime;
            }
        }
    }
}
