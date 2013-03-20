using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Controls;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for dynSearchUI.xaml
    /// </summary>
    public partial class dynSearchUI : UserControl
    {
        public dynSearchController Controller;
        public TextBox SearchTextBox;
        public ListBox ResultList;

        public ObservableCollection<dynNodeUI> VisibleNodes { get { return Controller.VisibleNodes; } }

        public dynSearchUI( dynSearchController controller )
        {
            Controller = controller;
            InitializeComponent();

            SearchTextBox = (TextBox) this.RSearchBox;
            ResultList = (ListBox) this.RSearchResultsListBox;

            ResultList.SelectionMode = SelectionMode.Single;

            this.PreviewKeyDown += new KeyEventHandler(KeyHandler);
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
