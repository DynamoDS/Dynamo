using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using Dynamo.Logging;
using Dynamo.PackageManager.ViewModels;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using Button = System.Windows.Controls.Button;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerSearchView.xaml
    /// </summary>
    public partial class PackageManagerSearchView : Window
    {
        public PackageManagerSearchViewModel ViewModel { get;  }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pm"></param>
        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {
            ViewModel = pm;
            this.DataContext = ViewModel;
            pm.PackageManagerClientViewModel.ViewModelOwner = this;
            InitializeComponent();
            ViewModel.RegisterTransientHandlers();
            ViewModel.RequestShowFileDialog += OnRequestShowFileDialog;
            ViewModel.RequestDisableTextSearch += ViewModel_RequestDisableTextSearch;
            Logging.Analytics.TrackScreenView("PackageManager");
        }

        private void ViewModel_RequestDisableTextSearch(object sender, PackagePathEventArgs e)
        {
            this.searchTextBox.IsEnabled = false;
            this.clearSearchTextBox.IsEnabled = false;
            this.filterResultsButton.IsEnabled = false;
            this.sortResultsButton.IsEnabled = false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = DataContext as PackageManagerSearchViewModel;

            ViewModel.RequestShowFileDialog -= OnRequestShowFileDialog;
            ViewModel.RequestDisableTextSearch -= ViewModel_RequestDisableTextSearch;
            viewModel.UnregisterTransientHandlers();
            
            // Clears the search text so that the 'Please Wait' prompt appears next time this dialog is opened.
            viewModel.SearchText = string.Empty;
            Owner.Focus();
            base.OnClosing(e);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = this.searchTextBox.Text;
            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                searchTerm = null;
            }
            (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults(searchTerm);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemStackPanel_MouseDown(object sender, RoutedEventArgs e)
        {
            var lbi = sender as StackPanel;
            if (lbi == null) return;

            var viewModel = lbi.DataContext as PackageManagerSearchElementViewModel;
            if (viewModel == null) return;

            viewModel.SearchElementModel.IsExpanded = !viewModel.SearchElementModel.IsExpanded;
        }

        private void OnShowContextMenuFromLeftClicked(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void OnSortButtonClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

        private void OnShowFilterContextMenuFromLeftClicked(object sender, RoutedEventArgs e)
        {
            var button = (System.Windows.Controls.Button)sender;
            button.ContextMenu.DataContext = button.DataContext;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }

        private void OnFilterButtonClicked(object sender, RoutedEventArgs e)
        {
            OnShowFilterContextMenuFromLeftClicked(sender, e);
        }

        private void OnInstallLatestButtonDropDownClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

        private void OnInstallVersionButtonDropDownClicked(object sender, RoutedEventArgs e)
        {
            OnShowContextMenuFromLeftClicked(sender, e);
        }

        private void OnRequestShowFileDialog(object sender, PackagePathEventArgs e)
        {
            string initialPath = (this.DataContext as PackageManagerSearchViewModel)
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

        private void PackageManagerSearchView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
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

        /// <summary>
        /// Executes a command that opens the package details view extension.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.DataContext is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            ViewModel.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.SearchElementModel);
        }

        /// <summary>
        /// Fires when the user clicks the 'X' button to dismiss a package toast notification.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseToastButton_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.DataContext is PackageDownloadHandle packageDownloadHandle)
            {
                ViewModel.ClearToastNotificationCommand.Execute(packageDownloadHandle);
            }
            else if(button.DataContext is PackageManagerSearchElement packageSearchElement)
            {
                ViewModel.ClearToastNotificationCommand.Execute(packageSearchElement);
            }
            return;
        }
    }
}
