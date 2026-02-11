using System;
using cAlgo.API;

namespace cAlgo
{
    public class IBFibController
    {
        private readonly InitialBalance _indicator;
        private readonly IBFibModel _fibModel;
        private readonly IBFibView _fibView;

        public IBFibController(InitialBalance indicator, IBFibModel fibModel, IBFibView fibView)
        {
            _indicator = indicator;
            _fibModel = fibModel;
            _fibView = fibView;
        }

        public void Update(DateTime startTime, DateTime endTime, double ibHigh, double ibLow)
        {
            // Check if Fib levels are enabled
            if (!_indicator.ShowFibLevels)
            {
                _fibView.Clear();
                return;
            }

            // Reset and calculate fib levels
            _fibModel.Reset();
            _fibModel.CalculateFibLevels(ibHigh, ibLow);

            // Draw fib lines
            _fibView.DrawFibLines(
                startTime, 
                endTime, 
                _fibModel,
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
            _fibView.Clear();
        }
    }
}
