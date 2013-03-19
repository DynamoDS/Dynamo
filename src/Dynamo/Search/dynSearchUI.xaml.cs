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
        public Grid ParentGrid;
        public ListBox ResultList;
        public StackPanel Container;

        public ObservableCollection<dynNodeUI> VisibleNodes { get { return Controller.VisibleNodes; } }

        public dynSearchUI( dynSearchController controller )
        {
            Controller = controller;
            InitializeComponent();

            ParentGrid = (Grid)this.Content;
            Container = (StackPanel) ParentGrid.Children[0];
            SearchTextBox = (TextBox) Container.Children[0];
            ResultList = (ListBox) ((ScrollViewer) Container.Children[1]).Content;

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Controller.SendFirstResultToWorkspace();
            }
        }

        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Controller.SearchAndUpdateUI( this.SearchTextBox.Text.Trim() );
        }

    }
}
