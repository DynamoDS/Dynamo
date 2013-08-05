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
using System.Windows.Shapes;
using Dynamo.Utilities;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for DynamoInstalledPackagesView.xaml
    /// </summary>
    public partial class InstalledPackagesView : Window
    {
        public InstalledPackagesView()
        {

            this.Owner = dynSettings.Bench;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.DataContext = dynSettings.PackageLoader;
            InitializeComponent();

        }

        private void BrowseOnline_OnClick(object sender, RoutedEventArgs e)
        {
            dynSettings.PackageManagerClient.GoToWebsite();
        }
    }
}
