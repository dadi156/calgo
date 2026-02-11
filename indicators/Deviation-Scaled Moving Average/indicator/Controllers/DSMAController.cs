using cAlgo.API;

public class DSMAController
{
    private DSMAModel model;
    private DSMAView view;
    
    public DSMAController(int period, IndicatorDataSeries output, Indicator indicator)
    {
        model = new DSMAModel(period, indicator);
        view = new DSMAView(output);
    }
    
    public void ProcessBar(int index, DataSeries closePrices)
    {
        // Get calculation from model
        double dsmaValue = model.Calculate(index, closePrices);
        
        // Update view with new value
        view.UpdateView(index, dsmaValue);
    }
}
