using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System;

namespace Dynamo.Controls
{
    class ViewSettingsChangedEventArgs : EventArgs
    {
        internal ViewSettingsChangedEventArgs(double x, double y, double zoom)
        {
            X = x;
            Y = y;
            Zoom = zoom;
        }

        internal double X { get; private set; }
        internal double Y { get; private set; }
        internal double Zoom { get; private set; }
    }

    internal delegate void ViewSettingsChangedHandler(ViewSettingsChangedEventArgs args);

    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        internal event ViewSettingsChangedHandler ViewSettingsChanged;

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

            this.IsManipulationEnabled = true;
            this.ManipulationStarted += child_ManipulationStarted;
            this.ManipulationCompleted += child_ManipulationCompleted;
            this.ManipulationDelta += child_ManipulationDelta;
        }

        private double lastZoom;
        private bool zoomManipulated;
        private void child_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            WorkspaceViewModel vm = DataContext as WorkspaceViewModel;
            lastZoom = vm.Zoom;
            zoomManipulated = false;

            var tt = GetTranslateTransform(child);
            origin = new Point(tt.X, tt.Y);
        }

        private void child_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (e.TotalManipulation.Translation.X == 0 && e.TotalManipulation.Translation.Y == 0)
            {
                e.Cancel();
            }

            WorkspaceViewModel vm = DataContext as WorkspaceViewModel;
            vm.HandleTouchRelease(sender, e);
        }

        private void child_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            WorkspaceViewModel vm = DataContext as WorkspaceViewModel;
            if (e.Manipulators.Count() == 1 && vm.HandleTouchDrag(sender, e)) return;

            if (e.Manipulators.Count() == 1 && !zoomManipulated)
            {
                // Touch one finger to pan
                var tr = e.CumulativeManipulation.Translation;
                SetTranslateTransformOrigin(new Point2D(
                    origin.X + tr.X,
                    origin.Y + tr.Y));
                vm.SetCurrentOffsetCommand.Execute(GetTranslateTransformOrigin());
            }
            else
            {
                // Touch two or more fingers to zoom
                var sc = e.CumulativeManipulation.Scale;
                var newZoom = lastZoom * Math.Sqrt(sc.X * sc.Y);
                var anchorX = (e.Manipulators.Average(m => m.GetPosition(this).X) - vm.Model.X) / vm.Zoom;
                var anchorY = (e.Manipulators.Average(m => m.GetPosition(this).Y) - vm.Model.Y) / vm.Zoom;

                vm.OnRequestZoomPinch(this, new ZoomEventArgs(
                    newZoom, new Point2D(anchorX, anchorY)));
                zoomManipulated = true;
            }

            if (vm.ResetFitViewToggleCommand.CanExecute(null))
                vm.ResetFitViewToggleCommand.Execute(null);
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

            var st = GetScaleTransform(child);
            NotifyViewSettingsChanged(tt.X, tt.Y, st.ScaleX);
        }

        public void SetZoom(double zoom)
        {
            var st = GetScaleTransform(child);
            st.ScaleX = zoom;
            st.ScaleY = zoom;

            var tt = GetTranslateTransform(child);
            NotifyViewSettingsChanged(tt.X, tt.Y, zoom);
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
                if (vm.ResetFitViewToggleCommand.CanExecute(null))
                    vm.ResetFitViewToggleCommand.Execute(null);
            }
        }

        private void child_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null &&
                (e.ChangedButton == MouseButton.Middle
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
                e != null && 
                (e.ChangedButton == MouseButton.Middle
                || e.ChangedButton == MouseButton.Left && IsInPanMode()))
            {
                child.ReleaseMouseCapture();
            }
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if ((child == null) || !child.IsMouseCaptured)
                return;

            var vm = DataContext as WorkspaceViewModel;
            if (vm == null) return;

            // Change ZoomBorder's child translation
            Vector v = start - e.GetPosition(this);
            SetTranslateTransformOrigin(new Point2D
            {
                X = origin.X - v.X,
                Y = origin.Y - v.Y
            });

            // Update WorkspaceModel without triggering property changed
            vm.SetCurrentOffsetCommand.Execute(GetTranslateTransformOrigin());

            // Reset Fit View Toggle
            if (vm.ResetFitViewToggleCommand.CanExecute(null))
                vm.ResetFitViewToggleCommand.Execute(null);
        }

        private bool IsInPanMode()
        {
            var ws = DataContext as WorkspaceViewModel;
            if (ws == null) return false;
            return ws.IsPanning;
        }

        private void NotifyViewSettingsChanged(double x, double y, double zoom)
        {
            var handler = ViewSettingsChanged;
            if (handler != null)
            {
                handler(new ViewSettingsChangedEventArgs(x, y, zoom));
            }
        }

        #endregion
    }
}
