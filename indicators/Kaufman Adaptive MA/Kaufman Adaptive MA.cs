using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class KaufmanAdaptiveMA : Indicator
    {
        // === PARAMETERS ===
        
        [Parameter("Source", Group = "Moving Average")]
        public DataSeries Source { get; set; }

        [Parameter("Period", DefaultValue = 10, MinValue = 1, Group = "Moving Average")]
        public int Period { get; set; }
        
        [Parameter("Fast Period", DefaultValue = 2, MinValue = 1, Group = "Moving Average")]
        public int FastPeriod { get; set; }
        
        [Parameter("Slow Period", DefaultValue = 30, MinValue = 1, Group = "Moving Average")]
        public int SlowPeriod { get; set; }
        
        [Parameter("ER Threshold", DefaultValue = 0.3, MinValue = 0.1, MaxValue = 0.9, Group = "Trend Detector")]
        public double ERThreshold { get; set; }
        
        // === OUTPUTS ===
        
        [Output("Uptrend", LineColor = "Lime", PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries KamaUp { get; set; }
        
        [Output("Downtrend", LineColor = "Red", PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries KamaDown { get; set; }
        
        [Output("Ranging", LineColor = "LightSteelBlue", PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries KamaRange { get; set; }

        // === PRIVATE VARIABLES ===
        
        private IndicatorDataSeries _kama;
        private IndicatorDataSeries _efficiencyRatio;
        private double _fastSC;
        private double _slowSC;

        // === INITIALIZE ===
        
        protected override void Initialize()
        {
            _kama = CreateDataSeries();
            _efficiencyRatio = CreateDataSeries();
            
            // Calculate smoothing constants
            _fastSC = 2.0 / (FastPeriod + 1.0);
            _slowSC = 2.0 / (SlowPeriod + 1.0);
        }

        // === CALCULATE ===
        
        public override void Calculate(int index)
        {
            // Not enough bars yet
            if (index < Period)
            {
                _kama[index] = Source[index];
                KamaRange[index] = Source[index];
                return;
            }

            // === STEP 1: Calculate Efficiency Ratio (ER) ===
            
            // Direction = Net price change over Period
            double direction = Math.Abs(Source[index] - Source[index - Period]);
            
            // Volatility = Sum of all price changes over Period
            double volatility = 0;
            for (int i = 0; i < Period; i++)
            {
                volatility += Math.Abs(Source[index - i] - Source[index - i - 1]);
            }
            
            // ER = Direction / Volatility
            // If volatility is zero, set ER to zero
            double er = volatility > 0 ? direction / volatility : 0;
            _efficiencyRatio[index] = er;

            // === STEP 2: Calculate Smoothing Constant (SC) ===
            
            // SC = [ER * (FastSC - SlowSC) + SlowSC]^2
            double sc = er * (_fastSC - _slowSC) + _slowSC;
            sc = sc * sc;

            // === STEP 3: Calculate KAMA ===
            
            // KAMA = Previous KAMA + SC * (Price - Previous KAMA)
            if (index == Period)
            {
                // First KAMA value = simple average
                double sum = 0;
                for (int i = 0; i <= Period; i++)
                {
                    sum += Source[index - i];
                }
                _kama[index] = sum / (Period + 1);
            }
            else
            {
                _kama[index] = _kama[index - 1] + sc * (Source[index] - _kama[index - 1]);
            }

            // === STEP 4: Determine Color Based on Market State ===
            
            // Reset all series to NaN
            KamaUp[index] = double.NaN;
            KamaDown[index] = double.NaN;
            KamaRange[index] = double.NaN;
            
            // Check if ranging or trending
            if (er < ERThreshold)
            {
                // RANGING MARKET - White color
                KamaRange[index] = _kama[index];
                if (index > Period)
                    KamaRange[index - 1] = _kama[index - 1];
            }
            else
            {
                // TRENDING MARKET - Check direction
                if (_kama[index] > _kama[index - 1])
                {
                    // UPTREND - Green color
                    KamaUp[index] = _kama[index];
                    KamaUp[index - 1] = _kama[index - 1];
                }
                else
                {
                    // DOWNTREND - Red color
                    KamaDown[index] = _kama[index];
                    KamaDown[index - 1] = _kama[index - 1];
                }
            }
        }
    }
}
