using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Notifications.View;
using DynamoUtilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Newtonsoft.Json;
using Microsoft.Web.WebView2.Wpf;
using Dynamo.Utilities;
using Dynamo.Configuration;
using Dynamo.Models;

namespace Dynamo.Notifications
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class ScriptObject
    {
        Action<object[]> onMarkAllAsRead;
        Action<int> onNotificationPopupUpdated;

        internal ScriptObject(Action<object []> onMarkAllAsRead, Action<int> onNotificationPopupUpdated)
        {
            this.onMarkAllAsRead = onMarkAllAsRead;
            this.onNotificationPopupUpdated = onNotificationPopupUpdated;
        }

        public void SetNotificationsAsRead(object[] ids)
        {
            onMarkAllAsRead(ids);
        }
        /// <summary>
        /// This function will be triggered in NotificationCenter side passing the current height it has so we can update popup height in Dynamo
        /// </summary>
        /// <param name="height"></param>
        public void UpdateNotificationWindowSize(int height)
        {
            onNotificationPopupUpdated(height);
        }
    }

    public class NotificationCenterController : IDisposable
    {
        private readonly NotificationUI notificationUIPopup;
        private readonly DynamoView dynamoView;
        private readonly DynamoViewModel dynamoViewModel;
        private readonly Button notificationsButton;

        private static readonly int notificationPopupHorizontalOffset = -288;
        private static readonly int notificationPopupVerticalOffset = 5;
        private static readonly int limitOfMonthsFilterNotifications = 6;

        private static readonly string htmlEmbeddedFile = "Dynamo.Notifications.Packages.NotificationCenter.build.index.html";
        private static readonly string jsEmbeddedFile = "Dynamo.Notifications.Packages.NotificationCenter.build.index.bundle.js";
        private static readonly string NotificationCenterButtonName = "notificationsButton";
        internal DirectoryInfo webBrowserUserDataFolder;

        internal AsyncMethodState initState = AsyncMethodState.NotStarted;

        private readonly DynamoLogger logger;
        private string jsonStringFile;
        private NotificationsModel notificationsModel;
        private const int PopupMaxHeigth = 598;

        internal NotificationCenterController(DynamoView view, DynamoLogger dynLogger)
        {
            dynamoView = view;
            dynamoViewModel = dynamoView.DataContext as DynamoViewModel;
            //When executing Dynamo as Sandbox or inside any host like Revit, FormIt, Civil3D the WebView2 cache folder will be located in the AppData folder
            var userDataDir = new DirectoryInfo(dynamoViewModel.Model.PathManager.UserDataDirectory);
            webBrowserUserDataFolder = userDataDir.Exists ? userDataDir : null;

            notificationsButton = (Button)view.ShortcutBar.FindName(NotificationCenterButtonName);

            dynamoView.SizeChanged += DynamoView_SizeChanged;
            dynamoView.LocationChanged += DynamoView_LocationChanged;
            notificationsButton.Click += NotificationsButton_Click;
            dynamoView.PreviewMouseDown += DynamoView_PreviewMouseDown;

            notificationUIPopup = new NotificationUI
            {
                IsOpen = false,
                PlacementTarget = notificationsButton,
                Placement = PlacementMode.Bottom,
                HorizontalOffset = notificationPopupHorizontalOffset,
                VerticalOffset = notificationPopupVerticalOffset
            };
            logger = dynLogger;

            // If user turns on the feature, they will need to restart Dynamo to see the count
            // This ensures no network traffic when Notification center feature is turned off
            if (dynamoViewModel.PreferenceSettings.EnableNotificationCenter && !dynamoViewModel.Model.NoNetworkMode )
            {
                notificationUIPopup.webView.Loaded += InitializeBrowserAsync;
                RequestNotifications();
            }   
        }

        private async void InitializeBrowserAsync(object sender, RoutedEventArgs e)
        {
            if (webBrowserUserDataFolder != null)
            {
                //This indicates in which location will be created the WebView2 cache folder
                notificationUIPopup.webView.CreationProperties = new CoreWebView2CreationProperties()
                {
                    UserDataFolder = webBrowserUserDataFolder.FullName
                };
            }               
            notificationUIPopup.webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

            initState = AsyncMethodState.Started;
            await notificationUIPopup.webView.EnsureCoreWebView2Async();
            initState = AsyncMethodState.Done;
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            AddNotifications(notificationsModel.Notifications);

            string setTitle = String.Format("window.setTitle('{0}');", Properties.Resources.NotificationsCenterTitle);
            InvokeJS(setTitle);

            string setBottomButtonText = String.Format("window.setBottomButtonText('{0}');", Properties.Resources.NotificationsCenterBottomButtonText);
            InvokeJS(setBottomButtonText);
        }

        private void AddNotifications(List<NotificationItemModel> notifications)
        {
            var notificationsList = JsonConvert.SerializeObject(notifications);
            InvokeJS($"window.setNotifications({notificationsList});");
        }

        private void RequestNotifications()
        {
            var uri = DynamoUtilities.PathHelper.GetServiceBackendAddress(this, "notificationAddress");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Timeout = Configurations.NotificationsDefaultTimeOut;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                jsonStringFile = reader.ReadToEnd();
                notificationsModel = JsonConvert.DeserializeObject<NotificationsModel>(jsonStringFile);

                //We are adding a limit of months to grab the notifications
                var limitDate = DateTime.Now.AddMonths(-limitOfMonthsFilterNotifications);
                notificationsModel.Notifications = notificationsModel.Notifications.Where(x => x.Created >= limitDate).ToList();
            }

            CountUnreadNotifications();
            notificationUIPopup.webView.NavigationCompleted += WebView_NavigationCompleted;        
        }

        private void CountUnreadNotifications()
        {
            var notificationsNumber = 0;
            foreach (var notification in notificationsModel.Notifications)
            {
                if (!dynamoViewModel.Model.PreferenceSettings.ReadNotificationIds.Contains(notification.Id))
                {
                    notification.IsRead = false;
                    notificationsNumber++;
                }
            }

            var shortcutToolbarViewModel = (ShortcutToolbarViewModel)dynamoView.ShortcutBar.DataContext;
            shortcutToolbarViewModel.NotificationsNumber = notificationsNumber;
        }

        internal void OnMarkAllAsRead(object[] ids)
        {
            string[] notificationIds = ids.Select(x => x.ToString()).
                Where(x => !dynamoViewModel.Model.PreferenceSettings.ReadNotificationIds.Contains(x.ToString())).ToArray();

            dynamoViewModel.Model.PreferenceSettings.ReadNotificationIds.AddRange(notificationIds);

            var shortcutToolbarViewModel = (ShortcutToolbarViewModel)dynamoView.ShortcutBar.DataContext;
            shortcutToolbarViewModel.NotificationsNumber = 0;
        }

        internal void OnNotificationPopupUpdated(int height)
        {
            var notificationsViewModel = notificationUIPopup.DataContext as NotificationsUIViewModel;
            if(notificationsViewModel != null && height > 0 && height < PopupMaxHeigth)
            {
                notificationsViewModel.PopupRectangleHeight = height;
            }
            notificationUIPopup.UpdatePopupSize();
        }

        // Handler for new Webview2 tab window request
        private void WebView_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri) { UseShellExecute = true });
            e.Handled = true;
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
                // More initialization options
                // Context menu disabled
                notificationUIPopup.webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                // Opening hyper-links using default system browser instead of WebView2 tab window
                notificationUIPopup.webView.CoreWebView2.NewWindowRequested += WebView_NewWindowRequested;
                notificationUIPopup.webView.CoreWebView2.NavigateToString(htmlString);
                // Hosts an object that will expose the properties and methods to be called from the javascript side
                notificationUIPopup.webView.CoreWebView2.AddHostObjectToScript("scriptObject", 
                    new ScriptObject(OnMarkAllAsRead, OnNotificationPopupUpdated));

                notificationUIPopup.webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                notificationUIPopup.webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            }
        }

        private void DynamoView_LocationChanged(object sender, EventArgs e)
        {
            if (notificationUIPopup != null)
            {
                notificationUIPopup.Placement = PlacementMode.Bottom;
                notificationUIPopup.UpdatePopupLocation();
            }
        }

        private void DynamoView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.UpdatePopupLocation();
        }

        /// <summary>
        /// Dismiss notifications panel by clicking outside the panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DynamoView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string popupBellID = "FontAwesome5.FontAwesome";
            if (!notificationUIPopup.IsOpen) return;
            if(e.OriginalSource.ToString() != popupBellID)
            {
                notificationUIPopup.IsOpen = false;
            }
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
                InvokeJS(@"window.RequestNotifications('" + DynamoUtilities.PathHelper.GetServiceBackendAddress(this, "notificationAddress") + "');");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (dynamoView != null)
            {
                dynamoView.SizeChanged -= DynamoView_SizeChanged;
                dynamoView.LocationChanged -= DynamoView_LocationChanged;
                dynamoView.PreviewMouseDown -= DynamoView_PreviewMouseDown;
            }

            if (notificationUIPopup != null)
            {
                notificationUIPopup.IsOpen = false;
                notificationsButton.Click -= NotificationsButton_Click;

                if (notificationUIPopup.webView != null)
                {
                    notificationUIPopup.webView.Visibility = Visibility.Hidden;
                    notificationUIPopup.webView.Loaded -= InitializeBrowserAsync;
                    notificationUIPopup.webView.NavigationCompleted -= WebView_NavigationCompleted;
                    notificationUIPopup.webView.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;

                    if (notificationUIPopup.webView.CoreWebView2 != null)
                    {
                        notificationUIPopup.webView.CoreWebView2.Stop();
                        notificationUIPopup.webView.CoreWebView2.NewWindowRequested -= WebView_NewWindowRequested;
                    }
                    notificationUIPopup.webView.Dispose();
                }
            }
        }

        /// <summary>
        /// Dispose function for DocumentationBrowser
        /// </summary>
        public void Dispose()
        {
            if (initState == AsyncMethodState.Started)
            {
                Log("NotificationCenterController is being disposed but async initialization is still not done");
            }
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Log(string msg)
        {
            if (DynamoModel.IsTestMode)
            {
                System.Console.WriteLine(msg);
            }
            else
            {
                logger?.Log(msg);
            }
        }
    }
}
