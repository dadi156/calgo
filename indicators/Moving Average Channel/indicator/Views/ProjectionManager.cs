using System;
using cAlgo.API;

namespace cAlgo.Indicators
{
    public class ProjectionManager
    {
        private readonly Chart _chart;
        private readonly MovingAverageChannel _indicator;
        private readonly DataManager _dataManager;

        public ProjectionManager(Chart chart, MovingAverageChannel indicator, DataManager dataManager)
        {
            _chart = chart;
            _indicator = indicator;
            _dataManager = dataManager;
        }

        // Draw projection lines (dashed lines)
        public void DrawProjectionLines()
        {
            // Get projection data from data manager
            var currentMTFTime = _dataManager.GetCurrentMTFTime();
            var nextMTFTime = _dataManager.GetNextMTFTime();

            if (currentMTFTime == default(DateTime) || nextMTFTime == default(DateTime))
                return;

            // Only draw projections from the latest MTF bar
            if (!IsLatestMTFBar(currentMTFTime))
                return;

            var projectionResult = _dataManager.CalculateProjection();
            var currentMTFResult = _dataManager.GetCurrentMTFResult();

            if (projectionResult == null || currentMTFResult == null || 
                !ValidationHelper.IsValidResult(currentMTFResult))
                return;

            try
            {
                // Remove old projection lines first
                RemoveOldProjectionLines();

                // Draw new projection lines (dashed) - 7 lines now
                DrawAllProjectionLines(currentMTFTime, nextMTFTime, currentMTFResult, projectionResult);
            }
            catch (Exception)
            {
                // If drawing fails, continue silently
            }
        }

        // Check if this is the latest MTF bar
        private bool IsLatestMTFBar(DateTime currentMTFTime)
        {
            var mtfBars = _dataManager.MTFBars;
            if (mtfBars == null || mtfBars.Count == 0)
                return false;

            // Find the latest MTF bar time
            var latestMTFTime = mtfBars.OpenTimes[mtfBars.Count - 1];
            
            // Only draw projections if current MTF time is the latest one
            return currentMTFTime == latestMTFTime;
        }

        // Draw 7 projection lines now (added Median)
        private void DrawAllProjectionLines(DateTime currentMTFTime, DateTime nextMTFTime, 
                                          MAResult currentMTFResult, MAResult projectionResult)
        {
            // High Line Projection
            if (_indicator.HighLine.LineOutput.IsVisible)
            {
                DrawSingleProjection("AMA_High_Proj_Current", currentMTFTime, currentMTFResult.HighMA,
                                  nextMTFTime, projectionResult.HighMA, 
                                  _indicator.HighLine.LineOutput.Color);
            }

            // Low Line Projection
            if (_indicator.LowLine.LineOutput.IsVisible)
            {
                DrawSingleProjection("AMA_Low_Proj_Current", currentMTFTime, currentMTFResult.LowMA,
                                  nextMTFTime, projectionResult.LowMA, 
                                  _indicator.LowLine.LineOutput.Color);
            }

            // Close Line Projection
            if (_indicator.CloseLine.LineOutput.IsVisible)
            {
                DrawSingleProjection("AMA_Close_Proj_Current", currentMTFTime, currentMTFResult.CloseMA,
                                  nextMTFTime, projectionResult.CloseMA, 
                                  _indicator.CloseLine.LineOutput.Color);
            }

            // Open Line Projection
            if (_indicator.OpenLine.LineOutput.IsVisible)
            {
                DrawSingleProjection("AMA_Open_Proj_Current", currentMTFTime, currentMTFResult.OpenMA,
                                  nextMTFTime, projectionResult.OpenMA, 
                                  _indicator.OpenLine.LineOutput.Color);
            }

            // NEW: Median Line Projection
            if (_indicator.MedianLine.LineOutput.IsVisible)
            {
                DrawSingleProjection("AMA_Median_Proj_Current", currentMTFTime, currentMTFResult.MedianMA,
                                  nextMTFTime, projectionResult.MedianMA, 
                                  _indicator.MedianLine.LineOutput.Color);
            }

            // Lower Reversion Zone Projection (was Fib382)
            if (_indicator.LowerReversionZone.LineOutput.IsVisible)
            {
                DrawSingleProjection("AMA_LowerReversion_Proj_Current", currentMTFTime, currentMTFResult.Fib382MA,
                                  nextMTFTime, projectionResult.Fib382MA, 
                                  _indicator.LowerReversionZone.LineOutput.Color);
            }

            // Upper Reversion Zone Projection (was Fib618)
            if (_indicator.UpperReversionZone.LineOutput.IsVisible)
            {
                DrawSingleProjection("AMA_UpperReversion_Proj_Current", currentMTFTime, currentMTFResult.Fib618MA,
                                  nextMTFTime, projectionResult.Fib618MA, 
                                  _indicator.UpperReversionZone.LineOutput.Color);
            }
        }

        // Draw one projection line (dashed)
        private void DrawSingleProjection(string lineName, DateTime time1, double y1, 
                                        DateTime time2, double y2, Color color)
        {
            // Draw the projection line
            var trendLine = _chart.DrawTrendLine(lineName, time1, y1, time2, y2, color);
            
            // Make it dashed and thicker
            if (trendLine != null)
            {
                trendLine.LineStyle = LineStyle.DotsRare;
                trendLine.Thickness = 2;
            }
        }

        // Remove old projection lines
        private void RemoveOldProjectionLines()
        {
            try
            {
                // Remove 7 projection lines now (added Median)
                _chart.RemoveObject("AMA_High_Proj_Current");
                _chart.RemoveObject("AMA_Low_Proj_Current");
                _chart.RemoveObject("AMA_Close_Proj_Current");
                _chart.RemoveObject("AMA_Open_Proj_Current");
                _chart.RemoveObject("AMA_Median_Proj_Current");  // NEW
                _chart.RemoveObject("AMA_LowerReversion_Proj_Current");
                _chart.RemoveObject("AMA_UpperReversion_Proj_Current");
            }
            catch (Exception)
            {
                // If removal fails, continue
            }
        }
    }
}
