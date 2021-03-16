using System.Collections.ObjectModel;
using System.Linq;

namespace Dynamo.ViewModels
{
    public class PreferencesViewModel : ViewModelBase
    {

        private ObservableCollection<string> _languagesList;
        private ObservableCollection<string> _fontSizeList;
        private ObservableCollection<string> _numberFormatList;
        private string selectedLanguage;
        private string selectedFontSize;
        private string selectedNumberFormat;
        private bool runSettingsIsChecked;
        private bool runPreviewIsChecked;

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
                RaisePropertyChanged("SelectedLanguage");
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
                RaisePropertyChanged("SelectedFontSize");
            }
        }

        /// <summary>
        /// Controls the Selected option in Number Format ComboBox
        /// </summary>
        public string SelectedNumberFormat
        {
            get
            {
                return selectedNumberFormat;
            }
            set
            {
                selectedNumberFormat = value;
                RaisePropertyChanged("SelectedNumberFormat");
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the RunSettings radio button
        /// </summary>
        public bool RunSettingsIsChecked
        {
            get
            {
                return runSettingsIsChecked;
            }
            set
            {
                runSettingsIsChecked = value;
                RaisePropertyChanged("RunSettingsIsChecked");
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the Show Run Preview toogle button
        /// </summary>
        public bool RunPreviewIsChecked
        {
            get
            {
                return runPreviewIsChecked;
            }
            set
            {
                runPreviewIsChecked = value;
                RaisePropertyChanged("RunPreviewIsChecked");
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
                RaisePropertyChanged("LanguagesList");
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
                RaisePropertyChanged("FontSizeList");
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
                RaisePropertyChanged("NumberFormatList");
            }
        }

        /// <summary>
        /// The PreferencesViewModel constructor basically initialize all the ItemsSource for the corresponding ComboBox in the View (PreferencesView.xaml)
        /// </summary>
        public PreferencesViewModel()
        {
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
            SelectedNumberFormat = Wpf.Properties.Resources.DynamoViewSettingMenuNumber0000;

            //By Default the Default Run Settings radio button will be in Manual
            RunSettingsIsChecked = true;

        }
    }
}
