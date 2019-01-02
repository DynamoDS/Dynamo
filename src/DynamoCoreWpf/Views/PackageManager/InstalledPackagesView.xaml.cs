using System.Windows;
using System.Windows.Controls;
using Dynamo.ViewModels;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for DynamoInstalledPackagesView.xaml
    /// </summary>
    public partial class InstalledPackagesView : Window
    {
        private readonly InstalledPackagesViewModel viewModel;

        public InstalledPackagesView(InstalledPackagesViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DataContext = viewModel;
            InitializeComponent();
            Logging.Analytics.TrackScreenView("PackageManager");
        }

        private void BrowseOnline_OnClick(object sender, RoutedEventArgs e)
        {
            viewModel.GoToWebsite();
        }

        private void MoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.IsOpen = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Owner.Focus();
        }
    }
}
