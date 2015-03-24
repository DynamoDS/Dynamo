using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        // See OnExpanderButtonMouseLeftButtonUp for details.
        private bool ignoreMouseEnter;

        // This property is used to prevent a bug.
        // When user clicks on category it scrolls instead of category content expanding.
        // The reason is "CategoryTreeView" does not show full content because it is too
        // big. On category clicking WPF makes autoscroll and doesn't expand content of 
        // category. We are counting count of calling BringIntoViewCount() functions.        
        private int bringIntoViewCount;
        private int BringIntoViewCount
        {
            get
            {
                return bringIntoViewCount;
            }
            set
            {
                bringIntoViewCount = value >= 2 ? 2 : value;
            }
        }

        public LibraryView()
        {
            InitializeComponent();

            // Invalidate the DataContext here because it will be set at a later 
            // time through data binding expression. This way debugger will not 
            // display warnings for missing properties.
            this.DataContext = null;
        }

        /// <summary>
        /// When user tries to use mouse wheel there can be several cases.
        /// </summary>
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Case 1: User scrolls the mouse wheel when Shift key is being held down. This 
            // action triggers a horizontal scroll on the library view (so that lengthy names 
            // can be revealed). Setting 'Handled' to 'false' allows the underlying scroll bar
            // to handle the mouse wheel event.
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = false;
                return;
            }

            // Case 2: If the mouse is outside of the library view, but mouse wheel messages 
            // get sent to it anyway. In such case there is nothing to change here. The 'Handled'
            // is not set to 'true' here because the mouse wheel messages should be routed to the 
            // ScrollViewer on tool-tip for further processing.
            if (!(sender as UIElement).IsMouseOver)
                return;

            // Case 3: Mouse wheel without any modifier keys, it scrolls the library view 
            // vertically. In this case 'VerticalOffset' is updated, 'Handled' is also set 
            // so that mouse wheel message routing ends here.
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
            BringIntoViewCount++;
            var expanderContent = (sender as FrameworkElement);

            var buttons = expanderContent.ChildOfType<ListView>();
            if (buttons != null)
                buttons.UnselectAll();

            e.Handled = true;
        }

        private void OnSubCategoryListViewPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void OnMemberMouseEnter(object sender, MouseEventArgs e)
        {
            if (ignoreMouseEnter)
            {
                ignoreMouseEnter = false;
                return;
            }

            FrameworkElement fromSender = sender as FrameworkElement;
            libraryToolTipPopup.PlacementTarget = fromSender;
            libraryToolTipPopup.SetDataContext(fromSender.DataContext);
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            libraryToolTipPopup.SetDataContext(null);
        }

        private void OnTreeViewItemPreviewMouseLeftButton(object sender, MouseButtonEventArgs e)
        {
            var categoryButton = sender as TreeViewItem;
            if (!(categoryButton.DataContext is RootNodeCategoryViewModel))
                return;

            var wrapPanels = categoryButton.ChildrenOfType<LibraryWrapPanel>();
            if (!wrapPanels.Any())
                return;

            var selectedElement = e.OriginalSource as FrameworkElement;
            var selectedClass = selectedElement.DataContext as NodeCategoryViewModel;
            // Continue work with real class: not null, not ClassInformationViewModel.
            if (selectedClass == null || selectedClass is ClassInformationViewModel)
                return;

            // Go through all available for current top category LibraryWrapPanel.
            // Select class if wrapPanel contains selectedClass.
            // Unselect class in other case.
            foreach (var wrapPanel in wrapPanels)
            {
                if (wrapPanel.MakeOrClearSelection(selectedClass))
                {
                    // If class button was clicked, then handle, otherwise leave it.
                    e.Handled = selectedClass.SubCategories.Count == 0;
                    selectedElement.BringIntoView();
                }
            }

            ExpandCategory(categoryButton, selectedClass);
            e.Handled = !(selectedClass is RootNodeCategoryViewModel);
        }

        private void ExpandCategory(TreeViewItem sender, NodeCategoryViewModel selectedClass)
        {
            // Get all category items.
            var categories = sender.Items.OfType<NodeCategoryViewModel>();

            // Get all current expanded categories.
            List<NodeCategoryViewModel> allExpandedCategories = categories.
                Where(cat => (!(cat is ClassesNodeCategoryViewModel) && cat.IsExpanded == true)).ToList();

            var categoryToBeExpanded = categories.Where(cat => cat == selectedClass).FirstOrDefault();

            // If categoryToBeExpanded is null, that means not category button, but class button was clicked.
            // During loop we will find out to which category this clicked class belongs.
            if (categoryToBeExpanded != null)
                categoryToBeExpanded.IsExpanded = !categoryToBeExpanded.IsExpanded;

            // Get expanded categories that should be collapsed.
            var categoriesToBeCollapsed = allExpandedCategories.Remove(categoryToBeExpanded);

            // Close all open categories, except one, that contains class.
            // Or if category was clicked, also expand it and close others.
            foreach (var categoryToBeCollapsed in allExpandedCategories)
            {
                // If class button was clicked.
                if (categoryToBeExpanded == null)
                {
                    var categoryClasses = categoryToBeCollapsed.Items[0] as ClassesNodeCategoryViewModel;
                    // Ensure, that this class is not part of current category.
                    if (categoryClasses != null)
                        categoryToBeCollapsed.IsExpanded = categoryClasses.Items.Contains(selectedClass);
                }
                // If category button was clicked.
                else
                {
                    categoryToBeCollapsed.IsExpanded = false;
                }
            }
        }

        private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            // Because of bug we mark event as handled for all automatic requests 
            // until count of our requests less than 1. First request is done for
            // opened top category when dynamo starts.
            e.Handled = BringIntoViewCount < 2;
        }

        // Clicking on a member node results in a node being placed on the canvas.
        // Another mouse-enter event will be sent right after this left-button-up, 
        // which brings up the tool-tip. This isn't desirable, so setting a flag 
        // here to ignore the immediate mouse-enter event.
        private void OnExpanderButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Button).DataContext is NodeCategoryViewModel)
            {
                libraryToolTipPopup.SetDataContext(null, true);
                ignoreMouseEnter = true;
            }
        }

        private void OnAddButtonPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var button = sender as Button;
                var contextMenu = button.ContextMenu;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
            }

            e.Handled = true;
        }
    }
}
