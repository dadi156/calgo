using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class InitialBalance : Indicator
    {
        public int GetTimezoneOffset()
        {
            return _serverToUserOffset;
        }

        public void UpdateFibLevels(DateTime startTime, DateTime endTime, double ibHigh, double ibLow)
        {
            _fibController.Update(startTime, endTime, ibHigh, ibLow);
            _projectionController.Update(startTime, endTime, ibHigh, ibLow);
        }
    }
}
