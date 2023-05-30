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

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Interaction logic for PackageManagerView.xaml
    /// </summary>
    public partial class PackageManagerView : Window
    {
        private DynamoViewModel dynamoViewModel;
        public PackageManagerSearchViewModel PkgSearchVM { get; set; }

        public PackageManagerView(DynamoView dynamoView, DynamoViewModel dynamoViewModel, PackageManagerSearchViewModel pm)
        {
            this.dynamoViewModel = dynamoViewModel;
            this.PkgSearchVM = pm;
            this.DataContext = PkgSearchVM;

            InitializeComponent();

            PkgSearchVM.RegisterTransientHandlers();
            PkgSearchVM.RequestShowFileDialog += OnRequestShowFileDialog;

            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Open,
                Categories.PackageManager);

            dynamoView.EnableEnvironment(false);
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


        /// <summary>
        /// Executes a command that opens the package details view extension.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewDetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.DataContext is PackageManagerSearchElementViewModel packageManagerSearchElementViewModel)) return;

            PkgSearchVM.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.Model);
        }

        /// <summary>
        /// Call this method to optionally bring up terms of use dialog. User 
        /// needs to accept terms of use before any packages can be downloaded 
        /// from package manager.
        /// </summary>
        /// <returns>Returns true if the terms of use for downloading a package 
        /// is accepted by the user, or false otherwise. If this method returns 
        /// false, then download of package should be terminated.</returns>
        /// 
        private bool DisplayTermsOfUseForAcceptance()
        {
            var prefSettings = dynamoViewModel.Model.PreferenceSettings;
            if (prefSettings.PackageDownloadTouAccepted)
                return true; // User accepts the terms of use.

            Window packageManParent = null;
            //If any Guide is being executed then the ShowTermsOfUse Window WON'T be modal otherwise will be modal (as in the normal behavior)
            if (dynamoViewModel.MainGuideManager != null && GuideFlowEvents.IsAnyGuideActive)
                packageManParent = PackageManagerWindow;
            var acceptedTermsOfUse = TermsOfUseHelper.ShowTermsOfUseDialog(false, null, packageManParent);
            prefSettings.PackageDownloadTouAccepted = acceptedTermsOfUse;

            // User may or may not accept the terms.
            return prefSettings.PackageDownloadTouAccepted;
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
            //managePackageCommandEvent?.Dispose();
            Analytics.TrackEvent(Actions.Close, Categories.PackageManager);
            //viewModel.PackagePathsViewModel.SaveSettingCommand.Execute(null);
            //viewModel.TrustedPathsViewModel?.SaveSettingCommand?.Execute(null);
            //dynViewModel.ShowHideFileTrustWarningIfCurrentWorkspaceTrusted();

            //viewModel.CommitPackagePathsForInstall();
            //PackagePathView.Dispose();
            //TrustedPathView.Dispose();
            //Dispose();

            //dynViewModel.PreferencesViewModel.TrustedPathsViewModel.PropertyChanged -= TrustedPathsViewModel_PropertyChanged;
            //dynViewModel.CheckCustomGroupStylesChanges(originalCustomGroupStyles);
            (this.Owner as DynamoView).EnableEnvironment(true);

            Close();
        }


        #endregion

        private void WindowClosed(object sender, EventArgs e)
        {
            PkgSearchVM.RequestShowFileDialog -= OnRequestShowFileDialog;
        }
    }
}
