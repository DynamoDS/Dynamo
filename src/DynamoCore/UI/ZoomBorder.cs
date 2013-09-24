using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

namespace Dynamo.Controls
{
    public class ZoomBorder : Border
    {
        private FrameworkElement _mouseArea;
        public FrameworkElement MouseArea
        {
            get
            {
                if (_mouseArea == null)
                {
                    FrameworkElement outerCanvas = Parent as FrameworkElement;
                    _mouseArea = outerCanvas.Parent as FrameworkElement;
                }
                return _mouseArea;
            }
        }
        private UIElement child = null;
        private Point origin;
        private Point start;
        public EndlessGrid EndlessGrid {set; get;}

        private bool _panMode;
        public bool PanMode
        {
            get
            {
                return _panMode;
            }

            set
            {
                if (value)
                    this.Cursor = CursorsLibrary.HandPan;
                else
                    this.Cursor = Cursors.Arrow;
                _panMode = value;
            }
        }

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
                //this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                //this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                //this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                //  child_PreviewMouseRightButtonDown);
                Loaded += ZoomBorder_Loaded;
            }
        }

        void ZoomBorder_Loaded(object sender, RoutedEventArgs e)
        {
            // Uses Outer Canvas to trigger events
            MouseArea.MouseWheel += child_MouseWheel;
            MouseArea.MouseDown += child_MouseDown;
            MouseArea.MouseUp += child_MouseUp;
            MouseArea.MouseMove += child_MouseMove;
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

            updateGrid();
        }

        public void SetZoom(double zoom)
        {
            var st = GetScaleTransform(child);
            st.ScaleX = zoom;
            st.ScaleY = zoom;
        }

        private void updateGrid()
        {
            if (EndlessGrid != null)
            {
                var tt = GetTranslateTransform(child);
                var st = GetScaleTransform(child);
                EndlessGrid.TranslateAndZoomChanged(tt, st.ScaleX);
            }
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
            }
        }

        //private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (child != null)
        //    {
        //        var tt = GetTranslateTransform(child);
        //        start = e.GetPosition(this);
        //        origin = new Point(tt.X, tt.Y);
        //        this.Cursor = Cursors.Hand;
        //        child.CaptureMouse();
        //    }
        //}

        //private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (child != null)
        //    {
        //        child.ReleaseMouseCapture();
        //        this.Cursor = Cursors.Arrow;
        //    }
        //}

        private void child_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null &&
                ( e.ChangedButton == MouseButton.Middle
                || e.ChangedButton == MouseButton.Left && _panMode ))
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = CursorsLibrary.HandPanActive;
                child.CaptureMouse();
            }
        }

        private void child_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null && 
                ( e.ChangedButton == MouseButton.Middle
                || e.ChangedButton == MouseButton.Left && _panMode ))
            {
                child.ReleaseMouseCapture();

                if (!_panMode)
                    this.Cursor = Cursors.Arrow;
                else
                    this.Cursor = CursorsLibrary.HandPan;
            }
        }

        //void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    this.Reset();
        //}

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }

        #endregion
    }

    public class EndlessGrid : Canvas
    {
        #region Dependency Properties

        public static readonly DependencyProperty GridSpacingProperty;
        public static readonly DependencyProperty GridThicknessProperty;
        public static readonly DependencyProperty GridLineColorProperty;

        #endregion // Dependency Properties

        #region Static Constructor

        static EndlessGrid()
        {
            GridSpacingProperty = DependencyProperty.Register(
                "GridSpacing", typeof(int), typeof(EndlessGrid),
                new PropertyMetadata(100));

            GridThicknessProperty = DependencyProperty.Register(
                "GridThickness", typeof(int), typeof(EndlessGrid),
                new PropertyMetadata(2));

            GridLineColorProperty = DependencyProperty.Register(
                "GridLineColor", typeof(Color), typeof(EndlessGrid),
                new PropertyMetadata(Color.FromArgb(255, 232, 232, 232)));
        }

        #endregion

        #region Public Properties

        public int GridSpacing
        {
            get { return (int)base.GetValue(GridSpacingProperty); }
            set { base.SetValue(GridSpacingProperty, value); }
        }

        public int GridThickness
        {
            get { return (int)base.GetValue(GridThicknessProperty); }
            set { base.SetValue(GridThicknessProperty, value); }
        }

        public Color GridLineColor
        {
            get { return (Color)base.GetValue(GridLineColorProperty); }
            set { base.SetValue(GridLineColorProperty, value); }
        }

        #endregion

        #region Private Properties

        protected int _offset;
        protected FrameworkElement ViewingRegion;
        protected TranslateTransform _viewingTranslate;
        protected double _viewingZoom;
        protected double _viewingWidth;
        protected double _viewingHeight;

        #endregion

        public EndlessGrid(FrameworkElement viewingRegion)
        {
            this.ViewingRegion = viewingRegion;
            this.RenderTransform = new TranslateTransform();
            
            this.Loaded += EndlessGrid_Loaded;
        }

        public void TranslateAndZoomChanged(TranslateTransform actualTranslate, double zoom)
        {
            _viewingTranslate = actualTranslate;
            _viewingZoom = zoom;

            RecalculateGridPosition();
        }

        #region Event Handling

        void EndlessGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayRegion_SizeChanged(null, null);

            CreateGrid();
            this.ViewingRegion.SizeChanged += DisplayRegion_SizeChanged;
        }

        private void DisplayRegion_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewingSizeChanged(ViewingRegion.ActualWidth, ViewingRegion.ActualHeight);
        }

        #endregion

        #region Helper Methods

        protected void CreateGrid()
        {
            this.Children.Clear();

            Background = new SolidColorBrush(Colors.Transparent);

            _offset = (int)Math.Ceiling(GridSpacing * 2 / WorkspaceModel.ZOOM_MINIMUM);

            this.Width = _viewingWidth / WorkspaceModel.ZOOM_MINIMUM + _offset * 2;
            this.Height = _viewingHeight / WorkspaceModel.ZOOM_MINIMUM + _offset * 2;

            TranslateTransform tt = (this.RenderTransform as TranslateTransform);
            tt.X = -_offset;
            tt.Y = -_offset;

            // Draw Vertical Grid Lines
            for (double i = 0; i < this.Width; i += GridSpacing)
            {
                var xLine = new Line();
                xLine.Stroke = new SolidColorBrush(GridLineColor);
                xLine.StrokeThickness = GridThickness;
                xLine.X1 = i;
                xLine.Y1 = 0;
                xLine.X2 = i;
                xLine.Y2 = Height;
                xLine.HorizontalAlignment = HorizontalAlignment.Left;
                xLine.VerticalAlignment = VerticalAlignment.Center;
                this.Children.Add(xLine);
            }

            // Draw Horizontal Grid Lines
            for (double i = 0; i < this.Height; i += GridSpacing)
            {
                var yLine = new Line();
                yLine.Stroke = new SolidColorBrush(GridLineColor);
                yLine.StrokeThickness = GridThickness;
                yLine.X1 = 0;
                yLine.Y1 = i;
                yLine.X2 = Width;
                yLine.Y2 = i;
                yLine.HorizontalAlignment = HorizontalAlignment.Left;
                yLine.VerticalAlignment = VerticalAlignment.Center;
                this.Children.Add(yLine);
            }
        }

        protected void RecalculateGridPosition()
        {
            TranslateTransform tt = (this.RenderTransform as TranslateTransform);
            if (_viewingZoom != 0)
            {
                double scaledGridSpacing = GridSpacing * _viewingZoom;
                tt.X = -_offset - ((int)(_viewingTranslate.X / scaledGridSpacing)) * GridSpacing;
                tt.Y = -_offset - ((int)(_viewingTranslate.Y / scaledGridSpacing)) * GridSpacing;
            }
        }

        protected void ViewingSizeChanged(double viewingWidth, double viewingHeight)
        {
            _viewingWidth = viewingWidth;
            _viewingHeight = viewingHeight;

            CreateGrid();
            RecalculateGridPosition();
        }

        #endregion
    }
}
