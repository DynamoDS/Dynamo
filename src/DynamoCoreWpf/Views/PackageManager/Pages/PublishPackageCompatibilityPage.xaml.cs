using System.Windows;
using System.Windows.Controls;
using Dynamo.PackageManager;

namespace Views.PackageManager.Pages
{
    /// <summary>
    /// Interaction logic for PublishPackageCompatibilityPage.xaml
    /// </summary>
    public partial class PublishPackageCompatibilityPage : Page
    {
        private PublishPackageViewModel PublishPackageViewModel;

        public PublishPackageCompatibilityPage()
        {
            InitializeComponent();

            this.DataContextChanged += PublishPackageCompatibilityPage_DataContextChanged;
            this.Tag = "Compatibility info";
        }

        private void PublishPackageCompatibilityPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;
        }

        private void HostEntry_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            PublishPackageViewModel.SelectedHosts.Clear();
            PublishPackageViewModel.SelectedHostsString = string.Empty;
            foreach (var host in PublishPackageViewModel.KnownHosts)
            {
                if (host.IsSelected)
                {
                    PublishPackageViewModel.SelectedHosts.Add(host.HostName);
                    PublishPackageViewModel.SelectedHostsString += host.HostName + ", ";
                }
            }
            // Format string since it will be displayed
            PublishPackageViewModel.SelectedHostsString = PublishPackageViewModel.SelectedHostsString.Trim().TrimEnd(',');
        }

        public void Dispose()
        {
            this.PublishPackageViewModel = null;
            this.DataContextChanged -= PublishPackageCompatibilityPage_DataContextChanged;  
        }
    }

}
