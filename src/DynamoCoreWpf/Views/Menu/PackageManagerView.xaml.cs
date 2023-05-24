using System.Windows;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Wpf.Views.PackageManager;
using static Dynamo.ViewModels.SearchViewModel;

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Interaction logic for PackageManagerView.xaml
    /// </summary>
    public partial class PackageManagerView : Window
    {
        public PackageManagerView(DynamoView dynamoView)
        {
            InitializeComponent();

            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Open,
                Categories.PackageManager);

            Owner = dynamoView;

            dynamoView.EnableEnvironment(false);
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
    }
}
