using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Controls
{
    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

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
                this.MouseUp += child_MouseUp;
                this.MouseDown += child_MouseDown;

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

        #region Child Events

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = e.Delta > 0 ? .1 : -.1;

                if (!(e.Delta > 0) && (st.ScaleX <= .2 || st.ScaleY <= .2))
                    return;

                Point relative = e.GetPosition(child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                //Debug.WriteLine(st.ScaleX);

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;

                dynWorkspaceViewModel viewModel = DataContext as dynWorkspaceViewModel;
                if (viewModel.SetZoomCommand.CanExecute(st.ScaleX))
                    viewModel.SetZoomCommand.Execute(st.ScaleX);
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
            if (child != null && e.ChangedButton == MouseButton.Middle)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null && e.ChangedButton == MouseButton.Middle)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
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
