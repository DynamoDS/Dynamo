using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Search;
using Dynamo.Utilities;

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

        private void OnNoMatchFoundButtonClick(object sender, RoutedEventArgs e)
        {
            var searchViewModel = this.DataContext as SearchViewModel;

            // Clear SearchText in ViewModel, as result search textbox clears as well.
            searchViewModel.SearchText = "";
        }

        // This event is raised only, when we can't go down, to next member.
        // I.e. we are now at the last member button and we have to move to next member group.
        private void MemberGroupsKeyDown(object sender, KeyEventArgs e)
        {
            var memberInFocus = (Keyboard.FocusedElement as ListBoxItem).Content;
            var merberGroups = (sender as ListBox).Items;

            int numberOfFocusedMemberGroup = 0;

            // Find out to which memberGroup focused member belong.
            for (int i = 0; i < merberGroups.Count; i++)
            {
                var memberGroup = merberGroups[i];
                if (memberGroup is SearchMemberGroup)
                {
                    bool memberGroupFound = false;

                    foreach (var member in (memberGroup as SearchMemberGroup).Members)
                        if (member.Equals(memberInFocus))
                        {
                            memberGroupFound = true;
                            break;
                        }

                    if (memberGroupFound)
                    {
                        numberOfFocusedMemberGroup = i;
                        break;
                    }
                }
            }

            int nextFocusedMemberGroupNumber = numberOfFocusedMemberGroup;
            // If user presses down, then we need to set focus to the next member group.
            // Otherwise to previous.
            if (e.Key == Key.Down)
                nextFocusedMemberGroupNumber++;
            if (e.Key == Key.Up)
                nextFocusedMemberGroupNumber--;

            // This case is raised, when we move out of list of member groups.
            // I.e. to class buttons list or to another category.
            // TODO: Create this functionality later.
            if (nextFocusedMemberGroupNumber < 0 || nextFocusedMemberGroupNumber > merberGroups.Count - 1) return;

            var nextFocusedMemberGroup = (sender as ListBox).ItemContainerGenerator.
                                            ContainerFromIndex(nextFocusedMemberGroupNumber) as ListBoxItem;

            var nextFocusedMembers = WPF.FindChild<ListBox>(nextFocusedMemberGroup, "MembersListBox");

            // Focus can be set to first as well as to last member.
            // If we move down, then to first one.
            // If we move up, then to last one.
            if (e.Key == Key.Down)
                (nextFocusedMembers.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem).Focus();
            if (e.Key == Key.Up)
                (nextFocusedMembers.ItemContainerGenerator.ContainerFromIndex(nextFocusedMembers.Items.Count - 1) as ListBoxItem).Focus();
            
            e.Handled = true;                                    
        }

    }
}
