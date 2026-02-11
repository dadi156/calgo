using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class VolumeSpreadAnalysis : Indicator
    {
        #region Core Calculations

        private double CalculateCloseLocation(int index)
        {
            double high = Bars.HighPrices[index];
            double low = Bars.LowPrices[index];
            double close = Bars.ClosePrices[index];

            if (high == low)
                return 0.5;

            return (close - low) / (high - low);
        }

        private double CalculateRangeEfficiency(int index)
        {
            double high = Bars.HighPrices[index];
            double low = Bars.LowPrices[index];
            double open = Bars.OpenPrices[index];
            double close = Bars.ClosePrices[index];

            double range = high - low;
            if (range == 0)
                return 0;

            return (close - open) / range;
        }

        private double CalculateTrimmedMeanVolume(int index)
        {
            int count = 0;

            // Fill buffer
            for (int i = index - LookbackPeriod; i < index; i++)
            {
                if (i >= 0)
                    _volumeBuffer[count++] = Bars.TickVolumes[i];
            }

            if (count < 3)
            {
                if (count == 0)
                    return Bars.TickVolumes[index];

                double sum = 0;
                for (int i = 0; i < count; i++)
                    sum += _volumeBuffer[i];
                return sum / count;
            }

            // Copy to sort buffer and sort
            Array.Copy(_volumeBuffer, _sortBuffer, count);
            Array.Sort(_sortBuffer, 0, count);

            // Calculate trim count
            int trimCount = (int)Math.Floor(count * TrimPercent / 100.0);
            trimCount = Math.Max(trimCount, 1);

            // Calculate trimmed mean manually
            int trimmedCount = count - 2 * trimCount;
            if (trimmedCount <= 0)
            {
                double sum = 0;
                for (int i = 0; i < count; i++)
                    sum += _sortBuffer[i];
                return sum / count;
            }

            double trimmedSum = 0;
            for (int i = trimCount; i < count - trimCount; i++)
                trimmedSum += _sortBuffer[i];

            return trimmedSum / trimmedCount;
        }

        private double CalculateSpreadPercentile(int index, double currentSpread)
        {
            int countLower = 0;

            for (int i = index - LookbackPeriod + 1; i < index; i++)
            {
                if (i >= 0)
                {
                    double spread = Bars.HighPrices[i] - Bars.LowPrices[i];
                    if (spread < currentSpread)
                        countLower++;
                }
            }

            return (double)countLower / (LookbackPeriod - 1) * 100;
        }

        #endregion
    }
}
