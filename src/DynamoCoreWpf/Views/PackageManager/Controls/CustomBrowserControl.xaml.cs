using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Utilities;
using Lucene.Net.Util;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for CustomBrowserControl.xaml
    /// </summary>
    public partial class CustomBrowserControl : UserControl, IDisposable
    {

        public bool DisableRemove
        {
            get { return (bool)GetValue(DisableRemoveProperty); }
            set { SetValue(DisableRemoveProperty, value); }
        }

        public static readonly DependencyProperty DisableRemoveProperty =
            DependencyProperty.Register("DisableRemove", typeof(bool), typeof(CustomBrowserControl), new PropertyMetadata(false));

        /// <summary>
        /// Binds the ItemsSource of the TreeView
        /// </summary>
        public ObservableCollection<PackageItemRootViewModel> Root
        {
            get { return (ObservableCollection<PackageItemRootViewModel>)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register("Root", typeof(ObservableCollection<PackageItemRootViewModel>), typeof(CustomBrowserControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, propertyChangedCallback: OnItemsSourceChanged));


        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var root = (CustomBrowserControl)d;

            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= root.Root_CollectionChanged;
            }

            if (e.NewValue != null)
            {
                var coll = (ObservableCollection<PackageItemRootViewModel>)e.NewValue;
                coll.CollectionChanged += root.Root_CollectionChanged;
            }
        }

        private void Root_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                UpdateCustomTreeView(sender);
            }));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomBrowserControl()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            var currentRoot = this.Root;
            if (currentRoot != null)
            {
                currentRoot.CollectionChanged -= Root_CollectionChanged;
            }
        }

        /// <summary>
        /// Updates the visuals of the treeview elements once they are rendered
        /// </summary>
        internal void RefreshCustomTreeView()
        {
            var treeView = this.customTreeView;
            treeView.ApplyTemplate();
            treeView.UpdateLayout();

            var visualChildren = FindVisualChildren<TreeViewItem>(treeView);
            if (visualChildren == null || visualChildren.Count() == 0) return;

            var root = visualChildren.First();
            if (root == null) return;

            root.ApplyTemplate();
            root.IsExpanded = true; 
            root.UpdateLayout();

            var hmarker = FindVisualChild<System.Windows.Shapes.Rectangle>(root, "HorizontalMarker");
            if (hmarker != null) hmarker.Visibility = Visibility.Hidden;

            root.IsSelected = true;

            ApplyVisualLogic(visualChildren);
        }

        private static TreeViewItem GetLastTreeViewItem(TreeViewItem item)
        {
            var parent = FindParent<TreeViewItem>(item);

            if (parent != null)
            {
                parent.ApplyTemplate();
                parent.IsExpanded = true;
                parent.UpdateLayout();

                var children = FindVisualChildrenShallow<TreeViewItem>(parent);
                if (children == null) return null;

                // Remove everything but folders
                children = children.Where(x => (x.Header as PackageItemRootViewModel).DependencyType.Equals(DependencyType.Folder));
                // For folders containing only file items, return null
                if (!children.Any()) return null;
                if (children.Last() == item) return item;
            }
            else
            {
                // If the item has no parent, then remove the horizontal marker
                var hmarker = FindVisualChild<System.Windows.Shapes.Rectangle>(item, "HorizontalMarker");
                if (hmarker != null) hmarker.Visibility = Visibility.Hidden;
            }

            return null;
        }

        private static void ApplyVisualLogic(IEnumerable<TreeViewItem> treeViewItems)
        {
            foreach (object item in treeViewItems)
            {
                if (item is TreeViewItem topItem)
                {
                    topItem.IsExpanded = true;
                    topItem.ApplyTemplate();

                    var lastItem = GetLastTreeViewItem(topItem);
                    if (lastItem != null)
                    {
                        var marker = FindVisualChild<System.Windows.Shapes.Rectangle>(topItem, "VerticalMarker");
                        if (marker != null)
                        {
                            marker.Height = 28;
                            marker.VerticalAlignment = VerticalAlignment.Top;
                        }
                    }
                }
            }
        }
            
        private void UpdateCustomTreeView(object sender)
        {
            if ((sender as ObservableCollection<PackageItemRootViewModel>).Count == 0) return;

            RefreshCustomTreeView();
        }

        /// <summary>
        /// Updates the currently displayed files based on which folder is selected
        /// This method is invoked in 2 ways:
        /// 1. As ItemSelectionChange event of the customTreeView element
        /// 2. On Page navigation (each Page displays different section of the PreviewPackageContents)        
        /// Will not fire if the Page that's triggering it is not currently enabled
        /// </summary>
        /// <param name="sender">The TreeView element. We are interested in the currently SelectedItem.</param>
        /// <param name="e">Arguments</param>
        internal void customTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var customTree = sender as TreeView;
            if (customTree == null) return;

            var parentPage = FindLogicalParent<Page>(customTree);
            if (parentPage == null || !parentPage.IsEnabled) return;

            var selectedItem = customTree.SelectedItem as PackageItemRootViewModel;

            var viewModel = this.DataContext as PublishPackageViewModel;
            viewModel.RootContents.Clear();

            if (selectedItem != null)
            {
                viewModel.RootContents.AddRange(new ObservableCollection<PackageItemRootViewModel> (selectedItem.ChildItems
                    .OrderBy(x => x.DependencyType.Equals(DependencyType.Folder) ? 0 : 1)
                    .ThenBy(x => x.DisplayName)
                    .ToList()));
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) { return; }

            var treeViewItem = FindParent<TreeViewItem>(button);
            if (treeViewItem != null) {

                var treeView = this.customTreeView;
                var selectedItem = treeView.SelectedItem;

                var rootItem = treeViewItem.Header as PackageItemRootViewModel;
                if (rootItem == null) { return; }

                var viewModel = this.DataContext as PublishPackageViewModel;

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    viewModel.RemoveItemCommand.Execute(rootItem);
                }));
            }
        }

        #region Utility

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;

            return FindParent<T>(parentObject);
        }

        private static T FindLogicalParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = LogicalTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;

            return FindLogicalParent<T>(parentObject);
        }


        // Helper method to find visual children in a WPF control
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private static IEnumerable<T> FindVisualChildrenShallow<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    else if(child != null && child is not T)
                    {
                        foreach (T childOfChild in FindVisualChildrenShallow<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }


        private static T FindVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && (child as FrameworkElement).Name == name)
                {
                    return child as T;
                }

                T result = FindVisualChild<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        #endregion
    }

    #region Helpers

    public static class TreeViewItemHelper
    {
        public static GridLength GetIndent(DependencyObject obj)
        {
            return (GridLength)obj.GetValue(IndentProperty);
        }

        public static void SetIndent(DependencyObject obj, GridLength value)
        {
            obj.SetValue(IndentProperty, value);
        }

        // Using a DependencyProperty as the backing store for Indent. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndentProperty =
            DependencyProperty.RegisterAttached("Indent", typeof(GridLength), typeof(TreeViewItemHelper), new PropertyMetadata(new GridLength(0)));
    }

    public class IndentConverter : IValueConverter
    {
        private const int IndentSize = 20;  // hard-coded into the XAML template

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new GridLength(((GridLength)value).Value + IndentSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class HasChildrenToStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewItem treeViewItem)
            {
                // Check if the TreeViewItem has any nested items (child items)
                bool hasChildren = treeViewItem.Items.Count > 0;

                // Define styles for TreeViewItems with and without children
                Style styleWithChildren = Application.Current.FindResource("TreeViewItemWithChildrenStyle") as Style;
                Style styleWithoutChildren = Application.Current.FindResource("TreeViewItemWithoutChildrenStyle") as Style;

                // Return the appropriate style based on the presence of children
                return hasChildren ? styleWithChildren : styleWithoutChildren;
            }

            // Default style or value if the input is not a TreeViewItem
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HasChildrenToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewItem treeViewItem)
            {
                // Check if the TreeViewItem has any nested items (child items)
                bool hasChildren = treeViewItem.Items.Count > 0;

                // Return the appropriate style based on the presence of children
                return hasChildren ? Visibility.Hidden : Visibility.Visible;
            }

            // Default style or value if the input is not a TreeViewItem
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ChildrenItemsContainsFolderToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<PackageItemRootViewModel> children)
            {
                bool containsFolders = children.Any(x => x.DependencyType.Equals(DependencyType.Folder));

                // Return visible if there are any folders in this root item's children
                return containsFolders ? Visibility.Visible : Visibility.Collapsed;
            }

            // Default style or value if the input is not a TreeViewItem
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SortingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Collections.IList collection = value as System.Collections.IList;
            var view = new ListCollectionView(collection);
            var sort = new SortDescription(parameter.ToString(), ListSortDirection.Ascending);
            view.SortDescriptions.Add(sort);

            return view;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public static class MyTreeViewHelper
    {
        private static TreeViewItem _currentItem = null;

        // IsMouseDirectlyOverItem:  A DependencyProperty that will be true only on the 
        // TreeViewItem that the mouse is directly over.  I.e., this won't be set on that 
        // parent item.
        //
        // This is the only public member, and is read-only.

        // The property key (since this is a read-only DP)
        private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey =
            DependencyProperty.RegisterAttachedReadOnly("IsMouseDirectlyOverItem",
                                                typeof(bool),
                                                typeof(MyTreeViewHelper),
                                                new FrameworkPropertyMetadata(null, new CoerceValueCallback(CalculateIsMouseDirectlyOverItem)));

        // The DP itself
        public static readonly DependencyProperty IsMouseDirectlyOverItemProperty =
            IsMouseDirectlyOverItemKey.DependencyProperty;

        // A strongly-typed getter for the property.
        public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
        }

        // A coercion method for the property
        private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
        {
            // This method is called when the IsMouseDirectlyOver property is being calculated
            // for a TreeViewItem.  
            if (item == _currentItem)
                return true;
            else
                return false;
        }

        // UpdateOverItem:  A private RoutedEvent used to find the nearest encapsulating
        // TreeViewItem to the mouse's current position.
        private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent(
            "UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyTreeViewHelper));

        // Class constructor
        static MyTreeViewHelper()
        {
            // Get all Mouse enter/leave events for TreeViewItem.
            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseEnterEvent, new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeaveEvent, new MouseEventHandler(OnMouseTransition), true);

            // Listen for the UpdateOverItemEvent on all TreeViewItem's.
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UpdateOverItemEvent, new RoutedEventHandler(OnUpdateOverItem));
        }


        // OnUpdateOverItem:  This method is a listener for the UpdateOverItemEvent.  When it is received,
        // it means that the sender is the closest TreeViewItem to the mouse (closest in the sense of the tree,
        // not geographically).

        static void OnUpdateOverItem(object sender, RoutedEventArgs args)
        {
            // Mark this object as the tree view item over which the mouse
            // is currently positioned.
            _currentItem = sender as TreeViewItem;

            // Tell that item to re-calculate the IsMouseDirectlyOverItem property
            _currentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);

            // Prevent this event from notifying other tree view items higher in the tree.
            args.Handled = true;
        }

        // OnMouseTransition:  This method is a listener for both the MouseEnter event and
        // the MouseLeave event on TreeViewItems.  It updates the _currentItem, and updates
        // the IsMouseDirectlyOverItem property on the previous TreeViewItem and the new
        // TreeViewItem.

        static void OnMouseTransition(object sender, MouseEventArgs args)
        {
            lock (IsMouseDirectlyOverItemProperty)
            {
                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
                }

                // Get the element that is currently under the mouse.
                IInputElement currentPosition = Mouse.DirectlyOver;

                // See if the mouse is still over something (any element, not just a tree view item).
                if (currentPosition != null)
                {
                    // Yes, the mouse is over something.
                    // Raise an event from that point.  If a TreeViewItem is anywhere above this point
                    // in the tree, it will receive this event and update _currentItem.

                    RoutedEventArgs newItemArgs = new RoutedEventArgs(UpdateOverItemEvent);
                    currentPosition.RaiseEvent(newItemArgs);

                }
            }
        }
    }

    #endregion
}
