using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibrarySearchView.xaml
    /// </summary>
    public partial class LibrarySearchView : UserControl
    {
        private SearchViewModel viewModel;

        public LibrarySearchView()
        {
            InitializeComponent();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;

            var searchElement = listBoxItem.DataContext as SearchElementBase;
            if (searchElement != null)
            {
                searchElement.Execute();
                e.Handled = true;
            }
        }

        private void OnClassButtonCollapse(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var classButton = sender as ListViewItem;
            if ((classButton == null) || !classButton.IsSelected) return;

            classButton.IsSelected = false;
            e.Handled = true;
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
