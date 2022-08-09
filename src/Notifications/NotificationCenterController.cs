using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Controls;
using Dynamo.Notifications.View;
using Dynamo.ViewModels;

namespace Dynamo.Notifications
{
    public class NotificationCenterController
    {
        private readonly NotificationUI notificationUIPopup;
        private readonly DynamoView dynamoView;
        private readonly DynamoViewModel dynamoViewModel;
        private readonly Button notificationsButton;

        private static readonly int notificationPopupHorizontalOffset = -285;
        private static readonly int notificationPopupVerticalOffset = 10;

        private static readonly string htmlEmbeddedFile = "Dynamo.Notifications.node_modules._dynamods.notifications_center.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.Notifications.node_modules._dynamods.notifications_center.build.index.bundle.js";
        private static readonly string NotificationCenterButtonName = "notificationsButton";

        internal NotificationCenterController(DynamoView view)
        {
            dynamoView = view;
            dynamoViewModel = dynamoView.DataContext as DynamoViewModel;
            notificationsButton = (Button)view.ShortcutBar.FindName(NotificationCenterButtonName);

            dynamoView.SizeChanged += DynamoView_SizeChanged;
            dynamoView.LocationChanged += DynamoView_LocationChanged;
            notificationsButton.Click += NotificationsButton_Click;

            notificationUIPopup = new NotificationUI
            {
                IsOpen = false,
                PlacementTarget = notificationsButton,
                Placement = PlacementMode.Bottom,
                HorizontalOffset = notificationPopupHorizontalOffset,
                VerticalOffset = notificationPopupVerticalOffset
            };
            notificationUIPopup.webView.EnsureCoreWebView2Async();
            notificationUIPopup.webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;            
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string htmlString = string.Empty;

            using (Stream stream = assembly.GetManifestResourceStream(htmlEmbeddedFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                htmlString = reader.ReadToEnd();
            }

            using (Stream stream = assembly.GetManifestResourceStream(jsEmbeddedFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                var jsString = reader.ReadToEnd();
                htmlString = htmlString.Replace("mainJs", jsString);
            }

            if(notificationUIPopup.webView.CoreWebView2 != null)
                notificationUIPopup.webView.CoreWebView2.NavigateToString(htmlString);
        }

        internal void Dispose()
        {
            notificationUIPopup.webView.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
            dynamoView.SizeChanged -= DynamoView_SizeChanged;
            dynamoView.LocationChanged -= DynamoView_LocationChanged;
            notificationsButton.Click -= NotificationsButton_Click;
        }

        private void DynamoView_LocationChanged(object sender, EventArgs e)
        {
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.UpdatePopupLocation();
        }

        private void DynamoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.UpdatePopupLocation();
        }

        // Notification Center button click handler
        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.dynamoViewModel.PreferenceSettings.EnableNotificationCenter)
            {
                notificationUIPopup.IsOpen = !notificationUIPopup.IsOpen;
                if (notificationUIPopup.IsOpen)
                    notificationUIPopup.webView.Focus();
            }
            else
            {
                this.dynamoViewModel.MainGuideManager.CreateRealTimeInfoWindow(Properties.Resources.NotificationCenterDisabledMsg);
            }
        }        
    }
}
