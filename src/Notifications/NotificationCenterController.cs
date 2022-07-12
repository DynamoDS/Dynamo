using Dynamo.Controls;
using Dynamo.Notifications.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dynamo.Notifications
{
    public class NotificationCenterController
    {
        NotificationUI notificationUIPopup;
        DynamoView dynamoView;
        Button notificationsButton;

        private static int notificationPopupHorizontalOffset = -285;
        private static int notificationPopupVerticalOffset = 10;

        private static string htmlEmbeddedFile = "Dynamo.Notifications.Web.index.html";
        private static string fontEmbeddedFile = "Dynamo.Notifications.Web.ArtifaktElement-Regular.woff";
        private static string jsEmbeddedFile = "Dynamo.Notifications.Web.index.bundle.js";

        public NotificationCenterController(DynamoView dynamoView)
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

            using (Stream stream = assembly.GetManifestResourceStream(jsEmbeddedFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                var jsString = reader.ReadToEnd();
                htmlString = htmlString.Replace("#mainJs", jsString);
            }

            using (Stream stream = assembly.GetManifestResourceStream(fontEmbeddedFile))
            {
                var resourceBase64 = Utilities.ResourceUtilities.ConvertToBase64(stream);
                htmlString = htmlString.Replace("#fontStyle", resourceBase64);
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
    }
}
