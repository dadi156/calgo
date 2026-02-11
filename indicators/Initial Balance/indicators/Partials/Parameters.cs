using cAlgo.API;

namespace cAlgo
{
    public partial class InitialBalance : Indicator
    {
        [Parameter("Type", DefaultValue = IBPeriodType.Monthly, Group = "Period")]
        public IBPeriodType PeriodType { get; set; }

        [Parameter("Offset", DefaultValue = 0, MinValue = -100, MaxValue = 0, Group = "Period")]
        public int PeriodOffset { get; set; }

        [Parameter("Mode", DefaultValue = DailyIBMode.MarketSession, Group = "Daily Period Type")]
        public DailyIBMode DailyMode { get; set; }

        [Parameter("Market Session", DefaultValue = MarketSession.Sydney, Group = "Daily Period Type")]
        public MarketSession DailySession { get; set; }

        [Parameter("Hours", DefaultValue = DailyIBHours.Hour1, Group = "Daily Period Type")]
        public DailyIBHours DailyHours { get; set; }

        [Parameter("Start Hour", DefaultValue = 7, MinValue = 0, MaxValue = 23, Group = "Daily Period Type")]
        public int HoursStartHour { get; set; }

        [Parameter("Custom Range", DefaultValue = "03/11/2025 05:00, 10/11/2025 05:00", Group = "Custom Range Period Type")]
        public string CustomRange { get; set; }

        [Parameter("Enable", DefaultValue = true, Group = "Fibonacci Levels")]
        public bool ShowFibLevels { get; set; }

        [Parameter("Projection", DefaultValue = FibProjectionMode.None, Group = "Fibonacci Levels")]
        public FibProjectionMode FibProjection { get; set; }

        [Parameter("88.6% Level", DefaultValue = false, Group = "Level Lines")]
        public bool Show_Fib_88_60 { get; set; }

        [Parameter("78.6% Level", DefaultValue = true, Group = "Level Lines")]
        public bool Show_Fib_78_6 { get; set; }

        [Parameter("61.8% Level", DefaultValue = false, Group = "Level Lines")]
        public bool Show_Fib_61_8 { get; set; }

        [Parameter("50% Level", DefaultValue = true, Group = "Level Lines")]
        public bool Show_Fib_50 { get; set; }

        [Parameter("38.2% Level", DefaultValue = false, Group = "Level Lines")]
        public bool Show_Fib_38_2 { get; set; }

        [Parameter("23.6% Level", DefaultValue = true, Group = "Level Lines")]
        public bool Show_Fib_23_6 { get; set; }

        [Parameter("11.4% Level", DefaultValue = false, Group = "Level Lines")]
        public bool Show_Fib_11_40 { get; set; }

        [Parameter("Extend Lines", DefaultValue = -1, MinValue = -1, MaxValue = 100, Group = "Initial Balance Lines")]
        public int ExtendLines { get; set; }

        [Parameter("Labels", DefaultValue = true, Group = "Initial Balance Lines")]
        public bool ShowLabels { get; set; }

        [Parameter("Line Style", DefaultValue = LineStyle.Solid, Group = "Initial Balance Lines")]
        public LineStyle LineStyle { get; set; }

        [Parameter("Thickness", DefaultValue = 2, MinValue = 1, MaxValue = 5, Group = "Initial Balance Lines")]
        public int LineThickness { get; set; }

        [Parameter("High Line", DefaultValue = "Green", Group = "Initial Balance Lines")]
        public Color HighLineColor { get; set; }

        [Parameter("Low Line", DefaultValue = "Red", Group = "Initial Balance Lines")]
        public Color LowLineColor { get; set; }

        [Parameter("Line Style", DefaultValue = LineStyle.Dots, Group = "Fibonacci Level Lines")]
        public LineStyle FibLineStyle { get; set; }

        [Parameter("Thickness", DefaultValue = 1, MinValue = 1, MaxValue = 5, Group = "Fibonacci Level Lines")]
        public int FibLineThickness { get; set; }

        [Parameter("Color", DefaultValue = "DimGray", Group = "Fibonacci Level Lines")]
        public Color FibLineColor { get; set; }

        [Parameter("Timezone (UTC+/-)", DefaultValue = 7, MinValue = -12, MaxValue = 14, Group = "Timezone & Session Start Hour")]
        public int UserTimezone { get; set; }

        [Parameter("Sydney", DefaultValue = 3, MinValue = 0, MaxValue = 23, Group = "Timezone & Session Start Hour")]
        public int SydneyStartHour { get; set; }

        [Parameter("Tokyo", DefaultValue = 7, MinValue = 0, MaxValue = 23, Group = "Timezone & Session Start Hour")]
        public int TokyoStartHour { get; set; }

        [Parameter("London", DefaultValue = 15, MinValue = 0, MaxValue = 23, Group = "Timezone & Session Start Hour")]
        public int LondonStartHour { get; set; }

        [Parameter("New York", DefaultValue = 20, MinValue = 0, MaxValue = 23, Group = "Timezone & Session Start Hour")]
        public int NYStartHour { get; set; }
    }
}
