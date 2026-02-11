using cAlgo.API;

namespace cAlgo
{
    public partial class FairValueGap : Indicator
    {
        #region Helper Methods
        
        /// <summary>
        /// Check if selected timeframe is higher than current timeframe
        /// Returns true if selectedTF > currentTF (e.g., 1H > 1M = true)
        /// </summary>
        private bool IsHigherTimeframe(TimeFrame selectedTF, TimeFrame currentTF)
        {
            // Convert timeframes to minutes for comparison
            long selectedMinutes = GetTimeframeInMinutes(selectedTF);
            long currentMinutes = GetTimeframeInMinutes(currentTF);
            
            return selectedMinutes > currentMinutes;
        }
        
        /// <summary>
        /// Convert TimeFrame to minutes for comparison
        /// </summary>
        private long GetTimeframeInMinutes(TimeFrame tf)
        {
            // Use TimeSpan to get total minutes
            if (tf == TimeFrame.Tick)
                return 0;
            
            // For standard timeframes, convert to TimeSpan
            var timeSpan = tf.ToTimeSpan();
            return (long)timeSpan.TotalMinutes;
        }
        
        #endregion
    }
}
