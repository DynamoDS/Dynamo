using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using Dynamo.Logging;
using Dynamo.PackageManager.ViewModels;
using Dynamo.UI;
using Dynamo.ViewModels;
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
        
        public PackageManagerSearchView(PackageManagerSearchViewModel pm)
        {
            ViewModel = pm;
            this.DataContext = ViewModel;
            InitializeComponent();
            ViewModel.RegisterTransientHandlers();
            ViewModel.RequestShowFileDialog += OnRequestShowFileDialog;
            Logging.Analytics.TrackScreenView("PackageManager");
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = DataContext as PackageManagerSearchViewModel;

            ViewModel.RequestShowFileDialog -= OnRequestShowFileDialog;
            viewModel.UnregisterTransientHandlers();

            Owner.Focus();
            base.OnClosing(e);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults(this.searchTextBox.Text);
        }

        /// <summary>
        /// TODO: mark private in Dynamo 3.0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemStackPanel_MouseDown(object sender, RoutedEventArgs e)
        {
            var lbi = sender as StackPanel;
            if (lbi == null) return;

            var viewModel = lbi.DataContext as PackageManagerSearchElementViewModel;
            if (viewModel == null) return;

            viewModel.Model.IsExpanded = !viewModel.Model.IsExpanded;
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
                System.Windows.Forms.MessageBox.Show(errorMessage, Wpf.Properties.Resources.UnableToAccessPackageDirectory, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            this.ViewModel.ViewPackageDetailsCommand.Execute(null);
        }

        /// <summary>
        /// Fires when the user clicks the 'X' button to dismiss a package download toast notification.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseDownloadToastButton_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            
            if (!(button.DataContext is PackageDownloadHandle packageDownloadHandle)) return;
            ViewModel.ClearDownloadToastNotificationCommand.Execute(packageDownloadHandle);
        }
    }
}
