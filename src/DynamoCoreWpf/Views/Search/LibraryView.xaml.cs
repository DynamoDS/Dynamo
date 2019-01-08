using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibraryView.xaml
    /// </summary>
    public partial class LibraryView : UserControl
    {
        // See OnExpanderButtonMouseLeftButtonUp for details.
        private bool ignoreMouseEnter;
        private LibraryDragAndDrop dragDropHelper = new LibraryDragAndDrop();

        private SearchViewModel ViewModel
        {
            get { return DataContext as SearchViewModel; }
        }

        public LibraryView()
        {
            InitializeComponent(); 

            // Invalidate the DataContext here because it will be set at a later 
            // time through data binding expression. This way debugger will not 
            // display warnings for missing properties.
            this.DataContext = null;
        }

        /// <summary>
        /// When user tries to use mouse wheel there can be several cases.
        /// </summary>
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Case 1: User scrolls the mouse wheel when Shift key is being held down. This 
            // action triggers a horizontal scroll on the library view (so that lengthy names 
            // can be revealed). Setting 'Handled' to 'false' allows the underlying scroll bar
            // to handle the mouse wheel event.
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                e.Handled = false;
                return;
            }

            // Case 2: If the mouse is outside of the library view, but mouse wheel messages 
            // get sent to it anyway. In such case there is nothing to change here. The 'Handled'
            // is not set to 'true' here because the mouse wheel messages should be routed to the 
            // ScrollViewer on tool-tip for further processing.
            if (!(sender as UIElement).IsMouseOver)
                return;

            // Case 3: Mouse wheel without any modifier keys, it scrolls the library view 
            // vertically. In this case 'VerticalOffset' is updated, 'Handled' is also set 
            // so that mouse wheel message routing ends here.
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnClassButtonCollapse(object sender, MouseButtonEventArgs e)
        {
            var classButton = sender as TreeViewItem;
            if ((classButton == null) || !classButton.IsSelected) return;

            classButton.IsSelected = false;
            e.Handled = true;
        }

        /// When a category is collapsed, the selection of underlying sub-category 
        /// list is cleared. As a result any visible ClassInformationView will be hidden.
        private void OnExpanderCollapsed(object sender, System.Windows.RoutedEventArgs e)
        {
            var expanderContent = (sender as FrameworkElement);

            var buttons = expanderContent.ChildOfType<ListView>();
            if (buttons != null)
                buttons.UnselectAll();

            e.Handled = true;
        }

        private void OnSubCategoryListViewPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void OnMemberMouseEnter(object sender, MouseEventArgs e)
        {
            if (ignoreMouseEnter)
            {
                ignoreMouseEnter = false;
                return;
            }

            FrameworkElement fromSender = sender as FrameworkElement;
            libraryToolTipPopup.PlacementTarget = fromSender;
            var memberVM = fromSender.DataContext as NodeSearchElementViewModel;
            if (memberVM != null)
            {
                libraryToolTipPopup.SetDataContext(memberVM);
            }
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            libraryToolTipPopup.SetDataContext(null);
        }

        private void OnTreeViewItemPreviewMouseLeftButton(object sender, MouseButtonEventArgs e)
        {
            var categoryButton = sender as TreeViewItem;
            if (!(categoryButton.DataContext is RootNodeCategoryViewModel))
                return;

            var wrapPanels = categoryButton.ChildrenOfType<LibraryWrapPanel>();
            if (!wrapPanels.Any())
                return;

            var selectedElement = e.OriginalSource as FrameworkElement;
            var selectedClass = selectedElement.DataContext as NodeCategoryViewModel;
            // Continue work with real class: not null, not ClassInformationViewModel.
            if (selectedClass == null || selectedClass is ClassInformationViewModel)
                return;

            // Go through all available for current top category LibraryWrapPanel.
            // Select class if wrapPanel contains selectedClass.
            // Unselect class in other case.
            foreach (var wrapPanel in wrapPanels)
            {
                if (wrapPanel.MakeOrClearSelection(selectedClass))
                {
                    // If class button was clicked, then handle, otherwise leave it.
                    e.Handled = selectedClass.IsClassButton;

                    var classInfoPanel = wrapPanel.Children.Cast<FrameworkElement>().
                        Where(child => child.DataContext is ClassInformationViewModel).FirstOrDefault();

                    classInfoPanel.Loaded += (send, handler) =>
                        {
                            // This call is made whenever a class button is clicked on to expand the class 
                            // information (right after the corresponding "ClassInformationView" view is created).
                            // "selectedElement" here can either be the TextBlock or Image on the class button.
                            // 
                            // Required height is calculated by adding class button and "ClassInformationView" heights.
                            // If the required height is larger than the visible library height, then the class 
                            // button is placed on the top of library, with the rest of the space occupied by the 
                            // "ClassInformationView".

                            var selectedClassButton = WpfUtilities.FindUpVisualTree<Border>(selectedElement);
                            var height = classInfoPanel.RenderSize.Height + selectedClassButton.RenderSize.Height;
                            if (height > ScrollLibraryViewer.ActualHeight)
                            {
                                selectedClassButton.BringIntoView(new Rect(0, 0, 0, ScrollLibraryViewer.ActualHeight));
                            }
                            else
                            {
                                // If the class button is already visible on the library, simply bring the 
                                // "ClassInformationView" into view. Otherwise, the class button is brought into view.
                                if (IsElementVisible(selectedClassButton, ScrollLibraryViewer))
                                    classInfoPanel.BringIntoView();
                                else
                                    selectedClassButton.BringIntoView();
                            }
                        };
                }
            }

            ExpandCategory(categoryButton.Items.OfType<NodeCategoryViewModel>(), selectedClass);

            e.Handled = !(selectedClass is RootNodeCategoryViewModel);
        }

        private bool IsElementVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            // "element" here represents the "Border" object which contains TextBlock/Image of 
            // a class button. "container" here is the "ScrollLibraryViewer" on the library. 
            // The bounds of this class button is transformed to the rectangle it occupies in 
            // the "ScrollLibraryViewer" before being compared to the the region of container 
            // to determine if it lies outside of the container.
            // 
            var elementRect = new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight);
            var containerRect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            elementRect = element.TransformToAncestor(container).TransformBounds(elementRect);
            return containerRect.IntersectsWith(elementRect);
        }

        private bool ExpandCategory(IEnumerable<NodeCategoryViewModel> categories, NodeCategoryViewModel selectedClass)
        {
            bool foundSelectedClass = false;

            // Get all current expanded categories.
            var allExpandedCategories = categories.Where(cat =>
            {
                return cat.IsExpanded && (!(cat is ClassesNodeCategoryViewModel));

            }).ToList();
            var categoryToBeExpanded = categories.FirstOrDefault(cat => cat == selectedClass);

            // If categoryToBeExpanded is null, that means the clicked item does not
            // represent a category button, but a class button. In the recursive call 
            // the category in which this clicked class belong will be identified.
            if (categoryToBeExpanded != null)
            {
                categoryToBeExpanded.IsExpanded = !categoryToBeExpanded.IsExpanded;
                foundSelectedClass = true;
            }

            // Get expanded categories that should be collapsed.
            allExpandedCategories.Remove(categoryToBeExpanded);

            // Close all expanded categories except the one that contains the target
            // class button. If the clicked NodeCategoryViewModel represents a category
            // itself, then expand it and close out other sibling NodeCategoryViewModel.
            foreach (var expandedCategory in allExpandedCategories)
            {
                var searchFurtherInNextLevel = true;

                // If class button was clicked.
                if (selectedClass.IsClassButton)
                {
                    var categoryClasses = expandedCategory.Items[0] as ClassesNodeCategoryViewModel;
                    if (categoryClasses != null) // There are classes under this category...
                    {
                        if (expandedCategory.IsClassButton)
                        {
                            // If the category does not contain any sub category, 
                            // then we won't look for the selected class within it.
                            expandedCategory.IsExpanded = false;
                            searchFurtherInNextLevel = false;
                        }
                        else if (categoryClasses.Items.Contains(selectedClass))
                        {
                            // If the category contains the selected class directly 
                            // within, then keep it expanded instead of collapsing it.
                            expandedCategory.IsExpanded = true;

                            // Found the selected class! Collapse all other sub categories.
                            foreach (var ele in expandedCategory.SubCategories)
                                ele.IsExpanded = false;

                            searchFurtherInNextLevel = false;
                        }
                    }
                }

                if (searchFurtherInNextLevel)
                {
                    var childCategories = expandedCategory.Items.OfType<NodeCategoryViewModel>();
                    expandedCategory.IsExpanded = ExpandCategory(childCategories, selectedClass);
                }

                // If the category remains expanded after this, we can 
                // be sure that the selected class was found within it.
                foundSelectedClass = foundSelectedClass || expandedCategory.IsExpanded;
            }

            return foundSelectedClass;
        }

        // Clicking on a member node results in a node being placed on the canvas.
        // Another mouse-enter event will be sent right after this left-button-up, 
        // which brings up the tool-tip. This isn't desirable, so setting a flag 
        // here to ignore the immediate mouse-enter event.
        private void OnExpanderButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Button).DataContext is NodeCategoryViewModel)
            {
                libraryToolTipPopup.SetDataContext(null, true);
                ignoreMouseEnter = true;
            }
        }

        private void OnAddButtonPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var button = sender as Button;
                var contextMenu = button.ContextMenu;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.PlacementTarget = button;
                contextMenu.IsOpen = true;
            }

            e.Handled = true;
        }

        #region Drag&Drop

        private void OnExpanderButtonMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var searchElementVm = GetDataContext(e.OriginalSource);

            //sender can be RootSearchElementVM or ClassInformationViewModel. 
            //And we should just fire HandleMouseLeftButtonDown, when ViewModel is NodeSearchElementViewModel
            if (searchElementVm != null)
            {
                dragDropHelper.HandleMouseDown(e.GetPosition(null), searchElementVm);
            }
        }

        private void OnExpanderButtonPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !(e.OriginalSource is DependencyObject))
                return;

            var searchElementVm = GetDataContext(e.OriginalSource);
            
            //sender can be RootSearchElementVM or ClassInformationViewModel. 
                //And we should just fire HandleMouseMove, when ViewModel is NodeSearchElementViewModel
            if (searchElementVm != null)
            {
                dragDropHelper.HandleMouseMove((DependencyObject)e.OriginalSource, e.GetPosition(null));
            }
        }

        private NodeSearchElementViewModel GetDataContext(object source)
        {
            var frameworkElement = source as FrameworkElement;
            
            if (frameworkElement != null)
            {
                return frameworkElement.DataContext as NodeSearchElementViewModel;
            }

            var frameworkContentElement = source as FrameworkContentElement;
            if (frameworkContentElement != null)
            {
                return frameworkContentElement.DataContext as NodeSearchElementViewModel;
            }

            return null;
        }

        #endregion

        private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            var treeItem = e.OriginalSource as TreeViewItem;
            if (treeItem == null) return;

            var treeItemVM = treeItem.DataContext as NodeCategoryViewModel;
            if (treeItemVM == null) return;

            // Do not fire Dispatcher every time, when some treeview item is expanded.
            // Here we check that it's really class, that was clicked in search. 
            if (String.IsNullOrEmpty(ViewModel.ClassNameToBeOpened)
                || treeItemVM.FullCategoryName != ViewModel.ClassNameToBeOpened) return;

            // We use dispatcher with lower priority in order to wait until UI is rendered.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = e.OriginalSource as TreeViewItem;
                if (item == null) return;

                var transform = item.TransformToVisual(CategoryTreeView);
                var positionInScrollViewer = transform.Transform(new Point(0, 0));

                ScrollLibraryViewer.ScrollToVerticalOffset(positionInScrollViewer.Y);
                ViewModel.ClassNameToBeOpened = String.Empty;
            }),
                DispatcherPriority.Loaded, null);

        }
    }
}
