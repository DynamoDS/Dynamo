using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
