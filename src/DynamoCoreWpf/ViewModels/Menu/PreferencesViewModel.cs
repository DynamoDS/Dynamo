using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PythonServices;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.Wpf.ViewModels.Core.Converters;
using DynamoUtilities;
using ViewModels.Core;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// The next enum will contain the possible values for Scaling (Visual Settings -> Geometry Scaling section)
    /// </summary>
    public enum GeometryScaleSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge
    }

    /// <summary>
    /// Preferences data context
    /// </summary>
    public class PreferencesViewModel : ViewModelBase, INotifyPropertyChanged
    {
        #region Private Properties
        private string savedChangesLabel;
        private string savedChangesTooltip;
        private string currentWarningMessage;
        private string selectedPackagePathForInstall;

        private string selectedLanguage;
        private string selectedNumberFormat;
        private string selectedPythonEngine;

        private ObservableCollection<string> languagesList;
        private ObservableCollection<string> packagePathsForInstall;
        private ObservableCollection<string> fontSizeList;
        private ObservableCollection<int> groupStyleFontSizeList;
        private ObservableCollection<string> numberFormatList;
        private StyleItem addStyleControl;
        private ObservableCollection<string> pythonEngineList;

        private RunType runSettingsIsChecked;
        private NodeAutocompleteSuggestion nodeAutocompleteSuggestion;
        private Dictionary<string, TabSettings> preferencesTabs;

        private readonly PreferenceSettings preferenceSettings;
        private readonly DynamoPythonScriptEditorTextOptions pythonScriptEditorTextOptions;
        private readonly DynamoViewModel dynamoViewModel;
        private readonly InstalledPackagesViewModel installedPackagesViewModel;

        private bool isWarningEnabled;
        private bool isSaveButtonEnabled = true;
        private bool isVisibleAddStyleBorder;
        private bool isEnabledAddStyleButton;
        private GeometryScalingOptions optionsGeometryScale = null;
        private GeometryScaleSize defaultGeometryScaling = GeometryScaleSize.Medium;
        #endregion Private Properties

        public GeometryScaleSize DefaultGeometryScaling
        {
            get
            {
                return defaultGeometryScaling;
            }
            set
            {
                if(defaultGeometryScaling != value)
                {
                    defaultGeometryScaling = value;
                    SelectedDefaultScaleFactor = GeometryScalingOptions.ConvertUIToScaleFactor((int)defaultGeometryScaling);
                    RaisePropertyChanged(nameof(DefaultGeometryScaling));
                }              
            }
        }

        /// <summary>
        /// This property will be used by the Preferences screen to store and retrieve all the settings from the expanders
        /// </summary>
        public Dictionary<string, TabSettings> PreferencesTabs
        {
            get
            {
                return preferencesTabs;
            }
            set
            {
                preferencesTabs = value;
                RaisePropertyChanged(nameof(PreferencesTabs));
            }
        }

        /// <summary>
        /// Controls what the SavedChanges label will display
        /// </summary>
        public string SavedChangesLabel
        {
            get
            {
                return savedChangesLabel;
            }
            set
            {
                savedChangesLabel = value;
                RaisePropertyChanged(nameof(SavedChangesLabel));
            }
        }

        /// <summary>
        /// Controls what SavedChanges label's tooltip will display
        /// </summary>
        public string SavedChangesTooltip
        {
            get
            {
                return savedChangesTooltip;
            }
            set
            {
                savedChangesTooltip = value;
                RaisePropertyChanged(nameof(SavedChangesTooltip));

            }
        }

        /// <summary>
        /// Returns all installed packages
        /// </summary>
        public ObservableCollection<PackageViewModel> LocalPackages => installedPackagesViewModel.LocalPackages;

        /// <summary>
        /// Returns all available filters
        /// </summary>
        public ObservableCollection<PackageFilter> Filters => installedPackagesViewModel.Filters;

        //This includes all the properties that can be set on the General tab
        #region General Properties
        /// <summary>
        /// Controls the Selected option in Language ComboBox
        /// </summary>
        public string SelectedLanguage
        {
            get
            {
                return selectedLanguage;
            }
            set
            {
                if (selectedLanguage != value)
                {
                    selectedLanguage = value;
                    RaisePropertyChanged(nameof(SelectedLanguage));
                    if (Configurations.SupportedLocaleDic.TryGetValue(selectedLanguage, out string locale))
                    {
                        preferenceSettings.Locale = locale;
                        dynamoViewModel.MainGuideManager?.CreateRealTimeInfoWindow(Res.PreferencesViewLanguageSwitchHelp, true);
                    }
                }
            }
        }

        /// <summary>
        /// Controls the Selected option in Number Format ComboBox
        /// </summary>
        public string SelectedNumberFormat
        {
            get
            {
                return preferenceSettings.NumberFormat;
            }
            set
            {
                selectedNumberFormat = value;
                preferenceSettings.NumberFormat = value;
                RaisePropertyChanged(nameof(SelectedNumberFormat));
            }
        }

        /// <summary>
        /// This property holds the Geometry Scale factor selected in the Preferences panel (when a new workspace is created this will be the Geometry Scale used)
        /// </summary>
        public double SelectedDefaultScaleFactor
        {
            get
            {
                return preferenceSettings.DefaultScaleFactor;
            }
            set
            {
                preferenceSettings.DefaultScaleFactor = value;
                RaisePropertyChanged(nameof(SelectedDefaultScaleFactor));
            }
        }

        /// <summary>
        /// Time Interval for backup files in minutes
        /// Serialized as milliseconds in preferences setting.
        /// </summary>
        public int BackupIntervalInMinutes
        {
            get
            {
                return preferenceSettings.BackupInterval/60000;
            }
            set
            {
                preferenceSettings.BackupInterval = value * 60000;
                RaisePropertyChanged(nameof(BackupIntervalInMinutes));
            }
        }

        /// <summary>
        /// Maximum number of recent files on startup page.
        /// </summary>
        public int MaxNumRecentFiles
        {
            get
            {
                return preferenceSettings.MaxNumRecentFiles;
            }
            set
            {
                preferenceSettings.MaxNumRecentFiles = value;
                RaisePropertyChanged(nameof(MaxNumRecentFiles));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the RunSettings radio button
        /// </summary>
        public bool RunSettingsIsChecked
        {
            get
            {
                return runSettingsIsChecked == RunType.Manual;
            }
            set
            {
                if (value)
                {
                    preferenceSettings.DefaultRunType = RunType.Manual;
                    runSettingsIsChecked = RunType.Manual;
                }
                else
                {
                    preferenceSettings.DefaultRunType = RunType.Automatic;
                    runSettingsIsChecked = RunType.Automatic;
                }
                RaisePropertyChanged(nameof(RunSettingsIsChecked));
            }
        }        

        /// <summary>
        /// Controls the IsChecked property in the Show Run Preview toogle button
        /// </summary>
        public bool RunPreviewIsChecked
        {
            get
            {
                return preferenceSettings.ShowRunPreview;
            }
            set
            {
                preferenceSettings.ShowRunPreview = value;
                dynamoViewModel.ShowRunPreview = value;
                RaisePropertyChanged(nameof(RunPreviewIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the Show Static Splash Screen toogle button
        /// </summary>
        public bool StaticSplashScreenEnabled
        {
            get
            {
                return preferenceSettings.EnableStaticSplashScreen;
            }
            set
            {
                preferenceSettings.EnableStaticSplashScreen = value;
                RaisePropertyChanged(nameof(StaticSplashScreenEnabled));
            }
        }

        /// <summary>
        /// Controls the Enabled property in the Show Run Preview toogle button
        /// </summary>
        public bool RunPreviewEnabled
        {
            get
            {
                return dynamoViewModel.HomeSpaceViewModel.RunSettingsViewModel.RunButtonEnabled;
            }
        }

        /// <summary>
        /// LanguagesList property contains the list of all the languages listed in: https://wiki.autodesk.com/display/LOCGD/Dynamo+Languages
        /// </summary>
        public ObservableCollection<string> LanguagesList
        {
            get
            {
                return languagesList;
            }
            set
            {
                languagesList = value;
                RaisePropertyChanged(nameof(LanguagesList));
            }
        }

        /// <summary>
        /// PackagePathsForInstall contains the list of all package paths where
        /// packages can be installed.
        /// </summary>
        public ObservableCollection<string> PackagePathsForInstall
        {
            get
            {
                var allowedFileExtensions = new string[] { ".dll", ".ds" };
                if (packagePathsForInstall == null || !packagePathsForInstall.Any())
                {
                    var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    // Filter Builtin Packages and ProgramData paths from list of paths for download
                    var customPaths = preferenceSettings.CustomPackageFolders.Where(
                        x => x != DynamoModel.BuiltInPackagesToken && !x.StartsWith(programDataPath));
                    //filter out paths that have extensions ending in .dll or .ds
                    var directoryPaths = customPaths.Where(path => !(Path.HasExtension(path) && allowedFileExtensions.Contains(Path.GetExtension(path).ToLower())));

                    packagePathsForInstall = new ObservableCollection<string>();
                    foreach (var path in directoryPaths)
                    {
                            packagePathsForInstall.Add(path);
                    }
                }
                return packagePathsForInstall;
            }
            set
            {
                packagePathsForInstall = value;
                RaisePropertyChanged(nameof(PackagePathsForInstall));
            }
        }

        /// <summary>
        /// Currently selected package path where new packages will be downloaded.
        /// </summary>
        public string SelectedPackagePathForInstall
        {
            get
            {
                return selectedPackagePathForInstall;
            }
            set
            {
                if (selectedPackagePathForInstall != value)
                {
                    selectedPackagePathForInstall = value;
                    RaisePropertyChanged(nameof(SelectedPackagePathForInstall));
                }
            }
        }

        /// <summary>
        /// Flag specifying whether loading built-in packages
        /// is disabled, if true, or enabled, if false.
        /// </summary>
        public bool DisableBuiltInPackages 
        { 
            get 
            {
                return preferenceSettings.DisableBuiltinPackages;
            } 
            set 
            {
                preferenceSettings.DisableBuiltinPackages = value;
                PackagePathsViewModel.SetPackagesScheduledState(PathManager.BuiltinPackagesDirectory, value);
                RaisePropertyChanged(nameof(DisableBuiltInPackages));
            }
        }

        /// <summary>
        /// Flag specifying whether loading custom packages
        /// is disabled, if true, or enabled, if false.
        /// </summary>
        public bool DisableCustomPackages 
        { 
            get
            {
                return preferenceSettings.DisableCustomPackageLocations;
            }
            set
            {
                preferenceSettings.DisableCustomPackageLocations = value;
                foreach(var path in preferenceSettings.CustomPackageFolders.Where(x => x != DynamoModel.BuiltInPackagesToken))
                {
                    PackagePathsViewModel.SetPackagesScheduledState(path, value);
                }
                RaisePropertyChanged(nameof(DisableCustomPackages));
            } 
        }

        /// <summary>
        /// Flag specifying whether trust warnings should be shown
        /// when opening .dyn files from unstrusted locations.
        /// </summary>
        public bool DisableTrustWarnings
        {
            get
            {
                return preferenceSettings.DisableTrustWarnings;
            }
            // We keep this setter private to avoid view extensions calling it directly.
            // Access modifiers are not intended for security, but it's simple enough to hook a toggle to the UI
            // without binding, and this makes it clear it's not an API.
            internal set
            {
                preferenceSettings.SetTrustWarningsDisabled(value);
            }
        }

        /// <summary>
        /// GroupStyleFontSizeList contains the list of sizes for defined fonts to be applied to a GroupStyle
        /// </summary>
        public ObservableCollection<int> GroupStyleFontSizeList
        {
            get
            {
                return groupStyleFontSizeList;
            }
            set
            {
                groupStyleFontSizeList = value;
                RaisePropertyChanged(nameof(GroupStyleFontSizeList));
            }
        }

        /// <summary>
        /// NumberFormatList contains the list of the format for numbers, right now in Dynamo has the next formats: 0, 0.0, 0.00, 0.000, 0.0000
        /// </summary>
        public ObservableCollection<string> NumberFormatList
        {
            get
            {
                return numberFormatList;
            }
            set
            {
                numberFormatList = value;
                RaisePropertyChanged(nameof(NumberFormatList));
            }
        }
        #endregion

        //This includes all the properties that can be set on the Visual Settings tab
        #region VisualSettings Properties
        /// <summary>
        /// This will contain a list of all the Styles created by the user in the Styles list ( Visual Settings -> Group Styles section)
        /// </summary>
        public ObservableCollection<GroupStyleItem> StyleItemsList
        {
            get { return preferenceSettings.GroupStyleItemsList.ToObservableCollection(); }
            set
            {
                preferenceSettings.GroupStyleItemsList = value.ToList<GroupStyleItem>();
                RaisePropertyChanged(nameof(StyleItemsList));
            }
        }

        /// <summary>
        /// Used to add styles to the StyleItemsListe while also update the saved changes label
        /// </summary>
        /// <param name="style">style to be added</param>
        public void AddStyle(StyleItem style)
        {
            preferenceSettings.GroupStyleItemsList.Add(new GroupStyleItem {
                HexColorString = style.HexColorString,
                Name = style.Name,
                FontSize = style.FontSize,
                GroupStyleId = style.GroupStyleId,
                IsDefault = style.IsDefault
            });
            RaisePropertyChanged(nameof(StyleItemsList));
        }
     
        /// <summary>
        /// This flag will be in true when the Style that user is trying to add already exists (otherwise will be false - Default)
        /// </summary>
        public bool IsWarningEnabled
        {
            get
            {
                return isWarningEnabled;
            }
            set
            {
                isWarningEnabled = value;
                RaisePropertyChanged(nameof(IsWarningEnabled));
            }
        }

        /// <summary>
        /// This property will hold the warning message that has to be shown in the warning icon next to the TextBox
        /// </summary>
        public string CurrentWarningMessage
        {
            get
            {
                return currentWarningMessage;
            }
            set
            {
                currentWarningMessage = value;
                RaisePropertyChanged(nameof(CurrentWarningMessage));
            }
        }

        /// <summary>
        /// This property describes if the SaveButton will be enabled or not (when trying to save a new Style).
        /// </summary>
        public bool IsSaveButtonEnabled
        {
            get
            {
                return isSaveButtonEnabled;
            }
            set
            {
                isSaveButtonEnabled = value;
                RaisePropertyChanged(nameof(IsSaveButtonEnabled));
            }
        }

        /// <summary>
        /// This property was created just a container for default information when the user is adding a new Style
        /// When users press the Add Style button some controls are shown so the user can populate them, this property will contain default values shown
        /// </summary>
        public StyleItem AddStyleControl
        {
            get
            {
                return addStyleControl;
            }
            set
            {
                addStyleControl = value;
                RaisePropertyChanged(nameof(AddStyleControl));
            }
        }

        /// <summary>
        /// This property is used as a container for the description text (GeometryScalingOptions.DescriptionScaleRange) for each radio button (Visual Settings -> Geometry Scaling section)
        /// </summary>
        public GeometryScalingOptions OptionsGeometryScale
        {
            get
            {
                return optionsGeometryScale;
            }
            set
            {
                optionsGeometryScale = value;
                RaisePropertyChanged(nameof(OptionsGeometryScale));
            }
        }

        /// <summary>
        /// Controls the binding for the ShowEdges toogle in the Preferences->Visual Settings->Display Settings section
        /// </summary>
        public bool ShowEdges
        {
            get
            {
                return dynamoViewModel.RenderPackageFactoryViewModel.ShowEdges;
            }
            set
            {
                dynamoViewModel.RenderPackageFactoryViewModel.ShowEdges = value;
                RaisePropertyChanged(nameof(ShowEdges));
            }
        }

        /// <summary>
        /// Control to use hardware acceleration
        /// </summary>
        public bool UseHardwareAcceleration
        {
            get
            {
                return dynamoViewModel.Model.PreferenceSettings.UseHardwareAcceleration;
            }
            set
            {
                dynamoViewModel.Model.PreferenceSettings.UseHardwareAcceleration = value;
                RaisePropertyChanged(nameof(UseHardwareAcceleration));
            }
        }

        /// <summary>
        /// Controls the binding for the IsolateSelectedGeometry toogle in the Preferences->Visual Settings->Display Settings section
        /// </summary>
        public bool IsolateSelectedGeometry
        {
            get
            {
                return dynamoViewModel.BackgroundPreviewViewModel.IsolationMode;
            }
            set
            {
                dynamoViewModel.BackgroundPreviewViewModel.IsolationMode = value;
                RaisePropertyChanged(nameof(IsolateSelectedGeometry));
            }
        }

        /// <summary>
        /// This property is bind to the Render Precision Slider and control the amount of tessellation applied to objects in background preview
        /// </summary>
        public int TessellationDivisions
        {
            get
            {
                return dynamoViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions;
            }
            set
            {
                dynamoViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions = value;
                RaisePropertyChanged(nameof(TessellationDivisions));
            }
        }

        /// <summary>
        /// Indicates if preview bubbles should be displayed on nodes.
        /// </summary>
        public bool ShowPreviewBubbles
        {
            get
            {
                return preferenceSettings.ShowPreviewBubbles;
            }
            set
            {
                preferenceSettings.ShowPreviewBubbles = value;
                RaisePropertyChanged(nameof(ShowPreviewBubbles));
            }
        }

        /// <summary>
        /// Indicates if line numbers should be displayed on code block nodes.
        /// </summary>
        public bool ShowCodeBlockLineNumber
        {
            get
            {
                return preferenceSettings.ShowCodeBlockLineNumber;
            }
            set
            {
                preferenceSettings.ShowCodeBlockLineNumber = value;
                RaisePropertyChanged(nameof(ShowCodeBlockLineNumber));
            }
        }

        /// <summary>
        /// This property will make Visible or Collapse the AddStyle Border defined in the GroupStyles section
        /// </summary>
        public bool IsVisibleAddStyleBorder 
        {
            get
            {
                return isVisibleAddStyleBorder;
            } 
            set
            {
                isVisibleAddStyleBorder = value;
                RaisePropertyChanged(nameof(IsVisibleAddStyleBorder));
            }
        }

        /// <summary>
        /// This property will Enable or Disable the AddStyle button defined in the GroupStyles section
        /// </summary>
        public bool IsEnabledAddStyleButton 
        {
            get
            {
                return isEnabledAddStyleButton;
            }
            set
            {
                isEnabledAddStyleButton = value;
                RaisePropertyChanged(nameof(IsEnabledAddStyleButton));
            }
        }
        #endregion

        //This includes all the properties that can be set on the Features tab
        #region Features Properties
        /// <summary>
        /// Python Template File Path
        /// </summary>
        public string PythonTemplateFilePath
        {
            get
            {
                return preferenceSettings.PythonTemplateFilePath;
            }
            set
            {
                preferenceSettings.PythonTemplateFilePath = value;
                RaisePropertyChanged(nameof(PythonTemplateFilePath));
            }
        }

        /// <summary>
        /// PythonEnginesList contains the list of Python engines available
        /// </summary>
        public ObservableCollection<string> PythonEnginesList
        {
            get
            {
                return pythonEngineList;
            }
            set
            {
                pythonEngineList = value;
                RaisePropertyChanged(nameof(PythonEnginesList));
            }
        }

        /// <summary>
        /// Controls the Selected option in Python Engine combobox
        /// </summary>
        public string SelectedPythonEngine
        {
            get
            {
                return selectedPythonEngine;
            }
            set
            {
                if (value != selectedPythonEngine)
                {
                    selectedPythonEngine = value;
                    if(value != Res.DefaultPythonEngineNone)
                    {
                        preferenceSettings.DefaultPythonEngine = value;
                    }
                    else{
                        preferenceSettings.DefaultPythonEngine = string.Empty;
                    }

                    RaisePropertyChanged(nameof(SelectedPythonEngine));
                }
            }
        }
        
        /// <summary>
        /// Controls the IsChecked property in the "Hide IronPython alerts" toggle button
        /// </summary>
        public bool HideIronPythonAlertsIsChecked
        {
            get
            {
                return preferenceSettings.IsIronPythonDialogDisabled;
            }
            set
            {
                preferenceSettings.IsIronPythonDialogDisabled = value;
                RaisePropertyChanged(nameof(HideIronPythonAlertsIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Show Whitespace in Python editor" toggle button
        /// </summary>
        public bool ShowWhitespaceIsChecked
        {
            get
            {
                return preferenceSettings.ShowTabsAndSpacesInScriptEditor;
            }
            set
            {
                pythonScriptEditorTextOptions.ShowWhiteSpaceCharacters(value);
                preferenceSettings.ShowTabsAndSpacesInScriptEditor = value;
                RaisePropertyChanged(nameof(ShowWhitespaceIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Notification Center" toggle button
        /// </summary>
        public bool NotificationCenterIsChecked
        {
            get
            {
                return preferenceSettings.EnableNotificationCenter;
            }
            set
            {
                preferenceSettings.EnableNotificationCenter = value;
                RaisePropertyChanged(nameof(NotificationCenterIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Extensions" toggle button, to enable persisted extensions, that will remember
        /// extensions setting as per the last session.
        /// </summary>
        public bool PersistExtensionsIsChecked
        {
            get
            {
                return preferenceSettings.EnablePersistExtensions;
            }
            set
            {
                preferenceSettings.EnablePersistExtensions = value;
                RaisePropertyChanged(nameof(PersistExtensionsIsChecked));
            }
        }

        #region [ Node Autocomplete ]

        /// <summary>
        /// Controls the IsChecked property in the "Node autocomplete" toogle button
        /// </summary>
        public bool NodeAutocompleteIsChecked
        {
            get
            {
                return preferenceSettings.EnableNodeAutoComplete;
            }
            set
            {
                preferenceSettings.EnableNodeAutoComplete = value;
                RaisePropertyChanged(nameof(NodeAutocompleteIsChecked));
                RaisePropertyChanged(nameof(EnableHideNodesToggle));
                RaisePropertyChanged(nameof(EnableConfidenceLevelSlider));
            }
        }

        /// <summary>
        /// Controls if the the Node autocomplete Machine Learning option is checked for the radio buttons
        /// </summary>
        public bool NodeAutocompleteMachineLearningIsChecked
        {
            get
            {
                return preferenceSettings.DefaultNodeAutocompleteSuggestion == NodeAutocompleteSuggestion.MLRecommendation;
            }
            set
            {
                if (value)
                {
                    preferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.MLRecommendation;
                    nodeAutocompleteSuggestion = NodeAutocompleteSuggestion.MLRecommendation;
                }
                else
                {
                    preferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;
                    nodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;
                }

                dynamoViewModel.HomeSpaceViewModel.NodeAutoCompleteSearchViewModel.ResetAutoCompleteSearchViewState();
                RaisePropertyChanged(nameof(nodeAutocompleteSuggestion));
                RaisePropertyChanged(nameof(NodeAutocompleteMachineLearningIsChecked));
                RaisePropertyChanged(nameof(EnableHideNodesToggle));
                RaisePropertyChanged(nameof(EnableConfidenceLevelSlider));
            }
        }

        /// <summary>
        /// Controls if the the Node autocomplete Machine Learning option is beta from feature flag
        /// </summary>
        public bool NodeAutocompleteMachineLearningIsBeta
        {
            get
            {
                return DynamoModel.FeatureFlags.CheckFeatureFlag("NodeAutocompleteMachineLearningIsBeta", false);
            }
        }

        /// <summary>
        /// Contains the numbers of result of the ML recommendation
        /// </summary>
        public int MLRecommendationNumberOfResults
        {
            get
            {
                return preferenceSettings.MLRecommendationNumberOfResults;
            }
            set
            {
                preferenceSettings.MLRecommendationNumberOfResults = value;
                RaisePropertyChanged(nameof(MLRecommendationNumberOfResults));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Hide nodes below a specific confidence level" toogle button
        /// </summary>
        public bool HideNodesBelowSpecificConfidenceLevelIsChecked
        {
            get
            {
                return preferenceSettings.HideNodesBelowSpecificConfidenceLevel;
            }
            set
            {
                preferenceSettings.HideNodesBelowSpecificConfidenceLevel = value;
                RaisePropertyChanged(nameof(HideNodesBelowSpecificConfidenceLevelIsChecked));
                RaisePropertyChanged(nameof(EnableConfidenceLevelSlider));
            }
        }        

        /// <summary>
        /// Contains the confidence level of a ML recommendation
        /// </summary>
        public int MLRecommendationConfidenceLevel
        {
            get
            {
                return preferenceSettings.MLRecommendationConfidenceLevel;
            }
            set
            {
                preferenceSettings.MLRecommendationConfidenceLevel = value;
                RaisePropertyChanged(nameof(MLRecommendationConfidenceLevel));
            }
        }

        /// <summary>
        /// If the user can click on the Hide Nodes toggle
        /// </summary>
        public bool EnableHideNodesToggle
        {
            get
            {
                return NodeAutocompleteIsChecked && NodeAutocompleteMachineLearningIsChecked;
            }
        }

        /// <summary>
        /// If the user can click on the confidence level Slider
        /// </summary>
        public bool EnableConfidenceLevelSlider
        {
            get
            {
                return NodeAutocompleteIsChecked && NodeAutocompleteMachineLearningIsChecked && HideNodesBelowSpecificConfidenceLevelIsChecked;
            }
        }

        #endregion        

        /// <summary>
        /// Controls the IsChecked property in the "Enable T-spline nodes" toogle button
        /// </summary>
        public bool EnableTSplineIsChecked
        {
            get
            {
                return !preferenceSettings.NamespacesToExcludeFromLibrary.Contains(
                    "ProtoGeometry.dll:Autodesk.DesignScript.Geometry.TSpline");
            }
            set
            {
                HideUnhideNamespace(!value, "ProtoGeometry.dll", "Autodesk.DesignScript.Geometry.TSpline");
                RaisePropertyChanged(nameof(EnableTSplineIsChecked));
            }
        }

        /// <summary>
        /// This method updates the node search library to either hide or unhide nodes that belong
        /// to a specified assembly name and namespace. These nodes will be hidden from the node
        /// library sidebar and from the node search.
        /// </summary>
        /// <param name="hide">Set to true to hide, set to false to unhide.</param>
        /// <param name="library">The assembly name of the library.</param>
        /// <param name="namespc">The namespace of the nodes to be hidden.</param>
        internal void HideUnhideNamespace(bool hide, string library, string namespc)
        {
            var str = library + ':' + namespc;
            var namespaces = preferenceSettings.NamespacesToExcludeFromLibrary;

            if (hide)
            {
                if (!namespaces.Contains(str))
                {
                    namespaces.Add(str);
                }
            }
            else // unhide
            {
                namespaces.Remove(str);
            }
        }

        private void AddPythonEnginesOptions()
        {
            var options = new ObservableCollection<string> { Res.DefaultPythonEngineNone };
            foreach (var item in PythonEngineManager.Instance.AvailableEngines)
            {
                options.Add(item.Name);
            }
            PythonEnginesList = options;
        }
        #endregion

        /// <summary>
        /// Package Search Paths view model.
        /// </summary>
        public PackagePathViewModel PackagePathsViewModel { get; set; }

        /// <summary>
        /// Trusted Paths view model.
        /// </summary>
        public TrustedPathViewModel TrustedPathsViewModel { get; set; }

        /// <summary>
        /// Returns a boolean value indicating if the Settings importing was successful or not
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool importSettings(string filePath)
        {
            var newPreferences = PreferenceSettings.Load(filePath);
            if (!newPreferences.IsCreatedFromValidFile)
            {
                return false;
            }
            newPreferences.CopyProperties(preferenceSettings);

            return setSettings(newPreferences);
        }

        /// <summary>
        /// Returns a boolean value indicating if the Settings importing was successful or not by sending the content of the xml file
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool importSettingsContent(string content)
        {
            var newPreferences = PreferenceSettings.LoadContent(content);
            if (!newPreferences.IsCreatedFromValidFile)
            {
                return false;
            }
            newPreferences.CopyProperties(preferenceSettings);

            return setSettings(newPreferences);
        }

        private bool setSettings(PreferenceSettings newPreferences)
        {
            // Explicit copy
            preferenceSettings.SetTrustWarningsDisabled(newPreferences.DisableTrustWarnings);
            preferenceSettings.SetTrustedLocations(newPreferences.TrustedLocations);
            TrustedPathsViewModel?.InitializeTrustedLocations();

            // Set the not explicit Binding
            runSettingsIsChecked = preferenceSettings.DefaultRunType;
            var engine = PythonEnginesList.FirstOrDefault(x => x.Equals(preferenceSettings.DefaultPythonEngine));
            SelectedPythonEngine = string.IsNullOrEmpty(engine) ? Res.DefaultPythonEngineNone : preferenceSettings.DefaultPythonEngine;
            dynamoViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions = preferenceSettings.RenderPrecision;
            dynamoViewModel.RenderPackageFactoryViewModel.ShowEdges = preferenceSettings.ShowEdges;
            PackagePathsForInstall = null;
            PackagePathsViewModel?.InitializeRootLocations();
            SelectedPackagePathForInstall = preferenceSettings.SelectedPackagePathForInstall;

            dynamoViewModel.IsShowingConnectors = preferenceSettings.ShowConnector;
            dynamoViewModel.IsShowingConnectorTooltip = preferenceSettings.ShowConnectorToolTip;
            foreach (var item in dynamoViewModel.Watch3DViewModels)
            {
                var preferenceItem = preferenceSettings.BackgroundPreviews.Where(i => i.Name == item.PreferenceWatchName).FirstOrDefault();
                if (preferenceItem != null)
                {
                    item.Active = preferenceItem.IsActive;
                }
            }

            preferenceSettings.SanitizeValues();
            RaisePropertyChanged(string.Empty);
            return true;
        }

        internal void InitializeGeometryScaling()
        {
            int UIScaleFactor = GeometryScalingOptions.ConvertScaleFactorToUI((int)SelectedDefaultScaleFactor);

            if (Enum.IsDefined(typeof(GeometryScaleSize), UIScaleFactor))
            {
                DefaultGeometryScaling = (GeometryScaleSize)UIScaleFactor;
            }
        }

        /// <summary>
        /// The PreferencesViewModel constructor basically initialize all the ItemsSource for the corresponding ComboBox in the View (PreferencesView.xaml)
        /// </summary>
        public PreferencesViewModel(DynamoViewModel dynamoViewModel)
        {
            this.preferenceSettings = dynamoViewModel.PreferenceSettings;
            this.pythonScriptEditorTextOptions = dynamoViewModel.PythonScriptEditorTextOptions;
            this.dynamoViewModel = dynamoViewModel;

            if (dynamoViewModel.PackageManagerClientViewModel != null)
            {
                installedPackagesViewModel = new InstalledPackagesViewModel(dynamoViewModel, dynamoViewModel.PackageManagerClientViewModel.PackageManagerExtension.PackageLoader);
            }

            // Scan for engines
            AddPythonEnginesOptions();

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged += PythonEnginesChanged;

            //Sets SelectedPythonEngine.
            //If the setting is empty it corresponds to the default python engine
            var engine = PythonEnginesList.FirstOrDefault(x => x.Equals(preferenceSettings.DefaultPythonEngine));
            SelectedPythonEngine  = string.IsNullOrEmpty(engine) ? Res.DefaultPythonEngineNone : preferenceSettings.DefaultPythonEngine;

            // Fill language list using supported locale dictionary keys in current thread locale
            LanguagesList = Configurations.SupportedLocaleDic.Keys.ToObservableCollection();
            SelectedLanguage = Configurations.SupportedLocaleDic.FirstOrDefault(x => x.Value == preferenceSettings.Locale).Key;

            GroupStyleFontSizeList = preferenceSettings.PredefinedGroupStyleFontSizes;

            // Number format settings
            NumberFormatList = new ObservableCollection<string>
            {
                Res.DynamoViewSettingMenuNumber0,
                Res.DynamoViewSettingMenuNumber00,
                Res.DynamoViewSettingMenuNumber000,
                Res.DynamoViewSettingMenuNumber0000,
                Res.DynamoViewSettingMenuNumber00000
            };
            SelectedNumberFormat = preferenceSettings.NumberFormat;

            runSettingsIsChecked = preferenceSettings.DefaultRunType;
            RunPreviewIsChecked = preferenceSettings.ShowRunPreview;

            //By Default the warning state of the Visual Settings tab (Group Styles section) will be disabled
            isWarningEnabled = false;

            // Initialize group styles with default and non-default GroupStyleItems
            StyleItemsList = GroupStyleItem.DefaultGroupStyleItems.AddRange(preferenceSettings.GroupStyleItemsList.Where(style => style.IsDefault != true)).ToObservableCollection();

            //When pressing the "Add Style" button some controls will be shown with some values by default so later they can be populated by the user
            AddStyleControl = new StyleItem() { Name = string.Empty, HexColorString = GetRandomHexStringColor() };

            //This piece of code will populate all the description text for the RadioButtons in the Geometry Scaling section.
            optionsGeometryScale = new GeometryScalingOptions();

            optionsGeometryScale.DescriptionScaleRange = new ObservableCollection<string>();
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, GeometryScalingViewModel.scaleRanges[GeometryScaleSize.Small].Item2,
                                                                                              GeometryScalingViewModel.scaleRanges[GeometryScaleSize.Small].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, GeometryScalingViewModel.scaleRanges[GeometryScaleSize.Medium].Item2,
                                                                                              GeometryScalingViewModel.scaleRanges[GeometryScaleSize.Medium].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, GeometryScalingViewModel.scaleRanges[GeometryScaleSize.Large].Item2,
                                                                                              GeometryScalingViewModel.scaleRanges[GeometryScaleSize.Large].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, GeometryScalingViewModel.scaleRanges[GeometryScaleSize.ExtraLarge].Item2,
                                                                                              GeometryScalingViewModel.scaleRanges[GeometryScaleSize.ExtraLarge].Item3));

            SavedChangesLabel = string.Empty;
            SavedChangesTooltip = string.Empty;

            // Add tabs
            preferencesTabs = new Dictionary<string, TabSettings>();
            preferencesTabs.Add("General", new TabSettings() { Name = "General", ExpanderActive = string.Empty });
            preferencesTabs.Add("Features",new TabSettings() { Name = "Features", ExpanderActive = string.Empty });
            preferencesTabs.Add("VisualSettings",new TabSettings() { Name = "VisualSettings", ExpanderActive = string.Empty });
            preferencesTabs.Add("Package Manager", new TabSettings() { Name = "Package Manager", ExpanderActive = string.Empty });

            //create a packagePathsViewModel we'll use to interact with the package search paths list.
            var loadPackagesParams = new LoadPackageParams
            {
                Preferences = preferenceSettings
            };
            var customNodeManager = dynamoViewModel.Model.CustomNodeManager;
            var packageLoader = dynamoViewModel.Model.GetPackageManagerExtension()?.PackageLoader;            
            PackagePathsViewModel = new PackagePathViewModel(packageLoader, loadPackagesParams, customNodeManager);
            TrustedPathsViewModel = new TrustedPathViewModel(this.preferenceSettings, this.dynamoViewModel?.Model?.Logger);

            PropertyChanged += Model_PropertyChanged;
            InitializeCommands();
        }

        public event EventHandler<PythonTemplatePathEventArgs> RequestShowFileDialog;
        public virtual void OnRequestShowFileDialog(object sender, PythonTemplatePathEventArgs e)
        {
            if (RequestShowFileDialog != null)
            {
                RequestShowFileDialog(sender, e);
            }
        }

        public DelegateCommand AddPythonPathCommand { get; private set; }
        public DelegateCommand DeletePythonPathCommand { get; private set; }
        public DelegateCommand UpdatePythonPathCommand { get; private set; }        

        private void InitializeCommands()
        {
            AddPythonPathCommand = new DelegateCommand(p => AddPath());
            DeletePythonPathCommand = new DelegateCommand(p => RemovePath(), p => CanDelete());
            UpdatePythonPathCommand = new DelegateCommand(p => UpdatePathAt());
        }

        // Add python template path
        private void AddPath()
        {
            var args = new PythonTemplatePathEventArgs();

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            try
            {
                PathHelper.IsValidPath(args.Path);
            }
            catch (Exception)
            {
                // return
                return;
            }

            PythonTemplateFilePath = args.Path;
            RaiseCanExecuteChanged();
        }

        // Add python template path
        private void RemovePath()
        {
            PythonTemplateFilePath = String.Empty;
            RaiseCanExecuteChanged();
        }

        // Update Python path
        private void UpdatePathAt()
        {
            var args = new PythonTemplatePathEventArgs
            {
                Path = PythonTemplateFilePath
            };

            ShowFileDialog(args);

            if (args.Cancel)
                return;

            PythonTemplateFilePath = args.Path;
        }

        private bool CanDelete()
        {
            return !string.IsNullOrEmpty(PythonTemplateFilePath);
        }

        private void ShowFileDialog(PythonTemplatePathEventArgs e)
        {
            OnRequestShowFileDialog(this, e);
        }

        private void RaiseCanExecuteChanged()
        {
            AddPythonPathCommand.RaiseCanExecuteChanged();
            DeletePythonPathCommand.RaiseCanExecuteChanged();
            UpdatePythonPathCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Called from DynamoViewModel::UnsubscribeAllEvents()
        /// </summary>
        internal virtual void UnsubscribeAllEvents()
        {
            PropertyChanged -= Model_PropertyChanged;
            PythonEngineManager.Instance.AvailableEngines.CollectionChanged -= PythonEnginesChanged;
        }

        /// <summary>
        /// Listen for changes to the custom package paths and update package paths for install accordingly
        /// </summary>
        private void PackagePathsViewModel_RootLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // New path was added
                    var newPath = e.NewItems[0] as string;
                    PackagePathsForInstall.Add(newPath);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Path was removed
                    var removedPath = e.OldItems[0] as string;
                    var updateSelection = SelectedPackagePathForInstall == removedPath;
                    if (PackagePathsForInstall.Remove(removedPath) && updateSelection && PackagePathsForInstall.Count > 0)
                    {
                        // Path selected was removed
                        // Select first path in list
                        SelectedPackagePathForInstall = PackagePathsForInstall[0];
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Path was updated
                    newPath = e.NewItems[0] as string;
                    removedPath = e.OldItems[0] as string;
                    updateSelection = SelectedPackagePathForInstall == removedPath;
                    var index = PackagePathsForInstall.IndexOf(removedPath);
                    if (index != -1)
                    {
                        PackagePathsForInstall[index] = newPath;
                    }

                    if (updateSelection)
                    {
                        // Update selection of the updated path was selected
                        SelectedPackagePathForInstall = newPath;
                    }
                    break;
                default:
                    throw new NotSupportedException("Operation not supported");
            }
        }

        /// <summary>
        /// Store selection to preferences
        /// </summary>
        internal void CommitPackagePathsForInstall()
        {
            PackagePathsViewModel.RootLocations.CollectionChanged -= PackagePathsViewModel_RootLocations_CollectionChanged;
            preferenceSettings.SelectedPackagePathForInstall = SelectedPackagePathForInstall;
        }

        /// <summary>
        /// Force reload of paths and get current selection from preferences
        /// </summary>
        internal void InitPackagePathsForInstall()
        {
            PackagePathsForInstall = null;
            SelectedPackagePathForInstall = preferenceSettings.SelectedPackagePathForInstall;
            PackagePathsViewModel.RootLocations.CollectionChanged += PackagePathsViewModel_RootLocations_CollectionChanged;
        }

        /// <summary>
        /// Init all package filters
        /// </summary>
        internal void InitPackageListFilters()
        {
            installedPackagesViewModel?.PopulateFilters();
        }

        /// <summary>
        /// Listen for the PropertyChanged event and updates the saved changes label accordingly
        /// </summary>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string description = string.Empty;

            // C# does not support going through all cases when one of the case is true
            switch (e.PropertyName)
            {
                case nameof(SelectedLanguage):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewLanguageLabel), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(SelectedNumberFormat):
                    description = Res.ResourceManager.GetString(nameof(Res.DynamoViewSettingMenuNumberFormat), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(SelectedPackagePathForInstall):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewSelectedPackagePathForDownload), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(DisableBuiltInPackages):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewDisableBuiltInPackages), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(DisableCustomPackages):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewDisableCustomPackages), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(RunSettingsIsChecked):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewRunSettingsLabel), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(RunPreviewIsChecked):
                    description = Res.ResourceManager.GetString(nameof(Res.DynamoViewSettingShowRunPreview), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(StyleItemsList):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewVisualSettingsGroupStyles), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(OptionsGeometryScale):
                    description = Res.ResourceManager.GetString(nameof(Res.DynamoViewSettingsMenuChangeScaleFactor), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowEdges):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewVisualSettingShowEdges), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(IsolateSelectedGeometry):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewVisualSettingsIsolateSelectedGeo), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(UseHardwareAcceleration):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesSettingHardwareAcceleration), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(BackupIntervalInMinutes):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesSettingBackupInterval), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(MaxNumRecentFiles):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesSettingMaxRecentFiles), System.Globalization.CultureInfo.InvariantCulture);
                    UpdateRecentFiles();
                    goto default;
                case nameof(PythonTemplateFilePath):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesSettingCustomPythomTemplate), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(TessellationDivisions):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewVisualSettingsRenderPrecision), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(SelectedPythonEngine):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewDefaultPythonEngine), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(HideIronPythonAlertsIsChecked):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewIsIronPythonDialogDisabled), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowWhitespaceIsChecked):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewShowWhitespaceInPythonEditor), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(NodeAutocompleteIsChecked):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewEnableNodeAutoComplete), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(EnableTSplineIsChecked):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewEnableTSplineNodes), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowPreviewBubbles):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewShowPreviewBubbles), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowCodeBlockLineNumber):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewShowCodeBlockNodeLineNumber), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(DisableTrustWarnings):
                    description = Res.ResourceManager.GetString(nameof(Res.PreferencesViewTrustWarningHeader), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                // We track these this in two places, one in preference panel,
                // one where user make such switch in Node AutoComplete UI
                case nameof(nodeAutocompleteSuggestion):
                    if (nodeAutocompleteSuggestion == NodeAutocompleteSuggestion.MLRecommendation)
                        description = nameof(NodeAutocompleteSuggestion.MLRecommendation);
                    else
                        description = nameof(NodeAutocompleteSuggestion.ObjectType);
                    goto default;
                case nameof(MLRecommendationConfidenceLevel):
                    // Internal use only, no need to localize for now
                    description = "Confidence Level";
                    goto default;
                default:
                    if (!string.IsNullOrEmpty(description))
                    {
                        // Log switch on each setting and use description equals to label name
                        Dynamo.Logging.Analytics.TrackEvent(
                            Actions.Switch,
                            Categories.Preferences,
                            description);
                        UpdateSavedChangesLabel();
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates the contents to display by the SavedChanges label and its tooltip
        /// </summary>
        internal void UpdateSavedChangesLabel()
        {
            SavedChangesLabel = Res.PreferencesViewSavedChangesLabel;
            //Sets the last saved time in the en-US format
            SavedChangesTooltip = Res.PreferencesViewSavedChangesTooltip + " " + DateTime.Now.ToString(@"HH:mm");
        }

        /// <summary>
        /// This method will remove the current Style selected from the Styles list by name
        /// </summary>
        /// <param name="styleName"></param>
        internal void RemoveStyleEntry(string styleName)
        {
            GroupStyleItem itemToRemovePreferences = preferenceSettings.GroupStyleItemsList.FirstOrDefault(x => x.Name.Equals(styleName));
            preferenceSettings.GroupStyleItemsList.Remove(itemToRemovePreferences);
            RaisePropertyChanged(nameof(StyleItemsList));
            UpdateSavedChangesLabel();
        }

        /// <summary>
        /// This method will check if the name of Style that is being created already exists in the Styles list
        /// </summary>
        /// <param name="item">target style item to check</param>
        /// <returns></returns>
        internal bool IsStyleNameValid(StyleItem item)
        {
            return StyleItemsList.Where(x => x.Name.Equals(item.Name)).Any();
        }

        /// <summary>
        /// This method will check if the name of Style that is being created already exists in the Styles list
        /// </summary>
        /// <param name="item">target style to be checked</param>
        /// <returns></returns>
        internal bool ValidateStyleGuid(StyleItem item)
        {
            return StyleItemsList.Where(x => x.Name.Equals(item.Name)).Any();
        }

        /// <summary>
        /// This method will remove a specific style control from the Styles list
        /// </summary>
        internal void ResetAddStyleControl()
        {
            IsEnabledAddStyleButton = true;
            IsSaveButtonEnabled = true;
            AddStyleControl = new StyleItem();
            IsWarningEnabled = false;
            IsVisibleAddStyleBorder = false;          
        }

        /// <summary>
        /// This method will enable the warning icon next to the GroupName TextBox and other buttons needed
        /// </summary>
        /// <param name="warningMessage">Message that will be displayed when the mouse is over the warning</param>
        internal void EnableGroupStyleWarningState(string warningMessage)
        {
            CurrentWarningMessage = warningMessage;
            IsWarningEnabled = true;
            IsSaveButtonEnabled = false;
        }

        /// <summary>
        /// This Method will generate a random color string in a Hexadecimal format
        /// </summary>
        /// <returns></returns>
        internal string GetRandomHexStringColor()
        {
            Random r = new Random();
            Color color = Color.FromArgb(255, (byte)r.Next(), (byte)r.Next(), (byte)r.Next());
            return ColorTranslator.ToHtml(color).Replace("#", "");
        }

        private void PythonEnginesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddPythonEnginesOptions();
            }
        }

        private void UpdateRecentFiles()
        {
            if (dynamoViewModel.RecentFiles.Count > MaxNumRecentFiles)
            {
                dynamoViewModel.RecentFiles.RemoveRange(MaxNumRecentFiles, dynamoViewModel.RecentFiles.Count - MaxNumRecentFiles);
            }
        }
    }

    public class PythonTemplatePathEventArgs : EventArgs
    {
        /// <summary>
        /// Indicate whether user wants to set the current path.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Indicate the path for Custom Python Template.
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// This class will contain the Enum value and the corresponding description for each radio button in the Visual Settings -> Geometry Scaling section
    /// </summary>
    public class GeometryScalingOptions
    {
        //The Enum values can be Small, Medium, Large or Extra Large
        [Obsolete("This property is deprecated and will be removed in a future version of Dynamo")]
        public GeometryScaleSize EnumProperty { get; set; }

        /// <summary>
        /// This property will contain the description of each of the radio buttons in the Visual Settings -> Geometry Scaling section
        /// </summary>
        public ObservableCollection<string> DescriptionScaleRange { get; set; }

        /// <summary>
        /// This method is used to convert a index (representing a RadioButton in the UI) to a ScaleFactor
        /// </summary>
        /// <param name="index">This value is the index for the RadioButton in the Geometry Scaling section. 
        /// It can have the values:
        ///   0 - Small
        ///   1 - Medium (Default)
        ///   2 - Large
        ///   3 - Extra Large
        /// </param>
        /// <returns>The Scale Factor (-2, 0, 2, 4)</returns>
        public static int ConvertUIToScaleFactor (int index)
        {
            return (index - 1) * 2;
        }

        /// <summary>
        /// This method is used to conver a Scale Factor to a index so we can Check a Radio Button in the UI
        /// </summary>
        /// <param name="scaleValue">This values is the Scale that we need to convert to index (representing a RadioButton)
        /// It can have the values:
        /// - 2 - Small
        ///   0 - Medium (Default)
        ///   2 - Large
        ///   4 - Extra Large
        /// </param>
        /// <returns>The radiobutton index (0,1,2,3)</returns>
        public static int ConvertScaleFactorToUI(int scaleValue)
        {
           return (scaleValue / 2) + 1;
        }
    }

    /// <summary>
    /// This class represent a Tab and is used for store just one Expander info(due that just one Expander can be expanded at one time)
    /// </summary>
    public class TabSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Tab Name (e.g. Features or Visual Settings)
        /// </summary>
        public string Name;
        private string expanderActive;

        /// <summary>
        /// This property hold the name for the current Expander expanded
        /// </summary>
        public string ExpanderActive
        {
            get
            {
                return expanderActive;
            }
            set
            {
                if(value != null)
                {
                    expanderActive = value;
                    OnPropertyChanged(nameof(ExpanderActive));
                }
                else
                {
                    expanderActive = string.Empty;
                }
            }
        }
    }
}
