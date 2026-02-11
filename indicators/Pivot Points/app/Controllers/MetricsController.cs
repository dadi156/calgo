using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class MetricsController
    {
        private readonly PivotPoints _indicator;
        private readonly MetricsModel _metricsModel;
        private readonly PivotMetricsView _metricsView;

        public MetricsController(PivotPoints indicator)
        {
            _indicator = indicator;
            _metricsModel = new MetricsModel();
            _metricsView = new PivotMetricsView(_indicator.Chart, _indicator.Symbol,
                _indicator.MetricsDefaultColumns); // Pass the parameter
        }

        public void Update(PeriodPivotPointsModel period)
        {
            _metricsModel.Clear();

            if (period == null || period.PivotData == null)
            {
                _metricsView.UpdateDisplay(_metricsModel, "", 0, _indicator.MetricsPanelDisplay,
                    ParseMargin(_indicator.MetricsPanelMargin), _indicator.ActiveLevelHighlight,
                    _indicator.MetricsButtonsPosition);
                return;
            }

            // Build list of all levels with their zones
            var levelZones = BuildLevelZones(period.PivotData);

            // Check for overlaps
            bool hasOverlap = DetectOverlap(levelZones);
            _metricsModel.HasOverlappingZones = hasOverlap;

            if (!hasOverlap)
            {
                // Initialize all levels
                InitializeLevels(levelZones);

                // Calculate metrics for the period
                CalculatePressures(period, levelZones);

                // Calculate dominance
                CalculateDominance(levelZones);

                // Detect active level
                DetectActiveLevel(levelZones, _indicator.Bars.ClosePrices.LastValue);
            }

            // Update display
            var margin = ParseMargin(_indicator.MetricsPanelMargin);
            _metricsView.UpdateDisplay(_metricsModel, period.PeriodName, GetPeriodBarCount(period),
                _indicator.MetricsPanelDisplay, margin, _indicator.ActiveLevelHighlight,
                _indicator.MetricsButtonsPosition);
        }

        private List<LevelZone> BuildLevelZones(PivotPointsData pivotData)
        {
            var zones = new List<LevelZone>();
            double zonePercent = _indicator.ZoneSizePercent / 100.0;

            // Pivot Point zone
            double r1Dist = pivotData.ResistanceLevels[0] - pivotData.PivotLevel;
            double s1Dist = pivotData.PivotLevel - pivotData.SupportLevels[0];
            zones.Add(new LevelZone
            {
                Price = pivotData.PivotLevel,
                Label = "PP",
                UpperBound = pivotData.PivotLevel + (r1Dist * zonePercent),
                LowerBound = pivotData.PivotLevel - (s1Dist * zonePercent)
            });

            // Resistance zones
            for (int i = 0; i < pivotData.LevelsToShow && i < pivotData.ResistanceLevels.Length; i++)
            {
                double rPrice = pivotData.ResistanceLevels[i];
                double lowerDist, upperDist;

                if (i == 0)
                    lowerDist = rPrice - pivotData.PivotLevel;
                else
                    lowerDist = rPrice - pivotData.ResistanceLevels[i - 1];

                if (i < pivotData.ResistanceLevels.Length - 1)
                    upperDist = pivotData.ResistanceLevels[i + 1] - rPrice;
                else
                    upperDist = lowerDist;

                zones.Add(new LevelZone
                {
                    Price = rPrice,
                    Label = $"R{i + 1}",
                    UpperBound = rPrice + (upperDist * zonePercent),
                    LowerBound = rPrice - (lowerDist * zonePercent)
                });
            }

            // Support zones
            for (int i = 0; i < pivotData.LevelsToShow && i < pivotData.SupportLevels.Length; i++)
            {
                double sPrice = pivotData.SupportLevels[i];
                double upperDist, lowerDist;

                if (i == 0)
                    upperDist = pivotData.PivotLevel - sPrice;
                else
                    upperDist = pivotData.SupportLevels[i - 1] - sPrice;

                if (i < pivotData.SupportLevels.Length - 1)
                    lowerDist = sPrice - pivotData.SupportLevels[i + 1];
                else
                    lowerDist = upperDist;

                zones.Add(new LevelZone
                {
                    Price = sPrice,
                    Label = $"S{i + 1}",
                    UpperBound = sPrice + (upperDist * zonePercent),
                    LowerBound = sPrice - (lowerDist * zonePercent)
                });
            }

            return zones;
        }

        private bool DetectOverlap(List<LevelZone> zones)
        {
            return _indicator.ZoneSizePercent > 50.0;
        }

        private void InitializeLevels(List<LevelZone> zones)
        {
            foreach (var zone in zones)
            {
                _metricsModel.InitializeLevel(zone.Price);
            }
        }

        private void CalculatePressures(PeriodPivotPointsModel period, List<LevelZone> zones)
        {
            var bars = _indicator.Bars;
            int startIndex = -1, endIndex = -1;

            // Find bar indices for the period
            for (int i = 0; i < bars.Count; i++)
            {
                if (bars.OpenTimes[i] >= period.StartTime && startIndex == -1)
                    startIndex = i;
                if (bars.OpenTimes[i] >= period.EndTime)
                {
                    endIndex = i - 1;
                    break;
                }
            }

            if (endIndex == -1)
                endIndex = bars.Count - 1;

            if (startIndex == -1 || endIndex < startIndex)
                return;

            // Process each bar in the period
            for (int i = startIndex; i <= endIndex; i++)
            {
                double open = bars.OpenPrices[i];
                double high = bars.HighPrices[i];
                double low = bars.LowPrices[i];
                double close = bars.ClosePrices[i];
                long volume = (long)bars.TickVolumes[i];

                foreach (var zone in zones)
                {
                    if (open >= zone.LowerBound && open <= zone.UpperBound)
                    {
                        bool isBullish = close > open;
                        double buyPressure, sellPressure;

                        if (isBullish)
                        {
                            buyPressure = (high - low) / _indicator.Symbol.PipSize;
                            sellPressure = ((high - close) + (open - low)) / _indicator.Symbol.PipSize;
                        }
                        else
                        {
                            sellPressure = (high - low) / _indicator.Symbol.PipSize;
                            buyPressure = ((high - open) + (close - low)) / _indicator.Symbol.PipSize;
                        }

                        double body = Math.Abs(close - open);
                        double upperWick = high - Math.Max(open, close);
                        double lowerWick = Math.Min(open, close) - low;
                        double range = high - low;

                        long buyVolume, sellVolume;

                        if (range > 0)
                        {
                            double bodyWeight = body / range;
                            double upperWickWeight = upperWick / range;
                            double lowerWickWeight = lowerWick / range;

                            if (isBullish)
                            {
                                buyVolume = (long)(volume * (bodyWeight + lowerWickWeight));
                                sellVolume = (long)(volume * upperWickWeight);
                            }
                            else
                            {
                                sellVolume = (long)(volume * (bodyWeight + upperWickWeight));
                                buyVolume = (long)(volume * lowerWickWeight);
                            }
                        }
                        else
                        {
                            buyVolume = volume / 2;
                            sellVolume = volume / 2;
                        }

                        _metricsModel.AddPressure(zone.Price, buyPressure, sellPressure);
                        _metricsModel.AddVolume(zone.Price, buyVolume, sellVolume);
                        _metricsModel.AddBar(zone.Price, isBullish);

                        double rangePips = (high - low) / _indicator.Symbol.PipSize;
                        double bodyPips = Math.Abs(close - open) / _indicator.Symbol.PipSize;
                        _metricsModel.AddWastedEffort(zone.Price, rangePips, bodyPips, isBullish);

                        break;
                    }
                }
            }
        }

        private void CalculateDominance(List<LevelZone> zones)
        {
            foreach (var zone in zones)
            {
                if (!_metricsModel.LevelMetrics.ContainsKey(zone.Price))
                    continue;

                var pressure = _metricsModel.LevelMetrics[zone.Price];

                // Bars Dominance
                if (pressure.BullishBars > 0 && pressure.BearishBars > 0)
                {
                    if (pressure.BullishBars >= pressure.BearishBars)
                    {
                        double ratio = (double)pressure.BullishBars / pressure.BearishBars;
                        pressure.BarsDominance = $"Buy {ratio:F1}x";
                        pressure.BarsDominanceIsBullish = true;
                    }
                    else
                    {
                        double ratio = (double)pressure.BearishBars / pressure.BullishBars;
                        pressure.BarsDominance = $"Sell {ratio:F1}x";
                        pressure.BarsDominanceIsBullish = false;
                    }
                }
                else if (pressure.BullishBars > 0)
                {
                    pressure.BarsDominance = "Buy Only";
                    pressure.BarsDominanceIsBullish = true;
                }
                else if (pressure.BearishBars > 0)
                {
                    pressure.BarsDominance = "Sell Only";
                    pressure.BarsDominanceIsBullish = false;
                }
                else
                {
                    pressure.BarsDominance = "-";
                }

                // Volume Dominance
                if (pressure.BullishVolume > 0 && pressure.BearishVolume > 0)
                {
                    if (pressure.BullishVolume >= pressure.BearishVolume)
                    {
                        double ratio = (double)pressure.BullishVolume / pressure.BearishVolume;
                        pressure.VolumeDominance = $"Buy {ratio:F1}x";
                        pressure.VolumeDominanceIsBullish = true;
                    }
                    else
                    {
                        double ratio = (double)pressure.BearishVolume / pressure.BullishVolume;
                        pressure.VolumeDominance = $"Sell {ratio:F1}x";
                        pressure.VolumeDominanceIsBullish = false;
                    }
                }
                else if (pressure.BullishVolume > 0)
                {
                    pressure.VolumeDominance = "Buy Only";
                    pressure.VolumeDominanceIsBullish = true;
                }
                else if (pressure.BearishVolume > 0)
                {
                    pressure.VolumeDominance = "Sell Only";
                    pressure.VolumeDominanceIsBullish = false;
                }
                else
                {
                    pressure.VolumeDominance = "-";
                }

                // Pressure Dominance
                if (pressure.BuyPressure > 0 && pressure.SellPressure > 0)
                {
                    if (pressure.BuyPressure >= pressure.SellPressure)
                    {
                        double ratio = pressure.BuyPressure / pressure.SellPressure;
                        pressure.PressureDominance = $"Buy {ratio:F1}x";
                        pressure.PressureDominanceIsBullish = true;
                    }
                    else
                    {
                        double ratio = pressure.SellPressure / pressure.BuyPressure;
                        pressure.PressureDominance = $"Sell {ratio:F1}x";
                        pressure.PressureDominanceIsBullish = false;
                    }
                }
                else if (pressure.BuyPressure > 0)
                {
                    pressure.PressureDominance = "Buy Only";
                    pressure.PressureDominanceIsBullish = true;
                }
                else if (pressure.SellPressure > 0)
                {
                    pressure.PressureDominance = "Sell Only";
                    pressure.PressureDominanceIsBullish = false;
                }
                else
                {
                    pressure.PressureDominance = "-";
                }
            }
        }

        private void DetectActiveLevel(List<LevelZone> zones, double currentPrice)
        {
            foreach (var zone in zones)
            {
                if (currentPrice >= zone.LowerBound && currentPrice <= zone.UpperBound)
                {
                    _metricsModel.ActiveLevel = zone.Price;
                    break;
                }
            }
        }

        private int GetPeriodBarCount(PeriodPivotPointsModel period)
        {
            var bars = _indicator.Bars;
            int count = 0;

            for (int i = 0; i < bars.Count; i++)
            {
                if (bars.OpenTimes[i] >= period.StartTime && bars.OpenTimes[i] < period.EndTime)
                    count++;
                else if (bars.OpenTimes[i] >= period.EndTime)
                    break;
            }

            return count;
        }

        private Thickness ParseMargin(string marginString)
        {
            try
            {
                var parts = marginString.Split(',');
                if (parts.Length != 4)
                    return new Thickness(0, 40, 0, 0);

                double left = double.Parse(parts[0].Trim());
                double top = double.Parse(parts[1].Trim());
                double right = double.Parse(parts[2].Trim());
                double bottom = double.Parse(parts[3].Trim());

                if (left < 0 || top < 0 || right < 0 || bottom < 0)
                    return new Thickness(0, 40, 0, 0);

                return new Thickness(left, top, right, bottom);
            }
            catch
            {
                return new Thickness(0, 40, 0, 0);
            }
        }
    }

    public class LevelZone
    {
        public double Price { get; set; }
        public string Label { get; set; }
        public double UpperBound { get; set; }
        public double LowerBound { get; set; }
    }
}
