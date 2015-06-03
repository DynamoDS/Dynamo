using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for ClassInformationView.xaml
    /// </summary>
    public partial class ClassInformationView : UserControl
    {
        private const int TruncatedMembersCount = 5;
        private ClassInformationViewModel castedDataContext;

        public bool FocusItemOnSelection { get; set; }

        public ClassInformationView()
        {
            InitializeComponent();

            secondaryHeaderStrip.HeaderActivated += OnHeaderButtonClick;
        }

        private void OnHeaderButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (sender as FrameworkElement).DataContext as HeaderStripItem;
            if (selectedItem.Text == Configurations.HeaderAction)
            {
                castedDataContext.CurrentDisplayMode = ClassInformationViewModel.DisplayMode.Action;
                secondaryMembers.ItemsSource = castedDataContext.ActionMembers;
            }

            if (selectedItem.Text == Configurations.HeaderQuery)
            {
                castedDataContext.CurrentDisplayMode = ClassInformationViewModel.DisplayMode.Query;
                secondaryMembers.ItemsSource = castedDataContext.QueryMembers;
            }

            TruncateSecondaryMembers();

            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;

            ExecuteSearchElement(listBoxItem);
            e.Handled = true;

            if (FocusItemOnSelection)
                listBoxItem.Focus();
        }

        private void OnMemberButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

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
                libraryToolTipPopup.SetDataContext(null, true);
            }
        }

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

        private void GridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            castedDataContext = this.DataContext as ClassInformationViewModel;
            if (castedDataContext == null)
                return;

            bool hasCreateMembers = castedDataContext.CreateMembers.Any();
            bool hasActionMembers = castedDataContext.ActionMembers.Any();
            bool hasQueryMembers = castedDataContext.QueryMembers.Any();

            primaryHeaderStrip.HeaderStripItems = castedDataContext.PrimaryHeaderItems;
            secondaryHeaderStrip.HeaderStripItems = castedDataContext.SecondaryHeaderItems;

            castedDataContext.CurrentDisplayMode = ClassInformationViewModel.DisplayMode.None;

            castedDataContext.HiddenSecondaryMembersCount = 0;

            // Case when CreateMembers list is not empty.
            // We should present CreateMembers in primaryMembers.            
            if (hasCreateMembers)
            {
                primaryMembers.ItemsSource = castedDataContext.CreateMembers;

                if (hasActionMembers)
                {
                    // "Action" members available.
                    castedDataContext.CurrentDisplayMode = ClassInformationViewModel.DisplayMode.Action;

                    secondaryMembers.ItemsSource = castedDataContext.ActionMembers;
                }
                else if (hasQueryMembers)
                {
                    // No "Action" members but "Query" members are available.
                    castedDataContext.CurrentDisplayMode = ClassInformationViewModel.DisplayMode.Query;

                    secondaryMembers.ItemsSource = castedDataContext.QueryMembers;
                }

                TruncateSecondaryMembers();
                return;
            }

            // Case when CreateMembers list is empty and ActionMembers list isn't empty.
            // ActionMembers will be presented in primaryMembers.
            // Depending on availibility of QueryMembers it will be shown as secondaryHeaderLeft.
            if (hasActionMembers)
            {
                primaryMembers.ItemsSource = castedDataContext.ActionMembers;

                if (hasQueryMembers)
                {
                    castedDataContext.CurrentDisplayMode = ClassInformationViewModel.DisplayMode.Query;

                    secondaryMembers.ItemsSource = castedDataContext.QueryMembers;
                }

                TruncateSecondaryMembers();
                return;
            }

            // Case when CreateMembers and ActionMembers lists are empty.
            // If QueryMembers is not empty the list will be presented in primaryMembers. 
            if (hasQueryMembers)
            {
                primaryMembers.ItemsSource = castedDataContext.QueryMembers;
            }
        }

        private void OnMoreButtonClick(object sender, RoutedEventArgs e)
        {
            IEnumerable<NodeSearchElementViewModel> collection = castedDataContext.ActionMembers;
            if (castedDataContext.CurrentDisplayMode == ClassInformationViewModel.DisplayMode.Query)
                collection = castedDataContext.QueryMembers;

            secondaryMembers.ItemsSource = collection;

            castedDataContext.HiddenSecondaryMembersCount = 0;
        }

        private void TruncateSecondaryMembers()
        {
            if (castedDataContext.CurrentDisplayMode == ClassInformationViewModel.DisplayMode.None)
                return;

            IEnumerable<NodeSearchElementViewModel> collection = castedDataContext.ActionMembers;
            if (castedDataContext.CurrentDisplayMode == ClassInformationViewModel.DisplayMode.Query)
                collection = castedDataContext.QueryMembers;

            secondaryMembers.ItemsSource = collection.Take(TruncatedMembersCount);

            castedDataContext.HiddenSecondaryMembersCount = collection.Count() - TruncatedMembersCount;
        }

        // This method will work only, when user presses Shift.
        private void OnShiftMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta);
        }

        // Main grid in "ClassInformationView" contains of 2 lists: primary members and secondary members.
        // When, these is no way to move inside one of these lists, main grid decides where focus
        // should move next.
        private void OnMainGridKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key != Key.Down) && (e.Key != Key.Up))
                return;

            var focusedMemberButton = Keyboard.FocusedElement as ListBoxItem;
            var focusedButtonContent = focusedMemberButton.Content as BrowserInternalElement;

            bool hasPrimaryMembers = primaryMembers.Items.Count > 0;
            bool hasSecondaryMembers = secondaryMembers.Items.Count > 0;

            if (e.Key == Key.Down)
            {
                // If there is no secondary members, we stay at the bottom of ClassInformationView.
                if (!hasSecondaryMembers)
                {
                    e.Handled = true;
                    return;
                }

                // If focused element is already inside secondary members, that means we are at the last member.
                // And there is no way to go further, so we stay here.
                if (secondaryMembers.Items.Contains(focusedButtonContent))
                {
                    e.Handled = true;
                    return;
                }

                var generator = secondaryMembers.ItemContainerGenerator;
                (generator.ContainerFromIndex(0) as ListBoxItem).Focus();
                e.Handled = true;
                return;
            }

            // Next code assumes, that Up key was pressed.

            // We are at the first member of primary members, we have to move back to class button.
            if (primaryMembers.Items.Contains(focusedButtonContent))
                return;

            // We are at the first member of secondary members, 
            // we have to move to last member of primary members.
            if (secondaryMembers.Items.Contains(focusedButtonContent))
            {
                var generator = primaryMembers.ItemContainerGenerator;
                (generator.ContainerFromIndex(primaryMembers.Items.Count - 1) as ListBoxItem).Focus();
                e.Handled = true;
            }
        }
    }
}
