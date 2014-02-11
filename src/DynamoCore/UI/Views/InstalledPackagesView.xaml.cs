using System.Windows;
using System.Windows.Controls;
using Dynamo.Core;
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
            DataContext = DynamoSettings.PackageLoader;
            InitializeComponent();
        }

        private void BrowseOnline_OnClick(object sender, RoutedEventArgs e)
        {
            DynamoSettings.PackageManagerClient.GoToWebsite();
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
