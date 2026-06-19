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

        //The popup is shown at the top-right of the canvas, anchored to the persistent background_grid
        //(which survives workspace swaps, unlike the per-workspace viewControlPanel). Its top-right
        //corner coincides with the old viewControlPanel anchor, so these insets reproduce the original
        //popup position.
        //Distance from the popup's right edge to the canvas right edge.
        private const double RightInset = 9;
        //Distance from the popup's top edge to the canvas top edge.
        private const double TopInset = 10;
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
        /// When true the popup stays open until it is dismissed (by the user or by a caller). When
        /// false the popup auto-closes after <see cref="AutoCloseSeconds"/> seconds.</param>
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
            // When popup already exist, replace it
            CloseRealTimeInfoWindow();
            StopAutoCloseTimer();

            // Create the popup
            toastPopup = new RealTimeInfoWindow()
            {
                StaysOpen = stayOpen,
                HeaderContent = headerText,
                HyperlinkText = hyperlinkText,
                HyperlinkUri = hyperlinkUri,
                FileLinkUri = fileLinkUri
            };

            var showFileLink = !string.IsNullOrEmpty(fileLinkUri?.ToString());
            toastPopup.SetToastMessage(content, showFileLink, fileLinkUri);
            toastPopup.UpdateVisualState();

            // Custom placement pins the popup to the top-right corner of the background_grid canvas
            // regardless of the popup's width. background_grid is the canvas background declared
            // directly in DynamoView, so it persists across workspace swaps (only the WorkspaceView
            // content inside it changes) - unlike the per-workspace viewControlPanel, which gets torn
            // out of the visual tree during file open and used to leave the popup orphaned. Its
            // top-right corner coincides with the old viewControlPanel anchor. It is always laid out
            // by the time a toast can be shown, since ToastManager is created in DynamoView_Loaded.
            toastPopup.Placement = PlacementMode.Custom;
            toastPopup.CustomPopupPlacementCallback = PlaceTopRight;

            var hostUIElement = GuideUtilities.FindChild(mainRootElement, "background_grid");
            if (hostUIElement != null)
            {
                toastPopup.PlacementTarget = hostUIElement;
            }

            toastPopup.IsOpen = true;

            // Only start auto-close timer if the notification should not stay open
            if (!stayOpen)
            {
                StartAutoCloseTimer(AutoCloseSeconds);
            }
        }

        /// <summary>
        /// Custom placement callback that positions the popup just inside the top-right corner of the
        /// placement target (background_grid), independent of the popup's width.
        /// </summary>
        private CustomPopupPlacement[] PlaceTopRight(Size popupSize, Size targetSize, Point offset)
        {
            // Clamp so a canvas narrower than the popup does not push the toast off the left edge.
            var x = Math.Max(RightInset, targetSize.Width - popupSize.Width - RightInset);
            var y = TopInset;
            return new[] { new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.None) };
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
