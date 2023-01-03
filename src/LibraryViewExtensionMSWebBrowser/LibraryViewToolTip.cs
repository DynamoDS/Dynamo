using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.UI.Controls;

namespace Dynamo.LibraryViewExtensionMSWebBrowser
{
    public class FloatingLibraryTooltipPopup : LibraryToolTipPopup
    {
        private double overridenTargetY = 0;
        private CustomPopupPlacementCallback basePlacementCallback = null;

        public FloatingLibraryTooltipPopup(double y)
        {
            overridenTargetY = y;

            // Store the old callback for getting the placement from the base class.
            // As such, later in the callback in this class, it is only needed to adjust the
            // placement.
            basePlacementCallback = CustomPopupPlacementCallback;
            CustomPopupPlacementCallback = PlacementCallback;
        }

        /// <summary>
        /// Set the y position for the tooltip popup which will override the y position returned
        /// from the placement callback from the base class.
        /// </summary>
        /// <param name="y"></param>
        internal void UpdateYPosition(double y)
        {
            overridenTargetY = y;
        }

        /// <summary>
        /// The callback method which will return the placement for the popup.
        /// This will get the placement by calling the callback method from the base class and
        /// then update the y postion.
        /// </summary>
        /// <param name="popup"></param>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private CustomPopupPlacement[] PlacementCallback(Size popup, Size target, Point offset)
        {
            var placements = basePlacementCallback(popup, target, offset);
            var point = placements[0].Point;
            point.Y = overridenTargetY;
            placements[0].Point = point;
            return placements;
        }
    }
}
