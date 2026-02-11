using cAlgo.API;

namespace cAlgo.Indicators
{
    public partial class MovingAverageChannel : Indicator
    {
        // MA Type Selector
        [Parameter("Type", DefaultValue = MATypes.SuperSmoother, Group = "Moving Average")]
        public MATypes MAType { get; set; }

        // MA Periods - RENAMED
        [Parameter("Periods", DefaultValue = 3, MinValue = 1, Group = "General MA Periods")]
        public int GeneralMAPeriod { get; set; }

        [Parameter("Smoothing", DefaultValue = 3, MinValue = 1, Group = "General MA Periods")]
        public int GeneralMASmoothPeriod { get; set; }

        [Parameter("Deviation Scaled", DefaultValue = 20, MinValue = 5, Group = "Deviation Scaled & SuperSmoother Periods")]
        public double DSMAPeriod { get; set; }

        [Parameter("SuperSmoother", DefaultValue = 10, MinValue = 1, Group = "Deviation Scaled & SuperSmoother Periods")]
        public int SuperSmootherPeriod { get; set; }

        [Parameter("Hull", DefaultValue = 20, MinValue = 1, Group = "Hull Period")]
        public int HullMAPeriod { get; set; }

        // Multi-Timeframe Parameters
        [Parameter("Multi-Timeframe", DefaultValue = false, Group = "Multi-Timeframe")]
        public bool EnableMultiTimeframe { get; set; }

        [Parameter("Timeframe", DefaultValue = "Daily", Group = "Multi-Timeframe")]
        public TimeFrame SelectedTimeframe { get; set; }

        // Display Parameters
        [Parameter("Line Style", DefaultValue = LineStyles.TrendLines, Group = "Display")]
        public LineStyles LineStyle { get; set; }

        [Parameter("Projections", DefaultValue = false, Group = "Display")]
        public bool ShowProjections { get; set; }
    }

    // Parameters class for passing settings
    public class MAParameters
    {
        // MA Type
        public MATypes MAType { get; set; }
        
        // General MA settings (used by Simple, Exponential, and Wilder)
        public int GeneralMAPeriod { get; set; }
        public int GeneralMASmoothPeriod { get; set; }
        
        // DSMA settings
        public double DSMAPeriod { get; set; }
        
        // SuperSmoother settings
        public int SuperSmootherPeriod { get; set; }
        
        // Hull MA settings
        public int HullMAPeriod { get; set; }
        
        // Multi-timeframe settings
        public bool EnableMultiTimeframe { get; set; }
        public TimeFrame SelectedTimeframe { get; set; }
        
        // Display settings
        public LineStyles LineStyle { get; set; }
        public bool ShowProjections { get; set; }

        public MAParameters(MATypes maType, int generalMAPeriod, int generalMASmoothPeriod,
                           double dsmaPeriod, int superSmootherPeriod, int hullMAPeriod,
                           bool enableMultiTimeframe, TimeFrame selectedTimeframe, 
                           LineStyles lineStyle, bool showProjections)
        {
            MAType = maType;
            GeneralMAPeriod = generalMAPeriod;
            GeneralMASmoothPeriod = generalMASmoothPeriod;
            DSMAPeriod = dsmaPeriod;
            SuperSmootherPeriod = superSmootherPeriod;
            HullMAPeriod = hullMAPeriod;
            EnableMultiTimeframe = enableMultiTimeframe;
            SelectedTimeframe = selectedTimeframe;
            LineStyle = lineStyle;
            ShowProjections = showProjections;
        }

        // Get the period based on selected MA type
        public int GetPeriod()
        {
            switch (MAType)
            {
                case MATypes.Simple:
                case MATypes.Exponential:
                case MATypes.Wilder:
                    return GeneralMAPeriod;
                case MATypes.DeviationScaled:
                    return (int)DSMAPeriod;
                case MATypes.SuperSmoother:
                    return SuperSmootherPeriod;
                case MATypes.Hull:
                    return HullMAPeriod;
                default:
                    return GeneralMAPeriod;
            }
        }

        // Helper method to check if trendlines should be used
        public bool UseTrendlines()
        {
            return LineStyle == LineStyles.TrendLines;
        }
    }

    // Enum for MA Type selection - ADDED WILDER
    public enum MATypes
    {
        DeviationScaled,    // Deviation Scaled MA
        Exponential,        // Exponential Moving Average (smoothed with EMA)
        Hull,               // Hull Moving Average
        Simple,             // Simple Moving Average (smoothed with SMA)
        SuperSmoother,      // SuperSmoother MA
        Wilder              // Wilder's Smoothed Moving Average
    }

    // Enum for Line Style options
    public enum LineStyles
    {
        StairSteps,    // Shows step-like lines (regular output lines)
        TrendLines     // Shows diagonal trendlines between MTF bars
    }
}
