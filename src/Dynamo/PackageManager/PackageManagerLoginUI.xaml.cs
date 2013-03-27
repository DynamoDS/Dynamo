using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Dynamo.Nodes.PackageManager;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PackageManagerLoginUI.xaml
    /// </summary>
    public partial class PackageManagerLoginUI : UserControl
    {
        public PackageManagerLoginController Controller { get; internal set; }

        public PackageManagerLoginUI( PackageManagerLoginController controller )
        {
            InitializeComponent();

            this.Controller = controller;

            this.LoginContainerStackPanel.IsVisibleChanged += delegate { if (this.LoginContainerStackPanel.Visibility == Visibility.Visible) Controller.NavigateToLogin(); };
        }

        public void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            Controller.LoginButtonClick(sender, e);
        }

        private void WebBrowserNavigatedEvent(object sender, NavigationEventArgs e)
        {
            Controller.WebBrowserNavigatedEvent(sender, e);
        }

    }
}
