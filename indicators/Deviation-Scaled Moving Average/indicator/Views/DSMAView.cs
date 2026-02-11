using cAlgo.API;

public class DSMAView
{
    private IndicatorDataSeries outputSeries;
    
    public DSMAView(IndicatorDataSeries output)
    {
        outputSeries = output;
    }
    
    public void UpdateView(int index, double value)
    {
        outputSeries[index] = value;
    }
}
