using System;
using System.Collections.Generic;

namespace cAlgo
{
    /// <summary>
    /// Arnaud Legoux Moving Average Calculator
    /// Gaussian-weighted, offset-controlled responsiveness
    /// </summary>
    public class ALMACalculator : IMACalculator
    {
        private List<double> values;
        private const double Offset = 0.85;
        private const double Sigma = 6.0;

        public double Calculate(double currentValue, int period, int index, double previousValue, StateManager stateManager)
        {
            if (stateManager.FirstValidBar)
            {
                values = new List<double>();
                values.Add(currentValue);
                stateManager.LastCalculatedIndex = index;
                stateManager.FirstValidBar = false;
                return currentValue;
            }

            if (index == stateManager.LastCalculatedIndex)
            {
                if (values.Count > 0)
                    values[values.Count - 1] = currentValue;
            }
            else
            {
                values.Add(currentValue);
                stateManager.LastCalculatedIndex = index;
            }

            if (values.Count > period)
            {
                int itemsToRemove = values.Count - period;
                values.RemoveRange(0, itemsToRemove);
            }

            return CalculateALMA(period);
        }

        private double CalculateALMA(int period)
        {
            if (values.Count == 0) return 0;

            double m = Offset * (period - 1);
            double s = period / Sigma;

            double weightSum = 0;
            double sum = 0;

            for (int i = 0; i < values.Count; i++)
            {
                double weight = Math.Exp(-((i - m) * (i - m)) / (2 * s * s));
                sum += values[i] * weight;
                weightSum += weight;
            }

            return weightSum > 0 ? sum / weightSum : values[values.Count - 1];
        }

        public void Reset()
        {
            values?.Clear();
            values = null;
        }

        public string GetName() => "Arnaud Legoux Moving Average";
    }
}
