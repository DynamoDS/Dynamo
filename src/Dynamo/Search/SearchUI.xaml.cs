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
        public TextBox SearchTextBox;
        public ListBox ResultList;
        
        public ObservableCollection<dynNodeUI> VisibleNodes { get { return Controller.VisibleNodes; } }

        public SearchUI( SearchController controller )
        {
            Controller = controller;

            InitializeComponent();

            SearchTextBox = (TextBox) this.RSearchBox;
            ResultList = (ListBox) this.RSearchResultsListBox;

            ResultList.SelectionMode = SelectionMode.Single;

            this.PreviewKeyDown += new KeyEventHandler(KeyHandler);
            SearchTextBox.IsVisibleChanged += delegate { SearchTextBox.Focus();
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
            if (SelectedIndex() == this.ResultList.Items.Count - 1
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
            this.ResultList.SelectedIndex = i;
            this.ResultList.ScrollIntoView( this.ResultList.Items[i] );
        }

        public int SelectedIndex()
        {
            return this.ResultList.SelectedIndex;
        }
    }
}
