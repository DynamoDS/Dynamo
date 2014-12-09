using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

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

        private void Initialize(UIElement element)
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

        public Point GetTranslateTransformOrigin()
        {
            var tt = GetTranslateTransform(child);
            return new Point(tt.X, tt.Y);
        }

        public void SetTranslateTransformOrigin(Point2D p)
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
                //double zoom = e.Delta > 0 ? .1 : -.1;
                double zoom = e.Delta > 0 ? 1 : -1;
                Point mousePosition = e.GetPosition(child);
                WorkspaceViewModel vm = DataContext as WorkspaceViewModel;
                vm.OnRequestZoomToViewportPoint(this, new ZoomEventArgs(zoom, mousePosition.AsDynamoType()));

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
            return ((DataContext as WorkspaceViewModel).IsPanning);
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
            this.itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("GridLines")
            {
                Mode = BindingMode.OneWay
            });
        }
    }
}
