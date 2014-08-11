using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Controls
{
    public class LibraryWrapPanel : WrapPanel
    {
        private int selectedItemIndex = -1;
        private const double ClassObjectWidth = 66.0;
        private const double ClassObjectHeight = 66.0;
        private ObservableCollection<ClassObjectBase> collection;
        private ClassObject currentClass;

        protected override void OnInitialized(EventArgs e)
        {
            // ListView should never be null.
            var classListView = WPF.FindUpVisualTree<ListView>(this);
            collection = classListView.ItemsSource as ObservableCollection<ClassObjectBase>;
            classListView.SelectionChanged += OnClassViewSelectionChanged;

            base.OnInitialized(e);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (sizeInfo.WidthChanged) // Only recorder when width changes.
                OrderListItems();

            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.Children == null || this.Children.Count == 0)
                return finalSize;

            double x = 0, y = 0, currentRowHeight = 0;
            foreach (UIElement child in this.Children)
            {
                var desiredSize = child.DesiredSize;
                if ((x + desiredSize.Width) > finalSize.Width)
                {
                    x = 0;
                    y = y + currentRowHeight;
                    currentRowHeight = 0;
                }

                child.Arrange(new Rect(x, y, desiredSize.Width, desiredSize.Height));
                x = x + desiredSize.Width;
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
            selectedItemIndex = TranslateSelectionIndex(index);
            currentClass = collection[index] as ClassObject;
            OrderListItems(); // Selection change, we may need to reorder items.
        }

        private int TranslateSelectionIndex(int selection)
        {
            if (selection < GetClassDetailsIndex())
                return selection;

            return selection - 1;
        }

        private int GetClassDetailsIndex()
        {
            var query = collection.Select(c => c).Where(c => c is ClassDetails);
            var classObjectBase = query.ElementAt(0);
            return collection.IndexOf(classObjectBase);
        }

        private void OrderListItems()
        {
            if (double.IsNaN(this.ActualWidth))
                return;
            if (collection == null || (collection.Count <= 1) || currentClass == null)
                return;

            // Find out where ClassDetails object is positioned in collection.
            var currentClassDetailsIndex = GetClassDetailsIndex();
            var classObjectBase = collection[currentClassDetailsIndex];

            // If there is no selection, then mark the class details as hidden.
            var classDetails = classObjectBase as ClassDetails;
            if (classDetails != null && (selectedItemIndex == -1))
            {
                classDetails.ClassDetailsVisibility = Visibility.Collapsed;
                return;
            }

            // Otherwise, if we get here it means the class details is shown!
            classDetails.ClassDetailsVisibility = Visibility.Visible;

            //Add members of selected class to class details
            classDetails.ActionMembers = currentClass.ActionMembers;
            classDetails.CreateMembers = currentClass.CreateMembers;
            classDetails.QueryMembers = currentClass.QueryMembers;

            // When we know the number of items on a single row, through selected 
            // item index we will find out where the expanded StandardPanel sit.
            var itemsPerRow = ((int)Math.Floor(ActualWidth / ClassObjectWidth));
            var d = ((double)selectedItemIndex) / itemsPerRow;
            var selectedItemRow = ((int)Math.Floor(d));

            // Calculate the correct index where ClassDetails object should go 
            // in the collection. If the selected item is on the first row (i.e. 
            // row #0), then the ClassDetails object should be at the index 
            // 'itemsPerRow'. Similarly, if the selected item is on row #N, then
            // ClassDetails object should be at the index '(N + 1) * itemsPerRow'.
            var correctClassDetailsIndex = ((selectedItemRow + 1) * itemsPerRow);

            // We need to move the ClassDetails object to the right index.
            classObjectBase = collection[currentClassDetailsIndex];
            collection.RemoveAt(currentClassDetailsIndex);
            collection.Insert(correctClassDetailsIndex, classObjectBase);
        }
    }
}
