using System.Collections.Generic;

namespace cAlgo.Indicators
{
    /// <summary>
    /// Button position options
    /// </summary>
    public enum ToggleButtonsPosition
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// Interface for configuring toggle buttons
    /// </summary>
    public interface IToggleConfiguration
    {
        List<ToggleButton> GetButtons();
        void OnToggleChanged(string buttonId, bool isEnabled);
    }

    /// <summary>
    /// Definition of a single toggle button
    /// </summary>
    public class ToggleButton
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool InitialState { get; set; }

        public ToggleButton(string id, string label, bool initialState = true)
        {
            Id = id;
            Label = label;
            InitialState = initialState;
        }
    }
}
