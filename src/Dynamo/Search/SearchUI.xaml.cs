using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Commands;
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

        public SearchUI( SearchViewModel viewModel )
        {
            this.DataContext = viewModel;
            InitializeComponent();
     
            this.PreviewKeyDown += viewModel.KeyHandler;

            SearchTextBox.IsVisibleChanged += delegate
                {
                    SearchTextBox.Focus();
                    SearchTextBox.SelectAll();
                    DynamoCommands.SearchCmd.Execute(null);
                };
        }

        public void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();
        }

    }
}
