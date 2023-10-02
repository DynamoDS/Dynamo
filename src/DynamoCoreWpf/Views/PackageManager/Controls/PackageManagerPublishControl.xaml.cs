using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Logging;
using Dynamo.UI;
using Dynamo.ViewModels;
using DynamoUtilities;
using Views.PackageManager.Pages;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerPublishControl.xaml
    /// </summary>
    public partial class PackageManagerPublishControl : UserControl
    {
        public Window Owner { get; set; }
        public PublishPackageViewModel PublishPackageViewModel { get; set; }
        private Dictionary<int, Page> PublishPages { get; set; }
        public ObservableCollection<string> Breadcrumbs { get; } = new ObservableCollection<string>();

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

            InitializePages();

            this.mainFrame.NavigationService.Navigate(PublishPages[0]);

            this.Loaded -= InitializeContext;
        }

        private void InitializePages()
        {
            PublishPages = new Dictionary<int, Page>();

            PublishPages[0] = new PublishPackagePublishPage();
            PublishPages[1] = new PublishPackageSelectPage();
            PublishPages[2] = new PublishPackagePreviewPage();
            PublishPages[3] = new PublishPackageReadyToPublishPage();
            PublishPages[4] = new PublishPackageFinishPage();

            foreach(var pageEntry in PublishPages) { pageEntry.Value.DataContext = PublishPackageViewModel; }

            Breadcrumbs.Add((string)PublishPages[0].Tag); // Initial breadcrumb
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

        private void OnMoreInfoClicked(object sender, MouseButtonEventArgs e)
        {
            PublishPackageViewModel.DynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.PublishPackageMoreInfoFile, UriKind.Relative)));
        }

        internal void BreadcrumbButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                switch (button.Tag)
                {
                    case "filesDialogButton":
                    case "folderDialogButton":
                        Breadcrumbs.Add((string)PublishPages[1].Tag);
                        this.mainFrame.NavigationService.Navigate(PublishPages[1]);
                        break;
                    case var value when value == (string)PublishPages[0].Tag:
                        int stepIndex = Breadcrumbs.IndexOf((string)PublishPages[0].Tag);
                        for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
                        {
                            Breadcrumbs.RemoveAt(i);
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[0]);
                        break;
                    case var value when value == (string)PublishPages[1].Tag:
                        stepIndex = Breadcrumbs.IndexOf((string)PublishPages[1].Tag);
                        for (int i = Breadcrumbs.Count - 1; i > stepIndex; i--)
                        {
                            Breadcrumbs.RemoveAt(i);
                        }
                        this.mainFrame.NavigationService.Navigate(PublishPages[1]); 
                        break;

                }
            }
        }
    }
}
