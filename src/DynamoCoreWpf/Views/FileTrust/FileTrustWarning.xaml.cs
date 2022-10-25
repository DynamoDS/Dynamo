using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.Core;
using Dynamo.Wpf.ViewModels.FileTrust;

namespace Dynamo.Wpf.Views.FileTrust
{
    /// <summary>
    /// Interaction logic for RealTimeInfoWindow.xaml
    /// </summary>
    public partial class FileTrustWarning : Popup
    {

        private DynamoView mainWindow;
        private DynamoViewModel dynViewModel;
        private FileTrustWarningViewModel fileTrustWarningViewModel;

        public FileTrustWarning(DynamoView dynamoViewWindow)
        {
            InitializeComponent();

            mainWindow = dynamoViewWindow;
            var dynamoView = dynamoViewWindow as DynamoView;
            if (dynamoView == null) return;

            dynViewModel = dynamoView.DataContext as DynamoViewModel;

            fileTrustWarningViewModel = dynViewModel.FileTrustViewModel;

            if (fileTrustWarningViewModel == null)
                fileTrustWarningViewModel = new FileTrustWarningViewModel();

            DataContext = fileTrustWarningViewModel;

            if (dynamoViewWindow == null) return;

            //Creating the background of the Popup
            BackgroundRectangle.Rect = new Rect(fileTrustWarningViewModel.PopupBordersOffSet, fileTrustWarningViewModel.PopupBordersOffSet, fileTrustWarningViewModel.PopupRectangleWidth, fileTrustWarningViewModel.PopupRectangleHeight);
            SetUpPopup();

            HomeWorkspaceModel.WorkspaceClosed += CloseWarningPopup;
            dynViewModel.PropertyChanged += DynViewModel_PropertyChanged;
        }

        private void DynViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DynamoViewModel.CurrentSpace))
            {
                if (dynViewModel.ViewingHomespace)
                {
                    ManagePopupActivation(true);
                }
                else
                {
                    IsOpen = false;
                }
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //When the ShowWarningPopup property is changed we need to enable or disable the Dynamo Run section
            if (e.PropertyName == nameof(FileTrustWarningViewModel.ShowWarningPopup))
            {
                var fileTrustWarningViewModel = sender as FileTrustWarningViewModel;
                if (fileTrustWarningViewModel == null) return;
                if (fileTrustWarningViewModel.ShowWarningPopup == true)
                {
                    FindPopupPlacementTarget();
                    //Force to run all the drawing events in the Dispatcher so later we can disable the button/combobox in the Run section
                    mainWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                    DisableRunInteractivity();
                }
                else
                {
                    EnableRunInteractivity();
                }
            }
        }

        private void FindPopupPlacementTarget()
        {
            var runSettingsControl = mainWindow.RunSettingsControl;
            if (runSettingsControl == null) return;
            var runSettingsViewModel = mainWindow.RunSettingsControl.DataContext as RunSettingsViewModel;

            UIElement popupPlacementTarget;
            if (runSettingsViewModel.SelectedRunTypeItem.RunType == RunType.Manual)
            {
                popupPlacementTarget = runSettingsControl.RunButton;
                double marginRight = 13;
                HorizontalOffset = -((runSettingsControl.RunButton.Width / 2) - marginRight);
            }
            else
            {
                popupPlacementTarget = runSettingsControl.RunTypesComboBox;
            }

            PlacementTarget = popupPlacementTarget;
            Placement = PlacementMode.Top;
        }

        private void DisableRunInteractivity()
        {
            dynViewModel.HomeSpace.RunSettings.RunEnabled = false;
            dynViewModel.HomeSpace.RunSettings.RunTypesEnabled = false;
            FileTrustWarningCheckBox.IsChecked = false;
        }

        private void EnableRunInteractivity()
        {
            dynViewModel.HomeSpace.RunSettings.RunEnabled = true;
            dynViewModel.HomeSpace.RunSettings.RunTypesEnabled = true;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var tabName = Properties.Resources.PreferencesSecuritySettingsTab;
            var expanderName = Properties.Resources.TrustedPathsExpanderName;
            PreferencesPanelUtilities.OpenPreferencesPanel(mainWindow, WindowStartupLocation.CenterOwner, tabName, expanderName);
        }

        private void CloseFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileTrustWarningViewModel.ShowWarningPopup = false;
            fileTrustWarningViewModel.DynFileDirectoryName = string.Empty;
            if (dynViewModel.CloseHomeWorkspaceCommand.CanExecute(null))
            {
                dynViewModel.CloseHomeWorkspaceCommand.Execute(null);
                if (FileTrustWarningCheckBox.IsChecked.Value == true)
                {
                    dynViewModel.MainGuideManager.CreateRealTimeInfoWindow(Properties.Resources.TrustLocationSkippedNotification);
                }
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            fileTrustWarningViewModel.AllowOneTimeTrust = true;
            fileTrustWarningViewModel.ShowWarningPopup = false;

            RunSettings.ForceBlockRun = false;
            if (FileTrustWarningCheckBox.IsChecked.Value == true)
            {
                if (dynViewModel.PreferenceSettings.AddTrustedLocation(fileTrustWarningViewModel.DynFileDirectoryName))
                    dynViewModel.MainGuideManager.CreateRealTimeInfoWindow(string.Format(Properties.Resources.TrustLocationAddedNotification, fileTrustWarningViewModel.DynFileDirectoryName));
            }
            if (dynViewModel.CurrentSpaceViewModel.RunSettingsViewModel.Model.RunType != RunType.Manual)
            {
                dynViewModel.Model.CurrentWorkspace.RequestRun();
            }
            else
            {
                (dynViewModel.HomeSpaceViewModel as HomeWorkspaceViewModel).CurrentNotificationMessage = Properties.Resources.RunReady;
                (dynViewModel.HomeSpaceViewModel as HomeWorkspaceViewModel).CurrentNotificationLevel = NotificationLevel.Mild;
            }
        }

        /// <summary>
        /// Relocate the Popup when the Dynamo window is moved or resized
        /// </summary>
        internal void UpdatePopupLocation()
        {
            if (IsOpen)
            {
                FindPopupPlacementTarget();
                var positionMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                positionMethod.Invoke(this, null);
            }
        }

        internal void CleanPopup()
        {
            if (fileTrustWarningViewModel != null)
            {
                fileTrustWarningViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                fileTrustWarningViewModel.DynFileDirectoryName = string.Empty;
            }

            HomeWorkspaceModel.WorkspaceClosed -= CloseWarningPopup;
            dynViewModel.PropertyChanged -= DynViewModel_PropertyChanged;
        }

        /// <summary>
        /// This method will show/hide the Popup when the main window is Activated or Deactivated
        /// </summary>
        internal void ManagePopupActivation(bool activate)
        {
            if (dynViewModel.FileTrustViewModel.ShowWarningPopup == !activate &&
               !string.IsNullOrEmpty(dynViewModel.FileTrustViewModel.DynFileDirectoryName) &&
               RunSettings.ForceBlockRun == true)
                IsOpen = activate;
        }

        private void SetUpPopup()
        {
            if (fileTrustWarningViewModel != null)
            {
                fileTrustWarningViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Close the warning popup
        /// </summary>
        internal void CloseWarningPopup()
        {
            fileTrustWarningViewModel.ShowWarningPopup = false;
            fileTrustWarningViewModel.DynFileDirectoryName = string.Empty;
        }
    }
}
