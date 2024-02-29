using System;
using System.Windows;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.PackageManager.UI;
using Dynamo.PackageManager;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views.PackageManager;
using static Dynamo.ViewModels.SearchViewModel;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.PackageManager.ViewModels;
using System.Windows.Controls;
using Dynamo.UI;
using Dynamo.Wpf.Utilities;
using DynamoUtilities;
using System.Collections.ObjectModel;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for PackageManagerView.xaml
    /// </summary>
    public partial class PackageManagerView : Window
    {
        /// <summary>
        /// The main View Model containing all other view models for each control
        /// </summary>
        public PackageManagerViewModel PackageManagerViewModel { get; set; }

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

            Close();
        }


        #endregion

        private void WindowClosed(object sender, EventArgs e)
        {
            this.packageManagerPublish.Dispose();
            this.PackageManagerViewModel.PackageSearchViewModel.RequestShowFileDialog -= OnRequestShowFileDialog;
        }
    }
}
