using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.NotificationCenter
{
    public class ViewExtension : IViewExtension
    {
        public static readonly string ExtensionName = "NotificationUI - WebView2";

        public string UniqueId { 
            get { return "2c24fadf-38c0-47fb-8b7d-18e6c3053754"; } 
        }

        public string Name => ExtensionName;

        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (!DynamoModel.IsTestMode)
            {
                //var dynamoView = (DynamoView)viewLoadedParams.DynamoWindow;
                //var shortcutBar = dynamoView.ShortcutBar;
                //var notificationsButton = (Button)shortcutBar.FindName("notificationsButton");
                //var notificationCenterController = new NotificationCenterController(dynamoView, notificationsButton);
            }
        }

        public void Shutdown()
        {
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
        }
    }
}
