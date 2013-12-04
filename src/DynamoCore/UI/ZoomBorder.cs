using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.UI.Commands;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Resources;
using System;
using System.Windows.Shapes;
using Dynamo.Core;

namespace Dynamo.Controls
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        public TranslateTransform GetChildTranslateTransform()
        {
            return GetTranslateTransform(child);
        }

        public ScaleTransform GetChildScaleTransform()
        {
            return GetScaleTransform(child);
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                Loaded += ZoomBorder_Loaded;
            }
        }

        void ZoomBorder_Loaded(object sender, RoutedEventArgs e)
        {
            // Uses Outer Canvas to trigger events
            this.MouseWheel += child_MouseWheel;
            this.MouseDown += child_MouseDown;
            this.MouseUp += child_MouseUp;
            this.MouseMove += child_MouseMove;
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        public void IncrementTranslateOrigin(double x, double y)
        {
            var tt = GetTranslateTransform(child);
            tt.X += x;
            tt.Y += y;
        }

        public Point GetTranslateTransformOrigin()
        {
            var tt = GetTranslateTransform(child);
            return new Point(tt.X, tt.Y);
        }

        public void SetTranslateTransformOrigin(Point p)
        {
            var tt = GetTranslateTransform(child);
            tt.X = p.X;
            tt.Y = p.Y;
        }

        public void SetZoom(double zoom)
        {
            var st = GetScaleTransform(child);
            st.ScaleX = zoom;
            st.ScaleY = zoom;
        }

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                double zoom = e.Delta > 0 ? .1 : -.1;
                Point mousePosition = e.GetPosition(child);
                WorkspaceViewModel vm = DataContext as WorkspaceViewModel;
                vm.OnRequestZoomToViewportPoint(this, new ZoomEventArgs(zoom, mousePosition));

                // Update WorkspaceModel without triggering property changed
                vm.SetCurrentOffsetCommand.Execute(GetTranslateTransformOrigin());

                // Reset Fit View Toggle
                if ( vm.ResetFitViewToggleCommand.CanExecute(null) )
                    vm.ResetFitViewToggleCommand.Execute(null);
            }
        }

        private void child_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null &&
                ( e.ChangedButton == MouseButton.Middle
                || e.ChangedButton == MouseButton.Left && IsInPanMode()))
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                child.CaptureMouse();
            }
        }

        private void child_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null && 
                ( e.ChangedButton == MouseButton.Middle
                || e.ChangedButton == MouseButton.Left && IsInPanMode()))
            {
                child.ReleaseMouseCapture();
            }
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    // Change ZoomBorder's child translation
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;

                    
                    WorkspaceViewModel vm = DataContext as WorkspaceViewModel;

                    // Update WorkspaceModel without triggering property changed
                    vm.SetCurrentOffsetCommand.Execute(GetTranslateTransformOrigin());
                    
                    // Reset Fit View Toggle
                    if (vm.ResetFitViewToggleCommand.CanExecute(null))
                        vm.ResetFitViewToggleCommand.Execute(null);
                }
            }
        }

        private bool IsInPanMode()
        {
            WorkspaceViewModel vm = DataContext as WorkspaceViewModel;
            return vm.CurrentState == WorkspaceViewModel.StateMachine.State.PanMode;
        }

        #endregion
    }

    public class EndlessGrid : Canvas 
    {
        private ItemsControl itemsControl;

        public EndlessGrid()
        {
            this.RenderTransform = new TranslateTransform();
            this.Loaded += EndlessGrid_Loaded;
        }

        void EndlessGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Create ItemsControl in Canvas to bind the grid line onto it
            this.itemsControl = new ItemsControl();

            FrameworkElementFactory factoryPanel = new FrameworkElementFactory(typeof(Canvas));
            factoryPanel.SetValue(StackPanel.IsItemsHostProperty, true);

            ItemsPanelTemplate template = new ItemsPanelTemplate();
            template.VisualTree = factoryPanel;

            itemsControl.ItemsPanel = template;
            

            this.Children.Add(itemsControl);

            this.Background = Brushes.Transparent;

            // Call ViewModel to compute data required for View
            ((EndlessGridViewModel)this.DataContext).InitializeOnce();

            CreateBinding();
        }
		
		private void CreateBinding()
        {
            // Visibility Binding
            this.itemsControl.SetBinding(FrameworkElement.VisibilityProperty, new Binding("FullscreenWatchShowing")
            {
                Converter = new InverseBoolToVisibilityConverter(),
                Mode = BindingMode.OneWay
            });

            // Size Binding
            this.SetBinding(EndlessGrid.WidthProperty, new Binding("Width")
            {
                Mode = BindingMode.OneWay
            });

            this.SetBinding(EndlessGrid.HeightProperty, new Binding("Height")
            {
                Mode = BindingMode.OneWay
            });

            // GridLine binds to ItemsControl
            this.itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("GridLines"){
                Mode = BindingMode.OneWay
            });
        }
    }
}
