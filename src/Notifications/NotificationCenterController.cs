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
        public NotificationCenterController(DynamoView dynamoView, Button notificationsButton)
        {
            notificationUIPopup = new NotificationUI();
            notificationUIPopup.IsOpen = false;
            notificationUIPopup.PlacementTarget = notificationsButton;
            notificationUIPopup.Placement = PlacementMode.Bottom;
            notificationUIPopup.HorizontalOffset = -285;
            notificationUIPopup.VerticalOffset = 10;

            dynamoView.SizeChanged += DynamoView_SizeChanged;
            dynamoView.LocationChanged += DynamoView_LocationChanged;
            notificationsButton.Click += NotificationsButton_Click;

            notificationUIPopup.webView.EnsureCoreWebView2Async();
            notificationUIPopup.webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;            
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var htmlFile = "Dynamo.Notifications.Web.index.html";
            var fontFile = "Dynamo.Notifications.Web.ArtifaktElement-Regular.woff";
            var jsFile = "Dynamo.Notifications.Web.index.bundle.js";

            string htmlString = string.Empty;

            using (Stream stream = assembly.GetManifestResourceStream(htmlFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                htmlString = reader.ReadToEnd();
            }

            using (Stream stream = assembly.GetManifestResourceStream(jsFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                var jsString = reader.ReadToEnd();
                htmlString = htmlString.Replace("#mainJs", jsString);
            }

            using (Stream stream = assembly.GetManifestResourceStream(fontFile))
            {
                var resourceBase64 = Utilities.ResourceUtilities.ConvertToBase64(stream);
                htmlString = htmlString.Replace("#fontStyle", resourceBase64);
            }

            notificationUIPopup.webView.CoreWebView2.NavigateToString(htmlString);

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
