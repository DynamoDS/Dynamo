using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Notifications;
using Dynamo.Notifications.View;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests.ViewExtensions
{
    public class NotificationsExtensionTests : DynamoTestUIBase
    {
        [Test]
        public void PressNotificationButtonAndShowPopup()
        {
            var shortcutBar = this.View.ShortcutBar;
            var notificationsButton = (Button)shortcutBar.FindName("notificationsButton");
            notificationsButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            var notificationExtension = this.View.viewExtensionManager.ViewExtensions.OfType<NotificationsViewExtension>().FirstOrDefault();
            NotificationUI notificationUI = null;

            // Wait for the NotificationCenterController webview2 control to finish initialization
            DispatcherUtil.DoEventsLoop(() =>
            {
                if (notificationExtension.notificationCenterController.initState == DynamoUtilities.AsyncMethodState.Done)
                {
                    notificationUI = PresentationSource.CurrentSources.OfType<System.Windows.Interop.HwndSource>()
                            .Select(h => h.RootVisual)
                            .OfType<FrameworkElement>()
                            .Select(f => f.Parent)
                            .OfType<NotificationUI>()
                            .FirstOrDefault(p => p.IsOpen);

                    return notificationUI != null;
                }
                return false;
            }, 180);
            
            Assert.NotNull(notificationUI, "Notification popup not part of the dynamo visual tree");
            var webView = notificationUI.FindName("webView");
            Assert.NotNull(webView, "WebView framework element not found.");
        }

        [Test]
        public void ValidateNotificationsUIEmbededFiles()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x=>x.ManifestModule.Name.Contains("Notifications.dll"));
            var htmlFile = "Dynamo.Notifications.Packages.NotificationCenter.build.index.html";

            var mainJstag = "mainJs";     

            using (Stream stream = assembly.GetManifestResourceStream(htmlFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                string htmlString = reader.ReadToEnd();
                Assert.IsTrue(htmlString.Contains(mainJstag));
            }
        }
    }
}
