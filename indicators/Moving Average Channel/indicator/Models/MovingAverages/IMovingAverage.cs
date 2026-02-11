using cAlgo.API;

namespace cAlgo.Indicators
{
    // Base interface - all MA types must use this
    public interface IMovingAverage
    {
        // Calculate single MA value for one price type
        double Calculate(int index, DataSeries priceSource);
        
        // Initialize the MA with first value
        void Initialize(double firstValue);
    }

    // MA calculation result - Now includes Median line (7 values total)
    public class MAResult
    {
        public double HighMA { get; }
        public double LowMA { get; }
        public double CloseMA { get; }
        public double OpenMA { get; }
        public double MedianMA { get; }  // NEW: Median line (50% between High and Low)
        
        // Only 2 Fibonacci levels we actually use (38.2% and 61.8%)
        public double Fib618MA { get; }  // 61.8% level - Upper Reversion Zone
        public double Fib382MA { get; }  // 38.2% level - Lower Reversion Zone
        
        public bool IsNewMTFBar { get; } // Flag to indicate new MTF bar formed

        // Constructor - now 7 values (added Median)
        public MAResult(double highMA, double lowMA, double closeMA, double openMA, 
                       double medianMA, double fib618MA, double fib382MA, bool isNewMTFBar = false)
        {
            HighMA = highMA;
            LowMA = lowMA;
            CloseMA = closeMA;
            OpenMA = openMA;
            MedianMA = medianMA;  // NEW
            
            // Store only 2 Fibonacci values we use
            Fib618MA = fib618MA;
            Fib382MA = fib382MA;
            
            IsNewMTFBar = isNewMTFBar;
        }
    }
}
