using System.Collections.Generic;
using System.Linq;

namespace cAlgo.Indicators
{
    public class PivotMetricsDataProvider : IMetricsDataProvider<PivotMetricsData>
    {
        private readonly MetricsModel _model;
        private readonly string _periodName;
        private readonly int _barCount;

        public PivotMetricsDataProvider(MetricsModel model, string periodName, int barCount)
        {
            _model = model;
            _periodName = periodName;
            _barCount = barCount;
        }

        public List<PivotMetricsData> GetSortedData()
        {
            return _model.LevelMetrics
                .Where(kvp => kvp.Value.TotalBars > 0)
                .OrderByDescending(kvp => kvp.Key)
                .Select(kvp => new PivotMetricsData
                {
                    Level = kvp.Key,
                    Pressure = kvp.Value,
                    IsActive = _model.ActiveLevel.HasValue && 
                               System.Math.Abs(kvp.Key - _model.ActiveLevel.Value) < 0.00001
                })
                .ToList();
        }

        public bool HasData => _model.LevelMetrics.Count > 0 && 
                                _model.LevelMetrics.Any(kvp => kvp.Value.TotalBars > 0);

        public string GetPanelTitle()
        {
            return $"PIVOT METRICS | {_periodName} | {_barCount} Bars";
        }

        public bool ShouldShowWarning(out string warningMessage)
        {
            if (_model.HasOverlappingZones)
            {
                warningMessage = "⚠️ Zone overlap.\nZone Size % should be ≤ 50%.";
                return true;
            }

            warningMessage = null;
            return false;
        }
    }

    public class PivotMetricsData
    {
        public double Level { get; set; }
        public LevelData Pressure { get; set; }
        public bool IsActive { get; set; }
    }
}
