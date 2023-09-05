using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Utilities;

namespace Dynamo.Notifications
{
    public class NotificationsViewExtension : IViewExtension, INotifyPropertyChanged
    {
        private ViewLoadedParams viewLoadedParams;
        private Action<Logging.NotificationMessage> notificationHandler;
        private ObservableCollection<Logging.NotificationMessage> notifications;
        private bool disposed;
        private NotificationCenterController notificationCenterController;
        /// <summary>
        /// Notifications data collection. PropertyChanged event is raised to help dealing WPF bind dispose.
        /// </summary>
        public ObservableCollection<Logging.NotificationMessage> Notifications
        {
            get { return notifications; }
            private set
            {
                if (notifications != value)
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
                return Properties.Resources.ExtensionName;
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
            if (!disposed)
            {
                UnregisterEventHandlers();
                //for some reason the menuItem was not being gc'd in tests without manually removing it
                viewLoadedParams?.dynamoMenu.Items.Remove(notificationsMenuItem.MenuItem);
                if (notificationsMenuItem != null)
                {
                    BindingOperations.ClearAllBindings(notificationsMenuItem.CountLabel);
                }
                notificationsMenuItem = null;
                disposed = true;
            }
        }

        private void UnregisterEventHandlers()
        {
            if (viewLoadedParams != null)
            {
                viewLoadedParams.NotificationRecieved -= notificationHandler;
            }
            if (notificationsMenuItem != null)
            {
                Notifications.CollectionChanged -= notificationsMenuItem.NotificationsChangeHandler;
            }
        }

        public void Loaded(ViewLoadedParams viewStartupParams)
        {
            viewLoadedParams = viewStartupParams;
            dynamoWindow = viewStartupParams.DynamoWindow;
            var viewModel = dynamoWindow.DataContext as DynamoViewModel;
            logger = viewModel.Model.Logger;

            Notifications = new ObservableCollection<Logging.NotificationMessage>();

            notificationHandler = (notificationMessage) =>
            {
                Notifications.Add(notificationMessage);
                AddNotifications();
            };

            viewStartupParams.NotificationRecieved += notificationHandler;

            //add a new menuItem to the Dynamo mainMenu.
            notificationsMenuItem = new NotificationsMenuItem(this);
            //null out the content of the notificationsMenu to get rid of 
            //the parent of the menuItem we created
            (notificationsMenuItem.MenuItem.Parent as ContentControl).Content = null;
            //place the menu into the DynamoMenu
            viewStartupParams.dynamoMenu.Items.Add(notificationsMenuItem.MenuItem);

            LoadNotificationCenter();
        }

        private void LoadNotificationCenter()
        {
            var dynamoView = viewLoadedParams.DynamoWindow as DynamoView;
            notificationCenterController = new NotificationCenterController(dynamoView, logger);
        }

        internal void AddNotifications()
        {
            Notifications.AddRange(logger.StartupNotifications);
            logger.ClearStartupNotifications();
        }

        public void Shutdown()
        {
            // Do nothing for now
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now
        }
    }
}
