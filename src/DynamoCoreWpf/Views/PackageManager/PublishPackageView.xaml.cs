using System;
using System.Windows;

using Dynamo.Controls;
using Dynamo.PackageManager.UI;
using Dynamo.Search.SearchElements;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using DynamoUtilities;

namespace Dynamo.PackageManager
{
    /// <summary>
    /// Interaction logic for PublishPackageView.xaml
    /// </summary>
    public partial class PublishPackageView : Window
    {
        public PublishPackageView(PublishPackageViewModel packageViewModel)
        {
            this.DataContext = packageViewModel;
            packageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;
            packageViewModel.ClearEntries += ClearEntries;
            packageViewModel.ClearFiles += ClearFiles;

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

        private void ClearEntries(PublishPackageViewModel sender)
        {
            PublishPackageViewModel newPackageViewModel = new PublishPackageViewModel(sender.DynamoViewModel);
            this.DataContext = newPackageViewModel;
            newPackageViewModel.ClearEntries += ClearEntries;
            newPackageViewModel.ClearFiles += ClearFiles;
            newPackageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;
            newPackageViewModel.RequestShowFolderBrowserDialog += OnRequestShowFolderBrowserDialog;
        }

        private void ClearFiles(PublishPackageViewModel sender)
        {
            PublishPackageViewModel newPackageViewModel = new PublishPackageViewModel(sender.DynamoViewModel);
            this.DataContext = newPackageViewModel;
            newPackageViewModel.ClearEntries += ClearEntries;
            newPackageViewModel.ClearFiles += ClearFiles;
            newPackageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;
            newPackageViewModel.RequestShowFolderBrowserDialog += OnRequestShowFolderBrowserDialog;

            // Copy all the details to newPackageViewModel.Package 
            // so that user do not have to re-filled in all those details.
            newPackageViewModel.Name = sender.Name;
            newPackageViewModel.MajorVersion = sender.MajorVersion;
            newPackageViewModel.MinorVersion = sender.MinorVersion;
            newPackageViewModel.BuildVersion = sender.BuildVersion;
            newPackageViewModel.Description = sender.Description;
            newPackageViewModel.Group = sender.Package.Group;
            newPackageViewModel.Keywords = sender.Keywords;
            newPackageViewModel.License = sender.License;
            newPackageViewModel.SiteUrl = sender.SiteUrl;
            newPackageViewModel.RepositoryUrl = sender.RepositoryUrl;
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
           
        }
    }

}
