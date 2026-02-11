using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    public class IBController
    {
        private readonly InitialBalance _indicator;
        private readonly IBModel _model;
        private readonly IBHighLowTrendlines _view;
        private readonly MarketData _marketData;
        private readonly Bars _mainBars;
        private readonly int _serverToUserOffset;
        private readonly Chart _chart;

        private Bars _periodBars;
        private Bars _ibCalculationBars;
        private int _lastProcessedBarIndex = -1;
        private int _currentPeriodBarIndex;

        private const string WarningTextName = "IB_CustomRange_Warning";

        public IBController(InitialBalance indicator, IBModel model, IBHighLowTrendlines view,
            MarketData marketData, Bars mainBars, int serverToUserOffset, Chart chart)
        {
            _indicator = indicator;
            _model = model;
            _view = view;
            _marketData = marketData;
            _mainBars = mainBars;
            _serverToUserOffset = serverToUserOffset;
            _chart = chart;

            InitializePeriodBars();
        }

        private void InitializePeriodBars()
        {
            TimeFrame periodTimeFrame = _indicator.PeriodType switch
            {
                IBPeriodType.Daily => TimeFrame.Daily,
                IBPeriodType.Weekly => TimeFrame.Weekly,
                IBPeriodType.Monthly => TimeFrame.Monthly,
                IBPeriodType.Quarterly => TimeFrame.Monthly,
                IBPeriodType.FourMonthly => TimeFrame.Monthly,
                IBPeriodType.SemiAnnual => TimeFrame.Monthly,
                IBPeriodType.Yearly => TimeFrame.Monthly,
                IBPeriodType.CustomRange => TimeFrame.Daily, // Placeholder, not used for custom
                _ => TimeFrame.Daily
            };

            _periodBars = _marketData.GetBars(periodTimeFrame);
            _ibCalculationBars = GetCalculationBars();
        }

        private Bars GetCalculationBars()
        {
            var chartTF = _mainBars.TimeFrame;

            TimeFrame calcTF;

            if (_indicator.PeriodType == IBPeriodType.CustomRange)
            {
                // Auto-select timeframe based on custom range duration
                calcTF = GetCustomRangeCalculationTimeFrame();
            }
            else
            {
                calcTF = _indicator.PeriodType switch
                {
                    IBPeriodType.Daily => GetSmallerTimeFrame(chartTF, TimeFrame.Hour, TimeFrame.Minute15, TimeFrame.Minute5),
                    IBPeriodType.Weekly => GetSmallerTimeFrame(chartTF, TimeFrame.Daily, TimeFrame.Hour4, TimeFrame.Hour),
                    IBPeriodType.Monthly => GetSmallerTimeFrame(chartTF, TimeFrame.Daily, TimeFrame.Hour4, TimeFrame.Hour),
                    IBPeriodType.Quarterly => GetSmallerTimeFrame(chartTF, TimeFrame.Weekly, TimeFrame.Daily, TimeFrame.Hour4),
                    IBPeriodType.FourMonthly => GetSmallerTimeFrame(chartTF, TimeFrame.Weekly, TimeFrame.Daily, TimeFrame.Hour4),
                    IBPeriodType.SemiAnnual => GetSmallerTimeFrame(chartTF, TimeFrame.Weekly, TimeFrame.Daily, TimeFrame.Hour4),
                    IBPeriodType.Yearly => GetSmallerTimeFrame(chartTF, TimeFrame.Monthly, TimeFrame.Weekly, TimeFrame.Daily),
                    _ => TimeFrame.Daily
                };
            }

            return _marketData.GetBars(calcTF);
        }

        private TimeFrame GetCustomRangeCalculationTimeFrame()
        {
            // Parse custom range string
            if (!TryParseCustomRange(out DateTime startLocal, out DateTime endLocal))
            {
                // If parsing fails, return default timeframe
                return TimeFrame.Hour;
            }

            // Calculate duration
            TimeSpan duration = endLocal - startLocal;
            double durationDays = duration.TotalDays;

            // Auto-select based on duration
            if (durationDays < 1)
                return TimeFrame.Minute15;  // Less than 1 day: use 15-minute bars
            else if (durationDays <= 7)
                return TimeFrame.Hour;       // 1-7 days: use hourly bars
            else if (durationDays <= 30)
                return TimeFrame.Hour4;      // 7-30 days: use 4-hour bars
            else
                return TimeFrame.Daily;      // More than 30 days: use daily bars
        }

        private TimeFrame GetSmallerTimeFrame(TimeFrame chartTF, TimeFrame preferred, TimeFrame fallback1, TimeFrame fallback2)
        {
            // Use any timeframe that is DIFFERENT from chart TF to avoid data loading issues
            int chartMinutes = GetTimeFrameMinutes(chartTF);

            if (GetTimeFrameMinutes(preferred) != chartMinutes)
                return preferred;
            if (GetTimeFrameMinutes(fallback1) != chartMinutes)
                return fallback1;
            if (GetTimeFrameMinutes(fallback2) != chartMinutes)
                return fallback2;

            return TimeFrame.Minute;
        }

        private int GetTimeFrameMinutes(TimeFrame tf)
        {
            if (tf == TimeFrame.Minute) return 1;
            if (tf == TimeFrame.Minute2) return 2;
            if (tf == TimeFrame.Minute3) return 3;
            if (tf == TimeFrame.Minute4) return 4;
            if (tf == TimeFrame.Minute5) return 5;
            if (tf == TimeFrame.Minute10) return 10;
            if (tf == TimeFrame.Minute15) return 15;
            if (tf == TimeFrame.Minute20) return 20;
            if (tf == TimeFrame.Minute30) return 30;
            if (tf == TimeFrame.Minute45) return 45;
            if (tf == TimeFrame.Hour) return 60;
            if (tf == TimeFrame.Hour2) return 120;
            if (tf == TimeFrame.Hour3) return 180;
            if (tf == TimeFrame.Hour4) return 240;
            if (tf == TimeFrame.Hour6) return 360;
            if (tf == TimeFrame.Hour8) return 480;
            if (tf == TimeFrame.Hour12) return 720;
            if (tf == TimeFrame.Daily) return 1440;
            if (tf == TimeFrame.Day2) return 2880;
            if (tf == TimeFrame.Day3) return 4320;
            if (tf == TimeFrame.Weekly) return 10080;
            if (tf == TimeFrame.Monthly) return 43200;
            return 60;
        }

        private bool TryParseCustomRange(out DateTime startLocal, out DateTime endLocal)
        {
            startLocal = DateTime.MinValue;
            endLocal = DateTime.MinValue;

            try
            {
                // Split by comma
                string[] parts = _indicator.CustomRange.Split(',');
                if (parts.Length != 2)
                    return false;

                string startStr = parts[0].Trim();
                string endStr = parts[1].Trim();

                // Try multiple formats to support both "03/09/2025 08:00" and "3/9/2025 8:00"
                string[] formats = new[]
                {
                    "dd/MM/yyyy HH:mm",   // 03/09/2025 08:00
                    "d/M/yyyy H:mm",      // 3/9/2025 8:00
                    "dd/MM/yyyy H:mm",    // 03/09/2025 8:00
                    "d/M/yyyy HH:mm",     // 3/9/2025 08:00
                    "d/MM/yyyy H:mm",     // 3/09/2025 8:00
                    "dd/M/yyyy H:mm",     // 03/9/2025 8:00
                    "d/MM/yyyy HH:mm",    // 3/09/2025 08:00
                    "dd/M/yyyy HH:mm"     // 03/9/2025 08:00
                };

                // Parse start datetime
                if (!DateTime.TryParseExact(startStr, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out startLocal))
                    return false;

                // Parse end datetime
                if (!DateTime.TryParseExact(endStr, formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out endLocal))
                    return false;

                // Check if start is after end (invalid)
                if (startLocal >= endLocal)
                {
                    // Set special marker to show different error
                    startLocal = DateTime.MaxValue;
                    endLocal = DateTime.MaxValue;
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ShowCustomRangeWarning()
        {
            string warningText = "INVALID FORMAT: Use \"DD/MM/YYYY HH:mm, DD/MM/YYYY HH:mm\" or \"D/M/YYYY H:mm, D/M/YYYY H:mm\"";
            var text = _chart.DrawStaticText(WarningTextName, warningText, 
                VerticalAlignment.Top, HorizontalAlignment.Center, Color.Red);
            text.FontFamily = "Consolas";
            text.FontSize = 11;
        }

        private void ShowInvalidDateOrderWarning()
        {
            string warningText = "INVALID RANGE: Start datetime must be BEFORE end datetime";
            var text = _chart.DrawStaticText(WarningTextName, warningText, 
                VerticalAlignment.Top, HorizontalAlignment.Center, Color.Red);
            text.FontFamily = "Consolas";
            text.FontSize = 11;
        }

        private void ClearCustomRangeWarning()
        {
            _chart.RemoveObject(WarningTextName);
        }

        private int GetPeriodMultiplier()
        {
            // Returns how many bars to skip per offset unit
            // For periods using Monthly bars as source, multiply by months in period
            return _indicator.PeriodType switch
            {
                IBPeriodType.Daily => 1,        // Uses Daily bars
                IBPeriodType.Weekly => 1,       // Uses Weekly bars
                IBPeriodType.Monthly => 1,      // Uses Monthly bars
                IBPeriodType.Quarterly => 3,    // Uses Monthly bars, 3 months per quarter
                IBPeriodType.FourMonthly => 4,  // Uses Monthly bars, 4 months per period
                IBPeriodType.SemiAnnual => 6,   // Uses Monthly bars, 6 months per half-year
                IBPeriodType.Yearly => 12,      // Uses Monthly bars, 12 months per year
                _ => 1
            };
        }

        private DateTime CalculateExtendedEndTime()
        {
            int extendLines = _indicator.ExtendLines;

            // ExtendLines = 0: No extension, use IB end time
            if (extendLines == 0)
                return _model.IBEnd;

            // ExtendLines = -1: Extend to current forming bar
            if (extendLines == -1)
                return _indicator.Server.Time;

            // ExtendLines > 0: Extend forward by N periods from current period end
            // ExtendLines = 1 means show up to end of current period
            // ExtendLines = 2 means show current period + 1 more period, etc.
            
            // For CustomRange period, extend by repeating the custom range duration
            if (_indicator.PeriodType == IBPeriodType.CustomRange)
            {
                TimeSpan customDuration = _model.CurrentPeriodEnd - _model.CurrentPeriodStart;
                return _model.CurrentPeriodEnd.Add(TimeSpan.FromTicks(customDuration.Ticks * (extendLines - 1)));
            }

            DateTime extendedTime = _model.CurrentPeriodEnd;

            // Add (ExtendLines - 1) additional periods
            int additionalPeriods = extendLines - 1;

            switch (_indicator.PeriodType)
            {
                case IBPeriodType.Daily:
                    extendedTime = extendedTime.AddDays(additionalPeriods);
                    break;

                case IBPeriodType.Weekly:
                    extendedTime = extendedTime.AddDays(additionalPeriods * 7);
                    break;

                case IBPeriodType.Monthly:
                    extendedTime = extendedTime.AddMonths(additionalPeriods);
                    break;

                case IBPeriodType.Quarterly:
                    extendedTime = extendedTime.AddMonths(additionalPeriods * 3);
                    break;

                case IBPeriodType.FourMonthly:
                    extendedTime = extendedTime.AddMonths(additionalPeriods * 4);
                    break;

                case IBPeriodType.SemiAnnual:
                    extendedTime = extendedTime.AddMonths(additionalPeriods * 6);
                    break;

                case IBPeriodType.Yearly:
                    extendedTime = extendedTime.AddYears(additionalPeriods);
                    break;
            }

            return extendedTime;
        }

        public void Update(int index)
        {
            if (index < _mainBars.Count - 1)
                return;

            // For CustomRange period, skip offset logic and process every update
            if (_indicator.PeriodType == IBPeriodType.CustomRange)
            {
                _model.Reset();
                CalculateCurrentPeriod();
                CalculateIBRange();

                // Check if parsing was successful by checking if times are valid
                if (_model.CurrentPeriodStart == DateTime.MinValue || _model.CurrentPeriodEnd == DateTime.MinValue)
                {
                    // Parsing failed - show format warning and don't draw lines
                    ShowCustomRangeWarning();
                    _view.Clear();
                    return;
                }
                else if (_model.CurrentPeriodStart == DateTime.MaxValue || _model.CurrentPeriodEnd == DateTime.MaxValue)
                {
                    // Date order is invalid - show date order warning and don't draw lines
                    ShowInvalidDateOrderWarning();
                    _view.Clear();
                    return;
                }

                // Parsing succeeded - clear any existing warning
                ClearCustomRangeWarning();

                if (!double.IsNaN(_model.IBHigh) && !double.IsNaN(_model.IBLow))
                {
                    DateTime endTime = CalculateExtendedEndTime();

                    _view.DrawLines(_model.IBStart, endTime,
                        _model.IBHigh, _model.IBLow);
                    _model.IsCalculated = true;

                    _indicator.UpdateFibLevels(_model.IBStart, endTime, _model.IBHigh, _model.IBLow);
                }
                return;
            }

            // Clear custom range warning for non-custom periods
            ClearCustomRangeWarning();

            // Calculate period bar index with offset
            // Multiply offset by period length to jump by complete periods
            int adjustedOffset = _indicator.PeriodOffset * GetPeriodMultiplier();
            _currentPeriodBarIndex = _periodBars.Count - 1 + adjustedOffset;

            // Validate the index is within available data
            if (_currentPeriodBarIndex < 0 || _currentPeriodBarIndex >= _periodBars.Count)
                return;

            if (_currentPeriodBarIndex == _lastProcessedBarIndex && _model.IsCalculated)
                return;

            _lastProcessedBarIndex = _currentPeriodBarIndex;
            _model.Reset();

            CalculateCurrentPeriod();
            CalculateIBRange();

            if (!double.IsNaN(_model.IBHigh) && !double.IsNaN(_model.IBLow))
            {
                // Calculate end time based on ExtendLines parameter
                DateTime endTime = CalculateExtendedEndTime();

                _view.DrawLines(_model.IBStart, endTime,
                    _model.IBHigh, _model.IBLow);
                _model.IsCalculated = true;

                // Update Fibonacci levels
                _indicator.UpdateFibLevels(_model.IBStart, endTime, _model.IBHigh, _model.IBLow);
            }
        }

        private DateTime ConvertServerToUserLocal(DateTime serverTime)
        {
            return serverTime.AddHours(-_serverToUserOffset);
        }

        private DateTime ConvertUserLocalToServer(DateTime userLocalTime)
        {
            return userLocalTime.AddHours(_serverToUserOffset);
        }

        private void CalculateCurrentPeriod()
        {
            // Handle CustomRange period type
            if (_indicator.PeriodType == IBPeriodType.CustomRange)
            {
                // Parse custom range string
                if (TryParseCustomRange(out DateTime startLocal, out DateTime endLocal))
                {
                    // Convert to server time
                    _model.CurrentPeriodStart = ConvertUserLocalToServer(startLocal);
                    _model.CurrentPeriodEnd = ConvertUserLocalToServer(endLocal);
                }
                else
                {
                    // If parsing fails, preserve the error marker values
                    // DateTime.MinValue = format error
                    // DateTime.MaxValue = date order error
                    _model.CurrentPeriodStart = startLocal;
                    _model.CurrentPeriodEnd = endLocal;
                }
                return;
            }

            var currentBar = _periodBars[_currentPeriodBarIndex];
            DateTime serverPeriodTime = currentBar.OpenTime;

            switch (_indicator.PeriodType)
            {
                case IBPeriodType.Daily:
                    {
                        // Convert to user local time
                        DateTime userLocalTime = ConvertServerToUserLocal(serverPeriodTime);
                        // Get midnight in user local time
                        DateTime userLocalMidnight = userLocalTime.Date;
                        // Convert back to server time
                        _model.CurrentPeriodStart = ConvertUserLocalToServer(userLocalMidnight);
                        _model.CurrentPeriodEnd = _model.CurrentPeriodStart.AddDays(1);
                    }
                    break;

                case IBPeriodType.Weekly:
                    {
                        // Convert to user local time
                        DateTime userLocalTime = ConvertServerToUserLocal(serverPeriodTime);
                        // Get midnight in user local time
                        DateTime userLocalMidnight = userLocalTime.Date;
                        // Get Monday in user local time
                        int daysFromMonday = (int)userLocalMidnight.DayOfWeek - (int)DayOfWeek.Monday;
                        if (daysFromMonday < 0) daysFromMonday += 7;
                        DateTime userLocalMonday = userLocalMidnight.AddDays(-daysFromMonday);
                        // Convert back to server time
                        _model.CurrentPeriodStart = ConvertUserLocalToServer(userLocalMonday);
                        _model.CurrentPeriodEnd = _model.CurrentPeriodStart.AddDays(7);
                    }
                    break;

                case IBPeriodType.Monthly:
                    {
                        // Convert to user local time
                        DateTime userLocalTime = ConvertServerToUserLocal(serverPeriodTime);
                        // Get first day of month in user local time
                        DateTime userLocalMonthStart = new DateTime(userLocalTime.Year, userLocalTime.Month, 1);
                        // Convert back to server time
                        _model.CurrentPeriodStart = ConvertUserLocalToServer(userLocalMonthStart);
                        _model.CurrentPeriodEnd = _model.CurrentPeriodStart.AddMonths(1);
                    }
                    break;

                case IBPeriodType.Yearly:
                    {
                        // Convert to user local time
                        DateTime userLocalTime = ConvertServerToUserLocal(serverPeriodTime);
                        // Get January 1st in user local time
                        DateTime userLocalYearStart = new DateTime(userLocalTime.Year, 1, 1);
                        // Convert back to server time
                        _model.CurrentPeriodStart = ConvertUserLocalToServer(userLocalYearStart);
                        _model.CurrentPeriodEnd = _model.CurrentPeriodStart.AddYears(1);
                    }
                    break;

                case IBPeriodType.Quarterly:
                    {
                        // Convert to user local time
                        DateTime userLocalTime = ConvertServerToUserLocal(serverPeriodTime);
                        // Get quarter start month (Jan=1, Apr=4, Jul=7, Oct=10)
                        int quarterStartMonth = ((userLocalTime.Month - 1) / 3) * 3 + 1;
                        DateTime userLocalQuarterStart = new DateTime(userLocalTime.Year, quarterStartMonth, 1);
                        // Convert back to server time
                        _model.CurrentPeriodStart = ConvertUserLocalToServer(userLocalQuarterStart);
                        _model.CurrentPeriodEnd = _model.CurrentPeriodStart.AddMonths(3);
                    }
                    break;

                case IBPeriodType.FourMonthly:
                    {
                        // Convert to user local time
                        DateTime userLocalTime = ConvertServerToUserLocal(serverPeriodTime);
                        // Get 4-month period start (Jan=1, May=5, Sep=9)
                        int fourMonthStartMonth = ((userLocalTime.Month - 1) / 4) * 4 + 1;
                        DateTime userLocalFourMonthStart = new DateTime(userLocalTime.Year, fourMonthStartMonth, 1);
                        // Convert back to server time
                        _model.CurrentPeriodStart = ConvertUserLocalToServer(userLocalFourMonthStart);
                        _model.CurrentPeriodEnd = _model.CurrentPeriodStart.AddMonths(4);
                    }
                    break;

                case IBPeriodType.SemiAnnual:
                    {
                        // Convert to user local time
                        DateTime userLocalTime = ConvertServerToUserLocal(serverPeriodTime);
                        // Get half-year start month (Jan=1, Jul=7)
                        int halfYearStartMonth = userLocalTime.Month <= 6 ? 1 : 7;
                        DateTime userLocalHalfYearStart = new DateTime(userLocalTime.Year, halfYearStartMonth, 1);
                        // Convert back to server time
                        _model.CurrentPeriodStart = ConvertUserLocalToServer(userLocalHalfYearStart);
                        _model.CurrentPeriodEnd = _model.CurrentPeriodStart.AddMonths(6);
                    }
                    break;
            }
        }

        private void CalculateIBRange()
        {
            switch (_indicator.PeriodType)
            {
                case IBPeriodType.Daily:
                    CalculateDailyIB();
                    break;
                case IBPeriodType.Weekly:
                    CalculateWeeklyIB();
                    break;
                case IBPeriodType.Monthly:
                    CalculateMonthlyIB();
                    break;
                case IBPeriodType.Quarterly:
                    CalculateQuarterlyIB();
                    break;
                case IBPeriodType.FourMonthly:
                    CalculateFourMonthlyIB();
                    break;
                case IBPeriodType.SemiAnnual:
                    CalculateSemiAnnualIB();
                    break;
                case IBPeriodType.Yearly:
                    CalculateYearlyIB();
                    break;
                case IBPeriodType.CustomRange:
                    CalculateCustomIB();
                    break;
            }
        }

        private void CalculateDailyIB()
        {
            DateTime ibStart;
            TimeSpan ibDuration;

            if (_indicator.DailyMode == DailyIBMode.Hours)
            {
                // Convert user local midnight to user local time with hours start hour
                DateTime userLocalMidnight = ConvertServerToUserLocal(_model.CurrentPeriodStart);
                DateTime userLocalHoursStart = userLocalMidnight.Date.AddHours(_indicator.HoursStartHour);

                // Convert back to server time
                ibStart = ConvertUserLocalToServer(userLocalHoursStart);
                ibDuration = TimeSpan.FromHours((int)_indicator.DailyHours);

                // For past periods (offset < 0), don't check if started - just use the calculated time
                if (_indicator.PeriodOffset == 0)
                {
                    // Only for current period, check if hours period has started yet
                    DateTime serverNow = _indicator.Server.Time;
                    if (serverNow < ibStart)
                    {
                        // Not started yet, use yesterday's period
                        ibStart = ibStart.AddDays(-1);
                    }
                }
            }
            else // Market Session
            {
                // Get session start hour based on selected session
                int sessionStartHour = _indicator.DailySession switch
                {
                    MarketSession.Sydney => _indicator.SydneyStartHour,
                    MarketSession.Tokyo => _indicator.TokyoStartHour,
                    MarketSession.London => _indicator.LondonStartHour,
                    MarketSession.NewYork => _indicator.NYStartHour,
                    _ => 0
                };

                // Convert user local midnight to user local time with session hour
                DateTime userLocalMidnight = ConvertServerToUserLocal(_model.CurrentPeriodStart);
                DateTime userLocalSessionStart = userLocalMidnight.Date.AddHours(sessionStartHour);

                // Convert back to server time
                ibStart = ConvertUserLocalToServer(userLocalSessionStart);
                ibDuration = TimeSpan.FromHours(8);

                // For past periods (offset < 0), don't check if started - just use the calculated time
                if (_indicator.PeriodOffset == 0)
                {
                    // Only for current period, check if session has started yet
                    DateTime serverNow = _indicator.Server.Time;
                    if (serverNow < ibStart)
                    {
                        // Session not started yet, use yesterday's session
                        ibStart = ibStart.AddDays(-1);
                    }
                }
            }

            DateTime ibEnd = ibStart.Add(ibDuration);

            // Store IB start and end times
            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void CalculateWeeklyIB()
        {
            // Use actual weekly bar's open time (respects broker's week start time)
            var currentWeekBar = _periodBars[_currentPeriodBarIndex];
            DateTime ibStart = currentWeekBar.OpenTime;
            DateTime ibEnd = ibStart.AddDays(1);

            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void CalculateMonthlyIB()
        {
            // Find the actual monthly bar open time for current month
            DateTime ibStart = FindMonthlyBarOpenTime(_model.CurrentPeriodStart);
            DateTime ibEnd = ibStart.AddDays(7);

            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void CalculateQuarterlyIB()
        {
            // Find the actual monthly bar open time for first month of quarter
            DateTime ibStart = FindMonthlyBarOpenTime(_model.CurrentPeriodStart);
            DateTime ibEnd = FindNextMonthBarOpenTime(ibStart);

            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void CalculateFourMonthlyIB()
        {
            // Find the actual monthly bar open time for first month of 4-month period
            DateTime ibStart = FindMonthlyBarOpenTime(_model.CurrentPeriodStart);
            DateTime ibEnd = FindNextMonthBarOpenTime(ibStart);

            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void CalculateSemiAnnualIB()
        {
            // Find the actual monthly bar open time for first month of half-year
            DateTime ibStart = FindMonthlyBarOpenTime(_model.CurrentPeriodStart);
            DateTime ibEnd = FindNextMonthBarOpenTime(ibStart);

            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void CalculateYearlyIB()
        {
            // Find the actual monthly bar open time for January
            DateTime ibStart = FindMonthlyBarOpenTime(_model.CurrentPeriodStart);
            DateTime ibEnd = FindNextMonthBarOpenTime(ibStart);

            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void CalculateCustomIB()
        {
            // For custom range, IB start and end are already set in CalculateCurrentPeriod
            // CurrentPeriodStart = custom start time
            // CurrentPeriodEnd = custom end time
            DateTime ibStart = _model.CurrentPeriodStart;
            DateTime ibEnd = _model.CurrentPeriodEnd;

            _model.IBStart = ibStart;
            _model.IBEnd = ibEnd;

            GetHighLowInRange(ibStart, ibEnd);
        }

        private void GetHighLowInRange(DateTime startTime, DateTime endTime)
        {
            double high = double.MinValue;
            double low = double.MaxValue;
            bool foundData = false;

            for (int i = 0; i < _ibCalculationBars.Count; i++)
            {
                var barTime = _ibCalculationBars.OpenTimes[i];

                if (barTime >= startTime && barTime < endTime)
                {
                    if (_ibCalculationBars.HighPrices[i] > high)
                        high = _ibCalculationBars.HighPrices[i];

                    if (_ibCalculationBars.LowPrices[i] < low)
                        low = _ibCalculationBars.LowPrices[i];

                    foundData = true;
                }
                else if (barTime >= endTime)
                {
                    break;
                }
            }

            if (foundData)
            {
                _model.IBHigh = high;
                _model.IBLow = low;
            }
        }

        private DateTime FindMonthlyBarOpenTime(DateTime periodStart)
        {
            // Find the monthly bar that contains or starts the period
            for (int i = _periodBars.Count - 1; i >= 0; i--)
            {
                var barTime = _periodBars.OpenTimes[i];

                // If this bar's month matches the period start month
                DateTime barInUserTime = ConvertServerToUserLocal(barTime);
                DateTime periodInUserTime = ConvertServerToUserLocal(periodStart);

                if (barInUserTime.Year == periodInUserTime.Year &&
                    barInUserTime.Month == periodInUserTime.Month)
                {
                    return barTime;
                }
            }

            // Fallback to period start if not found
            return periodStart;
        }

        private DateTime FindNextMonthBarOpenTime(DateTime currentMonthBarTime)
        {
            // Find the next monthly bar after the given time
            for (int i = 0; i < _periodBars.Count; i++)
            {
                var barTime = _periodBars.OpenTimes[i];

                if (barTime > currentMonthBarTime)
                {
                    return barTime;
                }
            }

            // Fallback: add 1 month
            return currentMonthBarTime.AddMonths(1);
        }
    }
}
