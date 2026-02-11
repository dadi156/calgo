using System;
using cAlgo.API;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NadarayaWatsonKernelRegression : Indicator
    {
        [Parameter("Source", Group = "Kernel Regression")]
        public DataSeries Source { get; set; }

        [Parameter("Bandwidth", DefaultValue = 8.0, MinValue = 1.0, MaxValue = 50.0, Group = "Kernel Regression")]
        public double Bandwidth { get; set; }

        [Output("Uptrend", LineColor = "Lime", PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries KernelUp { get; set; }

        [Output("Downtrend", LineColor = "Red", PlotType = PlotType.DiscontinuousLine)]
        public IndicatorDataSeries KernelDown { get; set; }

        private IndicatorDataSeries _kernelRegression;

        protected override void Initialize()
        {
            _kernelRegression = CreateDataSeries();
        }

        public override void Calculate(int index)
        {
            // Need enough bars for calculation
            if (index < 1)
            {
                _kernelRegression[index] = Source[index];
                return;
            }

            int startIndex = 0;
            double sumWeightedPrice = 0;
            double sumWeights = 0;

            // Calculate Nadaraya-Watson kernel regression
            for (int i = startIndex; i <= index; i++)
            {
                // Calculate distance (in bars)
                double distance = Math.Abs(index - i);

                // Gaussian kernel weight
                double weight = GaussianKernel(distance, Bandwidth);

                sumWeightedPrice += weight * Source[i];
                sumWeights += weight;
            }

            // Calculate the kernel regression value
            if (sumWeights > 0)
                _kernelRegression[index] = sumWeightedPrice / sumWeights;
            else
                _kernelRegression[index] = Source[index];

            // === Determine Color Based on Trend ===

            // Reset all series to NaN
            KernelUp[index] = double.NaN;
            KernelDown[index] = double.NaN;

            // Check trend direction
            if (_kernelRegression[index] >= _kernelRegression[index - 1])
            {
                // UPTREND - Green
                KernelUp[index] = _kernelRegression[index];
                KernelUp[index - 1] = _kernelRegression[index - 1];
            }
            else
            {
                // DOWNTREND - Red
                KernelDown[index] = _kernelRegression[index];
                KernelDown[index - 1] = _kernelRegression[index - 1];
            }
        }

        private double GaussianKernel(double distance, double bandwidth)
        {
            // Gaussian (normal) kernel function
            // K(u) = (1/sqrt(2*pi)) * exp(-0.5 * u^2)
            double u = distance / bandwidth;
            return Math.Exp(-0.5 * u * u);
        }
    }
}
