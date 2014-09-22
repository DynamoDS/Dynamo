using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Nodes.Search;
using Dynamo.UI.Controls;
using Dynamo.Utilities;

namespace Dynamo.Controls
{
    public class LibraryWrapPanel : WrapPanel
    {
        /// <summary>
        /// Field specifies the prospective index of selected class in collection.
        /// </summary>
        private int selectedClassProspectiveIndex = -1;
        private double classObjectWidth = double.NaN;
        private ObservableCollection<BrowserItem> collection;
        private BrowserInternalElement currentClass;

        protected override void OnInitialized(EventArgs e)
        {
            // ListView should never be null.
            var classListView = WPF.FindUpVisualTree<ListView>(this);
            collection = classListView.ItemsSource as ObservableCollection<BrowserItem>;
            collection.Add(new ClassInformation());
            classListView.SelectionChanged += OnClassViewSelectionChanged;

            base.OnInitialized(e);
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
                // StandardPanel still holds.
                var firstChild = this.Children[0];
                if (firstChild is StandardPanel)
                {
                    // If the following exception is thrown, please look at "LibraryWrapPanel.cs"
                    // where we insert both "BrowserItem" and "ClassInformation" items, ensure that
                    // the "ClassInformation" item is inserted last.
                    throw new InvalidOperationException("firstChild is StandardPanel. " +
                        "firstChild Type should be derived from BrowserItem");
                }

                classObjectWidth = firstChild.DesiredSize.Width;
            }

            double x = 0, y = 0, currentRowHeight = 0;

            int itemsPerRow = (int)Math.Floor(finalSize.Width / classObjectWidth);
            double sizeBetweenItems = (finalSize.Width - itemsPerRow*classObjectWidth) / (itemsPerRow+1);


            foreach (UIElement child in this.Children)
            {
                var desiredSize = child.DesiredSize;
                if ((x + desiredSize.Width) > finalSize.Width)
                {
                    x = 0;
                    y = y + currentRowHeight;
                    currentRowHeight = 0;
                }

                if ((child as FrameworkElement).DataContext is ClassInformation)
                //Then it's Standard panel, we do not need margin it.
                {
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
            var index = ((sender as ListView).SelectedIndex);
            int classInfoIndex = GetClassInformationIndex();

            // If user clicks on the same item when it is expanded, then 'OnClassButtonCollapse'
            // is invoked to deselect the item. This causes 'OnClassViewSelectionChanged' to be 
            // called again, with 'SelectedIndex' set to '-1', indicating that no item is selected,
            // in which case we need to hide the standard panel.
            if (index == -1)
            {
                if (classInfoIndex != -1)
                    (collection[classInfoIndex] as ClassInformation).ClassDetailsVisibility = false;
                OrderListItems();
                return;
            }
            else
                (collection[classInfoIndex] as ClassInformation).ClassDetailsVisibility = true;

            selectedClassProspectiveIndex = TranslateSelectionIndex(index);
            currentClass = collection[index] as BrowserInternalElement;
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
            var query = collection.Select(c => c).Where(c => c is ClassInformation);
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

            // If there is no selection, then mark the StandardPanel as hidden.
            var classInformation = classObjectBase as ClassInformation;
            if (classInformation != null && (selectedClassProspectiveIndex == -1))
                return;

            //Add members of selected class to StandardPanel            
            classInformation.PopulateMemberCollections(currentClass as BrowserInternalElement);

            // When we know the number of items on a single row, through selected 
            // item index we will find out where the expanded StandardPanel sit.
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
