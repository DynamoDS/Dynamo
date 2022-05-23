using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Controls;
using Dynamo.Wpf.ViewModels;
using Dynamo.Wpf.ViewModels.FileTrust;

namespace Dynamo.Wpf.Views.FileTrust
{
    /// <summary>
    /// Interaction logic for RealTimeInfoWindow.xaml
    /// </summary>
    public partial class FileTrustWarning : Popup
    {

        private Window mainWindow;
        private DynamoViewModel dynViewModel;
        private FileTrustWarningViewModel fileTrustWarningViewModel;

        public FileTrustWarning(Window dynamoViewWindow)
        {
            InitializeComponent();

            mainWindow = dynamoViewWindow;
            var dynamoView = dynamoViewWindow as DynamoView;
            if (dynamoView == null) return;

            dynViewModel = dynamoView.DataContext as DynamoViewModel;

            fileTrustWarningViewModel = dynViewModel.FileTrustWViewModel;

            if (fileTrustWarningViewModel == null)
                fileTrustWarningViewModel = new FileTrustWarningViewModel();

            DataContext = fileTrustWarningViewModel;

            if (dynamoViewWindow == null) return;
          
            //Creating the background of the Popup
            BackgroundRectangle.Rect = new Rect(fileTrustWarningViewModel.PopupBordersOffSet, fileTrustWarningViewModel.PopupBordersOffSet, fileTrustWarningViewModel.PopupRectangleWidth, fileTrustWarningViewModel.PopupRectangleHeight);     
            
            fileTrustWarningViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //When the ShowWarningPopup property is changed we need to enable or disable the Dynamo Run section
            if (e.PropertyName == "ShowWarningPopup" )
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
                    EnableRunInteractivity();
            }
        }

        private void FindPopupPlacementTarget()
        {
            var bottomBarGrid = mainWindow.FindName("bottomBarGrid") as Grid;
            if (bottomBarGrid == null) return;

            var runSettingsControl = bottomBarGrid.FindName("RunSettingsControl") as RunSettingsControl;
            if (runSettingsControl == null) return;
            var runSettingsViewModel = runSettingsControl.DataContext as RunSettingsViewModel;

            UIElement popupPlacementTarget;
            if (runSettingsViewModel.SelectedRunTypeItem.RunType == RunType.Manual)
            {
                var runButton = runSettingsControl.FindName("RunButton") as Button;
                if (runButton == null) return;
                popupPlacementTarget = runButton;
            }
            else
            {
                var runTypesComboBox = runSettingsControl.FindName("RunTypesComboBox") as ComboBox;
                if (runTypesComboBox == null) return;
                popupPlacementTarget = runTypesComboBox;
            }

            PlacementTarget = popupPlacementTarget;
            Placement = PlacementMode.Top;
        }

        private void DisableRunInteractivity()
        {
            dynViewModel.HomeSpace.RunSettings.RunEnabled = false;
            dynViewModel.HomeSpace.RunSettings.RunTypesEnabled = false;
            dynViewModel.HomeSpace.RunSettings.RunTypesComboBoxToolTipIsEnabled = true;
            FileTrustWarningCheckBox.IsChecked = false;
        }

        private void EnableRunInteractivity()
        {
            dynViewModel.HomeSpace.RunSettings.RunEnabled = true;
            if (FileTrustWarningCheckBox.IsChecked.Value == true)
            {
                dynViewModel.HomeSpace.RunSettings.RunTypesEnabled = true;
            }
            dynViewModel.HomeSpace.RunSettings.RunTypesComboBoxToolTipIsEnabled = false;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //Launch the Preference panel in the Security tab
        }

        private void CloseFileButton_Click(object sender, RoutedEventArgs e)
        {
            fileTrustWarningViewModel.ShowWarningPopup = false;
            if (dynViewModel.CloseHomeWorkspaceCommand.CanExecute(null))
                dynViewModel.CloseHomeWorkspaceCommand.Execute(null);
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            fileTrustWarningViewModel.ShowWarningPopup = false;
            if(FileTrustWarningCheckBox.IsChecked.Value == true)
            {
                if (string.IsNullOrEmpty(fileTrustWarningViewModel.DynFileDirectoryName)) return;
                if (dynViewModel.PreferenceSettings.TrustedLocations.Contains(fileTrustWarningViewModel.DynFileDirectoryName)) return;
                dynViewModel.PreferenceSettings.TrustedLocations.Add(fileTrustWarningViewModel.DynFileDirectoryName);
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
    }
}
