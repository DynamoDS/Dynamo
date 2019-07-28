using System;
using System.Windows;
using Dynamo.UI;
using Dynamo.ViewModels;
using DynamoUtilities;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PublishPackageView.xaml
    /// </summary>
    public partial class PublishPackageView : Window
    {
        /// <summary>
        /// Internal reference of PublishPackageViewModel
        /// </summary>
        private PublishPackageViewModel pkgViewModel;

        public PublishPackageView(PublishPackageViewModel packageViewModel)
        {
            this.DataContext = packageViewModel;
            pkgViewModel = packageViewModel;
            packageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;

            InitializeComponent();

            Title = string.Format(Wpf.Properties.Resources.PublishPackageViewTitle,
                packageViewModel.DynamoViewModel.BrandingResourceProvider.ProductName);
            packageViewModel.RequestShowFolderBrowserDialog += OnRequestShowFolderBrowserDialog;
            Logging.Analytics.TrackScreenView("PackageManager");
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            this.Dispatcher.BeginInvoke((Action) (Close));
        }

        private void OnRequestShowFolderBrowserDialog(object sender, PackagePathEventArgs e)
        {
            e.Cancel = true;
            // Handle for the case, initialPath does not exist.
            var errorCannotCreateFolder = PathHelper.CreateFolderIfNotExist(e.Path);
            if(errorCannotCreateFolder == null)
            {
                var dialog = new DynamoFolderBrowserDialog
                {
                    SelectedPath = e.Path,
                    Owner = this
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    e.Cancel = false;
                    e.Path = dialog.SelectedPath;
                }
            }
            else if (!String.IsNullOrEmpty(e.Path))
            {
                e.Cancel = false;
            }

        }

        private void HostEntry_CheckStateChanged(object sender, RoutedEventArgs e)
        {
            pkgViewModel.SelectedHosts.Clear();
            pkgViewModel.SelectedHostsString = string.Empty;
            foreach (var host in pkgViewModel.KnownHosts)
            {
                if (host.IsSelected)
                {
                    pkgViewModel.SelectedHosts.Add(host.HostName);
                    pkgViewModel.SelectedHostsString += host.HostName + ", ";
                }
            }
            // Format string since it will be displayed
            pkgViewModel.SelectedHostsString = pkgViewModel.SelectedHostsString.Trim().TrimEnd(',');
        }
    }

}
