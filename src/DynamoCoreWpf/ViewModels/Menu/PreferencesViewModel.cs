using System.Collections.ObjectModel;
using System.Linq;

namespace Dynamo.ViewModels
{
    public class PreferencesViewModel : ViewModelBase
    {

        private ObservableCollection<string> _languagesList;
        private ObservableCollection<string> _fontSizeList;
        private ObservableCollection<string> _numberFormatList;
        private string _selectedLanguage;
        private string _selectedFontSize;
        private string _selectedNumberFormat;
        private bool _runSettingsIsChecked;
        private bool _runPreviewIsChecked;

        /// <summary>
        /// Controls the Selected option in Language ComboBox
        /// </summary>
        public string SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                _selectedLanguage = value;
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
                return _selectedFontSize;
            }
            set
            {
                _selectedFontSize = value;
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
                return _selectedNumberFormat;
            }
            set
            {
                _selectedNumberFormat = value;
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
                return _runSettingsIsChecked;
            }
            set
            {
                _runSettingsIsChecked = value;
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
                return _runPreviewIsChecked;
            }
            set
            {
                _runPreviewIsChecked = value;
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

            string fontSizes = Wpf.Properties.Resources.PreferencesWindowFontSizes;
            FontSizeList = new ObservableCollection<string>(fontSizes.Split(','));
            SelectedFontSize = fontSizes.Split(',')[1];

            string numberFormats = Wpf.Properties.Resources.PreferencesWindowNumberFormats;
            NumberFormatList = new ObservableCollection<string>(numberFormats.Split(','));
            SelectedNumberFormat = numberFormats.Split(',').First();

            //By Default the Default Run Settings radio button will be in Manual
            RunSettingsIsChecked = true;

        }
    }
}
