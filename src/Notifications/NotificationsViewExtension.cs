using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Wpf.Extensions;
using Dynamo.Extensions;

namespace Dynamo.Notifications
{
    public class NotificationsViewExtension : IViewExtension
    {
        private ViewLoadedParams viewLoadedParams;
        private Action<Logging.NotificationMessage> notificationHandler;
        public ObservableCollection<Logging.NotificationMessage> Notifications { get; private set; }
        private NotificationsMenuItem notificationsMenuItem;

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
            UnregisterEventHandlers();
            //for some reason the menuItem was not being gc'd in tests without manually removing it
            viewLoadedParams.dynamoMenu.Items.Remove(notificationsMenuItem.MenuItem);
            BindingOperations.ClearAllBindings(notificationsMenuItem.CountLabel);
            notificationsMenuItem = null;
        }

        private void UnregisterEventHandlers()
        {
            viewLoadedParams.NotificationRecieved -= notificationHandler;
            Notifications.CollectionChanged -= notificationsMenuItem.NotificationsChangeHandler;
        }

        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            dynamoWindow = p.DynamoWindow;
            Notifications = new ObservableCollection<Logging.NotificationMessage>();
            
            notificationHandler = new Action<Logging.NotificationMessage>((notificationMessage) =>
            {
                Notifications.Add(notificationMessage);
            });

            p.NotificationRecieved += notificationHandler;
             
            //add a new menuItem to the Dynamo mainMenu.
            notificationsMenuItem = new NotificationsMenuItem(this);
            //null out the content of the notificationsMenu to get rid of 
            //the parent of the menuItem we created
            (notificationsMenuItem.MenuItem.Parent as ContentControl).Content = null;
            //place the menu into the DynamoMenu
            p.dynamoMenu.Items.Add(notificationsMenuItem.MenuItem);
        }

        public void Shutdown()
        {
            this.Dispose();
        }

        public void Startup(StartupParams p)
        {
           
        }
    }
}
