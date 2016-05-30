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

namespace Dynamo.WarningHelper
{
    /// <summary>
    /// Interaction logic for NotificationsMenuItem.xaml
    /// </summary>
    public partial class NotificationsMenuItem : UserControl
    {
        WarningHelperViewExtension notificationsModel;

        public NotificationsMenuItem(WarningHelper.WarningHelperViewExtension notificationsExtension)
        {
            this.notificationsModel = notificationsExtension;
            InitializeComponent();

            var showItem = new MenuItem();
            showItem.Header = "Display All Notifications";
            showItem.Click += (o, e) =>
            {
                //create a window to display the list of notificationsModels
                var window = new NotificationsView(notificationsExtension);
                window.Show();
            };

            var dismissItem = new MenuItem();
            dismissItem.Header = "Dismiss All Notifications";
            dismissItem.Click += (o, e) => { this.notificationsModel.Notifications.Clear(); };

            this.MenuItem.Items.Add(showItem);
            this.MenuItem.Items.Add(dismissItem);

            //create our icon
            var color = new SolidColorBrush(Colors.LightGray);
            this.imageicon.Source = FontAwesome.WPF.ImageAwesome.CreateImageSource(FontAwesome.WPF.FontAwesomeIcon.ExclamationCircle, color);

            //create some bindings
            //attach the visibility of the badge to the number of notifications without a binding...
            this.notificationsModel.Notifications.CollectionChanged += (o, e) => {
                if (this.notificationsModel.Notifications.Count > 0)
                {
                    BadgeGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    BadgeGrid.Visibility = Visibility.Hidden;
                }
            };

            // create a binding between the label and the count of notifications
            var binding = new Binding();
            binding.Path = new PropertyPath("Notifications.Count");
            //dataContext is the extension
            CountLabel.DataContext = notificationsExtension;
            CountLabel.SetBinding(TextBlock.TextProperty, binding);

        }
    }
}
