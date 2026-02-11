using System;

namespace cAlgo
{
    public class IBModel
    {
        public double IBHigh { get; set; }
        public double IBLow { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime IBStart { get; set; }
        public DateTime IBEnd { get; set; }
        public bool IsCalculated { get; set; }

        public void Reset()
        {
            IBHigh = double.NaN;
            IBLow = double.NaN;
            IsCalculated = false;
        }
    }
}
