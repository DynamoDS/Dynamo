using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibrarySearchView.xaml
    /// </summary>
    public partial class LibrarySearchView : UserControl
    {
        public UIElement SearchTextBox;
        private ListBoxItem HighlightedItem;
        private SearchViewModel viewModel;


        public LibrarySearchView()
        {
            InitializeComponent();

            // Invalidate the DataContext here because it will be set at a later 
            // time through data binding expression. This way debugger will not 
            // display warnings for missing properties.
            this.DataContext = null;

            Loaded += OnLibrarySearchViewLoaded;
        }

        private void OnLibrarySearchViewLoaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as SearchViewModel;
            viewModel.SearchTextChanged += OnSearchTextBoxKeyDown;

            // RequestReturnFocusToSearch calls, when workspace was clicked.
            // We should hide tooltip.
            viewModel.RequestReturnFocusToSearch += OnRequestCloseToolTip;
            // When workspace was changed, we should hide tooltip. 
            viewModel.RequestCloseSearchToolTip += OnRequestCloseToolTip;
        }

        // Changing text content of the search box should always bring up the 
        // "top result" (since there is no search term). Also, any possible tool-tip
        // that is displayed and highlighted item should be dismissed right away.
        private void OnSearchTextBoxKeyDown(object sender, EventArgs e)
        {
            topResultPanel.BringIntoView();

            if (string.IsNullOrEmpty(viewModel.SearchText))
            {
                UpdateHighlightedItem(null);
                CloseToolTipInternal(true);
            }
        }

        #region MethodButton

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;
            ExecuteSearchElement(listBoxItem);
            e.Handled = true;
        }

        private void OnMemberButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null)
            {
                // Case for top result.
                // Top result can have just one selected item.
                listBoxItem = GetListItemByIndex(sender as ListBox, 0);
                if (listBoxItem == null) return;
            }

            ExecuteSearchElement(listBoxItem);
            e.Handled = true;
        }

        private void ExecuteSearchElement(ListBoxItem listBoxItem)
        {
            var searchElement = listBoxItem.DataContext as NodeSearchElementViewModel;
            if (searchElement != null)
            {
                searchElement.ClickedCommand.Execute(null);
                CloseToolTipInternal(true);
            }
        }

        #endregion

        #region ClassButton

        private void OnClassButtonCollapse(object sender, MouseButtonEventArgs e)
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

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnNoMatchFoundButtonClick(object sender, RoutedEventArgs e)
        {
            // Clear SearchText in ViewModel, as result search textbox clears as well.
            viewModel.SearchText = "";
        }

        private void OnMemberGroupNameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is System.Windows.Documents.Run)) return;

            var memberGroup = sender as FrameworkElement;
            var memberGroupContext = memberGroup.DataContext as SearchMemberGroup;

            // Show all members of this group.
            memberGroupContext.ExpandAllMembers();

            // Make textblock underlined.
            var textBlock = e.OriginalSource as System.Windows.Documents.Run;
            textBlock.TextDecorations = TextDecorations.Underline;
        }

        private void OnPrefixTextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        #region ToolTip methods

        private void OnListBoxItemMouseEnter(object sender, MouseEventArgs e)
        {
            UpdateHighlightedItem(sender as ListBoxItem);
        }

        private void OnListBoxItemGotFocus(object sender, RoutedEventArgs e)
        {
            ShowTooltip(sender);
        }

        private void OnPopupMouseLeave(object sender, MouseEventArgs e)
        {
            UpdateHighlightedItem(null);
            CloseToolTipInternal();
        }

        private void OnListBoxItemLostFocus(object sender, RoutedEventArgs e)
        {
            // Hide tooltip immediately.
            CloseToolTipInternal(true);
        }

        private void ShowTooltip(object sender)
        {
            ListBoxItem fromSender = sender as ListBoxItem;
            if (fromSender == null) return;

            libraryToolTipPopup.PlacementTarget = fromSender;
            if ((fromSender.DataContext as NodeSearchElementViewModel).Visibility)
                libraryToolTipPopup.SetDataContext(fromSender.DataContext);
            else
                CloseToolTipInternal();
        }

        private void CloseToolTipInternal(bool closeImmediately = false)
        {
            libraryToolTipPopup.SetDataContext(null, closeImmediately);
        }

        private void OnRequestCloseToolTip(object sender, EventArgs e)
        {
            CloseToolTipInternal(true);
        }

        #endregion

        #region Key navigation

        // Key navigation functions as bubbling scheme:
        // Category(CategoryListView) <- CategoryContent(StackPanel) <- 
        // <- SubClasses(SubCategoryListView) OR MemberGroups(MemberGroupsListBox)
        // When element can't move further, it notifies its' parent about that.
        // And then parent decides what to do with it.

        private void UpdateHighlightedItem(ListBoxItem newItem)
        {
            if (HighlightedItem == newItem)
                return;

            // Unselect old value.
            if (HighlightedItem != null)
                HighlightedItem.IsSelected = false;

            HighlightedItem = newItem;

            // Select new value.
            if (HighlightedItem != null)
            {
                HighlightedItem.IsSelected = true;
                // If HighlightedItem is not visible for user, bring it and 5 next items into view.
                HighlightedItem.BringIntoView(new Rect(0, 0, 0, HighlightedItem.ActualHeight * 5));
                ShowTooltip(HighlightedItem);
            }
        }

        // Generates keydown event on currently selected item.
        // Only Up, Down and Enter buttons are supported.
        public void SelectNext(Key key)
        {
            if (key != Key.Up && key != Key.Down && key != Key.Enter)
                return;

            PresentationSource target;
            // For the first time set top result as HighlightedItem. 
            if (HighlightedItem == null)
            {
                UpdateHighlightedItem(GetListItemByIndex(topResultListBox, 0));
            }
            if (HighlightedItem == null) return;

            target = PresentationSource.FromVisual(HighlightedItem);
            if (target == null)
            {
                // During search, backing data collections typically get updated frequently.
                // This may result in corresponding NodeSearchElementViewModel being removed or 
                // updated. When that happens, the visual element 'HighlightedItem' that gets 
                // bound to the original NodeSearchElementViewModel then becomes DisconnectedItem.
                // In such cases, we will reset the HighlightedItem to 'topResultListBox'.
                HighlightedItem = GetSelectedListBoxItem(topResultListBox);
                if (HighlightedItem == null) return;

                target = PresentationSource.FromVisual(HighlightedItem);
            }

            var routedEvent = Keyboard.KeyDownEvent; // Event to send



            HighlightedItem.RaiseEvent(new KeyEventArgs(
                Keyboard.PrimaryDevice, target, 0, key)
                {
                    RoutedEvent = routedEvent
                });
        }

        private ListBoxItem GetSelectedListBoxItem(ListBox listbox)
        {
            if (!listbox.HasItems || (listbox.SelectedIndex == -1))
                return null;

            var generator = listbox.ItemContainerGenerator;
            return generator.ContainerFromItem(listbox.SelectedItem) as ListBoxItem;
        }

        // This event is used to move inside members.
        private void OnMembersListBoxKeyDown(object sender, KeyEventArgs e)
        {
            // For hovered by mouse item do not navigate.
            if (HighlightedItem.IsMouseOver)
            {
                e.Handled = true;
                return;
            }

            var selectedMember = HighlightedItem.DataContext as NodeSearchElementViewModel;
            var membersListBox = sender as ListBox;
            var members = membersListBox.Items;

            int selectedMemberIndex = 0;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i] as NodeSearchElementViewModel;
                if (member.Equals(selectedMember))
                {
                    selectedMemberIndex = i;
                    break;
                }
            }

            int nextselectedMemberIndex = selectedMemberIndex;
            if (e.Key == Key.Down)
                nextselectedMemberIndex++;
            if (e.Key == Key.Up)
                nextselectedMemberIndex--;

            if (nextselectedMemberIndex < 0 || (nextselectedMemberIndex >= members.Count))
                return;

            UpdateHighlightedItem(GetListItemByIndex(membersListBox, nextselectedMemberIndex));
            e.Handled = true;

        }

        // This event is raised only, when we can't go down, to next member.
        // I.e. we are now at the last member button and we have to move to next member group.
        private void MemberGroupsKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Down) && (e.Key != Key.Up))
                return;

            var selectedMember = HighlightedItem.DataContext as NodeSearchElementViewModel;
            var memberGroups = (sender as ListBox).Items;
            var memberGroupListBox = sender as ListBox;

            int selectedMemberGroupIndex = 0;

            // Find out to which memberGroup selected member belong.
            for (int i = 0; i < memberGroups.Count; i++)
            {
                var memberGroup = memberGroups[i] as SearchMemberGroup;
                if (memberGroup.ContainsMember(selectedMember))
                {
                    selectedMemberGroupIndex = i;
                    break;
                }
            }

            int nextSelectedMemberGroupIndex = selectedMemberGroupIndex;
            // If user presses down, then we need to set focus to the next member group.
            // Otherwise to previous.
            if (e.Key == Key.Down)
                nextSelectedMemberGroupIndex++;
            if (e.Key == Key.Up)
                nextSelectedMemberGroupIndex--;

            // The member group list box does not attempt to process the key event if it 
            // has moved beyond its available list of member groups. In this case, the 
            // key event is considered not handled and will be left to the parent visual 
            // (e.g. class button or another category) to handle.
            e.Handled = false;
            if (nextSelectedMemberGroupIndex < 0 || (nextSelectedMemberGroupIndex >= memberGroups.Count))
                return;

            var item = GetListItemByIndex(memberGroupListBox, nextSelectedMemberGroupIndex);
            var nextSelectedMembers = WpfUtilities.ChildOfType<ListBox>(item, "MembersListBox");

            // When moving on to the next member group list below (by pressing down arrow),
            // the focus should moved on to the first member in the member group list. Likewise,
            // when moving to the previous member group list above, the focus should be set on 
            // the last member in that list.
            var itemIndex = 0;
            if (e.Key == Key.Up)
                itemIndex = nextSelectedMembers.Items.Count - 1;


            UpdateHighlightedItem(GetListItemByIndex(nextSelectedMembers, itemIndex));

            e.Handled = true;
        }

        // The 'StackPanel' that contains 'SubCategoryListView' and 'MemberGroupsListBox'
        // handles this message. If none of these two list boxes are handling the key 
        // message, that means the currently selected list box item is the first/last item
        // in these two list boxes. When key message arrives here, it is then the 'StackPanel'
        // responsibility to move the selection on to the adjacent list box.
        private void OnCategoryContentKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Down) && (e.Key != Key.Up))
                return;

            // Selected member(in this scenario) can be only first/last member button or first/last class button.
            var selectedMember = HighlightedItem as FrameworkElement;
            var searchCategoryElement = sender as FrameworkElement;

            // selectedMember is method button.
            if (selectedMember.DataContext is NodeSearchElementViewModel)
            {
                var searchCategoryContent = searchCategoryElement.DataContext as SearchCategory;

                // Gotten here because of last method being listed, pressing 'down' array cannot 
                // move down further. Return here to allow higher level visual element to handle
                // the navigation (to a separate category).
                if (e.Key == Key.Down)
                    return;

                // Otherwise, pressed Key is Up.

#if SEARCH_SHOW_CLASSES
                // No class is found in this 'SearchCategory', return from here so that higher level
                // element gets to handle the navigational keys to move focus to the previous category.
                if (searchCategoryContent.Classes.Count == 0)
                    return;

                // Otherwise, we move to first class button.
                var listItem = FindFirstChildListItem(searchCategoryElement, "SubCategoryListView");
                if (listItem != null)
                    listItem.Focus();

                e.Handled = true;
#endif

                return;
            }

            // selectedMember is class button.
            if (selectedMember.DataContext is NodeCategoryViewModel)
            {
                // We are at the first row of class list. User presses up, we have to move to previous category.
                // We handle it further.
                if (e.Key == Key.Up)
                    return;

                // Otherwise user pressed down, we have to move to first member button.
                var memberGroupsListBox = WpfUtilities.ChildOfType<ListBox>(searchCategoryElement, "MemberGroupsListBox");
                var listItem = FindChildListItemByIndex(memberGroupsListBox, "MembersListBox");
                if (listItem != null)
                {
                    UpdateHighlightedItem(listItem);
                }

                e.Handled = true;
                return;
            }
        }

        private ListBoxItem FindFirstVisibleCategory(FrameworkElement librarySearchViewElement)
        {
            var firstCategory = FindChildListItemByIndex(librarySearchViewElement, "CategoryListView");

            int index = 1;
            while (firstCategory != null &&
                !WpfUtilities.ChildOfType<Expander>(firstCategory, string.Empty).IsExpanded)
            {
                firstCategory = FindChildListItemByIndex(librarySearchViewElement, "CategoryListView", index);
                index++;
            }

            return firstCategory;
        }

        private ListBoxItem FindChildListItemByIndex(FrameworkElement parent, string listName, int index = 0)
        {
            var list = WpfUtilities.ChildOfType<ListBox>(parent, listName);
            var generator = list.ItemContainerGenerator;
            if (0 <= index && index < list.Items.Count)
                return generator.ContainerFromIndex(index) as ListBoxItem;
            else
                return null;
        }

        /// <summary>
        /// 'CategoryListView' element contains the following child elements (either 
        /// directly, or indirectly nested): 'StackPanel', 'SubCategoryListView',
        /// 'MemberGroupsListBox' and 'MembersListBox'. If none of these child elements 
        /// choose to process the key event, it gets bubbled up here. This typically 
        /// happens for the following scenarios:
        /// 
        /// 1. Down key is pressed when selection is on last entry of 'MembersListBox'
        /// 2. Up key is pressed when selection is on item on first row of 'SubCategoryListView'
        /// 3. Up key is pressed when selection is on the first entry of 'MembersListBox'
        ///    and there are no classes.
        /// 
        /// </summary>
        private void OnCategoryKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Down) && (e.Key != Key.Up))
                return;

            // Selected member(in this scenario) can be only first/last member button or class button at the first row.
            var selectedMember = HighlightedItem;
            var selectedMemberContext = selectedMember.DataContext as NodeSearchElementViewModel;
            var categoryListView = sender as ListView;

            int categoryIndex = 0;
            for (int i = 0; i < categoryListView.Items.Count; i++)
            {
                var category = categoryListView.Items[i] as SearchCategory;
                if (category.ContainsClassOrMember(selectedMemberContext.Model))
                {
                    categoryIndex = i;
                    break;
                }
            }

            if (e.Key == Key.Down)
                categoryIndex++;
            if (e.Key == Key.Up)
                categoryIndex--;

            // The selection cannot be moved further up, returning here without handling the key event 
            // so that parent visual element gets to handle it and move selection up to 'Top Result' list.
            if (categoryIndex < 0) return;
            // We are at the last member and there is no way to move down.
            if (categoryIndex >= categoryListView.Items.Count)
            {
                e.Handled = true;
                return;
            }

            var nextSelectedCategory = GetVisibleCategory(categoryListView, categoryIndex, e.Key);
            if (nextSelectedCategory == null)
            {
                e.Handled = e.Key == Key.Down;
                return;
            }

            if (e.Key == Key.Up)
            {
                var memberGroupsList = WpfUtilities.ChildOfType<ListBox>(nextSelectedCategory, "MemberGroupsListBox");
                var lastMemberGroup = GetListItemByIndex(memberGroupsList, memberGroupsList.Items.Count - 1);
                var membersList = WpfUtilities.ChildOfType<ListBox>(lastMemberGroup, "MembersListBox");

                // If key is up, then we have to select the last method button.
                UpdateHighlightedItem(GetListItemByIndex(membersList, membersList.Items.Count - 1));
            }
            else // Otherwise, Down was pressed, and we have to select first class/method button.
            {
#if SEARCH_SHOW_CLASSES
                if (nextselectedCategoryContent.Classes.Count > 0)
                {
                    // If classes are presented, then focus on first class.
                    FindFirstChildListItem(nextselectedCategory, "SubCategoryListView").Focus();
                }
                else
                {
                    // If there are no classes, then focus on first method.
                    var memberGroupsList = FindFirstChildListItem(nextselectedCategory, "MemberGroupsListBox");
                    FindFirstChildListItem(memberGroupsList, "MembersListBox").Focus();
                }
#else
                // If there are no classes, then focus on first method.
                var memberGroupsList = FindChildListItemByIndex(nextSelectedCategory, "MemberGroupsListBox");
                UpdateHighlightedItem(FindChildListItemByIndex(memberGroupsList, "MembersListBox"));
#endif
            }
            e.Handled = true;
        }

        /// <summary>
        /// "MainGrid" contains both "topResultListBox" and "CategoryListView". When 
        /// "KeyDown" event bubbles up to the level of "MainGrid", it will then decide 
        /// on which element should the focus go (depending on the navigational key).
        /// This typically happens during the following scenarios:
        /// 
        /// 1. Up/down key is pressed when selected item is on "topResultListBox"
        /// 2. Up key is pressed when selected item is on first row of first category
        /// </summary>
        private void OnMainGridKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Down) && (e.Key != Key.Up))
                return;
            var librarySearchViewElement = sender as FrameworkElement;

            // We are at the top result. If down was pressed, 
            // that means we have to move to first class/method button.
            if (e.Key == Key.Down)
            {
                if (topResultListBox.IsMouseOver)
                {
                    e.Handled = true;
                    return;
                }

                //Unselect top result.
                if (e.OriginalSource is ListBox)
                    (e.OriginalSource as ListBox).UnselectAll();

                var firstCategory = FindFirstVisibleCategory(librarySearchViewElement);
                if (firstCategory == null)
                {
                    e.Handled = true;
                    return;
                }
#if SEARCH_SHOW_CLASSES
                var firstCategoryContent = firstCategory.Content as SearchCategory;
                // If classes presented, set focus on the first class button.
                if (firstCategoryContent.Classes.Count > 0)
                {
                    FindFirstChildListItem(firstCategory, "SubCategoryListView").Focus();
                    e.Handled = true;
                    return;
                }
#endif
                // Otherwise, set selection on the first method button.
                var firstMemberGroup = FindChildListItemByIndex(firstCategory, "MemberGroupsListBox");
                UpdateHighlightedItem(FindChildListItemByIndex(firstMemberGroup, "MembersListBox"));
            }
            else // Otherwise, Up was pressed. So, we have to move to top result.
            {
                UpdateHighlightedItem(FindChildListItemByIndex(this, "topResultListBox"));
            }

            e.Handled = true;
        }

        private static ListBoxItem GetVisibleCategory(ListBox parent, int startIndex, Key key)
        {
            if (parent.Equals(null)) return null;

            var index = startIndex;
            var generator = parent.ItemContainerGenerator;
            var category = generator.ContainerFromIndex(index) as ListBoxItem;

            while (category != null &&
                !WpfUtilities.ChildOfType<Expander>(category, string.Empty).IsExpanded)
            {
                if (key == Key.Down)
                    index++;
                if (key == Key.Up)
                    index--;

                if (0 <= index && index < parent.Items.Count)
                    category = generator.ContainerFromIndex(index) as ListBoxItem;
                else
                    category = null;
            }

            return category;
        }

        private ListBoxItem GetListItemByIndex(ListBox parent, int index)
        {
            if (parent.Equals(null)) return null;

            var generator = parent.ItemContainerGenerator;
            if ((index >= 0) && (index < parent.Items.Count))
                return generator.ContainerFromIndex(index) as ListBoxItem;

            return null;
        }

        // Everytime, when top result is updated, we have to select one first item.
        private void OnTopResultTargetUpdated(object sender, DataTransferEventArgs e)
        {
            // If we turn to regular view, we have to hide tooltip immediately.
            if (viewModel.CurrentMode != SearchViewModel.ViewMode.LibrarySearchView)
            {
                CloseToolTipInternal(true);
                UpdateHighlightedItem(null);
                return;
            }

            if (topResultListBox.Items.Count > 0)
            {
                // Update highlighted item when the ItemContainerGenerator is ready.
                topResultListBox.ItemContainerGenerator.StatusChanged += OnTopResultListBoxIcgStatusChanged;
            }
            else
            {
                // Or hide ToolTip if topResultListBox is empty.
                CloseToolTipInternal(true);
                UpdateHighlightedItem(null);
            }
        }

        // ListBox.ItemContainerGenerator works asynchronously. To make sure it is ready for use
        // we check status of it. If status is correct HighlightedItem updated. 
        private void OnTopResultListBoxIcgStatusChanged(object sender, EventArgs e)
        {
            if (topResultListBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                topResultListBox.ItemContainerGenerator.StatusChanged -= OnTopResultListBoxIcgStatusChanged;
                Dispatcher.BeginInvoke(new Action(() =>
                    {
                        UpdateHighlightedItem(GetListItemByIndex(topResultListBox, 0));
                    }),
                    DispatcherPriority.Loaded);
            }
        }

        #endregion

        // As soon as user hover on TopResult HighlightedIten should be updated to it.
        private void OnTopResultMouseEnter(object sender, MouseEventArgs e)
        {
            UpdateHighlightedItem(GetListItemByIndex(topResultListBox, 0));
        }

        // As soon as user goes out of TopResult HighlightedIten should set to null
        // because nothing is selected.
        private void OnTopResultMouseLeave(object sender, MouseEventArgs e)
        {
            UpdateHighlightedItem(null);
            CloseToolTipInternal();
        }

        // User collapsed a category. Function checks if HighlightedItem inside and deselect it.
        private void OnCategoryExpanderCollapsed(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Focus();

            if (HighlightedItem == null)
                return;

            var categoryToCollapse = (sender as Expander).DataContext as SearchCategory;
            var element = HighlightedItem.DataContext as NodeSearchElementViewModel;
            // When member belongs to collapsed category HighlightedItem should be set to null.
            if (categoryToCollapse.MemberGroups.Any(mg => mg.Members.Contains(element)))
            {
                UpdateHighlightedItem(null);
                CloseToolTipInternal(true);
            }
        }

        private void OnCategoryExpanderExpanded(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Focus();
        }
    }
}
