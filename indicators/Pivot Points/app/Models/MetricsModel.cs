using System.Collections.Generic;

namespace cAlgo.Indicators
{
    public class MetricsModel
    {
        public Dictionary<double, LevelData> LevelMetrics { get; private set; }
        public bool HasOverlappingZones { get; set; }
        public double? ActiveLevel { get; set; }

        public MetricsModel()
        {
            LevelMetrics = new Dictionary<double, LevelData>();
            HasOverlappingZones = false;
            ActiveLevel = null;
        }

        public void Clear()
        {
            LevelMetrics.Clear();
            HasOverlappingZones = false;
            ActiveLevel = null;
        }

        public void InitializeLevel(double level)
        {
            if (!LevelMetrics.ContainsKey(level))
            {
                LevelMetrics[level] = new LevelData();
            }
        }

        public void AddBar(double level, bool isBullish)
        {
            if (LevelMetrics.ContainsKey(level))
            {
                if (isBullish)
                    LevelMetrics[level].BullishBars++;
                else
                    LevelMetrics[level].BearishBars++;
            }
        }

        public void AddVolume(double level, long buyVolume, long sellVolume)
        {
            if (LevelMetrics.ContainsKey(level))
            {
                LevelMetrics[level].BullishVolume += buyVolume;
                LevelMetrics[level].BearishVolume += sellVolume;
            }
        }

        public void AddPressure(double level, double buyPressure, double sellPressure)
        {
            if (LevelMetrics.ContainsKey(level))
            {
                LevelMetrics[level].BuyPressure += buyPressure;
                LevelMetrics[level].SellPressure += sellPressure;
            }
        }

        public void AddWastedEffort(double level, double rangePips, double bodyPips, bool isBullish)
        {
            if (LevelMetrics.ContainsKey(level))
            {
                double wastedPips = rangePips - bodyPips;

                if (isBullish)
                {
                    LevelMetrics[level].TotalBullRangePips += rangePips;
                    LevelMetrics[level].TotalBullBodyPips += bodyPips;
                    LevelMetrics[level].TotalBullWastedPips += wastedPips;
                }
                else
                {
                    LevelMetrics[level].TotalBearRangePips += rangePips;
                    LevelMetrics[level].TotalBearBodyPips += bodyPips;
                    LevelMetrics[level].TotalBearWastedPips += wastedPips;
                }
            }
        }
    }

    public class LevelData
    {
        // Bull range
        public double TotalBullRangePips { get; set; }
        public double TotalBullBodyPips { get; set; }

        // Bear range
        public double TotalBearRangePips { get; set; }
        public double TotalBearBodyPips { get; set; }

        // Bars
        public int BullishBars { get; set; }
        public int BearishBars { get; set; }
        public int BarsDelta => BullishBars - BearishBars;
        public int TotalBars => BullishBars + BearishBars;
        public double BarsDeltaPercentage => TotalBars > 0 ? ((double)BarsDelta / TotalBars) * 100 : 0;

        // Pressure
        public double BuyPressure { get; set; }
        public double SellPressure { get; set; }
        public double Delta => BuyPressure - SellPressure;
        public double TotalPressure => BuyPressure + SellPressure;
        public double DeltaPercentage => TotalPressure > 0 ? (Delta / TotalPressure) * 100 : 0;

        // Volume
        public long BullishVolume { get; set; }
        public long BearishVolume { get; set; }
        public long VolumeDelta => BullishVolume - BearishVolume;
        public long TotalVolume => BullishVolume + BearishVolume;
        public double VolumeDeltaPercentage => TotalVolume > 0 ? ((double)VolumeDelta / TotalVolume) * 100 : 0;

        // Dominance
        public string BarsDominance { get; set; }
        public bool BarsDominanceIsBullish { get; set; }
        public string VolumeDominance { get; set; }
        public bool VolumeDominanceIsBullish { get; set; }
        public string PressureDominance { get; set; }
        public bool PressureDominanceIsBullish { get; set; }

        // Efficiency
        public double BuyEfficiency => BullishVolume > 0 ? (BuyPressure / BullishVolume) * 1000 : 0;
        public double SellEfficiency => BearishVolume > 0 ? (SellPressure / BearishVolume) * 1000 : 0;
        public double TotalEfficiency => TotalVolume > 0 ? (TotalPressure / TotalVolume) * 1000 : 0;

        // Absorption
        public double BuyAbsorption => BuyPressure > 0 ? (BullishVolume / BuyPressure) : 0;
        public double SellAbsorption => SellPressure > 0 ? (BearishVolume / SellPressure) : 0;
        public double TotalAbsorption => TotalPressure > 0 ? (TotalVolume / TotalPressure) : 0;

        // Wasted Effort
        public double TotalBullWastedPips { get; set; }
        public double TotalBearWastedPips { get; set; }
        public double BuyWastedEffort => TotalBullRangePips > 0
            ? (TotalBullWastedPips / TotalBullRangePips) * 100
            : 0;
        public double SellWastedEffort => TotalBearRangePips > 0
            ? (TotalBearWastedPips / TotalBearRangePips) * 100
            : 0;
        public double TotalWastedEffort => (TotalBullRangePips + TotalBearRangePips) > 0
            ? ((TotalBullWastedPips + TotalBearWastedPips) / (TotalBullRangePips + TotalBearRangePips)) * 100
            : 0;

        // Conviction
        public double BuyConviction => (TotalBullRangePips > 0 && BullishVolume > 0)
            ? (TotalBullBodyPips / TotalBullRangePips) * (TotalBullBodyPips / BullishVolume) * 1000
            : 0;
        public double SellConviction => (TotalBearRangePips > 0 && BearishVolume > 0)
            ? (TotalBearBodyPips / TotalBearRangePips) * (TotalBearBodyPips / BearishVolume) * 1000
            : 0;
        public double TotalConviction => BuyConviction - SellConviction;
    }
}
