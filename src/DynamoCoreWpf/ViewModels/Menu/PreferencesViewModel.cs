using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Wpf.ViewModels.Core.Converters;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Dynamo.ViewModels
{
    public class PreferencesViewModel : ViewModelBase
    {

        private ObservableCollection<string> _languagesList;
        private ObservableCollection<string> _fontSizeList;
        private ObservableCollection<string> _numberFormatList;
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

        private PreferenceSettings preferenceSettings;
        private DynamoPythonScriptEditorTextOptions pythonScriptEditorTextOptions;
        private HomeWorkspaceModel homeSpace;
        private DynamoViewModel dynamoViewModel;

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
                return _languagesList;
            }
            set
            {
                _languagesList = value;
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
                return _fontSizeList;
            }
            set
            {
                _fontSizeList = value;
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
                return _numberFormatList;
            }
            set
            {
                _numberFormatList = value;
                RaisePropertyChanged(nameof(NumberFormatList));
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

            //By Default the Default Run Settings radio button will be in Manual
            RunSettingsIsChecked = !preferenceSettings.RunTypeAutomatic;
        }
    }
}
