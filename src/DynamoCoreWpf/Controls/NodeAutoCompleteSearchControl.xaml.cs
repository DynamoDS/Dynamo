using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for AutoCompleteSearchControl.xaml
    /// Notice this control shares a lot of logic with InCanvasSearchControl for now
    /// But they will diverge eventually because of UI improvements to auto complete.
    /// </summary>
    public partial class NodeAutoCompleteSearchControl
    {
        ListBoxItem HighlightedItem;

        internal event Action<ShowHideFlags> RequestShowNodeAutoCompleteSearch;

        public NodeAutoCompleteSearchViewModel ViewModel
        {
            get { return DataContext as NodeAutoCompleteSearchViewModel; }
        }

        public NodeAutoCompleteSearchControl()
        {
            InitializeComponent();
            if (Application.Current != null)
            {
                Application.Current.Deactivated += currentApplicationDeactivated;
            }
            Unloaded += NodeAutoCompleteSearchControl_Unloaded;
        }

        private void NodeAutoCompleteSearchControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Deactivated -= currentApplicationDeactivated;
            }
        }

        private void currentApplicationDeactivated(object sender, EventArgs e)
        {
            OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
        }

        private void OnRequestShowNodeAutoCompleteSearch(ShowHideFlags flags)
        {
            if (RequestShowNodeAutoCompleteSearch != null)
            {
                RequestShowNodeAutoCompleteSearch(flags);
            }
        }

        private void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            BindingExpression binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            if (binding != null)
                binding.UpdateSource();

            // Search the filtered results to match the user input.
            if (ViewModel != null) 
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ViewModel.SearchAutoCompleteCandidates(SearchTextBox.Text);
                }), DispatcherPriority.Loaded);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null || e.OriginalSource is Thumb) return;

            ExecuteSearchElement(listBoxItem);
            OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
            e.Handled = true;
        }

        private void ExecuteSearchElement(ListBoxItem listBoxItem)
        {
            var searchElement = listBoxItem.DataContext as NodeSearchElementViewModel;
            if (searchElement != null)
            {
                searchElement.Position = ViewModel.InCanvasSearchPosition;
                PortViewModel port = ViewModel.PortViewModel;
                if (searchElement.CreateAndConnectCommand.CanExecute(port.PortModel))
                {
                    searchElement.CreateAndConnectCommand.Execute(port.PortModel);
                    Analytics.TrackEvent(
                    Dynamo.Logging.Actions.Select,
                    Dynamo.Logging.Categories.NodeAutoCompleteOperations,
                    searchElement.FullName);
                }
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

        private void OnNodeAutoCompleteSearchControlVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If visibility  is false, then stop processing it.
            if (!(bool)e.NewValue)
                return;

            // When launching this control, always start with clear search term.
            SearchTextBox.Clear();

            Analytics.TrackEvent(
            Dynamo.Logging.Actions.Open,
            Dynamo.Logging.Categories.NodeAutoCompleteOperations);

            // Visibility of textbox changed, but text box has not been initialized(rendered) yet.
            // Call asynchronously focus, when textbox will be ready.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTextBox.Focus();
                ViewModel.PopulateAutoCompleteCandidates();
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
                    OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
                    break;
                case Key.Enter:
                    if (HighlightedItem != null && ViewModel.CurrentMode != SearchViewModel.ViewMode.LibraryView)
                    {
                        ExecuteSearchElement(HighlightedItem);
                        OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
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

        private void OnMoreInfoClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.dynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(new Uri(Dynamo.Wpf.Properties.Resources.NodeAutocompleteDocumentationUriString, UriKind.Relative)));
        }

        internal void CloseAutocompletionWindow(object sender, RoutedEventArgs e)
        {
            OnRequestShowNodeAutoCompleteSearch(ShowHideFlags.Hide);
        }
    }
}
