using System;
using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Manages toggle buttons for metrics panel
    /// </summary>
    public class MetricsToggleManager
    {
        private readonly Chart _chart;
        private readonly IToggleConfiguration _config;
        private readonly Action _onAnyToggleChanged;
        private readonly Dictionary<string, Button> _buttons;
        private readonly Color _enabledColor = Color.LimeGreen;
        private readonly Color _disabledColor = Color.Gray;
        private ToggleButtonsPosition _currentPosition;

        public MetricsToggleManager(Chart chart, IToggleConfiguration config, Action onAnyToggleChanged)
        {
            _chart = chart;
            _config = config;
            _onAnyToggleChanged = onAnyToggleChanged;
            _buttons = new Dictionary<string, Button>();
            _currentPosition = ToggleButtonsPosition.None;
        }

        public void Show(ToggleButtonsPosition toggleButtonsPosition)
        {
            if (toggleButtonsPosition == ToggleButtonsPosition.None)
            {
                Hide();
                return;
            }

            // Remove existing buttons if position changed
            if (_currentPosition != toggleButtonsPosition && _buttons.Count > 0)
                Hide();

            // Create buttons if needed
            if (_buttons.Count == 0)
            {
                var buttons = _config.GetButtons();
                for (int i = 0; i < buttons.Count; i++)
                {
                    var buttonDef = buttons[i];
                    var button = CreateButton(buttonDef, i, toggleButtonsPosition);
                    _buttons[buttonDef.Id] = button;
                    _chart.AddControl(button);
                }
            }

            _currentPosition = toggleButtonsPosition;
        }

        public void Hide()
        {
            foreach (var button in _buttons.Values)
            {
                _chart.RemoveControl(button);
            }
            _buttons.Clear();
            _currentPosition = ToggleButtonsPosition.None;
        }

        public void UpdatePosition(ToggleButtonsPosition toggleButtonsPosition)
        {
            Show(toggleButtonsPosition);
        }

        private Button CreateButton(ToggleButton buttonDef, int index, ToggleButtonsPosition position)
        {
            var (hAlign, vAlign) = GetAlignment(position);
            var margin = CalculateMargin(index, position);

            var button = new Button
            {
                Text = buttonDef.Label,
                Width = 35,
                Height = 20,
                Padding = 0,
                FontSize = 9,
                BackgroundColor = buttonDef.InitialState ? _enabledColor : _disabledColor,
                ForegroundColor = Color.White,
                CornerRadius = 1,
                Margin = margin,
                HorizontalAlignment = hAlign,
                VerticalAlignment = vAlign
            };

            button.Click += (args) => OnButtonClick(buttonDef.Id, button);
            return button;
        }

        private void OnButtonClick(string buttonId, Button button)
        {
            bool newState = button.BackgroundColor == _disabledColor;
            button.BackgroundColor = newState ? _enabledColor : _disabledColor;
            
            _config.OnToggleChanged(buttonId, newState);
            _onAnyToggleChanged?.Invoke();
        }

        private (HorizontalAlignment, VerticalAlignment) GetAlignment(ToggleButtonsPosition position)
        {
            switch (position)
            {
                case ToggleButtonsPosition.TopLeft:
                    return (HorizontalAlignment.Left, VerticalAlignment.Top);
                case ToggleButtonsPosition.TopRight:
                    return (HorizontalAlignment.Right, VerticalAlignment.Top);
                case ToggleButtonsPosition.BottomLeft:
                    return (HorizontalAlignment.Left, VerticalAlignment.Bottom);
                case ToggleButtonsPosition.BottomRight:
                default:
                    return (HorizontalAlignment.Right, VerticalAlignment.Bottom);
            }
        }

        private Thickness CalculateMargin(int index, ToggleButtonsPosition position)
        {
            int buttonSpacing = 37; // 35 + 2px gap
            int offset = index * buttonSpacing + 5;

            bool isLeft = position == ToggleButtonsPosition.TopLeft || position == ToggleButtonsPosition.BottomLeft;
            bool isTop = position == ToggleButtonsPosition.TopLeft || position == ToggleButtonsPosition.TopRight;

            if (isLeft)
            {
                return isTop 
                    ? new Thickness(offset, 5, 0, 0)
                    : new Thickness(offset, 0, 0, 5);
            }
            else
            {
                return isTop
                    ? new Thickness(0, 5, offset, 0)
                    : new Thickness(0, 0, offset, 5);
            }
        }
    }
}
