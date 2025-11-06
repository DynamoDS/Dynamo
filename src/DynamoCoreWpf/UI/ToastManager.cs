using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Views.GuidedTour;

namespace Dynamo.Wpf.UI
{
    public class ToastManager
    {

        //The popup will be shown at the top-right section of the Dynamo window (at the left side of the statusBarPanel element)
        //The VerticalOffset is used to move vertically the popup (using as a reference the statusBarPanel position)
        private const double VerticalOffset = 10;
        //The HorizontalOffset is used to move horizontally the popup (using as a reference the statusBarPanel position)
        private const double HorizontalOffset = 110;

        private RealTimeInfoWindow toastPopup;
        private UIElement mainRootElement;

        public ToastManager(UIElement rootElement)
        {
            mainRootElement = rootElement;
        }

        /// <summary>
        /// Display a notification to display on top right corner of canvas
        /// and display message passed as param
        /// </summary>
        /// <param name="content">The target content to display.</param>
        /// <param name="stayOpen">boolean indicates if the popup will stay open until user dismiss it.</param>
        /// <param name="headerText">The header text to display.</param>
        /// <param name="showHeader">boolean indicates if the header will be shown.</param>
        /// <param name="showHyperlink">boolean indicates if the hyperlink will be shown.</param>
        /// <param name="hyperlinkText">The hyperlink text to display.</param>
        /// <param name="hyperlinkUri">The hyperlink uri to navigate to.</param>
        public void CreateRealTimeInfoWindow(
            string content,
            bool stayOpen = false,
            bool showHeader = false,
            string headerText = "",
            bool showHyperlink = false,
            string hyperlinkText = "",
            Uri hyperlinkUri = null)
        {
            //Search a UIElement with the Name "statusBarPanel" inside the Dynamo VisualTree
            UIElement hostUIElement = GuideUtilities.FindChild(mainRootElement, "statusBarPanel");

            // When popup already exist, replace it
            CloseRealTimeInfoWindow();
            // Otherwise creates the RealTimeInfoWindow popup and set up all the needed values
            // to show the popup over the Dynamo workspace
            toastPopup = new RealTimeInfoWindow()
            {
                VerticalOffset = VerticalOffset,
                HorizontalOffset = HorizontalOffset,
                Placement = PlacementMode.Left,
                TextContent = content,
                StaysOpen = stayOpen,
                ShowHeader = showHeader,
                HeaderContent = headerText,
                ShowHyperlink = showHyperlink,
                HyperlinkText = hyperlinkText,
                HyperlinkUri = hyperlinkUri
            };

            if (hostUIElement != null)
                toastPopup.PlacementTarget = hostUIElement;
            toastPopup.IsOpen = true;
        }

        /// <summary>
        /// Closes the Popup if exist and it's open
        /// </summary>
        public void CloseRealTimeInfoWindow()
        {
            if (toastPopup != null && toastPopup.IsOpen)
            {
                toastPopup.IsOpen = false;
            }
        }
        /// <summary>
        /// This method will be used when the PlacementTarget element of the Popup was moved or resize so we need to update the popup
        /// </summary>
        internal void UpdateLocation()
        {
            if (toastPopup != null)
            {
                toastPopup.UpdateLocation();
            }
        }

        /// <summary>
        /// Returns if the Popup (Toast Notification) is open or not.
        /// </summary>
        internal bool PopupIsVisible
        {
            get
            {
                return toastPopup != null && toastPopup.IsOpen;
            }
        }
    }
}
