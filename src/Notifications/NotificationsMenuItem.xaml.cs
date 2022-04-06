using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Practices.Prism;

namespace Dynamo.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationsMenuItem.xaml
    /// </summary>
    public partial class NotificationsMenuItem : UserControl
    {
        NotificationsViewExtension notificationsModel;
        public System.Collections.Specialized.NotifyCollectionChangedEventHandler NotificationsChangeHandler;
        public NotificationsMenuItem(Notifications.NotificationsViewExtension notificationsExtension)
        {
            this.notificationsModel = notificationsExtension;
            InitializeComponent();

              var showItem = new MenuItem();
              showItem.Header = Properties.Resources.Display;
              showItem.Click += (o, e) =>
              {
                  //create a window to display the list of notificationsModels
                  var window = new NotificationsView(notificationsExtension);
                  window.Show();
              };

              var dismissItem = new MenuItem();
              dismissItem.Header = Properties.Resources.Dismiss;
              dismissItem.Click += (o, e) => { this.notificationsModel.Notifications.Clear(); };

              //set some defaults
              dismissItem.IsEnabled = false;
              showItem.IsEnabled = false;
              BadgeGrid.Visibility = Visibility.Hidden;

              this.MenuItem.Items.Add(showItem);
              this.MenuItem.Items.Add(dismissItem);

              //create our icon
                var color = new SolidColorBrush(Colors.LightGray);
                this.imageicon.Source = FontAwesome.WPF.ImageAwesome.CreateImageSource(FontAwesome.WPF.FontAwesomeIcon.ExclamationCircle, color);
          
            //create some bindings
            //attach the visibility of the badge and menuItems enabledState to the number of notifications without a binding...

            NotificationsChangeHandler = (o, e) => {
                if (this.notificationsModel.Notifications.Count > 0)
                {
                    BadgeGrid.Visibility = Visibility.Visible;
                    dismissItem.IsEnabled = true;
                    showItem.IsEnabled = true;
                }
                else
                {
                    BadgeGrid.Visibility = Visibility.Hidden;
                    dismissItem.IsEnabled = false;
                    showItem.IsEnabled = false;
                }
            };

            notificationsModel.Notifications.CollectionChanged += NotificationsChangeHandler;
            notificationsModel.AddNotifications();

            // create a binding between the label and the count of notifications
            var binding = new Binding();
            binding.Path = new PropertyPath("Notifications.Count");
            //dataContext is the extension
            CountLabel.DataContext = notificationsExtension;
            CountLabel.SetBinding(TextBlock.TextProperty, binding);

        }
    }
}
