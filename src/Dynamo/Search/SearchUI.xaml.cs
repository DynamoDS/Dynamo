using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Search;
using Dynamo.Utilities;

namespace Dynamo.Search
{
    /// <summary>
    /// Interaction logic for SearchUI.xaml
    /// </summary>
    public partial class SearchUI : UserControl
    {
        public SearchViewModel ViewModel;
        
        public ObservableCollection<ISearchElement> VisibleNodes { get { return ViewModel.VisibleNodes; } }

        public SearchUI( SearchViewModel viewModel )
        {
            ViewModel = viewModel;

            InitializeComponent();

            SearchResultsListBox.SelectionMode = SelectionMode.Single;

            this.PreviewKeyDown += new KeyEventHandler(KeyHandler);
            SearchTextBox.IsVisibleChanged += delegate { SearchTextBox.Focus();
                                                         SearchTextBox.SelectAll();
                                                         ViewModel.SearchAndUpdateUI(this.SearchTextBox.Text.Trim());
            };
        }

        private bool HasPackageManagerElements;
        public void SearchOnline_Click(object sender, RoutedEventArgs e)
        {
            if (!this.HasPackageManagerElements)
            {
                this.HasPackageManagerElements = true;
                dynSettings.Controller.PackageManagerClient.RefreshAvailable();
            }
            else
            {
                this.HasPackageManagerElements = false;
                ViewModel.SearchDictionary.RemoveName((s) => s is PackageManagerSearchElement );
                ViewModel.SearchAndUpdateUI();
            }
            
        }
        
        private void KeyHandler(object sender, KeyEventArgs e)
        {
            ViewModel.KeyHandler(sender, e);
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SearchAndUpdateUI( this.SearchTextBox.Text.Trim() );
        }

        public void SelectNext()
        {
            if (SelectedIndex() == this.SearchResultsListBox.Items.Count - 1
                || SelectedIndex() == -1)
                return;

            SetSelected(SelectedIndex() + 1);
        }

        public void SelectPrevious()
        {
            if (SelectedIndex() == 0 || SelectedIndex() == -1)
                return;

            SetSelected(SelectedIndex() - 1);
        }

        public void SetSelected(int i)
        {
            this.SearchResultsListBox.SelectedIndex = i;
            if ( i < this.SearchResultsListBox.Items.Count )
                this.SearchResultsListBox.ScrollIntoView( this.SearchResultsListBox.Items[i] );
        }

        public int SelectedIndex()
        {
            return this.SearchResultsListBox.SelectedIndex;
        }
    }
}
