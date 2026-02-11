using System;
using cAlgo.API;
using cAlgo.API.Indicators;

public class DSMAModel
{
    // SuperSmoother filter coefficients
    public double A1 { get; private set; }
    public double B1 { get; private set; }
    public double C1 { get; private set; }
    public double C2 { get; private set; }
    public double C3 { get; private set; }
    
    // Settings
    public int Period { get; private set; }
    
    // Data storage
    private IndicatorDataSeries zeros;
    private IndicatorDataSeries filt;
    private IndicatorDataSeries dsmaValues;
    
    public DSMAModel(int period, Indicator indicator)
    {
        Period = period;
        
        // Create data series
        zeros = indicator.CreateDataSeries();
        filt = indicator.CreateDataSeries();
        dsmaValues = indicator.CreateDataSeries();
        
        // Calculate SuperSmoother coefficients
        CalculateCoefficients();
    }
    
    private void CalculateCoefficients()
    {
        double criticalPeriod = Period * 0.5;
        
        A1 = Math.Exp(-1.414 * Math.PI / criticalPeriod);
        B1 = 2 * A1 * Math.Cos(1.414 * Math.PI / criticalPeriod);
        C2 = B1;
        C3 = -A1 * A1;
        C1 = 1 - C2 - C3;
    }
    
    public double Calculate(int index, DataSeries closePrices)
    {
        // Handle first few bars
        if (index < 3)
        {
            zeros[index] = 0;
            filt[index] = 0;
            dsmaValues[index] = closePrices[index];
            return dsmaValues[index];
        }

        // Step 1: Calculate Zeros oscillator
        zeros[index] = closePrices[index] - closePrices[index - 2];

        // Step 2: Apply SuperSmoother filter
        filt[index] = C1 * (zeros[index] + zeros[index - 1]) / 2 + 
                     C2 * filt[index - 1] + 
                     C3 * filt[index - 2];
        
        // Fix NaN values
        if (double.IsNaN(filt[index]) || double.IsInfinity(filt[index]))
        {
            filt[index] = 0;
        }

        // Step 3: Calculate DSMA
        if (index >= Period + 2)
        {
            double rms = CalculateRMS(index);
            double scaledFilt = 0;
            
            if (rms > 0.000001)
            {
                scaledFilt = filt[index] / rms;
            }

            double alpha = Math.Abs(scaledFilt) * 5.0 / Period;
            alpha = Math.Max(0.001, Math.Min(0.999, alpha));

            dsmaValues[index] = alpha * closePrices[index] + 
                               (1 - alpha) * dsmaValues[index - 1];
        }
        else
        {
            // Use simple EMA for early bars
            double simpleAlpha = 2.0 / (Period + 1);
            dsmaValues[index] = simpleAlpha * closePrices[index] + 
                               (1 - simpleAlpha) * dsmaValues[index - 1];
        }

        // Fix NaN values
        if (double.IsNaN(dsmaValues[index]) || double.IsInfinity(dsmaValues[index]))
        {
            dsmaValues[index] = closePrices[index];
        }

        return dsmaValues[index];
    }
    
    private double CalculateRMS(int index)
    {
        double sumSquares = 0;
        int validPoints = 0;
        
        for (int i = 0; i < Period; i++)
        {
            int lookbackIndex = index - i;
            if (lookbackIndex >= 0 && !double.IsNaN(filt[lookbackIndex]))
            {
                sumSquares += filt[lookbackIndex] * filt[lookbackIndex];
                validPoints++;
            }
        }
        
        return validPoints > 0 ? Math.Sqrt(sumSquares / validPoints) : 0;
    }
}
