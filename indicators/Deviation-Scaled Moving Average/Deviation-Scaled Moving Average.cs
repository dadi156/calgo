using cAlgo.API;

[Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
public class DSMA : Indicator
{
    [Parameter("Period", DefaultValue = 40, MinValue = 5)]
    public int Period { get; set; }
    
    [Parameter]
    public DataSeries Source { get; set; }

    [Output("DSMA", LineColor = "LightSteelBlue")]
    public IndicatorDataSeries Result { get; set; }

    private DSMAController controller;

    protected override void Initialize()
    {
        // Create controller that manages everything
        controller = new DSMAController(Period, Result, this);
    }

    public override void Calculate(int index)
    {
        // Let controller handle all the work with Source parameter
        controller.ProcessBar(index, Source);
    }
}
