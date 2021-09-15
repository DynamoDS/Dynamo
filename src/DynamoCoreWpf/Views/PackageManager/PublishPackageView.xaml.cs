using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Logging;
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
        public PublishPackageViewModel PublishPackageViewModel { get; }

        public PublishPackageView(PublishPackageViewModel publishPackageViewModel)
        {
            this.DataContext = publishPackageViewModel;
            PublishPackageViewModel = publishPackageViewModel;
            publishPackageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;

            InitializeComponent();

            Title = string.Format(Wpf.Properties.Resources.PublishPackageViewTitle,
                publishPackageViewModel.DynamoViewModel.BrandingResourceProvider.ProductName);
            publishPackageViewModel.RequestShowFolderBrowserDialog += OnRequestShowFolderBrowserDialog;
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

        private void OnMoreInfoClicked(object sender, RoutedEventArgs e)
        {
            PublishPackageViewModel.DynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.PublishPackageMoreInfoFile, UriKind.Relative)));
        }

        /// <summary>
        /// When the use clicks close on this window, closes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Close,
                Categories.PackageManagerOperations);

            Close();
        }

        private void PreferencesPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
                Dynamo.Logging.Analytics.TrackEvent(
                    Actions.Move,
                    Categories.PackageManagerOperations);
            }
        }
    }
}
