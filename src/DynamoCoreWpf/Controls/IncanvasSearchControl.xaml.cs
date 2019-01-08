using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for IncanvasLibrarySearchControl.xaml
    /// </summary>
    public partial class InCanvasSearchControl
    {
        ListBoxItem HighlightedItem;

        internal event Action<ShowHideFlags> RequestShowInCanvasSearch;

        public SearchViewModel ViewModel
        {
            get { return DataContext as SearchViewModel; }
        }

        public InCanvasSearchControl()
        {
            InitializeComponent();
            if (Application.Current != null)
            {
                Application.Current.Deactivated += currentApplicationDeactivated;
            }
            Unloaded += InCanvasSearchControl_Unloaded; ;

        }

        private void InCanvasSearchControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Deactivated -= currentApplicationDeactivated;
            }
        }

        private void currentApplicationDeactivated(object sender, EventArgs e)
        {
            OnRequestShowInCanvasSearch(ShowHideFlags.Hide);
        }

        private void OnRequestShowInCanvasSearch(ShowHideFlags flags)
        {
            if (RequestShowInCanvasSearch != null)
            {
                RequestShowInCanvasSearch(flags);
            }
        }

        private void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();

            if (ViewModel != null)
                ViewModel.SearchCommand.Execute(null);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null || e.OriginalSource is Thumb) return;

            ExecuteSearchElement(listBoxItem);
            OnRequestShowInCanvasSearch(ShowHideFlags.Hide);
            e.Handled = true;
        }

        private void ExecuteSearchElement(ListBoxItem listBoxItem)
        {
            var searchElement = listBoxItem.DataContext as NodeSearchElementViewModel;
            if (searchElement != null)
            {
                searchElement.Position = ViewModel.InCanvasSearchPosition;
                searchElement.ClickedCommand.Execute(null);
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement fromSender = sender as FrameworkElement;
            if (fromSender == null) return;

            toolTipPopup.DataContext = fromSender.DataContext;
            toolTipPopup.IsOpen = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            toolTipPopup.DataContext = null;
            toolTipPopup.IsOpen = false;
        }

        private void OnInCanvasSearchControlVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If visibility  is false, then stop processing it.
            if (!(bool)e.NewValue)
                return;

            // Select text in text box.
            SearchTextBox.SelectAll();

            // Visibility of textbox changed, but text box has not been initialized(rendered) yet.
            // Call asynchronously focus, when textbox will be ready.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTextBox.Focus();
            }), DispatcherPriority.Loaded);
        }

        private void OnMembersListBoxUpdated(object sender, DataTransferEventArgs e)
        {
            var membersListBox = sender as ListBox;
            // As soon as listbox renders, select first member.
            membersListBox.ItemContainerGenerator.StatusChanged += OnMembersListBoxIcgStatusChanged;
        }

        private void OnMembersListBoxIcgStatusChanged(object sender, EventArgs e)
        {
            if (MembersListBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                MembersListBox.ItemContainerGenerator.StatusChanged -= OnMembersListBoxIcgStatusChanged;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var scrollViewer = MembersListBox.ChildOfType<ScrollViewer>();
                    scrollViewer.ScrollToTop();

                    UpdateHighlightedItem(GetListItemByIndex(MembersListBox, 0));
                }),
                    DispatcherPriority.Loaded);
            }
        }

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
                HighlightedItem.BringIntoView();
            }
        }

        private ListBoxItem GetListItemByIndex(ListBox parent, int index)
        {
            if (parent.Equals(null)) return null;

            var generator = parent.ItemContainerGenerator;
            if ((index >= 0) && (index < parent.Items.Count))
                return generator.ContainerFromIndex(index) as ListBoxItem;

            return null;
        }

        private void OnInCanvasSearchKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            int index;
            var members = MembersListBox.Items.Cast<NodeSearchElementViewModel>();
            NodeSearchElementViewModel highlightedMember = null;
            if (HighlightedItem != null)
                highlightedMember = HighlightedItem.DataContext as NodeSearchElementViewModel;

            switch (key)
            {
                case Key.Escape:
                    OnRequestShowInCanvasSearch(ShowHideFlags.Hide);
                    break;
                case Key.Enter:
                    if (HighlightedItem != null && ViewModel.CurrentMode != SearchViewModel.ViewMode.LibraryView)
                    {
                        ExecuteSearchElement(HighlightedItem);
                        OnRequestShowInCanvasSearch(ShowHideFlags.Hide);
                    }
                    break;
                case Key.Up:
                    index = MoveToNextMember(false, members, highlightedMember);
                    UpdateHighlightedItem(GetListItemByIndex(MembersListBox, index));
                    break;
                case Key.Down:
                    index = MoveToNextMember(true, members, highlightedMember);
                    UpdateHighlightedItem(GetListItemByIndex(MembersListBox, index));
                    break;
            }
        }

        internal int MoveToNextMember(bool moveForward,
            IEnumerable<NodeSearchElementViewModel> members, NodeSearchElementViewModel selectedMember)
        {
            int selectedMemberIndex = -1;
            for (int i = 0; i < members.Count(); i++)
            {
                var member = members.ElementAt(i);
                if (member.Equals(selectedMember))
                {
                    selectedMemberIndex = i;
                    break;
                }
            }

            int nextselectedMemberIndex = selectedMemberIndex;
            if (moveForward)
                nextselectedMemberIndex++;
            else
                nextselectedMemberIndex--;

            if (nextselectedMemberIndex < 0 || (nextselectedMemberIndex >= members.Count()))
                return selectedMemberIndex;

            return nextselectedMemberIndex;
        }

        private void OnMembersListBoxMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var listBox = sender as FrameworkElement;
            if (listBox == null)
                return;

            var scrollViewer = listBox.ChildOfType<ScrollViewer>();
            if (scrollViewer == null)
                return;

            // Make delta less to achieve smooth scrolling and not jump over other elements.
            var delta = e.Delta / 100;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - delta);
            // do not propagate to child items with scrollable content
            e.Handled = true;
        }
    }
}
