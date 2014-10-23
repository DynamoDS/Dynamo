using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.Nodes.Search;
using Dynamo.Controls;
using System.Windows.Data;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibrarySearchView.xaml
    /// </summary>
    public partial class LibrarySearchView : UserControl
    {
        public LibrarySearchView()
        {
            InitializeComponent();
        }

        #region MethodButton

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

            // TODO: this focus setter should be removed in future.
            // This item may get focus just by keyboard.
            listBoxItem.Focus();
        }

        #endregion

        #region ClassButton

        private void OnClassButtonCollapse(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var classButton = sender as ListViewItem;
            if ((classButton == null) || !classButton.IsSelected) return;

            classButton.IsSelected = false;
            e.Handled = true;
        }

        private void OnClassButtonGotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = e.OriginalSource as ListViewItem;

            // We select class only, when it was selected!
            // But not, when it got focus.
            if (listViewItem != null && listViewItem.IsSelected)
                return;

            e.Handled = true;
        }

        #endregion

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnNoMatchFoundButtonClick(object sender, RoutedEventArgs e)
        {
            var searchViewModel = this.DataContext as SearchViewModel;

            // Clear SearchText in ViewModel, as result search textbox clears as well.
            searchViewModel.SearchText = "";
        }

        #region ToolTip methods

        private void OnListBoxItemMouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem fromSender = sender as ListBoxItem;
            libraryToolTipPopup.PlacementTarget = fromSender;
            libraryToolTipPopup.SetDataContext(fromSender.DataContext);
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            libraryToolTipPopup.SetDataContext(null);
        }

        #endregion

        #region Key navigation

        // Key navigation functions as bubbling scheme:
        // Category(CategoryListView) <- CategoryContent(StackPanel) <- 
        // <- SubClasses(SubCategoryListView) OR MemberGroups(MemberGroupsListBox)
        // When element can't move further, it notifies its' parent about that.
        // And then parent decides what to do with it.


        // This event is raised only, when we can't go down, to next member.
        // I.e. we are now at the last member button and we have to move to next member group.
        private void MemberGroupsKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Down) && (e.Key != Key.Up))
                return;

            var memberInFocus = (Keyboard.FocusedElement as ListBoxItem).Content as BrowserInternalElement;
            var memberGroups = (sender as ListBox).Items;
            var memberGroupListBox = sender as ListBox;

            int focusedMemberGroupIndex = 0;

            // Find out to which memberGroup focused member belong.
            for (int i = 0; i < memberGroups.Count; i++)
            {
                var memberGroup = memberGroups[i] as SearchMemberGroup;
                if (memberGroup.ContainsMember(memberInFocus))
                {
                    focusedMemberGroupIndex = i;
                    break;
                }
            }

            int nextFocusedMemberGroupIndex = focusedMemberGroupIndex;
            // If user presses down, then we need to set focus to the next member group.
            // Otherwise to previous.
            if (e.Key == Key.Down)
                nextFocusedMemberGroupIndex++;
            if (e.Key == Key.Up)
                nextFocusedMemberGroupIndex--;

            // The member group list box does not attempt to process the key event if it 
            // has moved beyond its available list of member groups. In this case, the 
            // key event is considered not handled and will be left to the parent visual 
            // (e.g. class button or another category) to handle.
            e.Handled = false;
            if (nextFocusedMemberGroupIndex < 0 || nextFocusedMemberGroupIndex > memberGroups.Count - 1)
                return;

            var generator = memberGroupListBox.ItemContainerGenerator;
            var item = generator.ContainerFromIndex(nextFocusedMemberGroupIndex) as ListBoxItem;

            var nextFocusedMembers = WPF.FindChild<ListBox>(item, "MembersListBox");

            // When moving on to the next member group list below (by pressing down arrow),
            // the focus should moved on to the first member in the member group list. Likewise,
            // when moving to the previous member group list above, the focus should be set on 
            // the last member in that list.
            var itemIndex = 0;
            if (e.Key == Key.Up)
                itemIndex = nextFocusedMembers.Items.Count - 1;

            generator = nextFocusedMembers.ItemContainerGenerator;
            (generator.ContainerFromIndex(itemIndex) as ListBoxItem).Focus();

            e.Handled = true;
        }

        // The 'StackPanel' that contains 'SubCategoryListView' and 'MemberGroupsListBox'
        // handles this message. If none of these two list boxes are handling the key 
        // message, that means the currently focused list box item is the first/last item
        // in these two list boxes. When key message arrives here, it is then the 'StackPanel'
        // responsibility to move the focus on to the adjacent list box.
        private void OnCategoryContentKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Down) && (e.Key != Key.Up))
                return;

            // Member in focus(in this scenario) can be only first/last member button or first/last class button.
            var memberInFocus = Keyboard.FocusedElement as FrameworkElement;
            var searchCategoryElement = sender as FrameworkElement;

            // memberInFocus is method button.
            if (memberInFocus.DataContext is NodeSearchElement)
            {
                var searchCategoryContent = searchCategoryElement.DataContext as SearchCategory;

                // Gotten here because of last method being listed, pressing 'down' array cannot 
                // move down further. Return here to allow higher level visual element to handle
                // the navigation (to a separate category).
                if (e.Key == Key.Down)
                    return;

                // Otherwise, pressed Key is Up.

                // No class is found in this 'SearchCategory', return from here so that higher level
                // element gets to handle the navigational keys to move focus to the previous category.
                if (searchCategoryContent.Classes.Count == 0)
                    return;

                // Otherwise, we move to first class button.
                var listItem = FindFirstChildListItem(searchCategoryElement, "SubCategoryListView");
                if (listItem != null)
                    listItem.Focus();

                e.Handled = true;
                return;
            }

            // memberInFocus is class button.
            if (memberInFocus.DataContext is BrowserInternalElement)
            {
                // We are at the first row of class list. User presses up, we have to move to previous category.
                // We handle it further.
                if (e.Key == Key.Up)
                    return;

                // Otherwise user pressed down, we have to move to first member button.
                var memberGroupsListBox = WPF.FindChild<ListBox>(searchCategoryElement, "MemberGroupsListBox");
                var listItem = FindFirstChildListItem(memberGroupsListBox, "MembersListBox");
                if (listItem != null)
                    listItem.Focus();

                e.Handled = true;
                return;
            }
        }


        private ListBoxItem FindFirstChildListItem(FrameworkElement parent, string listName)
        {
            var list = WPF.FindChild<ListBox>(parent, listName);
            var generator = list.ItemContainerGenerator;
            return generator.ContainerFromIndex(0) as ListBoxItem;
        }

        #endregion

        private void OnMemberGroupNameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is TextBlock)) return;

            var memberGroup = sender as FrameworkElement;
            var memberGroupContext = memberGroup.DataContext as SearchMemberGroup;

            // Show all members of this group.
            memberGroupContext.ExpandAllMembers();
 
            // Make textblock underlined.
            var textBlock = e.OriginalSource as TextBlock;
            textBlock.TextDecorations = TextDecorations.Underline;
        }

        private void OnPrefixTextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }


    }
}
