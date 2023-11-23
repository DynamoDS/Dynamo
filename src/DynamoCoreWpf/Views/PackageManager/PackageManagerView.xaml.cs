using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Logging;
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

        public PackageManagerView(DynamoView dynamoView, PackageManagerViewModel packageManagerViewModel)
        {
            this.DataContext = this;
            this.PackageManagerViewModel = packageManagerViewModel;

            InitializeComponent();

            packageManagerViewModel.PackageSearchViewModel.RequestShowFileDialog += OnRequestShowFileDialog;
            packageManagerViewModel.PackageSearchViewModel.PackageManagerClientViewModel.ViewModelOwner = this;

            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Open,
                Categories.PackageManager);

            this.dynamoView = dynamoView;
            dynamoView.EnableEnvironment(false);
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
                    Categories.Preferences);
            }
        }


        /// <summary>
        /// Dialog close button handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent(Actions.Close, Categories.PackageManager);
            (this.Owner as DynamoView).EnableEnvironment(true);

            Close();
        }


        #endregion

        private void WindowClosed(object sender, EventArgs e)
        {
            this.packageManagerPublish.Dispose();
            this.PackageManagerViewModel.PackageSearchViewModel.RequestShowFileDialog -= OnRequestShowFileDialog;
            this.PackageManagerViewModel.PackageSearchViewModel.PackageManagerViewClose();
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
            var tabControl = this.projectManagerTabControl;

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
    }
}
