using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Exceptions;
using Dynamo.Logging;
using Dynamo.UI;
using Dynamo.UI.Views;
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
        private List<GroupStyleItem> originalCustomGroupStyles { get; set; }

        private Button colorButtonSelected;

        // Used for tracking the manage package command event
        // This is not a command any more but we keep it
        // around in a compatible way for now
        private IDisposable managePackageCommandEvent;

        /// <summary>
        /// Storing the original custom styles before the user could update them
        /// </summary>
        private void StoreOriginalCustomGroupStyles()
        {
            originalCustomGroupStyles = new List<GroupStyleItem>();
            foreach (var groupStyle in dynViewModel.PreferenceSettings.GroupStyleItemsList)
            {
                if (!groupStyle.IsDefault)
                {
                    originalCustomGroupStyles.Add(new GroupStyleItem() { GroupStyleId = groupStyle.GroupStyleId, HexColorString = groupStyle.HexColorString, FontSize = groupStyle.FontSize });
                }                
            }
        }

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
            dynViewModel.Owner = this;
            if (DataContext is PreferencesViewModel viewModelTemp)
            {
                viewModel = viewModelTemp;
            }

            InitRadioButtonsDescription();

            ResetGroupStyleForm();
            StoreOriginalCustomGroupStyles();
            displayConfidenceLevel();

            viewModel.InitializeGeometryScaling();

            viewModel.RequestShowFileDialog += OnRequestShowFileDialog;

            LibraryZoomScalingSlider.Value = dynViewModel.Model.PreferenceSettings.LibraryZoomScale;
            updateLibraryZoomScaleValueLabel(LibraryZoomScalingSlider);
        }

        /// <summary>
        ///Given that the PreferencesViewModel persists through the Dynamo session, 
        ///this method will setup all the necessary properties for when the Preferences window is opened.
        /// </summary>
        private void SetupPreferencesViewModel(DynamoViewModel dynamoViewModel)
        {
            //Clear the Saved Changes label and its corresponding tooltip when the Preferences Modal is opened
            dynamoViewModel.PreferencesViewModel.SavedChangesLabel = string.Empty;
            dynamoViewModel.PreferencesViewModel.SavedChangesTooltip = string.Empty;
            dynamoViewModel.PreferencesViewModel.PackagePathsViewModel?.InitializeRootLocations();
            dynamoViewModel.PreferencesViewModel.TrustedPathsViewModel?.InitializeTrustedLocations();

            // Init package paths for install 
            dynamoViewModel.PreferencesViewModel.InitPackagePathsForInstall();

            // Init all package filters 
            dynamoViewModel.PreferencesViewModel.InitPackageListFilters();

            dynamoViewModel.PreferencesViewModel.TrustedPathsViewModel.PropertyChanged += TrustedPathsViewModel_PropertyChanged;
        }

        /// <summary>
        /// Evaluates if the user interacts over the Trusted Locations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrustedPathsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            List<string> actions = typeof(TrustedPathViewModel.Action).GetFields().Select(a => a.Name).ToList();

            if (actions.Contains(e.PropertyName))
            {
                dynViewModel.CheckCurrentFileInTrustedLocation();
            }
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
            Analytics.TrackEvent(Actions.Close, Categories.Preferences);
            viewModel.PackagePathsViewModel.SaveSettingCommand.Execute(null);
            viewModel.TrustedPathsViewModel?.SaveSettingCommand?.Execute(null);
            dynViewModel.ShowHideFileTrustWarningIfCurrentWorkspaceTrusted();

            viewModel.CommitPackagePathsForInstall();
            PackagePathView.Dispose();
            TrustedPathView.Dispose();
            Dispose();

            dynViewModel.PreferencesViewModel.TrustedPathsViewModel.PropertyChanged -= TrustedPathsViewModel_PropertyChanged;
            dynViewModel.CheckCustomGroupStylesChanges(originalCustomGroupStyles);

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
            viewModel.IsVisibleAddStyleBorder = true;
            viewModel.IsEnabledAddStyleButton = false;
            groupNameBox.Focus();
            Logging.Analytics.TrackEvent(Actions.New, Categories.GroupStyleOperations, nameof(GroupStyleItem));
        }

        private void ResetGroupStyleForm()
        {
            viewModel.CurrentWarningMessage = string.Empty;
            viewModel.IsWarningEnabled = false;
            viewModel.IsSaveButtonEnabled = true;
            viewModel.IsVisibleAddStyleBorder = false;
            viewModel.IsEnabledAddStyleButton = true;
        }

        private void AddStyle_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var saveChangesButton = sender as Button;

            //Get the Grid that contains the Stack Panel that also contains the info related to the new style
            var grid = (saveChangesButton.Parent as Grid).Parent as Grid;

            var groupNameLabel = grid.FindName("groupNameBox") as TextBox;
            var colorHexString = grid.FindName("colorHexVal") as Label;
            var groupStyleFontSize = grid.FindName("groupStyleFontSize") as ComboBox;
            var groupStyleId = Guid.NewGuid();

            var newItem = new StyleItem() { Name = groupNameLabel.Text, HexColorString = colorHexString.Content.ToString(), FontSize = Convert.ToInt32(groupStyleFontSize.SelectedValue), GroupStyleId = groupStyleId };

            if (string.IsNullOrEmpty(newItem.Name))
                newItem.Name = "Input";

            //if the validation returns false it means that the new style that will be added doesn't exists
            if (string.IsNullOrEmpty(groupNameLabel.Text))
            {
                viewModel.EnableGroupStyleWarningState(Res.PreferencesViewEmptyStyleWarning);
            }
            //Means that the Style name to be created already exists
            else if (viewModel.IsStyleNameValid(newItem))
            {
                viewModel.EnableGroupStyleWarningState(Res.PreferencesViewAlreadyExistingStyleWarning);
            }
            //Means that the Style will be created successfully.
            else
            {
                viewModel.AddStyle(newItem);
                viewModel.ResetAddStyleControl();
                Logging.Analytics.TrackEvent(Actions.Save, Categories.GroupStyleOperations, nameof(GroupStyleItem));
            }          
        }

        private void AddStyle_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ResetAddStyleControl();
            Logging.Analytics.TrackEvent(Actions.Cancel, Categories.GroupStyleOperations, nameof(GroupStyleItem));
        }

        private void RemoveStyle_Click(object sender, RoutedEventArgs e)
        {
           var removeButton = sender as Button;

            //Get the Grid that contains all the buttons in the StyleItem
           var grid = (removeButton.Parent as Grid).Parent as Grid;

            //Find inside the Grid the label that contains the GroupName (unique id)
           var groupNameLabel = grid.FindName("groupNameLabel") as TextBlock;

            //Remove the selected style from the list
            viewModel.RemoveStyleEntry(groupNameLabel.Text.ToString());
            Logging.Analytics.TrackEvent(Actions.Delete, Categories.GroupStyleOperations, nameof(GroupStyleItem));
        }

        private void ButtonColorPicker_Click(object sender, RoutedEventArgs e)
        {
            var colorPicker = new CustomColorPicker();
            if (colorPicker == null) return;

            colorPicker.Placement = PlacementMode.Top;
            colorPicker.PlacementTarget = sender as UIElement;
            colorPicker.IsOpen = true;
            colorPicker.Closed += ColorPicker_Closed;
            colorButtonSelected = sender as Button;
        }

        private void ColorPicker_Closed(object sender, EventArgs e)
        {
            var colorPicker = sender as CustomColorPicker;
            if(colorPicker == null) return;  
            colorPicker.Closed -= ColorPicker_Closed;

            if (colorButtonSelected != null)
            {
                var viewModel = colorPicker.DataContext as CustomColorPickerViewModel;
                if (viewModel == null || viewModel.ColorPickerFinalSelectedColor == null)
                    return;
                colorButtonSelected.Background = new SolidColorBrush(viewModel.ColorPickerFinalSelectedColor.Value);
            }
        }

        private void onChangedGroupStyleColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Button colorButton = sender as Button;
                
                if (colorButton != null)
                {
                    GroupStyleItem selectedGroupStyle = (GroupStyleItem)colorButton.DataContext;
                    selectedGroupStyle.HexColorString = colorDialog.Color.R.ToString("X2") + colorDialog.Color.G.ToString("X2") + colorDialog.Color.B.ToString("X2");
                }                
            }
        }

        private void Log(ILogMessage obj)
        {
            dynViewModel.Model.Logger.Log(obj);
        }

        /// <summary>
        /// Unified handler for more info request from mouse left button click
        /// </summary>
        /// <param name="sender">sender control</param>
        /// <param name="e"></param>
        private void OnMoreInfoClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Label lable)
            {
                if (lable.Name == "Titleinfo")
                {
                    dynViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.NodeAutocompleteDocumentationUriString, UriKind.Relative)));
                }
                else if (lable.Name == "TrustWarningInfoLabel")
                {
                    dynViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Wpf.Properties.Resources.FileTrustWarningDocumentationUriString, UriKind.Relative)));

                }
            }
        }

        private void ReloadCPython_Click(object sender, RoutedEventArgs e)
        {
            dynViewModel.Model.OnRequestPythonReset(PythonServices.PythonEngineManager.CPython3EngineName);
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

        private void groupNameBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            var groupNameTextBox = sender as TextBox;
            if (groupNameBox == null) return;
            if (string.IsNullOrEmpty(groupNameBox.Text))
            {
                viewModel.IsSaveButtonEnabled = false;
                if (e.Key == Key.Return)
                {
                    viewModel.EnableGroupStyleWarningState(Res.PreferencesViewAlreadyExistingStyleWarning);
                }
            }
            else
            {
                viewModel.IsSaveButtonEnabled = true;
                viewModel.CurrentWarningMessage = string.Empty;
                viewModel.IsWarningEnabled = false;
                if (e.Key == Key.Return)
                {
                    AddStyle_SaveButton_Click(AddStyle_SaveButton, new RoutedEventArgs());
                }
            }
        }

        private void GroupStylesListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            GroupStylesListBox.UnselectAll();
        }

        private void DisableTrustWarningsChecked(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
            {
                viewModel.DisableTrustWarnings = (bool)(sender as ToggleButton).IsChecked;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            if (e.Delta > 0)
            {
                scrollviewer.LineUp();
            }
            else
            {
                scrollviewer.LineDown();
            }
            e.Handled = true;
        }

        private void importTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            string fileExtension = "*" + Path.GetExtension(PathManager.PreferenceSettingsFileName);
            string[] fileFilter = { string.Format(Res.FileDialogImportSettingsFiles, fileExtension) };
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = String.Join("|", fileFilter);
            openFileDialog.Title = Res.ImportSettingsDialogTitle;
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;

            var result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    bool isImported = viewModel.importSettings(openFileDialog.FileName);
                    if (isImported)
                    {
                        Wpf.Utilities.MessageBoxService.Show(
                            this, Res.ImportSettingsSuccessMessage, Res.ImportSettingsDialogTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Wpf.Utilities.MessageBoxService.Show(
                            this, Res.ImportSettingsFailedMessage, Res.ImportSettingsDialogTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    Analytics.TrackEvent(Actions.Import, Categories.Preferences, isImported.ToString());
                }
                catch (Exception ex)
                {
                    Wpf.Utilities.MessageBoxService.Show(
                        this, ex.Message, Res.ImportSettingsFailedMessage, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }            
        }

        private void OnMoreInfoClicked(object sender, MouseButtonEventArgs e)
        {
            dynViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Dynamo.Wpf.Properties.Resources.NodeAutocompleteDocumentationUriString, UriKind.Relative)));
        }

        private void exportTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new DynamoFolderBrowserDialog
            {
                Title = Res.ExportSettingsDialogTitle,
                Owner = this
            };

            //Saves the current settings before exporting the xml file
            dynViewModel.PreferenceSettings.SaveInternal(dynViewModel.Model.PathManager.PreferenceFilePath);

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPathFile = Path.Combine(dialog.SelectedPath, PathManager.PreferenceSettingsFileName);
                try
                {
                    if (File.Exists(selectedPathFile))
                    {
                        string uniqueId = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                        string suffixPlusDot = $"_{ uniqueId}.";
                        string uniqueFileName = PathManager.PreferenceSettingsFileName.Replace(".", suffixPlusDot);
                        selectedPathFile = Path.Combine(dialog.SelectedPath, uniqueFileName);
                    }

                    File.Copy(dynViewModel.Model.PathManager.PreferenceFilePath, selectedPathFile);
                    string argument = "/select, \"" + selectedPathFile + "\"";
                    System.Diagnostics.Process.Start("explorer.exe", argument);
                    Analytics.TrackEvent(Actions.Export, Categories.Preferences);
                }
                catch (Exception ex)
                {
                    Wpf.Utilities.MessageBoxService.Show(
                        this,
                        ex.Message,
                        Res.ExportSettingsFailedMessage,
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        // Show File path dialog
        private void OnRequestShowFileDialog(object sender, EventArgs e)
        {
            var args = e as PythonTemplatePathEventArgs;
            args.Cancel = true;

            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                // Navigate to initial folder.
                FileName = args.Path
            };

            //Filter python files.
            dialog.Filter = "Python File|*.py";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                args.Cancel = false;
                args.Path = dialog.FileName;
            }
        }

        // Number input textbox validation
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        internal void Dispose()
        {
            viewModel.RequestShowFileDialog -= OnRequestShowFileDialog;
        }

        int getExtraLeftSpace(int confidenceLevel)
        {
            int value = 16;

            for (int i = 1; i <= 9; i++)
            {
                if (confidenceLevel <= 9)
                {
                    break;
                }               
                else
                {
                    value--;
                    if ((confidenceLevel == 10) || confidenceLevel >= (i * 10) + 1 && confidenceLevel <= (i + 1) * 10)
                    {                        
                        break;
                    }
                }
            }
            return value;
        }       

        private void sliderConfidenceLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            displayConfidenceLevel();
        }

        private void displayConfidenceLevel()
        {
            if (this.lblConfidenceLevel != null && this.lblConfidenceLevelLabelStart != null)
            {
                int confidenceLevel = (int)lblConfidenceLevel.Content;

                int left = ((int)lblConfidenceLevel.Content * 3) + getExtraLeftSpace(confidenceLevel);
                this.lblConfidenceLevel.Margin = new Thickness(left, -15, 0, 0);
            }
        }

        private void zoomScaleLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;

            updateLibraryZoomScaleValueLabel(slider);            
        }

        private void updateLibraryZoomScaleValueLabel(Slider slider)
        {
            //Since the percentage goes from 25 to 300, the value is decremented by 25 to standardize. 
            double percentage = slider.Value - 25;

            //The margin value for the label goes from - 480 to 310, resulting in 790 pixels from the starting point to the end.
            //We also standardized the values ​​of the percentage(from 0 to 275).
            //The value is decreased to 480 because the margin begins at - 480
            //This is the relation between the margin in pixels and the value of the percentage
            double marginValue = (790 * percentage / 275) - 480;
            if (lblZoomScalingValue != null)
            {
                lblZoomScalingValue.Margin = new Thickness(marginValue, 0, 0, 0);
                lblZoomScalingValue.Content = slider.Value.ToString() + "%";
            }
        }
    }
}
