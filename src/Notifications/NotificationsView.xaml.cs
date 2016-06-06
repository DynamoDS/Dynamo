using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
