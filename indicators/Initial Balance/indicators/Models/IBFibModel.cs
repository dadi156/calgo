using System;

namespace cAlgo
{
    public class IBFibModel
    {
        public double Fib_11_40 { get; set; }
        public double Fib_23_6 { get; set; }
        public double Fib_38_2 { get; set; }
        public double Fib_50 { get; set; }
        public double Fib_61_8 { get; set; }
        public double Fib_78_6 { get; set; }
        public double Fib_88_60 { get; set; }

        public void Reset()
        {
            Fib_11_40 = double.NaN;
            Fib_23_6 = double.NaN;
            Fib_38_2 = double.NaN;
            Fib_50 = double.NaN;
            Fib_61_8 = double.NaN;
            Fib_78_6 = double.NaN;
            Fib_88_60 = double.NaN;
        }

        public void CalculateFibLevels(double high, double low)
        {
            double range = high - low;
            
            Fib_11_40 = low + (range * 0.114);
            Fib_23_6 = low + (range * 0.236);
            Fib_38_2 = low + (range * 0.382);
            Fib_50 = low + (range * 0.5);
            Fib_61_8 = low + (range * 0.618);
            Fib_78_6 = low + (range * 0.786);
            Fib_88_60 = low + (range * 0.886);
        }
    }
}
