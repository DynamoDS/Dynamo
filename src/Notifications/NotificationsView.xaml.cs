using System.Windows;
using System.Windows.Controls;

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
            this.Owner = model.dynamoWindow;
        }

        private void ShowDetails_ButtonClick(object sender, RoutedEventArgs e)
        {
            var detailsText = (sender as Button).Tag as FrameworkElement;
            if (detailsText.Visibility == Visibility.Collapsed)
            {
                detailsText.Visibility = Visibility.Visible;
            }
            else
            {
                detailsText.Visibility = Visibility.Collapsed;
            }

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            model.Notifications.Remove((sender as Button).DataContext as Logging.NotificationMessage);
        }
    }
}
