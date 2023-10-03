using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for CustomBrowserControl.xaml
    /// </summary>
    public partial class CustomBrowserControl : UserControl
    {
        private PublishPackageViewModel PublishPackageViewModel;

        /// <summary>
        /// Binds the ItemsSource of the TreeView
        /// </summary>
        public ObservableCollection<PackageItemRootViewModel> Root
        {
            get { return (ObservableCollection<PackageItemRootViewModel>)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Root.  This enables animation, styling, binding, etc...
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

        public CustomBrowserControl()
        {
            InitializeComponent();
        }

        private void UpdateCustomTreeView(object sender)
        {
            if ((sender as ObservableCollection<PackageItemRootViewModel>).Count == 0) return;

            //UnselectSelectedItem();

            var treeView = this.customTreeView;
            treeView.ApplyTemplate();
            treeView.UpdateLayout();
            //treeView.Items.Refresh();

            var visualChildren = FindVisualChildren<TreeViewItem>(treeView);
            if (visualChildren == null || visualChildren.Count() == 0) return;

            ApplyLastTreeItem(visualChildren);

            // setting the visual styles for each separate 'root' folder
            foreach(var item in visualChildren)
            {
                var rootItem = item.Header as PackageItemRootViewModel;
                if (rootItem.isChild) continue;

                item.ApplyTemplate();
                item.IsExpanded = true;
                item.UpdateLayout();    

                var hmarker = FindVisualChild<System.Windows.Shapes.Rectangle>(item, "HorizontalMarker");
                if (hmarker != null) hmarker.Visibility = Visibility.Hidden;
                var vmarker = FindVisualChild<System.Windows.Shapes.Rectangle>(item, "VerticalMarker");
                if (vmarker != null) vmarker.Visibility = Visibility.Hidden;

                var children = FindVisualChildren<TreeViewItem>(item);
                if(children != null)
                {
                    try
                    {   
                        var lastItem = children.Last();

                        lastItem.IsExpanded = true;
                        lastItem.UpdateLayout();
                        lastItem.ApplyTemplate();

                        var lmarker = FindVisualChild<System.Windows.Shapes.Rectangle>(lastItem, "LongMarker");
                        if (lmarker != null) lmarker.Visibility = Visibility.Hidden;
                    }
                    catch (Exception) { }
                }
            }

            visualChildren.First().IsSelected = true;
        }

        private void UnselectSelectedItem()
        {
            if (this.customTreeView.SelectedItem != null)
            {
                var container = FindTreeViewSelectedItemContainer(this.customTreeView, this.customTreeView.SelectedItem);
                if (container != null)
                {
                    container.IsSelected = false;
                }
            }
        }

        private static TreeViewItem FindTreeViewSelectedItemContainer(ItemsControl root, object selection)
        {
            var item = root.ItemContainerGenerator.ContainerFromItem(selection) as TreeViewItem;
            if (item == null)
            {
                foreach (var subItem in root.Items)
                {
                    item = FindTreeViewSelectedItemContainer((TreeViewItem)root.ItemContainerGenerator.ContainerFromItem(subItem), selection);
                    if (item != null)
                    {
                        break;
                    }
                }
            }

            return item;
        }

        private static void ApplyLastTreeItem(IEnumerable<TreeViewItem> treeViewItems) 
        {
            foreach (TreeViewItem item in treeViewItems)
            {
                item.ApplyTemplate();
                item.IsExpanded = true;
                item.UpdateLayout();

                var parent = FindParent<TreeViewItem>(item);
                if (parent == null)
                {
                    return;
                }
                var children = FindVisualChildren<TreeViewItem>(parent);
                if (children != null)
                {
                    var lastItem = children.Last();

                    if (item == lastItem)
                    {
                        lastItem.IsExpanded = true;
                        lastItem.UpdateLayout();
                        lastItem.ApplyTemplate();

                        var marker = FindVisualChild<System.Windows.Shapes.Rectangle>(item, "VerticalMarker");
                        if (marker != null) marker.Height = 12;
                    }
                }
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

        private void customTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var customTree = sender as TreeView;
            var selectedItem = customTree.SelectedItem as PackageItemRootViewModel;

            if (selectedItem != null)
            {
                var viewModel = this.DataContext as PublishPackageViewModel;
                //viewModel.RootContents = new ObservableCollection<PackageItemRootViewModel>( selectedItem.ChildItems
                //    .Where(x => !x.DependencyType.Equals(DependencyType.Folder)).ToList() );
                viewModel.RootContents = selectedItem.ChildItems;
            }
        }
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
        private const int IndentSize = 16;  // hard-coded into the XAML template

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

    public class DependencyTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DependencyType.Folder)
            {
                // Return visible only if the item is a Folder
                return Visibility.Visible;
            }

            // If the item is anything else (Assembly, File, Custom Node) return collapsed
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
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
