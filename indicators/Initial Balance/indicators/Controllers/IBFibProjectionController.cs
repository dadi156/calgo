using System;
using cAlgo.API;

namespace cAlgo
{
    public class IBFibProjectionController
    {
        private readonly InitialBalance _indicator;
        private readonly IBFibProjectionModel _projectionModel;
        private readonly IBFibProjectionView _projectionView;

        public IBFibProjectionController(InitialBalance indicator, 
            IBFibProjectionModel projectionModel, 
            IBFibProjectionView projectionView)
        {
            _indicator = indicator;
            _projectionModel = projectionModel;
            _projectionView = projectionView;
        }

        public void Update(DateTime startTime, DateTime endTime, double ibHigh, double ibLow)
        {
            // Check if projection is enabled
            if (!_indicator.ShowFibLevels || _indicator.FibProjection == FibProjectionMode.None)
            {
                _projectionView.Clear();
                return;
            }

            // Reset and calculate projection levels
            _projectionModel.Reset();
            double range = ibHigh - ibLow;

            // Calculate based on projection mode
            bool calculateUpward = _indicator.FibProjection == FibProjectionMode.Upward || 
                                   _indicator.FibProjection == FibProjectionMode.Both;
            bool calculateDownward = _indicator.FibProjection == FibProjectionMode.Downward || 
                                     _indicator.FibProjection == FibProjectionMode.Both;

            if (calculateUpward)
            {
                _projectionModel.CalculateUpwardProjection(ibHigh, range);
            }

            if (calculateDownward)
            {
                _projectionModel.CalculateDownwardProjection(ibLow, range);
            }

            // Draw projection lines
            _projectionView.DrawProjectionLines(
                startTime,
                endTime,
                _projectionModel,
                calculateUpward,
                calculateDownward,
                _indicator.Show_Fib_11_40,
                _indicator.Show_Fib_23_6,
                _indicator.Show_Fib_38_2,
                _indicator.Show_Fib_50,
                _indicator.Show_Fib_61_8,
                _indicator.Show_Fib_78_6,
                _indicator.Show_Fib_88_60
            );
        }

        public void Clear()
        {
            _projectionView.Clear();
        }
    }
}
