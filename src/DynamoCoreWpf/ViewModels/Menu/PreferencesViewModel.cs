using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Wpf.ViewModels.Core.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
    public class PreferencesViewModel : ViewModelBase
    {
        #region Private Properties
        private ObservableCollection<string> languagesList;
        private ObservableCollection<string> fontSizeList;
        private ObservableCollection<string> numberFormatList;
        private ObservableCollection<StyleItem> styleItemsList;
        private StyleItem addStyleControl;  
        private ObservableCollection<string> _pythonEngineList;

        private string selectedLanguage;
        private string selectedFontSize;
        private string selectedNumberFormat;
        private string selectedPythonEngine;
        private bool runPreviewEnabled;
        private bool runSettingsIsChecked;
        private bool runPreviewIsChecked;
        private bool hideIronPAlerts;
        private bool showWhitespace;
        private bool nodeAutocomplete;
        private bool enableTSpline;
        private bool showEdges;
        private bool isolateSelectedGeometry;

        private PreferenceSettings preferenceSettings;
        private DynamoPythonScriptEditorTextOptions pythonScriptEditorTextOptions;
        private HomeWorkspaceModel homeSpace;
        private DynamoViewModel dynamoViewModel;
        private bool isWarningEnabled;
        private GeometryScalingOptions optionsGeometryScal = null;
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
                return !preferenceSettings.RunTypeAutomatic;
            }
            set
            {
                preferenceSettings.RunTypeAutomatic = !value;
                runSettingsIsChecked = value;
                RaisePropertyChanged(nameof(RunSettingsIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the Show Run Preview toogle button
        /// </summary>
        public bool RunPreviewEnabled
        {
            get
            {
                return runPreviewEnabled;
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the Show Run Preview toogle button
        /// </summary>
        public bool RunPreviewIsChecked
        {
            get
            {
                return dynamoViewModel.ShowRunPreview;
            }
            set
            {
                dynamoViewModel.ShowRunPreview = value;
                RaisePropertyChanged(nameof(RunPreviewIsChecked));
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
        public GeometryScalingOptions OptionsGeometryScal
        {          
            get
            {
                return optionsGeometryScal;
            }
            set
            {
                optionsGeometryScal = value;
                RaisePropertyChanged(nameof(OptionsGeometryScal));
            }
        }

        /// <summary>
        /// Controls the binding for the ShowEdges toogle in the Preferences->Visual Settings->Display Settings section
        /// </summary>
        public bool ShowEdges
        {
            get
            {
                return showEdges;
            }
            set
            {
                showEdges = value;
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
                return isolateSelectedGeometry;
            }
            set
            {
                isolateSelectedGeometry = value;
                RaisePropertyChanged(nameof(IsolateSelectedGeometry));
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
                return _pythonEngineList;
            }
            set
            {
                _pythonEngineList = value;
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
                return preferenceSettings.DefaultPythonEngine;
            }
            set
            {
                if (value != preferenceSettings.DefaultPythonEngine)
                {
                    selectedPythonEngine = value;
                    preferenceSettings.DefaultPythonEngine = value;
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
        /// The PreferencesViewModel constructor basically initialize all the ItemsSource for the corresponding ComboBox in the View (PreferencesView.xaml)
        /// </summary>
        public PreferencesViewModel(DynamoViewModel dynamoViewModel)
        {
            this.preferenceSettings = dynamoViewModel.PreferenceSettings;
            this.pythonScriptEditorTextOptions = dynamoViewModel.PythonScriptEditorTextOptions;
            this.runPreviewEnabled = dynamoViewModel.HomeSpaceViewModel.RunSettingsViewModel.RunButtonEnabled;
            this.homeSpace = dynamoViewModel.HomeSpace;
            this.dynamoViewModel = dynamoViewModel;

            PythonEnginesList = new ObservableCollection<string>();
            PythonEnginesList.Add(Wpf.Properties.Resources.DefaultPythonEngineNone);
            AddPythonEnginesOptions();
            SelectedPythonEngine = preferenceSettings.DefaultPythonEngine;

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

            RunSettingsIsChecked = !preferenceSettings.RunTypeAutomatic;

            //By Default the warning state of the Visual Settings tab (Group Styles section) will be disabled
            isWarningEnabled = false;

            StyleItemsList = new ObservableCollection<StyleItem>();
          
            //When pressing the "Add Style" button some controls will be shown with some values by default so later they can be populated by the user
            AddStyleControl = new StyleItem() { GroupName = "", HexColorString = "#" + GetRandomHexStringColor() };

            //This piece of code will populate all the description text for the RadioButtons in the Geometry Scaling section.
            optionsGeometryScal = new GeometryScalingOptions();
            optionsGeometryScal.EnumProperty = GeometryScaleSize.Medium;
            optionsGeometryScal.DescriptionScaleRange = new ObservableCollection<string>();
            optionsGeometryScal.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Small].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Small].Item3));
            optionsGeometryScal.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Medium].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Medium].Item3));
            optionsGeometryScal.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Large].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Large].Item3));
            optionsGeometryScal.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.ExtraLarge].Item2,
                                                                                              scaleRanges[GeometryScaleSize.ExtraLarge].Item3));
        }

        /// <summary>
        /// This method will remove the current Style selected from the Styles list
        /// </summary>
        /// <param name="groupName"></param>
        internal void RemoveStyleEntry(string groupName)
        {
            StyleItem itemToRemove = (from item in StyleItemsList where item.GroupName.Equals(groupName) select item).FirstOrDefault();
            StyleItemsList.Remove(itemToRemove);
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
    }
}
