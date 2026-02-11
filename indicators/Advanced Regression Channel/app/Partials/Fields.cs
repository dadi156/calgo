using System;
using cAlgo.API;

namespace cAlgo
{
    public partial class AdvancedRegressionChannel : Indicator
    {
        private RegressionController _controller;
        private ChannelConfig _config;
        private OutputCollection _outputs;

        // Track parameter changes
        private int _lastPeriod;
        private RegressionType _lastRegressionType;
        private int _lastDegree;
        private double _lastChannelWidth;
        private bool _lastUseMultiTimeframe;
        private TimeFrame _lastSelectedTimeFrame;
        private bool _lastExtendToInfinity;
        private string _lastStartDateStr;
        private string _lastEndDateStr;
        private RegressionMode _lastRegressionMode;
        private bool _lastUseDateRange;
        private DateTime _startDate;
        private DateTime _endDate;

        // Track last processed bar index
        private int _lastProcessedIndex = -1;

        private int _serverToUserOffset;
    }
}
