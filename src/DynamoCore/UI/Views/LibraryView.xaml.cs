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

        /// <summary>
        /// When user tries to use mouse wheel there can be several cases.
        /// </summary>
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 1 case. 
            // User presses Shift and uses mouse wheel. That means user tries to 
            // scroll horizontally to the right side, to see the whole long name of node.
            // So, we go further, in scrollbar, that is under this one, that's why Handled is false.
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = false;
                return;
            }

            // 2 case.
            // User uses just mouse wheel. That means user wants to scroll down LibraryView.
            // So, we just change VerticalOffset, and there is no need to go further and change something.
            // Set Handled to true.
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
            var buttons = Dynamo.Utilities.WPF.FindChild<ListView>(expanderContent, "");
            if (buttons != null)
                buttons.UnselectAll();
        }

        private void OnSubCategoryListViewPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void OnMemberMouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement fromSender = sender as FrameworkElement;
            libraryToolTipPopup.PlacementTarget = fromSender;
            libraryToolTipPopup.SetDataContext(fromSender.DataContext);
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            libraryToolTipPopup.SetDataContext(null);
        }
    }
}
