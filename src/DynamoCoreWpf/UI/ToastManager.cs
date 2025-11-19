using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Views.GuidedTour;
using System.Windows.Threading;

namespace Dynamo.Wpf.UI
{
    public class ToastManager
    {

        //The popup will be shown at the top-right section of the Dynamo window (at the left side of the statusBarPanel element)
        //The VerticalOffset is used to move vertically the popup (using as a reference the statusBarPanel position)
        private const double VerticalOffset = 10;
        //The HorizontalOffset is used to move horizontally the popup (using as a reference the statusBarPanel position)
        private const double HorizontalOffset = 110;
        internal const int AutoCloseSeconds = 5;

        private RealTimeInfoWindow toastPopup;
        private UIElement mainRootElement;
        private DispatcherTimer closeTimer;

        public ToastManager(UIElement rootElement)
        {
            mainRootElement = rootElement;
        }

        /// <summary>
        /// Display a notification to display on top right corner of canvas
        /// and display message passed as param
        /// </summary>
        /// <param name="content">The target content to display.</param>
        /// <param name="stayOpen">
        /// boolean indicates if the popup will stay open until user dismiss it
        /// note: all toasts are still auto-closed after a certain duration.</param>
        /// <param name="headerText">The header text to display.</param>
        /// <param name="hyperlinkText">The hyperlink text to display.</param>
        /// <param name="hyperlinkUri">The hyperlink uri to navigate to.</param>
        /// <param name="fileLinkUri">The file link uri to navigate to.</param>
        public void CreateRealTimeInfoWindow(
            string content,
            bool stayOpen = false,
            string headerText = "",
            string hyperlinkText = "",
            Uri hyperlinkUri = null,
            Uri fileLinkUri = null)
        {
            //Search a UIElement with the Name "statusBarPanel" inside the Dynamo VisualTree
            UIElement hostUIElement = GuideUtilities.FindChild(mainRootElement, "statusBarPanel");

            // When popup already exist, replace it
            CloseRealTimeInfoWindow();
            StopAutoCloseTimer();

            // Create the popup
            toastPopup = new RealTimeInfoWindow()
            {
                VerticalOffset = VerticalOffset,
                HorizontalOffset = HorizontalOffset,
                Placement = PlacementMode.Left,
                StaysOpen = stayOpen,
                HeaderContent = headerText,
                HyperlinkText = hyperlinkText,
                HyperlinkUri = hyperlinkUri,
                FileLinkUri = fileLinkUri
            };

            var showFileLink = !string.IsNullOrEmpty(fileLinkUri?.ToString());
            toastPopup.SetToastMessage(content, showFileLink, fileLinkUri);
            toastPopup.UpdateVisualState();

            if (hostUIElement != null)
                toastPopup.PlacementTarget = hostUIElement;
            toastPopup.IsOpen = true;
            StartAutoCloseTimer(AutoCloseSeconds);
        }

        /// <summary>
        /// Closes the Popup if exist and it's open
        /// </summary>
        public void CloseRealTimeInfoWindow()
        {
            StopAutoCloseTimer();

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

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            CloseRealTimeInfoWindow();
        }

        private void StartAutoCloseTimer(int seconds)
        {
            if (seconds <= 0) return;
            StopAutoCloseTimer();
            closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
            closeTimer.Tick += AutoCloseTimer_Tick;
            closeTimer.Start();
        }

        private void StopAutoCloseTimer()
        {
            if (closeTimer == null) return;
            closeTimer.Stop();
            closeTimer.Tick -= AutoCloseTimer_Tick;
            closeTimer = null;
        }
    }
}
