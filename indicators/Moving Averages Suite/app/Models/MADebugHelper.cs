using System;
using System.Text;
using cAlgo.API;

namespace cAlgo
{
    // Helper class for debugging moving averages
    public class MADebugHelper
    {
        private readonly MovingAveragesSuite _indicator;
        private StringBuilder _debugLog;
        private bool _enabled;
        private int _maxLogSize = 10000;
        
        public MADebugHelper(MovingAveragesSuite indicator, bool enabled = true)
        {
            _indicator = indicator;
            _debugLog = new StringBuilder();
            _enabled = enabled;
        }
        
        public void Enable()
        {
            _enabled = true;
        }
        
        public void Disable()
        {
            _enabled = false;
        }
        
        public void Clear()
        {
            _debugLog.Clear();
        }
        
        public void Log(string message)
        {
            if (!_enabled) return;
            
            // Prevent log from getting too large
            if (_debugLog.Length > _maxLogSize)
            {
                _debugLog.Clear();
                _debugLog.AppendLine("--- Debug log reset due to size limit ---");
            }
            
            _debugLog.AppendLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
        
        public void LogCalculation(int index, CustomMAType maType, double value)
        {
            if (!_enabled) return;
            
            // Only log a subset of calculations to avoid performance issues
            if (index % 100 == 0 || index == _indicator.Bars.Count - 1)
            {
                Log($"{maType} at index {index}: Value = {value:F5}, Source = {_indicator.Source[index]:F5}");
            }
        }
        
        public void LogParameters(CustomMAType maType)
        {
            if (!_enabled) return;
            
            Log($"--- Parameters for {maType} ---");
            Log($"Period: {_indicator.Period}");
            
            switch (maType)
            {
                case CustomMAType.ArnaudLegoux:
                    Log($"Offset: {_indicator.Offset}");
                    Log($"Sigma: {_indicator.Sigma}");
                    break;
                case CustomMAType.MESAAdaptive:
                    Log($"Fast Limit: {_indicator.FastLimit}");
                    Log($"Slow Limit: {_indicator.SlowLimit}");
                    break;
                case CustomMAType.Jurik:
                    Log($"Phase: {_indicator.Phase}");
                    break;
                case CustomMAType.RegularizedExponential:
                    Log($"Lambda: {_indicator.Lambda}");
                    break;
                case CustomMAType.EhlersInstantaneous:
                    Log($"Alpha: {_indicator.Alpha}");
                    break;
            }
        }
        
        public void LogError(string method, Exception ex)
        {
            if (!_enabled) return;
            
            Log($"ERROR in {method}: {ex.Message}");
            Log($"Stack trace: {ex.StackTrace}");
        }
        
        public void DisplayOnChart()
        {
            if (!_enabled || _debugLog.Length == 0) return;
            
            try
            {
                _indicator.Chart.DrawStaticText(
                    "MADebugInfo", 
                    _debugLog.ToString(),
                    VerticalAlignment.Top,
                    HorizontalAlignment.Left,
                    Color.Red
                );
            }
            catch (Exception ex)
            {
                // If we can't display on chart, at least log the error
                _debugLog.AppendLine($"ERROR displaying debug info: {ex.Message}");
            }
        }
    }
}
