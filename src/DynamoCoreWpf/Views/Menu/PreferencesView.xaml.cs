using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Dynamo.Configuration;
using System.Windows.Media;
using Dynamo.ViewModels;
using Res = Dynamo.Wpf.Properties.Resources;
using System.Linq;
using Dynamo.Logging;

namespace Dynamo.Wpf.Views
{
    /// <summary>
    /// Interaction logic for PreferencesView.xaml
    /// </summary>
    public partial class PreferencesView : Window
    {
        private PreferencesViewModel viewModel;
        private DynamoViewModel dynViewModel;

        public PreferencesView(DynamoViewModel dynamoViewModel)
        {
            DataContext = new PreferencesViewModel(dynamoViewModel);
            dynViewModel = dynamoViewModel;

            
            InitializeComponent();

            //If we want the PreferencesView window to be modal, we need to assign the owner (since we created a new Style and not following the common Style)
            this.Owner = Application.Current.MainWindow;
            var viewModelTemp = DataContext as PreferencesViewModel;
            if (viewModelTemp != null)
            {
                viewModel = viewModelTemp;
            }

            InitRadioButtonsDescription();
        }

        private void InitRadioButtonsDescription()
        {
            RadioSmallDesc.Inlines.Add(viewModel.OptionsGeometryScal.DescriptionScaleRange[0]);

            RadioMediumDesc.Inlines.Add(new Run(Res.ChangeScaleFactorPromptDescriptionDefaultSetting) { FontWeight = FontWeights.Bold });
            RadioMediumDesc.Inlines.Add(" " + viewModel.OptionsGeometryScal.DescriptionScaleRange[1]);

            RadioLargeDesc.Inlines.Add(viewModel.OptionsGeometryScal.DescriptionScaleRange[1]);

            RadioExtraLargeDesc.Inlines.Add(viewModel.OptionsGeometryScal.DescriptionScaleRange[2]);
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //When the TitleBar is clicked this method will be executed
        private void PreferencesPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Drag functionality when the TitleBar is clicked with the left button and dragged to another place
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
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
                viewModel.StyleItemsList.Add(newItem);
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

        private void removeStyle_Click(object sender, RoutedEventArgs e)
        {
           var removeButton = sender as Button;

            //Get the Grid that contains all the buttons in the StyleItem
           var grid = (removeButton.Parent as Grid).Parent as Grid;

            //Find inside the Grid the label that contains the GroupName (unique id)
           var groupNameLabel = grid.FindName("groupNameLabel") as Label;

            //Remove the selected style from the list
            viewModel.RemoveStyleEntry(groupNameLabel.Content.ToString());
        }

        private void buttonColorPicker_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Button colorButton = sender as Button;
                if (colorButton != null)
                    colorButton.Background = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander currentExpander = sender as Expander;
            Grid parentGrid = currentExpander.Parent as Grid;
            foreach (Expander expander in parentGrid.Children)
            {
                if (expander != currentExpander)
                    expander.IsExpanded = false;

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
    }
}