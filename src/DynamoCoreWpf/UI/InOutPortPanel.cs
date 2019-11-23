using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    public class InOutPortPanel : Panel
    {
        
        
        public InOutPortPanel()
        {
            //TODO unsub
            this.Loaded += InOutPortPanel_Loaded;
           
        }

        private void InOutPortPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = WpfUtilities.FindUpVisualTree<ItemsControl>(this);
            (itemsControl.ItemsSource as ObservableCollection<PortViewModel>).CollectionChanged += InOutPortPanel_CollectionChanged;
            //TODO unsubscribe collection change.

            //TODO likely need to iterate current children and sub them.
            foreach(var child in itemsControl.ItemsSource)
            {
                subscribeEventToCommands(itemsControl.ItemContainerGenerator, child as PortViewModel);
            }
        }

        private void InOutPortPanel_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var generator = WpfUtilities.FindUpVisualTree<ItemsControl>(this).ItemContainerGenerator;
                foreach (var portVM in e.NewItems)
                {
                    subscribeEventToCommands(generator, portVM as PortViewModel);

                    //TODO when items removed, remove all their handlers.
                }
            }));
          
        }

        private void UseLevelGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            var vm = ((sender as FrameworkElement).DataContext as PortViewModel);
            if (vm.MouseLeftButtonDownOnLevelCommand.CanExecute(vm))
            {
                vm.MouseLeftButtonDownOnLevelCommand.Execute(vm);
            }
        }

        private void HighlightOverlay_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            e.Handled = true;

            var vm = ((sender as FrameworkElement).DataContext as PortViewModel);
            if (vm.MouseLeaveCommand.CanExecute(vm))
            {
                vm.MouseLeaveCommand.Execute(vm);
            }
        }

        private void HighlightOverlay_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            e.Handled = true;

            var vm = ((sender as FrameworkElement).DataContext as PortViewModel);
            if (vm.MouseEnterCommand.CanExecute(vm))
            {
                vm.MouseEnterCommand.Execute(vm);
            }
        }

        private void SnapRegion_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            var vm = ((sender as FrameworkElement).DataContext as PortViewModel);
            if (vm.MouseLeftButtonDownCommand.CanExecute(vm))
            {
                vm.MouseLeftButtonDownCommand.Execute(vm);
            }
        }

        private void SnapRegion_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            e.Handled = true;

            var vm = ((sender as FrameworkElement).DataContext as PortViewModel);
            if (vm.MouseLeaveCommand.CanExecute(vm))
            {
                vm.MouseLeaveCommand.Execute(vm);
            }
        }

        private void SnapRegion_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            e.Handled = true;

            var vm = ((sender as FrameworkElement).DataContext as PortViewModel);
            if (vm.MouseEnterCommand.CanExecute(vm))
            {
                vm.MouseEnterCommand.Execute(vm);
            }
        }

        private void MainGrid_MouseLeftDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            var vm = ((sender as FrameworkElement).DataContext as PortViewModel);
            if (vm.ConnectCommand.CanExecute(vm))
            {
                vm.ConnectCommand.Execute(vm);
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (this.Children.Count <= 0)
            {
                // A port list without any port in it.
                return base.ArrangeOverride(arrangeSize);
            }

            var itemsControl = WpfUtilities.FindUpVisualTree<ItemsControl>(this);
            var generator = itemsControl.ItemContainerGenerator;

            int itemIndex = 0;
            double x = 0, y = 0;
            foreach (UIElement child in this.Children)
            {
                var portVm = generator.ItemFromContainer(child) as PortViewModel;
                var lineIndex = portVm.PortModel.LineIndex;
                var multiplier = ((lineIndex == -1) ? itemIndex : lineIndex);
                var portHeight = portVm.PortModel.Height;

                y = multiplier * portHeight;
                child.Arrange(new Rect(x, y, arrangeSize.Width, portHeight));
                itemIndex = itemIndex + 1;
            }

            return base.ArrangeOverride(arrangeSize);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (this.Children.Count <= 0)
                return new Size(0, 0);

            var cumulative = new Size(0, 0);
            foreach (UIElement child in this.Children)
            {
                // Default behavior of getting each child to measure.
                child.Measure(constraint);

                // All children should be stacked from top to bottom, so we 
                // will take the largest child's width as the final width.
                if (cumulative.Width < child.DesiredSize.Width)
                    cumulative.Width = child.DesiredSize.Width;

                // Having one child item stack on top of another.
                cumulative.Height += child.DesiredSize.Height;
            }

            return cumulative;
        }

        private void subscribeEventToCommands(ItemContainerGenerator generator, PortViewModel portVM )
        {
            var portUI = generator.ContainerFromItem(portVM);
            var mainGrid = portUI.ChildOfType<Grid>().ChildOfType<Grid>("mainGrid");
            var useLevelGrid = portUI.ChildOfType<Grid>().ChildOfType<Grid>("useLevelGrid");
            var snapRegion = mainGrid.ChildOfType<Rectangle>("snapRegion");
            var highlightOverlay = mainGrid.ChildOfType<Rectangle>("highlightOverlay");


            //TODO unsub all events - may not be required since these dep objects are all children of this object.
            mainGrid.MouseLeftButtonDown += MainGrid_MouseLeftDown;

            snapRegion.MouseEnter += SnapRegion_MouseEnter;
            snapRegion.MouseLeave += SnapRegion_MouseLeave;
            snapRegion.MouseLeftButtonDown += SnapRegion_MouseLeftButtonDown;

            highlightOverlay.MouseEnter += HighlightOverlay_MouseEnter;
            highlightOverlay.MouseLeave += HighlightOverlay_MouseLeave;

            useLevelGrid.MouseLeftButtonDown += UseLevelGrid_MouseLeftButtonDown;
        }

       
    }
}
