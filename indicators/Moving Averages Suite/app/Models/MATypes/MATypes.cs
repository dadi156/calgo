using System;
using cAlgo.API;

namespace cAlgo
{
    // Enum to define available moving average types
    public enum CustomMAType
    {
        ArnaudLegoux,               // Arnaud Legoux Moving Average
        DoubleExponential,          // Double Exponential Moving Average (DEMA)
        DoubleSmoothedExponential,  // Double Smoothed Exponential Moving Average
        EhlersInstantaneous,        // Ehlers Instantaneous Moving Average
        Exponential,                // Exponential Moving Average
        ExponentialHull,            // Exponential Hull Moving Average
        FibonacciWeighted,          // Fibonacci-weighted Moving Average
        FractalAdaptive,            // Fractal Adaptive Moving Average (FRAMA)
        Gaussian,                   // Gaussian Moving Average
        Hull,                       // Hull Moving Average
        Jurik,                      // Jurik Moving Average
        KaufmanAdaptive,            // Kaufman's Adaptive Moving Average (KAMA)
        LaguerreFilter,             // Laguerre Filter Moving Average
        LinearRegression,           // Linear Regression Moving Average
        MESAAdaptive,               // MESA Adaptive Moving Average
        McGinleyDynamic,            // McGinley Dynamic Moving Average
        MomentumAdaptiveMA,         // Momentum Adaptive Moving Average
        RegularizedExponential,     // Regularized EMA
        Running,                    // Running Moving Average
        SineWeighted,               // Sine Weighted Moving Average
        Simple,                     // Simple Moving Average
        Tillson3,                   // Tim Tillson's triple EMA Moving Average
        TripleExponential,          // Triple Exponential Moving Average
        TriangularHull,             // Triangular Hull Moving Average
        VariableIndexDynamic,       // Variable Index Dynamic Average (VIDYA)
        VolumeWeighted,             // Volume Weighted Moving Average (VWMA)
        Weighted,                   // Weighted Moving Average
        ZeroLag                     // Zero Lag EMA
    }
}
