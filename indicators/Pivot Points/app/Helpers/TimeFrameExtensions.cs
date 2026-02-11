using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Extension methods for TimeFrame enum
    /// </summary>
    public static class TimeFrameExtensions
    {
        /// <summary>
        /// Converts a TimeFrame to its corresponding TimeSpan
        /// </summary>
        public static TimeSpan ToTimeSpan(this TimeFrame timeFrame)
        {
            if (timeFrame == TimeFrame.Minute)
                return TimeSpan.FromMinutes(1);
            else if (timeFrame == TimeFrame.Minute2)
                return TimeSpan.FromMinutes(2);
            else if (timeFrame == TimeFrame.Minute3)
                return TimeSpan.FromMinutes(3);
            else if (timeFrame == TimeFrame.Minute4)
                return TimeSpan.FromMinutes(4);
            else if (timeFrame == TimeFrame.Minute5)
                return TimeSpan.FromMinutes(5);
            else if (timeFrame == TimeFrame.Minute6)
                return TimeSpan.FromMinutes(6);
            else if (timeFrame == TimeFrame.Minute10)
                return TimeSpan.FromMinutes(10);
            else if (timeFrame == TimeFrame.Minute15)
                return TimeSpan.FromMinutes(15);
            else if (timeFrame == TimeFrame.Minute30)
                return TimeSpan.FromMinutes(30);
            else if (timeFrame == TimeFrame.Hour)
                return TimeSpan.FromHours(1);
            else if (timeFrame == TimeFrame.Hour2)
                return TimeSpan.FromHours(2);
            else if (timeFrame == TimeFrame.Hour3)
                return TimeSpan.FromHours(3);
            else if (timeFrame == TimeFrame.Hour4)
                return TimeSpan.FromHours(4);
            else if (timeFrame == TimeFrame.Hour6)
                return TimeSpan.FromHours(6);
            else if (timeFrame == TimeFrame.Hour8)
                return TimeSpan.FromHours(8);
            else if (timeFrame == TimeFrame.Hour12)
                return TimeSpan.FromHours(12);
            else if (timeFrame == TimeFrame.Daily)
                return TimeSpan.FromDays(1);
            else if (timeFrame == TimeFrame.Day2)
                return TimeSpan.FromDays(2);
            else if (timeFrame == TimeFrame.Day3)
                return TimeSpan.FromDays(3);
            else if (timeFrame == TimeFrame.Weekly)
                return TimeSpan.FromDays(7);
            else if (timeFrame == TimeFrame.Monthly)
                return TimeSpan.FromDays(30);
            else
                return TimeSpan.FromMinutes(1); // Default to 1 minute if unknown
        }
    }
}
