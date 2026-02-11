using System;
using cAlgo.API;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public partial class InitialBalance : Indicator
    {
        private IBController _controller;
        private IBFibController _fibController;
        private IBFibProjectionController _projectionController;
        private int _serverToUserOffset;

        protected override void Initialize()
        {
            // Auto-detect offset between server time and user local time
            DateTime utcNow = Server.TimeInUtc;
            DateTime serverNow = Server.Time;
            DateTime userLocalNow = utcNow.AddHours(UserTimezone);

            _serverToUserOffset = (int)System.Math.Round((serverNow - userLocalNow).TotalHours);

            var model = new IBModel();
            var view = new IBHighLowTrendlines(Chart, HighLineColor, LowLineColor, LineThickness, LineStyle, ShowLabels);
            _controller = new IBController(this, model, view, MarketData, Bars, _serverToUserOffset, Chart);

            // Initialize Fibonacci controller
            var fibModel = new IBFibModel();
            var fibView = new IBFibView(Chart, FibLineColor, FibLineThickness, FibLineStyle, ShowLabels);
            _fibController = new IBFibController(this, fibModel, fibView);

            // Initialize Fibonacci Projection controller
            var projectionModel = new IBFibProjectionModel();
            var projectionView = new IBFibProjectionView(Chart, 
                FibLineColor, FibLineThickness, FibLineStyle, ShowLabels,
                HighLineColor, LowLineColor, LineThickness, LineStyle);
            _projectionController = new IBFibProjectionController(this, projectionModel, projectionView);
        }

        public override void Calculate(int index)
        {
            _controller.Update(index);
        }
    }
}
