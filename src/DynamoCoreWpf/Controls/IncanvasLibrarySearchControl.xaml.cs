using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;
using System;
using System.Collections.Generic;
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

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for IncanvasLibrarySearchControl.xaml
    /// </summary>
    public partial class IncanvasLibrarySearchControl : UserControl
    {
        public IncanvasLibrarySearchControl()
        {
            InitializeComponent();
        }

        private SearchViewModel viewModel
        {
            get { return DataContext as SearchViewModel; }
        }

        private void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();

            if (viewModel != null)
                viewModel.SearchCommand.Execute(null);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;
            ExecuteSearchElement(listBoxItem);
            e.Handled = true;
        }

        private void ExecuteSearchElement(ListBoxItem listBoxItem)
        {
            var searchElement = listBoxItem.DataContext as NodeSearchElementViewModel;
            if (searchElement != null)
            {
                searchElement.ClickedCommand.Execute(null);
            }
        }
    }
}
