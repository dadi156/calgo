using System;

namespace cAlgo
{
    public class IBFibProjectionModel
    {
        // Upward projection levels (from IB High)
        public double Up_0 { get; set; }        // 0% = IB High
        public double Up_11_40 { get; set; }
        public double Up_23_6 { get; set; }
        public double Up_38_2 { get; set; }
        public double Up_50 { get; set; }
        public double Up_61_8 { get; set; }
        public double Up_78_6 { get; set; }
        public double Up_88_60 { get; set; }
        public double Up_100 { get; set; }      // 100% = IB High + range

        // Downward projection levels (from IB Low)
        public double Down_100 { get; set; }    // 100% = IB Low
        public double Down_88_60 { get; set; }
        public double Down_78_6 { get; set; }
        public double Down_61_8 { get; set; }
        public double Down_50 { get; set; }
        public double Down_38_2 { get; set; }
        public double Down_23_6 { get; set; }
        public double Down_11_40 { get; set; }
        public double Down_0 { get; set; }      // 0% = IB Low - range

        public void Reset()
        {
            // Reset upward levels
            Up_0 = double.NaN;
            Up_11_40 = double.NaN;
            Up_23_6 = double.NaN;
            Up_38_2 = double.NaN;
            Up_50 = double.NaN;
            Up_61_8 = double.NaN;
            Up_78_6 = double.NaN;
            Up_88_60 = double.NaN;
            Up_100 = double.NaN;

            // Reset downward levels
            Down_100 = double.NaN;
            Down_88_60 = double.NaN;
            Down_78_6 = double.NaN;
            Down_61_8 = double.NaN;
            Down_50 = double.NaN;
            Down_38_2 = double.NaN;
            Down_23_6 = double.NaN;
            Down_11_40 = double.NaN;
            Down_0 = double.NaN;
        }

        public void CalculateUpwardProjection(double ibHigh, double range)
        {
            Up_0 = ibHigh;
            Up_11_40 = ibHigh + (range * 0.114);
            Up_23_6 = ibHigh + (range * 0.236);
            Up_38_2 = ibHigh + (range * 0.382);
            Up_50 = ibHigh + (range * 0.5);
            Up_61_8 = ibHigh + (range * 0.618);
            Up_78_6 = ibHigh + (range * 0.786);
            Up_88_60 = ibHigh + (range * 0.886);
            Up_100 = ibHigh + range;
        }

        public void CalculateDownwardProjection(double ibLow, double range)
        {
            Down_100 = ibLow;
            Down_88_60 = ibLow - (range * 0.114);   // 100% - 88.6% = 11.4%
            Down_78_6 = ibLow - (range * 0.214);    // 100% - 78.6% = 21.4%
            Down_61_8 = ibLow - (range * 0.382);    // 100% - 61.8% = 38.2%
            Down_50 = ibLow - (range * 0.5);
            Down_38_2 = ibLow - (range * 0.618);    // 100% - 38.2% = 61.8%
            Down_23_6 = ibLow - (range * 0.764);    // 100% - 23.6% = 76.4%
            Down_11_40 = ibLow - (range * 0.886);   // 100% - 11.4% = 88.6%
            Down_0 = ibLow - range;
        }
    }
}
