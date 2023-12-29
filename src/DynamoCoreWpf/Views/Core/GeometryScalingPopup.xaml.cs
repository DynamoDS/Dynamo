using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.ViewModels;
using ViewModels.Core;
using System.Linq;
using Dynamo.Logging;
using System;
using Res = Dynamo.Wpf.Properties.Resources;
using System.Windows.Input;

namespace Dynamo.Views
{
    public partial class GeometryScalingPopup : Popup
    {
        private GeometryScalingViewModel viewModel;
        private DynamoViewModel dynamoViewModel;
        
        public GeometryScalingPopup(DynamoViewModel dynViewModel)
        {
            InitializeComponent();

            var defaultWorkspace = dynViewModel.Workspaces.FirstOrDefault();

            if (defaultWorkspace != null && viewModel == null && defaultWorkspace.GeoScalingViewModel != null)
                viewModel = defaultWorkspace.GeoScalingViewModel;
            DataContext = viewModel;
            dynamoViewModel = dynViewModel;
        }

        /// <summary>
        /// This event is generated every time the user clicks a Radio Button in the Geometry Scaling section
        /// The method just get the Radio Button clicked and saves the ScaleValue selected
        /// This are the values used for the scales:
        /// - 2 - Small
        ///   0 - Medium (Default)
        ///   2 - Large
        ///   4 - Extra Large
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Geometry_Scaling_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            var selectedButton = sender as Button;
            if (selectedButton == null) return;
            var buttons = GeometryScalingRadiosPanel.Children.OfType<Button>();

            int index = 0;

            ////We need to loop all the radiobuttons in the GeometryScaling section in order to find the index of the selected one
            foreach (var button in buttons)
            {
                if (button == selectedButton)
                {
                    viewModel.ScaleValue = GeometryScalingOptions.ConvertUIToScaleFactor(index);
                    break;
                }
                index++;
            }
            RunGraphWhenScaleFactorUpdated();
            this.IsOpen = false;
        }

        /// <summary>
        /// This method will run the graph only if the Geometry Scaling was updated otherwise will not be executed
        /// </summary>
        private void RunGraphWhenScaleFactorUpdated()
        {
            //If the new radio button selected (ScaleValue) is different than the current one in Dynamo, we update the current one
            if (dynamoViewModel.ScaleFactorLog != viewModel.ScaleValue)
            {
                dynamoViewModel.ScaleFactorLog = (int)viewModel.ScaleValue;
                dynamoViewModel.CurrentSpace.HasUnsavedChanges = true;

                //Due that binding are done before the constructor of this class we need to execute the Log only if the viewModel was assigned previously
                if (viewModel != null)
                {
                    Log(String.Format("Geometry working range changed to {0} ({1}, {2})",
                    viewModel.ScaleRange.scaleName, viewModel.ScaleRange.minValue, viewModel.ScaleRange.maxValue));
                    Dynamo.Logging.Analytics.TrackEvent(
                        Actions.Switch,
                        Categories.Preferences,
                        Res.PreferencesViewGeneralSettingsGeoScaling);
                }

                var allNodes = dynamoViewModel.HomeSpace.Nodes;
                dynamoViewModel.HomeSpace.MarkNodesAsModifiedAndRequestRun(allNodes, forceExecute: true);
            }
        }

        /// <summary>
        /// Relocate the Popup when the Dynamo window is moved or resized
        /// </summary>
        internal void UpdatePopupLocation()
        {
            if (IsOpen)
            {
                var positionMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                positionMethod.Invoke(this, null);
            }
        }

        private void Log(ILogMessage obj)
        {
            dynamoViewModel.Model.Logger.Log(obj);
        }

        private void Log(string message)
        {
            Log(LogMessage.Info(message));
        }
    }
}
