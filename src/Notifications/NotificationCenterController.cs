using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Notifications.View;

namespace Dynamo.Notifications
{
    public class NotificationCenterController
    {
        readonly NotificationUI notificationUIPopup;
        readonly DynamoView dynamoView;
        readonly Button notificationsButton;

        private static readonly int notificationPopupHorizontalOffset = -285;
        private static readonly int notificationPopupVerticalOffset = 10;

        private static readonly string htmlEmbeddedFile = "Dynamo.Notifications.node_modules._dynamods.notifications_center.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.Notifications.node_modules._dynamods.notifications_center.build.index.bundle.js";

        private event Action<ILogMessage> MessageLogged;

        internal NotificationCenterController(DynamoView dynamoView)
        {
            var shortcutBar = dynamoView.ShortcutBar;
            var notificationsButton = (Button)shortcutBar.FindName("notificationsButton");

            notificationUIPopup = new NotificationUI();
            notificationUIPopup.IsOpen = false;
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.HorizontalOffset = notificationPopupHorizontalOffset;
            notificationUIPopup.VerticalOffset = notificationPopupVerticalOffset;

            this.dynamoView = dynamoView;
            this.notificationsButton = notificationsButton;

            this.dynamoView.SizeChanged += DynamoView_SizeChanged;
            this.dynamoView.LocationChanged += DynamoView_LocationChanged;
            this.notificationsButton.Click += NotificationsButton_Click;

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

            //Fetch notification URL from config
            string notificationURL = FetchNotificationURL();

            using (Stream stream = assembly.GetManifestResourceStream(jsEmbeddedFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                var jsString = reader.ReadToEnd();
                jsString = jsString.Replace("http://demo9540080.mockable.io/notifications", notificationURL);
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

        private void NotificationsButton_Click(object sender, RoutedEventArgs e)
        {
            notificationUIPopup.IsOpen = !notificationUIPopup.IsOpen;
            if (notificationUIPopup.IsOpen)
                notificationUIPopup.webView.Focus();
        }

        private string FetchNotificationURL()
        {
            var path = this.GetType().Assembly.Location;
            var config = ConfigurationManager.OpenExeConfiguration(path);
            var key = config.AppSettings.Settings["notificationAddress"];
            string url = null;
            if (key != null)
            {
                url = key.Value;
            }

            OnMessageLogged(LogMessage.Info("Dynamo will use the notifications service at : "+ url));

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentException("Incorrectly formatted URL provided for Notification service.", "url");
            }

            return url;
        }

        private void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                this.MessageLogged(msg);
            }
        }
    }
}
