using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    /// <summary>
    /// Filters FVGs based on status and display settings
    /// Single Responsibility: Filtering logic only
    /// </summary>
    public class FVGFilterService
    {
        private readonly bool _showUnfilled;
        private readonly bool _showPartial;
        private readonly bool _showFilled;
        private readonly int _maxFVGsToDisplay;

        public FVGFilterService(bool showUnfilled, bool showPartial, bool showFilled, int maxFVGsToDisplay)
        {
            _showUnfilled = showUnfilled;
            _showPartial = showPartial;
            _showFilled = showFilled;
            _maxFVGsToDisplay = maxFVGsToDisplay;
        }

        /// <summary>
        /// Filter FVGs based on status settings and display limit
        /// Returns filtered and sorted list (newest first)
        /// </summary>
        public List<FVGModel> FilterFVGs(List<FVGModel> fvgList)
        {
            // STEP 1: Filter by status
            var filtered = fvgList.Where(fvg => ShouldShowFVG(fvg)).ToList();

            // STEP 2: Sort by FormationTime descending (newest first)
            filtered = filtered.OrderByDescending(fvg => fvg.FormationTime).ToList();

            // STEP 3: Limit number of FVGs (if not -1)
            if (_maxFVGsToDisplay > 0)
            {
                filtered = filtered.Take(_maxFVGsToDisplay).ToList();
            }

            return filtered;
        }

        /// <summary>
        /// Check if FVG should be shown based on status filters
        /// </summary>
        private bool ShouldShowFVG(FVGModel fvg)
        {
            switch (fvg.Status)
            {
                case FVGStatus.Unfilled:
                    return _showUnfilled;
                case FVGStatus.PartiallyFilled:
                    return _showPartial;
                case FVGStatus.Filled:
                    return _showFilled;
                default:
                    return true;
            }
        }
    }
}
