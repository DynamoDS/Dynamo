using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationsView.xaml
    /// </summary>
    public partial class NotificationsView : Window
    {
        private NotificationsViewExtension model;
        public NotificationsView(NotificationsViewExtension model)
        {
            this.model = model;
            DataContext = model;
            InitializeComponent();
            this.Closed += OnNotificationsViewClosed;
            this.StateChanged += MainWindowStateChangeRaised;
            this.Owner = model.dynamoWindow;
        }

        private void OnNotificationsViewClosed(object sender, EventArgs e)
        {
            this.Closed -= OnNotificationsViewClosed;
            this.StateChanged -= MainWindowStateChangeRaised;
        }

        private void ShowDetails_ButtonClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var detailsText = btn.Tag as FrameworkElement;
            if (detailsText.Visibility == Visibility.Collapsed)
            {
                detailsText.Visibility = Visibility.Visible;
                btn.Content = Properties.Resources.ButtonHideDetails;
            }
            else
            {
                detailsText.Visibility = Visibility.Collapsed;
                btn.Content = Properties.Resources.ButtonDetails;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            model.Notifications.Remove((sender as Button).DataContext as Logging.NotificationMessage);
        }

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }
    }
}
