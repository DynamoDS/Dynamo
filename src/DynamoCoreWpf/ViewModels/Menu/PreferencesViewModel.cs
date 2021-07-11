using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.Wpf.ViewModels.Core.Converters;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// The next enum will contain the posible values for Scaling (Visual Settings -> Geometry Scaling section)
    /// </summary>
    public enum GeometryScaleSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge
    }
    public class PreferencesViewModel : ViewModelBase, INotifyPropertyChanged
    {
        #region Private Properties
        private string savedChangesLabel;
        private string savedChangesTooltip;
        private ObservableCollection<string> languagesList;
        private ObservableCollection<string> packagePathsForInstall;
        private ObservableCollection<string> fontSizeList;
        private ObservableCollection<string> numberFormatList;
        private ObservableCollection<StyleItem> styleItemsList;
        private StyleItem addStyleControl;
        private ObservableCollection<string> pythonEngineList;

        private string selectedLanguage;
        private string selectedFontSize;
        private string selectedNumberFormat;
        private string selectedPythonEngine;
        private bool runPreviewEnabled;
        private bool runPreviewIsChecked;
        private bool hideIronPAlerts;
        private bool showWhitespace;
        private bool nodeAutocomplete;
        private bool enableTSpline;
        private bool showEdges;
        private bool isolateSelectedGeometry;
        private bool showCodeBlockLineNumber;
        private RunType runSettingsIsChecked;
        private Dictionary<string, TabSettings> preferencesTabs;

        private PreferenceSettings preferenceSettings;
        private DynamoPythonScriptEditorTextOptions pythonScriptEditorTextOptions;
        private HomeWorkspaceModel homeSpace;
        private DynamoViewModel dynamoViewModel;
        private bool isWarningEnabled;
        private GeometryScalingOptions optionsGeometryScale = null;

        private InstalledPackagesViewModel installedPackagesViewModel;
        #endregion Private Properties

        public GeometryScaleSize ScaleSize { get; set; }

        public Tuple<string, string, string> ScaleRange
        {
            get
            {
                return scaleRanges[ScaleSize];
            }
        }

        private Dictionary<GeometryScaleSize, Tuple<string, string, string>> scaleRanges = new Dictionary<GeometryScaleSize, Tuple<string, string, string>>
        {
            {GeometryScaleSize.Medium, new Tuple<string, string, string>("medium", "0.0001", "10,000")},
            {GeometryScaleSize.Small, new Tuple<string, string, string>("small", "0.000,001", "100")},
            {GeometryScaleSize.Large, new Tuple<string, string, string>("large", "0.01", "1,000,000")},
            {GeometryScaleSize.ExtraLarge, new Tuple<string, string, string>("extra large", "1", "100,000,000")}
        };

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
        /// Returns the state of the Preferences Window Debug Mode
        /// </summary>
        public bool PreferencesDebugMode
        {
            get
            {
                return DebugModes.IsEnabled("DynamoPreferencesMenuDebugMode");
            }
        }

        /// <summary>
        /// Returns all installed packages
        /// </summary>
        public ObservableCollection<PackageViewModel> LocalPackages => installedPackagesViewModel.LocalPackages;

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
                selectedLanguage = value;
                RaisePropertyChanged(nameof(SelectedLanguage));
            }
        }

        /// <summary>
        /// Controls the Selected option in Node Font Size ComboBox
        /// </summary>
        public string SelectedFontSize
        {
            get
            {
                return selectedFontSize;
            }
            set
            {
                selectedFontSize = value;
                RaisePropertyChanged(nameof(SelectedFontSize));
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
        /// LanguagesList property containt the list of all the languages listed in: https://wiki.autodesk.com/display/LOCGD/Dynamo+Languages
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
                return preferenceSettings.SelectedPackagePathForInstall;
            }
            set
            {
                if (preferenceSettings.SelectedPackagePathForInstall != value)
                {
                    preferenceSettings.SelectedPackagePathForInstall = value;
                    RaisePropertyChanged(nameof(SelectedPackagePathForInstall));
                }
            }
        }

        /// <summary>
        /// FontSizesList contains the list of sizes for fonts defined (the ones defined are Small, Medium, Large, Extra Large)
        /// </summary>
        public ObservableCollection<string> FontSizeList
        {
            get
            {
                return fontSizeList;
            }
            set
            {
                fontSizeList = value;
                RaisePropertyChanged(nameof(FontSizeList));
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
        public ObservableCollection<StyleItem> StyleItemsList
        {
            get { return styleItemsList; }
            set
            {
                styleItemsList = value;
                RaisePropertyChanged(nameof(StyleItemsList));
            }
        }

        /// <summary>
        /// Used to add styles to the StyleItemsListe while also update the saved changes label
        /// </summary>
        /// <param name="style"></param>
        public void AddStyle(StyleItem style)
        {
            StyleItemsList.Add(style);
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
                showEdges = value;
                dynamoViewModel.RenderPackageFactoryViewModel.ShowEdges = value;
                RaisePropertyChanged(nameof(ShowEdges));
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
                isolateSelectedGeometry = value;
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

        public bool ShowCodeBlockLineNumber
        {
            get
            {
                return preferenceSettings.ShowCodeBlockLineNumber;
            }
            set
            {
                preferenceSettings.ShowCodeBlockLineNumber = value;
                showCodeBlockLineNumber = value;
                RaisePropertyChanged(nameof(ShowCodeBlockLineNumber));
            }
        }
        #endregion

        //This includes all the properties that can be set on the Features tab
        #region Features Properties
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
        /// Controls the IsChecked property in the "Hide IronPython alerts" toogle button
        /// </summary>
        public bool HideIronPythonAlertsIsChecked
        {
            get
            {
                return preferenceSettings.IsIronPythonDialogDisabled;
            }
            set
            {
                hideIronPAlerts = value;
                preferenceSettings.IsIronPythonDialogDisabled = value;
                RaisePropertyChanged(nameof(HideIronPythonAlertsIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Show Whitespace in Python editor" toogle button
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
                showWhitespace = value;
                RaisePropertyChanged(nameof(ShowWhitespaceIsChecked));
            }
        }

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
                nodeAutocomplete = value;
                RaisePropertyChanged(nameof(NodeAutocompleteIsChecked));
            }
        }

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
                enableTSpline = value;
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

        /// <summary>
        /// Gets the different Python Engine versions availables from PythonNodeModels.dll
        /// </summary>
        /// <returns>Strings array with the different names</returns>
        private string[] GetPythonEngineOptions()
        {
            try
            {
                var enumType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s =>
                    {
                        try
                        {
                            return s.GetTypes();
                        }
                        catch (ReflectionTypeLoadException)
                        {
                            return new Type[0];
                        }
                    }).FirstOrDefault(t => t.FullName.Equals("PythonNodeModels.PythonEngineVersion"));

                return Enum.GetNames(enumType);
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private void AddPythonEnginesOptions()
        {
            var pythonEngineOptions = GetPythonEngineOptions();
            if (pythonEngineOptions.Length != 0)
            {
                foreach (var option in pythonEngineOptions)
                {
                    if (option != "Unspecified")
                    {
                        PythonEnginesList.Add(option);
                    }
                }
            }
            else
            {
                PythonEnginesList.Add("IronPython2");
                PythonEnginesList.Add("CPython3");
            }
        }
        #endregion

        /// <summary>
        /// Package Search Paths view model.
        /// </summary>
        public PackagePathViewModel PackagePathsViewModel { get; set; }

        /// <summary>
        /// The PreferencesViewModel constructor basically initialize all the ItemsSource for the corresponding ComboBox in the View (PreferencesView.xaml)
        /// </summary>
        public PreferencesViewModel(DynamoViewModel dynamoViewModel)
        {
            this.preferenceSettings = dynamoViewModel.PreferenceSettings;
            this.pythonScriptEditorTextOptions = dynamoViewModel.PythonScriptEditorTextOptions;
            this.runPreviewEnabled = dynamoViewModel.HomeSpaceViewModel.RunSettingsViewModel.RunButtonEnabled;
            this.homeSpace = dynamoViewModel.HomeSpace;
            this.dynamoViewModel = dynamoViewModel;
            this.installedPackagesViewModel = new InstalledPackagesViewModel(dynamoViewModel, 
                dynamoViewModel.PackageManagerClientViewModel.PackageManagerExtension.PackageLoader);

            PythonEnginesList = new ObservableCollection<string>();
            PythonEnginesList.Add(Wpf.Properties.Resources.DefaultPythonEngineNone);
            AddPythonEnginesOptions();

            //Sets SelectedPythonEngine.
            //If the setting is empty it corresponds to the default python engine
            _ = preferenceSettings.DefaultPythonEngine == string.Empty ? 
                SelectedPythonEngine = Res.DefaultPythonEngineNone : 
                SelectedPythonEngine = preferenceSettings.DefaultPythonEngine;

            SelectedPackagePathForInstall = preferenceSettings.SelectedPackagePathForInstall;

            string languages = Wpf.Properties.Resources.PreferencesWindowLanguages;
            LanguagesList = new ObservableCollection<string>(languages.Split(','));
            SelectedLanguage = languages.Split(',').First();

            FontSizeList = new ObservableCollection<string>();
            FontSizeList.Add(Wpf.Properties.Resources.ScalingSmallButton);
            FontSizeList.Add(Wpf.Properties.Resources.ScalingMediumButton);
            FontSizeList.Add(Wpf.Properties.Resources.ScalingLargeButton);
            FontSizeList.Add(Wpf.Properties.Resources.ScalingExtraLargeButton);
            SelectedFontSize = Wpf.Properties.Resources.ScalingMediumButton;

            NumberFormatList = new ObservableCollection<string>();
            NumberFormatList.Add(Wpf.Properties.Resources.DynamoViewSettingMenuNumber0);
            NumberFormatList.Add(Wpf.Properties.Resources.DynamoViewSettingMenuNumber00);
            NumberFormatList.Add(Wpf.Properties.Resources.DynamoViewSettingMenuNumber000);
            NumberFormatList.Add(Wpf.Properties.Resources.DynamoViewSettingMenuNumber0000);
            NumberFormatList.Add(Wpf.Properties.Resources.DynamoViewSettingMenuNumber00000);
            SelectedNumberFormat = preferenceSettings.NumberFormat;

            runSettingsIsChecked = preferenceSettings.DefaultRunType;
            RunPreviewIsChecked = preferenceSettings.ShowRunPreview;

            //By Default the warning state of the Visual Settings tab (Group Styles section) will be disabled
            isWarningEnabled = false;

            StyleItemsList = new ObservableCollection<StyleItem>();
          
            //When pressing the "Add Style" button some controls will be shown with some values by default so later they can be populated by the user
            AddStyleControl = new StyleItem() { GroupName = "", HexColorString = "#" + GetRandomHexStringColor() };

            //This piece of code will populate all the description text for the RadioButtons in the Geometry Scaling section.
            optionsGeometryScale = new GeometryScalingOptions();

            //This will set the default option for the Geometry Scaling Radio Buttons, the value is comming from the DynamoViewModel
            optionsGeometryScale.EnumProperty = (GeometryScaleSize)GeometryScalingOptions.ConvertScaleFactorToUI(dynamoViewModel.ScaleFactorLog);

            optionsGeometryScale.DescriptionScaleRange = new ObservableCollection<string>();
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Small].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Small].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Medium].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Medium].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Large].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Large].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.ExtraLarge].Item2,
                                                                                              scaleRanges[GeometryScaleSize.ExtraLarge].Item3));

            SavedChangesLabel = string.Empty;
            SavedChangesTooltip = string.Empty;

            preferencesTabs = new Dictionary<string, TabSettings>();
            preferencesTabs.Add("General", new TabSettings() { Name = "General", ExpanderActive = string.Empty });
            preferencesTabs.Add("Features",new TabSettings() { Name = "Features", ExpanderActive = string.Empty });
            preferencesTabs.Add("VisualSettings",new TabSettings() { Name = "VisualSettings", ExpanderActive = string.Empty });
            preferencesTabs.Add("Package Manager", new TabSettings() { Name = "Package Manager", ExpanderActive = string.Empty });

            //create a packagePathsViewModel we'll use to interact with the package search paths list.
            var loadPackagesParams = new LoadPackageParams
            {
                Preferences = preferenceSettings,
                PathManager = dynamoViewModel.Model.PathManager,
            };
            var customNodeManager = dynamoViewModel.Model.CustomNodeManager;
            var packageLoader = dynamoViewModel.Model.GetPackageManagerExtension()?.PackageLoader;            
            PackagePathsViewModel = new PackagePathViewModel(packageLoader, loadPackagesParams, customNodeManager);

            this.PropertyChanged += Model_PropertyChanged;
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
                    // Do nothing for now
                    break;
                case nameof(SelectedFontSize):
                    // Do nothing for now
                    break;
                case nameof(SelectedNumberFormat):
                    description = Res.DynamoViewSettingMenuNumberFormat;
                    goto default;
                case nameof(SelectedPackagePathForInstall):
                    description = Res.PreferencesViewSelectedPackagePathForDownload;
                    goto default;
                case nameof(RunSettingsIsChecked):
                    description = Res.PreferencesViewRunSettingsLabel;
                    goto default;
                case nameof(RunPreviewIsChecked):
                    description = Res.DynamoViewSettingShowRunPreview;
                    goto default;
                case nameof(StyleItemsList):
                    // Do nothing for now
                    break;
                case nameof(OptionsGeometryScale):
                    description = Res.DynamoViewSettingsMenuChangeScaleFactor;
                    goto default;
                case nameof(ShowEdges):
                    description = Res.PreferencesViewVisualSettingShowEdges;
                    goto default;
                case nameof(IsolateSelectedGeometry):
                    description = Res.PreferencesViewVisualSettingsIsolateSelectedGeo;
                    goto default;
                case nameof(TessellationDivisions):
                    description = Res.PreferencesViewVisualSettingsRenderPrecision;
                    goto default;
                case nameof(SelectedPythonEngine):
                    description = Res.PreferencesViewDefaultPythonEngine;
                    goto default;
                case nameof(HideIronPythonAlertsIsChecked):
                    description = Res.PreferencesViewIsIronPythonDialogDisabled;
                    goto default;
                case nameof(ShowWhitespaceIsChecked):
                    description = Res.PreferencesViewShowWhitespaceInPythonEditor;
                    goto default;
                case nameof(NodeAutocompleteIsChecked):
                    description = Res.PreferencesViewEnableNodeAutoComplete;
                    goto default;
                case nameof(EnableTSplineIsChecked):
                    description = Res.PreferencesViewEnableTSplineNodes;
                    goto default;
                case nameof(ShowCodeBlockLineNumber):
                    description = Res.PreferencesViewShowCodeBlockNodeLineNumber;
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
        /// This method will remove the current Style selected from the Styles list
        /// </summary>
        /// <param name="groupName"></param>
        internal void RemoveStyleEntry(string groupName)
        {
            StyleItem itemToRemove = (from item in StyleItemsList where item.GroupName.Equals(groupName) select item).FirstOrDefault();
            StyleItemsList.Remove(itemToRemove);
            UpdateSavedChangesLabel();
        }

        /// <summary>
        /// This method will check if the Style that is being created already exists in the Styles list
        /// </summary>
        /// <param name="item1"></param>
        /// <returns></returns>
        internal bool ValidateExistingStyle(StyleItem item1)
        {
            return StyleItemsList.Where(x => x.GroupName.Equals(item1.GroupName)).Any();
        }

        /// <summary>
        /// This method will remove a specific style control from the Styles list
        /// </summary>
        internal void ResetAddStyleControl()
        {
            AddStyleControl.GroupName = String.Empty;
            AddStyleControl.HexColorString = "#" + GetRandomHexStringColor();
            IsWarningEnabled = false;
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
    }

    /// <summary>
    /// This Class will act as a container for each of the StyleItems in the Styles list located in in the Visual Settings -> Group Styles section
    /// </summary>
    public class StyleItem : ViewModelBase
    {
        private string groupName;
        private string hexColorString;

        /// <summary>
        /// This property will containt the Group Name thas was added by the user when creating a new Style
        /// </summary>
        public string GroupName
        {
            get { return groupName; }
            set
            {
                groupName = value;
                RaisePropertyChanged(nameof(GroupName));
            }
        }

        /// <summary>
        /// This property represents a color in a hexadecimal representation (with the # character at the beginning of the string)
        /// </summary>
        public string HexColorString
        {
            get { return hexColorString; }
            set
            {
                hexColorString = value;
                RaisePropertyChanged(nameof(HexColorString));
            }
        }
    }

    /// <summary>
    /// This class will contain the Enum value and the corresponding description for each radio button in the Visual Settings -> Geometry Scaling section
    /// </summary>
    public class GeometryScalingOptions
    {
        //The Enum values can be Small, Medium, Large or Extra Large
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
