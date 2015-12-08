using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using GeometryModel3D = HelixToolkit.Wpf.SharpDX.GeometryModel3D;
using Model3D = HelixToolkit.Wpf.SharpDX.Model3D;
using Point = System.Windows.Point;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class Watch3DView
    {
        #region private members

        private Point rightMousePoint;

        #endregion

        #region public properties

        public Viewport3DX View
        {
            get { return watch_view; }
        }

        internal HelixWatch3DViewModel ViewModel { get; private set; }

        #endregion

        #region constructors

        public Watch3DView()
        {
            InitializeComponent();
            Loaded += ViewLoadedHandler;
            Unloaded += ViewUnloadedHandler;
        }

        #endregion

        #region event registration

        private void UnregisterEventHandlers()
        {
            UnregisterButtonHandlers();

            CompositionTarget.Rendering -= CompositionTargetRenderingHandler;

            if (ViewModel == null) return;

            UnRegisterViewEventHandlers();

            ViewModel.RequestAttachToScene -= ViewModelRequestAttachToSceneHandler;
            ViewModel.RequestCreateModels -= RequestCreateModelsHandler;
            ViewModel.RequestViewRefresh -= RequestViewRefreshHandler;
            ViewModel.RequestClickRay -= GetClickRay;
            ViewModel.RequestZoomToFit -= ViewModel_RequestZoomToFit;
        }

        private void RegisterButtonHandlers()
        {
            MouseLeftButtonDown += MouseButtonIgnoreHandler;
            MouseLeftButtonUp += MouseButtonIgnoreHandler;
            MouseRightButtonUp += view_MouseRightButtonUp;
            PreviewMouseRightButtonDown += view_PreviewMouseRightButtonDown;
        }

        private void RegisterViewEventHandlers()
        {
            watch_view.MouseDown += (sender, args) =>
            {
                ViewModel.OnViewMouseDown(sender, args);
            };

            watch_view.MouseUp += (sender, args) =>
            {
                ViewModel.OnViewMouseUp(sender, args);
            };

            watch_view.MouseMove += (sender, args) =>
            {
                ViewModel.OnViewMouseMove(sender, args);
            };
        }

        private void UnRegisterViewEventHandlers()
        {
            watch_view.MouseDown -= ViewModel.OnViewMouseDown;
            watch_view.MouseUp -= ViewModel.OnViewMouseUp;
            watch_view.MouseMove -= ViewModel.OnViewMouseMove;
         }		         

        private void UnregisterButtonHandlers()
        {
            MouseLeftButtonDown -= MouseButtonIgnoreHandler;
            MouseLeftButtonUp -= MouseButtonIgnoreHandler;
            MouseRightButtonUp -= view_MouseRightButtonUp;
            PreviewMouseRightButtonDown -= view_PreviewMouseRightButtonDown;
        }

        #endregion

        #region event handlers

        private void ViewUnloadedHandler(object sender, RoutedEventArgs e)
        {
            UnregisterEventHandlers();
        }

        private void ViewLoadedHandler(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as HelixWatch3DViewModel;

            if (ViewModel == null) return;

            CompositionTarget.Rendering += CompositionTargetRenderingHandler;

            RegisterButtonHandlers();

            RegisterViewEventHandlers();

            ViewModel.RequestAttachToScene += ViewModelRequestAttachToSceneHandler;
            ViewModel.RequestCreateModels += RequestCreateModelsHandler;
            ViewModel.RequestViewRefresh += RequestViewRefreshHandler;
            ViewModel.RequestClickRay += GetClickRay;
            ViewModel.RequestZoomToFit += ViewModel_RequestZoomToFit;
        }

        private void ViewModel_RequestZoomToFit(BoundingBox bounds)
        {
            watch_view.ZoomExtents(bounds.ToRect3D());
        }

        private void RequestViewRefreshHandler()
        {
            View.InvalidateRender();
        }

        private void RequestCreateModelsHandler(IEnumerable<IRenderPackage> packages)
        {
            if (CheckAccess())
            {
                ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages)));
            }
        }

        private void ViewModelRequestAttachToSceneHandler(Model3D model3D)
        {
            if (!model3D.IsAttached && View != null && View.RenderHost != null)
            {
                model3D.Attach(View.RenderHost);
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (xAdjust >= inputGrid.MinWidth)
            {
                Width = xAdjust;
            }

            if (yAdjust >= inputGrid.MinHeight)
            {
                Height = yAdjust;
            }
        }

        private void CompositionTargetRenderingHandler(object sender, EventArgs e)
        {
            ViewModel.UpdateNearClipPlane();
            ViewModel.ComputeFrameUpdate();
        }

        private void MouseButtonIgnoreHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void view_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightMousePoint = e.GetPosition(watch3D);
        }

        private void view_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if the mouse has moved, and this is a right click, we assume 
            // rotation. handle the event so we don't show the context menu
            // if the user wants the contextual menu they can click on the
            // node sidebar or top bar
            if (e.GetPosition(watch3D) != rightMousePoint)
            {
                e.Handled = true;
            }
        }

        #endregion

        private IRay GetClickRay(MouseEventArgs args)
        {
            var mousePos = args.GetPosition(this);

            var ray = View.Point2DToRay3D(new Point(mousePos.X, mousePos.Y));

            if (ray == null) return null;

            var position = new Point3D(0, 0, 0);
            var normal = new Vector3D(0, 0, 1);
            var pt3D = ray.PlaneIntersection(position, normal);

            if (pt3D == null) return null;

            return new Ray3(ray.Origin, ray.Direction);
        }
    }

    
}
