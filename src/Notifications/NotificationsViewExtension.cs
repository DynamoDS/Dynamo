using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dynamo.Notifications
{
    public class NotificationsViewExtension : IViewExtension
    {
        public ObservableCollection<Logging.NotificationMessage> Notifications { get; private set; }
     
        public string Name
        {
            get
            {
                return "NotificationsExtension";
            }
        }

        public string UniqueId
        {
            get
            {
                return "ef6cd025-514f-44cd-b6b1-69d9f5cce004";
            }
        }

        internal Window dynamoWindow;
        
        public void Dispose()
        {
           // UnregisterEventHandlers();
        }

        public void Loaded(ViewLoadedParams p)
        {
            dynamoWindow = p.DynamoWindow;

            p.NotificationRecieved += (notificationMessage) =>
            {
                Notifications.Add(notificationMessage);
            };
             
           
            Notifications = new ObservableCollection<Logging.NotificationMessage>();

            //add a new menuItem to the Dynamo mainMenu.
            var notificationsMenuItem = new NotificationsMenuItem(this);
            //null out the content of the notificationsMenu to get rid of 
            //the parent of the menuItem we created
            (notificationsMenuItem.MenuItem.Parent as ContentControl).Content = null;
            //place the menu into the DynamoMenu
            p.dynamoMenu.Items.Add(notificationsMenuItem.MenuItem);
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Startup(ViewStartupParams p)
        {
           
        }
    }
}
