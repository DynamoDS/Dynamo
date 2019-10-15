using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Logging;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Microsoft.Practices.Prism;

namespace Dynamo.Notifications
{
    public class NotificationsViewExtension : IViewExtension, INotifyPropertyChanged
    {
        private ViewLoadedParams viewLoadedParams;
        private Action<Logging.NotificationMessage> notificationHandler;

        private ObservableCollection<Logging.NotificationMessage> notifications;
        /// <summary>
        /// Notifications data collection. PropertyChanged event is raised to help dealing WPF bind dispose.
        /// </summary>
        public ObservableCollection<Logging.NotificationMessage> Notifications
        {
            get { return notifications; }
            private set
            {
                if(notifications != value)
                    notifications = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notifications)));
            }
        }

        private NotificationsMenuItem notificationsMenuItem;
        private DynamoLogger logger;

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

        public event PropertyChangedEventHandler PropertyChanged;

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
            var viewModel = dynamoWindow.DataContext as DynamoViewModel;
            logger = viewModel.Model.Logger;

            Notifications = new ObservableCollection<Logging.NotificationMessage>();
            
            notificationHandler = (notificationMessage) =>
            {
                Notifications.Add(notificationMessage);
                AddNotifications();
            };

            p.NotificationRecieved += notificationHandler;

            //add a new menuItem to the Dynamo mainMenu.
            notificationsMenuItem = new NotificationsMenuItem(this);
            //null out the content of the notificationsMenu to get rid of 
            //the parent of the menuItem we created
            (notificationsMenuItem.MenuItem.Parent as ContentControl).Content = null;
            //place the menu into the DynamoMenu
            p.dynamoMenu.Items.Add(notificationsMenuItem.MenuItem);
        }

        internal void AddNotifications()
        {
            Notifications.AddRange(logger.StartupNotifications);
            logger.ClearStartupNotifications();
        }

        public void Shutdown()
        {
            this.Dispose();
        }

        public void Startup(ViewStartupParams p)
        {
           
        }
    }
}
