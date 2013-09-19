using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Resources;
using System;

namespace Dynamo.Controls
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

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
                this.MouseWheel += child_MouseWheel;
                //this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                //this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.PreviewMouseUp += child_MouseUp;
                this.PreviewMouseDown += child_MouseDown;
                this.MouseMove += child_MouseMove;
                //this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                //  child_PreviewMouseRightButtonDown);
            }

            
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
}
