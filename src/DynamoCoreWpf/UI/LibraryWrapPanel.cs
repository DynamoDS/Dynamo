using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Search.SearchElements;
using Dynamo.UI.Controls;
using Dynamo.Utilities;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.Controls
{
    public class LibraryWrapPanel : WrapPanel
    {
        /// <summary>
        /// Field specifies the prospective index of selected class in collection.
        /// It is also used to guard against unnecessary "OrderListItems" 
        /// calls which repaints the UI. Here its value is set to "-2" to ensure that 
        /// "OrderListItems" method gets called at least once at the beginning (i.e. 
        /// when "ListView.SelectedIndex" is "-1", which would have avoided the first
        /// "OrderListItems" if "currentSelectedIndex" is set to "-1").
        /// </summary>
        private int selectedClassProspectiveIndex = -2;
        private double classObjectWidth = double.NaN;
        private ObservableCollection<ISearchEntryViewModel> collection;
        private NodeCategoryViewModel currentClass;
        private ListView classListView;

        internal bool MakeOrClearSelection(NodeCategoryViewModel selectedClass)
        {
            if (currentClass != null)
            {
                if (currentClass != selectedClass)
                {
                    // If 'itemIndex' is '-1', then the selection will be cleared,
                    // otherwise the selection is set to the same as 'itemIndex'.
                    var itemIndex = collection.IndexOf(selectedClass);
                    classListView.SelectedIndex = itemIndex;
                    return true; // The call is handled.
                }
                else
                {
                    // If current item is selected item, then class button was pressed second time.
                    // Selection should be cleaned.
                    classListView.SelectedIndex = -1;
                    return true;
                }
            }
            else
            {
                // No selection, if item is within collection, select it.
                var itemIndex = collection.IndexOf(selectedClass);
                if (itemIndex != -1)
                {
                    classListView.SelectedIndex = itemIndex;
                    return true; // The call is handled.
                }
            }

            // The call is not handled.
            return false;
        }

        protected override void OnInitialized(EventArgs e)
        {
            // ListView should never be null.
            classListView = WpfUtilities.FindUpVisualTree<ListView>(this);
            collection = classListView.ItemsSource as ObservableCollection<ISearchEntryViewModel>;
            collection.Add(new ClassInformationViewModel());
            classListView.SelectionChanged += OnClassViewSelectionChanged;

            this.KeyDown += OnLibraryWrapPanelKeyDown;

            base.OnInitialized(e);
        }

        private void OnLibraryWrapPanelKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var classButton = Keyboard.FocusedElement as ListBoxItem;

            // Enter collapses and expands class button.
            if (e.Key == Key.Enter)
            {
                classButton.IsSelected = !classButton.IsSelected;
                e.Handled = true;
                return;
            }

            var buttonsWrapPanel = sender as LibraryWrapPanel;
            var listButtons = buttonsWrapPanel.Children;

            // If focused element is NodeSearchElement, that means focused element is inside expanded class.
            if (classButton.DataContext is NodeSearchElement)
            {
                // If user presses Up, we have to move back to selected class.
                if (e.Key == Key.Up)
                {
                    var selectedClassButton = listButtons.OfType<ListViewItem>().
                        Where(button => button.IsSelected).FirstOrDefault();
                    if (selectedClassButton != null) selectedClassButton.Focus();
                    e.Handled = true;
                    return;
                }
                // Otherwise, let user move inside expanded class.
                return;
            }

            // If class is selected, we should move down to ClassDetails.
            else if (e.Key == Key.Down)
            {
                if (classButton.IsSelected)
                {
                    int classInfoIndex = GetClassInformationIndex();
                    var classInformationView = listButtons[classInfoIndex];
                    var firstMemberList = WpfUtilities.ChildOfType<ListBox>(classInformationView, "primaryMembers");
                    var generator = firstMemberList.ItemContainerGenerator;
                    (generator.ContainerFromIndex(0) as ListBoxItem).Focus();

                    e.Handled = true;
                    return;
                }
            }

            var selectedIndex = listButtons.IndexOf(classButton);
            int itemsPerRow = (int)Math.Floor(buttonsWrapPanel.ActualWidth / classButton.ActualWidth);

            int newIndex = GetIndexNextSelectedItem(e.Key, selectedIndex, itemsPerRow);

            // If index is out of range class list, that means we have to move to previous category
            // or to next member group.
            if ((newIndex < 0) || (newIndex > listButtons.Count))
            {
                e.Handled = false;
                return;
            }

            // Set focus on new item.
            listButtons[newIndex].Focus();

            e.Handled = true;
            return;
        }

        private int GetIndexNextSelectedItem(Key key, int selectedIndex, int itemsPerRow)
        {
            int newIndex = -1;
            int selectedRowIndex = selectedIndex / itemsPerRow + 1;

            switch (key)
            {
                case Key.Right:
                    {
                        newIndex = selectedIndex + 1;
                        int availableIndex = selectedRowIndex * itemsPerRow - 1;
                        if (newIndex > availableIndex) newIndex = selectedIndex;
                        break;
                    }
                case Key.Left:
                    {
                        newIndex = selectedIndex - 1;
                        int availableIndex = (selectedRowIndex - 1) * itemsPerRow;
                        if (newIndex < availableIndex) newIndex = selectedIndex;
                        break;
                    }
                case Key.Down:
                    {
                        newIndex = selectedIndex + itemsPerRow + 1;
                        // +1 because one of items is always ClassInformation.
                        break;
                    }
                case Key.Up:
                    {
                        newIndex = selectedIndex - itemsPerRow;
                        break;
                    }
            }
            return newIndex;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (sizeInfo.WidthChanged) // Only reorder when width changes.
                OrderListItems();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Children == null || this.Children.Count == 0)
                return finalSize;

            // Find out class object size.
            // First item is always class object.
            if (double.IsNaN(classObjectWidth))
            {
                // Make sure our assumption of the first child being a 
                // ClassInformationView still holds.
                var firstChild = this.Children[0];
                if (firstChild is ClassInformationView)
                {
                    // If the following exception is thrown, please look at "LibraryWrapPanel.cs"
                    // where we insert both "BrowserItem" and "ClassInformation" items, ensure that
                    // the "ClassInformation" item is inserted last.
                    throw new InvalidOperationException("firstChild is ClassInformationView. " +
                        "firstChild Type should be derived from BrowserItem");
                }

                classObjectWidth = firstChild.DesiredSize.Width;
            }

            double x = 0, y = 0, currentRowHeight = 0;

            var itemsPerRow = (int)Math.Floor(finalSize.Width / classObjectWidth);
            double sizeBetweenItems = (finalSize.Width - itemsPerRow * classObjectWidth) / (itemsPerRow + 1);

            foreach (UIElement child in this.Children)
            {
                var classInformation = (child as FrameworkElement).DataContext as ClassInformationViewModel;
                // Hidden ClassInformationView shouldn't be arranged.
                if (classInformation != null && !classInformation.ClassDetailsVisibility)
                    continue;

                var desiredSize = child.DesiredSize;
                if ((x + desiredSize.Width) > finalSize.Width)
                {
                    x = 0;
                    y = y + currentRowHeight;
                    currentRowHeight = 0;
                }

                if (classInformation != null)
                {
                    // Then it's ClassInformationView, we do not need margin it.
                    child.Arrange(new Rect(x, y, desiredSize.Width, desiredSize.Height));
                    x = x + desiredSize.Width;
                }
                else
                {
                    child.Arrange(new Rect(x + sizeBetweenItems, y, desiredSize.Width, desiredSize.Height));
                    x = x + desiredSize.Width + sizeBetweenItems;
                }
                if (desiredSize.Height > currentRowHeight)
                    currentRowHeight = desiredSize.Height;
            }

            return finalSize;
        }

        protected override bool HasLogicalOrientation
        {
            get { return false; } // Arrange items in two dimension.
        }

        private void OnClassViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedIndex = (sender as ListView).SelectedIndex;

            // As focus moves within the class details, class button gets selected which 
            // triggers a selection change. During a selection change the items in the 
            // wrap panel gets reordered through "OrderListItems", but this is not always 
            // necessary. Here we determine if the "translatedIndex" is the same as
            // "selectedClassProspectiveIndex", if so simply returns to avoid a repainting.
            var translatedIndex = TranslateSelectionIndex(selectedIndex);
            if (selectedClassProspectiveIndex == translatedIndex)
                return;

            selectedClassProspectiveIndex = translatedIndex;

            int classInfoIndex = GetClassInformationIndex();

            // If user clicks on the same item when it is expanded, then 'OnClassButtonCollapse'
            // is invoked to deselect the item. This causes 'OnClassViewSelectionChanged' to be 
            // called again, with 'SelectedIndex' set to '-1', indicating that no item is selected,
            // in which case we need to hide the ClassInformationView.
            if (selectedClassProspectiveIndex == -1)
            {
                if (classInfoIndex != -1)
                {
                    (collection[classInfoIndex] as ClassInformationViewModel).ClassDetailsVisibility = false;
                    currentClass = null;
                }
                OrderListItems();
                return;
            }
            else
            {
                (collection[classInfoIndex] as ClassInformationViewModel).ClassDetailsVisibility = true;
            }

            currentClass = collection[selectedIndex] as NodeCategoryViewModel;
            OrderListItems(); // Selection change, we may need to reorder items.
        }

        /// <summary>
        /// Function counts prospective index of selected class in collection.
        /// In case if index of selected class bigger then index of ClassInformation
        /// object it should be decreased by 1 because at next stages ClassInformation
        /// will free occupied index.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        private int TranslateSelectionIndex(int selection)
        {
            if (selection < GetClassInformationIndex())
                return selection;

            return selection - 1;
        }

        private int GetClassInformationIndex()
        {
            var query = collection.Select(c => c).Where(c => c is ClassInformationViewModel);
            var classObjectBase = query.ElementAt(0);
            return collection.IndexOf(classObjectBase);
        }

        private void OrderListItems()
        {
            if (double.IsNaN(this.ActualWidth))
                return;
            if (collection == null || (collection.Count <= 1) || currentClass == null)
                return;

            // Find out where ClassInformation object is positioned in collection.
            var currentClassInformationIndex = GetClassInformationIndex();
            var classObjectBase = collection[currentClassInformationIndex];

            // If there is no selection, then mark the ClassInformationView as hidden.
            var classInformation = classObjectBase as ClassInformationViewModel;
            if (classInformation != null && (selectedClassProspectiveIndex == -1))
                return;

            //Add members of selected class to ClassInformationView            
            classInformation.PopulateMemberCollections(currentClass);

            // When we know the number of items on a single row, through selected 
            // item index we will find out where the expanded ClassInformationView sit.
            var itemsPerRow = ((int)Math.Floor(ActualWidth / classObjectWidth));
            var d = ((double)selectedClassProspectiveIndex) / itemsPerRow;
            var selectedItemRow = ((int)Math.Floor(d));

            // Calculate the correct index where ClassInformation object should go 
            // in the collection. If the selected item is on the first row (i.e. 
            // row #0), then the ClassInformation object should be at the index 
            // 'itemsPerRow'. Similarly, if the selected item is on row #N, then
            // ClassInformation object should be at the index '(N + 1) * itemsPerRow'.
            var correctClassInformationIndex = ((selectedItemRow + 1) * itemsPerRow);

            // But correctClassInformationIndex must be less than collection.Count.
            if (correctClassInformationIndex >= collection.Count)
                correctClassInformationIndex = collection.Count - 1;

            // We need to move the ClassInformation object to the right index.            
            collection.RemoveAt(currentClassInformationIndex);
            collection.Insert(correctClassInformationIndex, classInformation);
        }
    }
}
