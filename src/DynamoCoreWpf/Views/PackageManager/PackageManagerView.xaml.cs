using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;

namespace Dynamo.PackageManager.UI
{
    internal class OpenPackageManagerEventArgs : EventArgs
    {
        private string tab;

        internal string Tab { get { return tab; } }

        internal OpenPackageManagerEventArgs(string _Tab)
        {
            tab = _Tab;
        }
    }

    /// <summary>
    /// The PackageManagerSizeEventArgs will be used only when we want to show the PackageManagerView using a specific Width and Height
    /// </summary>
    internal class PackageManagerSizeEventArgs : EventArgs
    {
        internal double Width;
        internal double Height;
        internal PackageManagerSizeEventArgs(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
    /// <summary>
    /// Interaction logic for PackageManagerView.xaml
    /// </summary>
    public partial class PackageManagerView : Window
    {
        /// <summary>
        /// The main View Model containing all other view models for each control
        /// </summary>
        public PackageManagerViewModel PackageManagerViewModel { get; set; }
        private DynamoView dynamoView;

        /// <summary>
        /// A bool controlling the appearance of the Package Publish component from feature flag
        /// TODO: remove this public property and archive the feature flag in Dynamo 4.0 ?
        /// </summary>
        public bool IsNewPMPublishWizardEnabled
        {
            get
            {
                return DynamoModel.FeatureFlags?.CheckFeatureFlag("IsNewPMPublishWizardEnabled", true) ?? true;
            }
        }


        public PackageManagerView(DynamoView dynamoView, PackageManagerViewModel packageManagerViewModel)
        {
            this.DataContext = this;
            this.PackageManagerViewModel = packageManagerViewModel;

            InitializeComponent();

            if (packageManagerViewModel != null )
            {
                packageManagerViewModel.PackageSearchViewModel.RequestShowFileDialog += OnRequestShowFileDialog;
                packageManagerViewModel.PackageSearchViewModel.PackageManagerClientViewModel.ViewModelOwner = this;
            }

            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Open,
                Categories.PackageManager);

            this.dynamoView = dynamoView;
            dynamoView.EnableOverlayBlocker(true);
        }

        private void OnRequestShowFileDialog(object sender, PackagePathEventArgs e)
        {
            string initialPath = this.PackageManagerViewModel.PackageSearchViewModel
                .PackageManagerClientViewModel.DynamoViewModel.Model.PathManager.DefaultPackagesDirectory;

            e.Cancel = true;

            // Handle for the case, initialPath does not exist.
            var errorCannotCreateFolder = PathHelper.CreateFolderIfNotExist(initialPath);
            if (errorCannotCreateFolder == null)
            {
                var dialog = new DynamoFolderBrowserDialog
                {
                    // Navigate to initial folder.
                    SelectedPath = initialPath,
                    Owner = this
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    e.Cancel = false;
                    e.Path = dialog.SelectedPath;
                }

            }
            else
            {
                string errorMessage = string.Format(Wpf.Properties.Resources.PackageFolderNotAccessible, initialPath);
                MessageBoxService.Show(errorMessage, Wpf.Properties.Resources.UnableToAccessPackageDirectory, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region ui tools

        /// <summary>
        /// handler for preferences dialog dragging action. When the TitleBar is clicked this method will be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageManagerPanel_MouseDown(object sender, MouseButtonEventArgs e)
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


        /// <summary>
        /// Dialog close button handler
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


        #endregion

        private void WindowClosed(object sender, EventArgs e)
        {
            this.packageManagerPublish?.Dispose();
            this.packageManagerSearch?.Dispose();
            (this.Owner as DynamoView).EnableOverlayBlocker(false);

            if (PackageManagerViewModel == null) return;
            this.PackageManagerViewModel.PackageSearchViewModel.RequestShowFileDialog -= OnRequestShowFileDialog;
            this.PackageManagerViewModel.PackageSearchViewModel.PackageManagerViewClose();
            this.PackageManagerViewModel.PublishPackageViewModel.CancelCommand.Execute();
        }

        private void SearchForPackagesButton_Click(object sender, RoutedEventArgs e)
        {
            // Search for Packages tab
            Navigate(Dynamo.Wpf.Properties.Resources.PackageManagerSearchTab);
        }

        private void PublishPackageButton_Click(object sender, RoutedEventArgs e)
        {
            // Publish a Package tab
            Navigate(Dynamo.Wpf.Properties.Resources.PackageManagerPublishTab);
        }

        /// <summary>
        ///  Navigatest to the selected tab
        /// </summary>
        /// <param name="tabName">Tab name to navigate to</param>
        internal void Navigate(string tabName)
        {
            var tabControl = this.packageManagerTabControl;

            var preferencesTab = (from TabItem tabItem in tabControl.Items
                                  where tabItem.Header.ToString().Equals(tabName)
                                  select tabItem).FirstOrDefault();
            if (preferencesTab == null) return;
            tabControl.SelectedItem = preferencesTab;
        }

        private void NavigateToPreferencesPanel()
        {
            var tabName = Dynamo.Wpf.Properties.Resources.PreferencesPackageManagerSettingsTab;
            var expanderName = Dynamo.Wpf.Properties.Resources.PackagePathsExpanderName;
            PreferencesPanelUtilities.OpenPreferencesPanel(dynamoView, WindowStartupLocation.CenterOwner, tabName, expanderName);
        }

        private void OnPackageManagerSettingsHyperlinkClicked(object sender, RoutedEventArgs e)
        {
            this.WindowClosed(this.CloseButton, e);
            this.Close();

            NavigateToPreferencesPanel();
        }

        private void CloseToastButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.loadingSearchWarningBar.Visibility = Visibility.Collapsed;
            this.loadingMyPackagesWarningBar.Visibility = Visibility.Collapsed;
        }

        private void tab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var selectedTab = sender as TabItem;
            if (selectedTab == null) return;
            var tabControl = selectedTab.Parent as TabControl;
            if (tabControl == null) return;
            var prevTab = tabControl.SelectedItem as TabItem;
            if (prevTab == null) return;

            if (prevTab.Name.Equals("publishTab") && !selectedTab.Name.Equals("publishTab"))
            {
                var isPublishing = PackageManagerViewModel.PublishPackageViewModel.UploadState.Equals(PackageUploadHandle.State.Uploading) ||
                    PackageManagerViewModel.PublishPackageViewModel.UploadState.Equals(PackageUploadHandle.State.Copying) ||
                    PackageManagerViewModel.PublishPackageViewModel.UploadState.Equals(PackageUploadHandle.State.Compressing);

                if (!PackageManagerViewModel.PublishPackageViewModel.AnyUserChanges() || isPublishing)
                {
                    selectedTab.IsSelected = true;
                    return;
                }

                MessageBoxResult response = DynamoModel.IsTestMode ? MessageBoxResult.OK :
                    MessageBoxService.Show(
                    this,
                    Dynamo.Wpf.Properties.Resources.DiscardChangesWarningPopupMessage,
                    Dynamo.Wpf.Properties.Resources.DiscardChangesWarningPopupCaption,
                    MessageBoxButton.YesNoCancel,
                    new System.Collections.Generic.List<string> {
                        Dynamo.Wpf.Properties.Resources.SaveButton,
                        Dynamo.Wpf.Properties.Resources.DiscardButton,
                        Dynamo.Wpf.Properties.Resources.CancelButton },
                    MessageBoxImage.Warning);

                if (response == MessageBoxResult.Yes || response == MessageBoxResult.No)
                {
                    HandlePackageManagerNavigation(response, selectedTab);
                }
                else
                {
                    // Don't do anything
                    e.Handled = true;
                }
            }

        }

        /// <summary>
        /// Handles communicating the result to the front end
        /// Yes - navigate away, but save the progress done so far
        /// No - navigate away, and discard the progress
        /// </summary>
        /// <param name="response"></param>
        /// <param name="selectedTab"></param>
        private void HandlePackageManagerNavigation(MessageBoxResult response, TabItem selectedTab)
        {
            selectedTab.IsSelected = true;

            var pmPublishControl = this.packageManagerPublish as PackageManagerPublishControl;
            pmPublishControl?.ResetPageOrder();

            var pmHost = this.packageManagerPublishHost;
            if (pmHost?.Wizard != null)
            {
                if (response == MessageBoxResult.Yes)
                {
                    pmHost.Wizard.NavigateToPage(1);
                }
                else if (response == MessageBoxResult.No)
                {
                    PackageManagerViewModel.PublishPackageViewModel.CancelCommand.Execute();

                    pmHost.Wizard.ResetProgress();
                }
            }
        }

        private void OnMoreInfoClicked(object sender, MouseButtonEventArgs e)
        {
            this.PackageManagerViewModel.PackageSearchViewModel
                .PackageManagerClientViewModel.DynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.PublishPackageMoreInfoFile, UriKind.Relative)));
        }
    }
}
