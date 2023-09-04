using Dynamo.PackageManager;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Dynamo.UI;
using Dynamo.ViewModels;
using DynamoUtilities;
using System.Diagnostics;
using Dynamo.Logging;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishControl.xaml
    /// </summary>
    public partial class PackageManagerPublishControl : UserControl
    {
        public Window Owner { get; set; }
        public PublishPackageViewModel PublishPackageViewModel { get; set; }

        public PackageManagerPublishControl()
        {
            InitializeComponent();

            this.Loaded += InitializeContext;
        }


        private void InitializeContext(object sender, RoutedEventArgs e)
        {
            // Set the owner of this user control
            this.Owner = Window.GetWindow(this);

            PublishPackageViewModel = this.DataContext as PublishPackageViewModel;

            PublishPackageViewModel.PublishSuccess += PackageViewModelOnPublishSuccess;

            PublishPackageViewModel.RequestShowFolderBrowserDialog += OnRequestShowFolderBrowserDialog;
            Logging.Analytics.TrackScreenView("PackageManager");
        }

        private void PackageViewModelOnPublishSuccess(PublishPackageViewModel sender)
        {
            //this.Dispatcher.BeginInvoke((Action)(Close));
            //PublishPackageViewModel.PublishSuccess -= PackageViewModelOnPublishSuccess;
        }

        private void OnRequestShowFolderBrowserDialog(object sender, PackagePathEventArgs e)
        {
            e.Cancel = true;
            // Handle for the case, initialPath does not exist.
            var errorCannotCreateFolder = PathHelper.CreateFolderIfNotExist(e.Path);
            if (errorCannotCreateFolder == null)
            {
                var dialog = new DynamoFolderBrowserDialog
                {
                    SelectedPath = e.Path
                };

                dialog.Owner = Owner;

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

        public void Dispose()
        {
            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Close,
                Categories.PackageManagerOperations);

            PublishPackageViewModel.RequestShowFolderBrowserDialog -= OnRequestShowFolderBrowserDialog;
        }

        /// <summary>
        /// Allows for the dragging of this custom-styled window. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PublishPackageView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Owner.DragMove();
                Dynamo.Logging.Analytics.TrackEvent(
                    Actions.Move,
                    Categories.PackageManagerOperations);
            }
        }

        /// <summary>
        /// Navigates to a predefined URL in the user's default browser.
        /// Currently used to make the MIT license text a clickable link.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void OnMoreInfoClicked(object sender, MouseButtonEventArgs e)
        {
            PublishPackageViewModel.DynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.PublishPackageMoreInfoFile, UriKind.Relative)));
        }

    }
}
