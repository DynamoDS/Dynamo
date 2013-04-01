using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Search;

namespace Dynamo.Search
{
    /// <summary>
    /// Interaction logic for SearchUI.xaml
    /// </summary>
    public partial class SearchUI : UserControl
    {
        public SearchController Controller;
        
        public ObservableCollection<ISearchElement> VisibleNodes { get { return Controller.VisibleNodes; } }

        public SearchUI( SearchController controller )
        {
            Controller = controller;

            InitializeComponent();

            SearchResultsListBox.SelectionMode = SelectionMode.Single;

            this.PreviewKeyDown += new KeyEventHandler(KeyHandler);
            SearchTextBox.IsVisibleChanged += delegate { SearchTextBox.Focus();
                                                         SearchTextBox.SelectAll();
                                                         Controller.SearchAndUpdateUI(this.SearchTextBox.Text.Trim());
            };
        }

        private void KeyHandler(object sender, KeyEventArgs e)
        {
            Controller.KeyHandler(sender, e);
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Controller.SearchAndUpdateUI( this.SearchTextBox.Text.Trim() );
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
            this.SearchResultsListBox.ScrollIntoView( this.SearchResultsListBox.Items[i] );
        }

        public int SelectedIndex()
        {
            return this.SearchResultsListBox.SelectedIndex;
        }
    }
}
