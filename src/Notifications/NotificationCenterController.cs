using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Notifications.View;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Newtonsoft.Json;

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

        private DynamoLogger logger;
        private static readonly DateTime notificationsCenterCreatedTime = DateTime.UtcNow;
        private static System.Timers.Timer timer;
        private string jsonStringFile;
        private NotificationsModel notificationsModel;

        internal NotificationCenterController(DynamoView view, DynamoLogger dynLogger)
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
            logger = dynLogger;

            RequestNotifications();
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            AddNotifications(notificationsModel.Notifications);
        }

        private void AddNotifications(List<NotificationItemModel> notifications)
        {
            var notificationsList = JsonConvert.SerializeObject(notifications);
            InvokeJS($"window.setNotifications({notificationsList});");
        }

        private void RequestNotifications()
        {
            var uri = DynamoUtilities.PathHelper.getServiceBackendAddress(this, "notificationAddress");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                jsonStringFile = reader.ReadToEnd();
                notificationsModel = JsonConvert.DeserializeObject<NotificationsModel>(jsonStringFile);

                var notificationsNumber = notificationsModel.Notifications.Count();

                var shortcutToolbarViewModel = (ShortcutToolbarViewModel)dynamoView.ShortcutBar.DataContext;
                shortcutToolbarViewModel.NotificationsNumber = notificationsNumber;
            }

            notificationUIPopup.webView.NavigationCompleted += WebView_NavigationCompleted;
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

            if (notificationUIPopup.webView.CoreWebView2 != null)
            {
                notificationUIPopup.webView.CoreWebView2.NavigateToString(htmlString);
            }
        }

        internal void Dispose()
        {
            notificationUIPopup.webView.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
            dynamoView.SizeChanged -= DynamoView_SizeChanged;
            dynamoView.LocationChanged -= DynamoView_LocationChanged;
            notificationsButton.Click -= NotificationsButton_Click;
            notificationUIPopup.webView.NavigationCompleted -= WebView_NavigationCompleted;
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

        /// <summary>
        /// Notification Center button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private async void InvokeJS(string script)
        {
            await notificationUIPopup.webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Invokes the script on the notification web-app side to update the URL to fetch notifications from 
        /// and that will trigger a re-render of the panel. If a URL is provided then that will be used
        /// else the address will be fetched from the application configuration file.
        /// </summary>
        /// <param name="url">(Optional) If provided, this URL will be used to fetch notifications.</param>
        public void RefreshNotifications(string url="") {
            if (!string.IsNullOrEmpty(url))
            {
                InvokeJS(@"window.RequestNotifications('" + url + "');");
            }
            else {
                InvokeJS(@"window.RequestNotifications('" + DynamoUtilities.PathHelper.getServiceBackendAddress(this, "notificationAddress") + "');");
            }
        }
    }
}
