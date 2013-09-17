using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dynamo.Controls;
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
            this.DataContext = dynSettings.PackageLoader;
            InitializeComponent();
        }

        private void BrowseOnline_OnClick(object sender, RoutedEventArgs e)
        {
            dynSettings.PackageManagerClient.GoToWebsite();
        }

        private void MoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.IsOpen = true;
            
            //if (e.LeftButton == MouseButtonState.Pressed)
            //{



            //}
        }

    }
}
