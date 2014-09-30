using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        public LibraryView()
        {
            InitializeComponent();
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = false;
                return;
            }

            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnClassButtonCollapse(object sender, MouseButtonEventArgs e)
        {
            var classButton = sender as ListViewItem;
            if ((classButton == null) || !classButton.IsSelected) return;

            classButton.IsSelected = false;
            e.Handled = true;
        }

        /// When a category is collapsed, the selection of underlying sub-category 
        /// list is cleared. As a result any visible StandardPanel will be hidden.
        private void OnExpanderCollapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            var expanderContent = (sender as FrameworkElement);
            var buttons = Dynamo.Utilities.WPF.FindChild<ListView>(expanderContent,"");
            if (buttons != null)
                buttons.UnselectAll();
        }

    }
}
