using System;

namespace cAlgo
{
    public class MAFactory
    {
        // Creates and returns the appropriate moving average implementation based on type
        public MAInterface CreateMA(CustomMAType maType, MovingAveragesSuite indicator)
        {
            MAInterface ma;

            switch (maType)
            {
                case CustomMAType.ArnaudLegoux:
                    ma = new ArnaudLegouxMA(indicator);
                    break;
                case CustomMAType.DoubleExponential:
                    ma = new DoubleExponentialMA(indicator);
                    break;
                case CustomMAType.DoubleSmoothedExponential:
                    ma = new DoubleSmoothedEMA(indicator);
                    break;
                case CustomMAType.EhlersInstantaneous:
                    ma = new EhlersInstantaneousMA(indicator);
                    break;
                case CustomMAType.Exponential:
                    ma = new ExponentialMA(indicator);
                    break;
                case CustomMAType.ExponentialHull:
                    ma = new ExponentialHullMA(indicator);
                    break;
                case CustomMAType.FibonacciWeighted:
                    ma = new FibonacciWeightedMA(indicator);
                    break;
                case CustomMAType.FractalAdaptive:
                    ma = new FractalAdaptiveMA(indicator);
                    break;
                case CustomMAType.Gaussian:
                    ma = new GaussianMA(indicator);
                    break;
                case CustomMAType.Hull:
                    ma = new HullMA(indicator);
                    break;
                case CustomMAType.Jurik:
                    ma = new JurikMA(indicator);
                    break;
                case CustomMAType.KaufmanAdaptive:
                    ma = new KaufmanAdaptiveMA(indicator);
                    break;
                case CustomMAType.LaguerreFilter:
                    ma = new LaguerreFilterMA(indicator);
                    break;
                case CustomMAType.LinearRegression:
                    ma = new LinearRegressionMA(indicator);
                    break;
                case CustomMAType.MESAAdaptive:
                    ma = new MesaAdaptiveMA(indicator);
                    break;
                case CustomMAType.McGinleyDynamic:
                    ma = new McGinleyDynamicMA(indicator);
                    break;
                case CustomMAType.MomentumAdaptiveMA:
                    ma = new MomentumAdaptiveMA(indicator);
                    break;
                case CustomMAType.RegularizedExponential:
                    ma = new RegularizedEMA(indicator);
                    break;
                case CustomMAType.Running:
                    ma = new RunningMA(indicator);
                    break;
                case CustomMAType.SineWeighted:
                    ma = new SineWeightedMA(indicator);
                    break;
                case CustomMAType.Simple:
                    ma = new SimpleMA(indicator);
                    break;
                case CustomMAType.Tillson3:
                    ma = new Tillson3MovingAverage(indicator);
                    break;
                case CustomMAType.TripleExponential:
                    ma = new TripleExponentialMA(indicator);
                    break;
                case CustomMAType.TriangularHull:
                    ma = new TriangularHullMA(indicator);
                    break;
                case CustomMAType.VariableIndexDynamic:
                    ma = new VariableIndexDynamicMA(indicator);
                    break;
                case CustomMAType.VolumeWeighted:
                    ma = new VolumeWeightedMA(indicator);
                    break;
                case CustomMAType.Weighted:
                    ma = new WeightedMA(indicator);
                    break;
                case CustomMAType.ZeroLag:
                    ma = new ZeroLagEMA(indicator);
                    break;
                default:
                    // Default to SMA if unknown type specified
                    ma = new SimpleMA(indicator);
                    break;
            }

            // Initialize the MA implementation
            ma.Initialize();

            return ma;
        }
    }
}
