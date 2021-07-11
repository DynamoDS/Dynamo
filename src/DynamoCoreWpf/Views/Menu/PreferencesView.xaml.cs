using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.ViewModels;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Interaction logic for PreferencesView.xaml
    /// </summary>
    public partial class PreferencesView : Window
    {
        private readonly PreferencesViewModel viewModel;
        private readonly DynamoViewModel dynViewModel;

        // Used for tracking the manage package command event
        // This is not a command any more but we keep it
        // around in a compatible way for now
        private IDisposable managePackageCommandEvent;

        /// <summary>
        /// Constructor of Preferences View
        /// </summary>
        /// <param name="dynamoViewModel"> Dynamo ViewModel</param>
        public PreferencesView(DynamoView dynamoView)
        {
            dynViewModel = dynamoView.DataContext as DynamoViewModel;
            SetupPreferencesViewModel(dynViewModel);

            DataContext = dynViewModel.PreferencesViewModel;
 
            InitializeComponent();
            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Open,
                Categories.Preferences);

            Owner = dynamoView;
            if (DataContext is PreferencesViewModel viewModelTemp)
            {
                viewModel = viewModelTemp;
            }

            InitRadioButtonsDescription();
        }

        /// <summary>
        ///Given that the PreferencesViewModel persists through the Dynamo session, 
        ///this method will setup all the necesary properties for when the Preferences window is opened.
        /// </summary>
        private void SetupPreferencesViewModel(DynamoViewModel dynamoViewModel)
        {
            //Clear the Saved Changes label and its corresponding tooltip when the Preferences Modal is opened
            dynamoViewModel.PreferencesViewModel.SavedChangesLabel = string.Empty;
            dynamoViewModel.PreferencesViewModel.SavedChangesTooltip = string.Empty;
            dynamoViewModel.PreferencesViewModel.PackagePathsViewModel?.InitializeRootLocations();
        }

        /// <summary>
        /// Add inline description for each geometry scalling radio button
        /// </summary>
        private void InitRadioButtonsDescription()
        {
            RadioSmallDesc.Inlines.Add(viewModel.OptionsGeometryScale.DescriptionScaleRange[0]);

            RadioMediumDesc.Inlines.Add(new Run(Res.ChangeScaleFactorPromptDescriptionDefaultSetting) { FontWeight = FontWeights.Bold });
            RadioMediumDesc.Inlines.Add(" " + viewModel.OptionsGeometryScale.DescriptionScaleRange[1]);

            RadioLargeDesc.Inlines.Add(viewModel.OptionsGeometryScale.DescriptionScaleRange[2]);

            RadioExtraLargeDesc.Inlines.Add(viewModel.OptionsGeometryScale.DescriptionScaleRange[3]);
        }

        /// <summary>
        /// Dialog close button handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            managePackageCommandEvent?.Dispose();
            Dynamo.Logging.Analytics.TrackEvent(
                Actions.Close,
                Categories.Preferences);
            viewModel.PackagePathsViewModel.SaveSettingCommand.Execute(null);
            PackagePathView.Dispose();
            Close();
        }

        /// <summary>
        /// handler for preferences dialog dragging action. When the TitleBar is clicked this method will be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreferencesPanel_MouseDown(object sender, MouseButtonEventArgs e)
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

        private void AddStyleButton_Click(object sender, RoutedEventArgs e)
        {
            AddStyleBorder.Visibility = Visibility.Visible;
            AddStyleButton.IsEnabled = false;
            groupNameBox.Focus();
        }

        private void AddStyle_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveChangesButton = sender as Button;

            //Get the Grid that contains the Stack Panel that also contains the info related to the new style
            var grid = (saveChangesButton.Parent as Grid).Parent as Grid;

            var groupNameLabel = grid.FindName("groupNameBox") as TextBox;

            var colorHexString = grid.FindName("colorHexVal") as Label;

            var newItem = new StyleItem() { GroupName = groupNameLabel.Text, HexColorString = colorHexString.Content.ToString() };

            if (string.IsNullOrEmpty(newItem.GroupName))
                newItem.GroupName = "Input";

            //if the validation returns false it means that the new style that will be added doesn't exists
            if (viewModel.ValidateExistingStyle(newItem) == false)
            {
                viewModel.AddStyle(newItem);
                viewModel.ResetAddStyleControl();
                AddStyleBorder.Visibility = Visibility.Collapsed;
                AddStyleButton.IsEnabled = true;
            }
            else
            {
                viewModel.IsWarningEnabled = true;
            }
            
        }

        private void AddStyle_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddStyleBorder.Visibility = Visibility.Collapsed;
            AddStyleButton.IsEnabled = true;
            viewModel.ResetAddStyleControl();
        }

        private void RemoveStyle_Click(object sender, RoutedEventArgs e)
        {
           var removeButton = sender as Button;

            //Get the Grid that contains all the buttons in the StyleItem
           var grid = (removeButton.Parent as Grid).Parent as Grid;

            //Find inside the Grid the label that contains the GroupName (unique id)
           var groupNameLabel = grid.FindName("groupNameLabel") as Label;

            //Remove the selected style from the list
            viewModel.RemoveStyleEntry(groupNameLabel.Content.ToString());
        }

        private void ButtonColorPicker_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Button colorButton = sender as Button;
                if (colorButton != null)
                    colorButton.Background = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
            }
        }

        /// <summary>
        /// This event is generated every time the user clicks a Radio Button in the Geometry Scaling section
        /// This are the values used for the scales:
        /// - 2 - Small
        ///   0 - Medium (Default)
        ///   2 - Large
        ///   4 - Extra Large
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Geometry_Scaling_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton selectedScaling = sender as RadioButton;
            var radioButtons = GeometryScalingRadiosPanel.Children.OfType<RadioButton>();

            int index = 0;
            int scaleValue = 0;

            //We need to loop all the radiobuttons in the GeometryScaling section in order to find the index of the selected one
            foreach (var radio in radioButtons)
            {
                if(radio == selectedScaling)
                {
                    scaleValue = GeometryScalingOptions.ConvertUIToScaleFactor(index);
                    break;
                }
                index++;
            }

            //If the new radio button selected (ScaleValue) is different than the current one in Dynamo, we update the current one
            if (dynViewModel.ScaleFactorLog != scaleValue)
            {
                dynViewModel.ScaleFactorLog = scaleValue;
                dynViewModel.CurrentSpace.HasUnsavedChanges = true;

                //Due that binding are done before the contructor of this class we need to execute the Log only if the viewModel was assigned previously
                if (viewModel != null)
                {
                    Log(String.Format("Geometry working range changed to {0} ({1}, {2})",
                    viewModel.ScaleRange.Item1, viewModel.ScaleRange.Item2, viewModel.ScaleRange.Item3));
                    viewModel.UpdateSavedChangesLabel();
                    Dynamo.Logging.Analytics.TrackEvent(
                        Actions.Switch,
                        Categories.Preferences,
                        Res.PreferencesViewVisualSettingsGeoScaling);
                }                 

                var allNodes = dynViewModel.HomeSpace.Nodes;
                dynViewModel.HomeSpace.MarkNodesAsModifiedAndRequestRun(allNodes, forceExecute: true);
            }
        }

        private void Log(ILogMessage obj)
        {
            dynViewModel.Model.Logger.Log(obj);
        }

        private void Log(string message)
        {
            Log(LogMessage.Info(message));
        }

        private void OnMoreInfoClicked(object sender, RoutedEventArgs e)
        {
            dynViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.NodeAutocompleteDocumentationUriString, UriKind.Relative)));
        }

        private void ReloadCPython_Click(object sender, RoutedEventArgs e)
        {
            dynViewModel.Model.OnRequestPythonReset("CPython3");
        }

        private void InstalledPackagesExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == e.Source)
            {
                managePackageCommandEvent = Analytics.TrackCommandEvent("ManagePackage");
            }
        }

        private void InstalledPackagesExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == e.Source)
            {
                managePackageCommandEvent?.Dispose();
            }
        }
    }
}