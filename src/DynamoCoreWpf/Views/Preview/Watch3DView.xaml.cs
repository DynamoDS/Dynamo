using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.SharpDX.Core;
using SharpDX;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using Point = System.Windows.Point;
using HelixToolkit.SharpDX.Core.Cameras;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for WatchControl.xaml
    /// </summary>
    public partial class Watch3DView
    {
        #region private members

        private Point rightMousePoint;
        private Point3D prevCamera;
        private bool runUpdateClipPlane = false;

        #endregion

        #region public properties

        [Obsolete("Do not use! This will change its type in a future version of Dynamo.")]
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

            ViewModel.RequestCreateModels -= RequestCreateModelsHandler;
            ViewModel.RequestRemoveModels -= RequestRemoveModelsHandler;
            ViewModel.RequestViewRefresh -= RequestViewRefreshHandler;
            ViewModel.RequestClickRay -= GetClickRay;
            ViewModel.RequestCameraPosition -= GetCameraPosition;
            ViewModel.RequestZoomToFit -= ViewModel_RequestZoomToFit;
            this.DataContext = null;
            if(watch_view != null)
            {
                watch_view.Items.Clear();
                watch_view.DataContext = null;
                watch_view.Dispose();
            }
        
        }

        private void RegisterEventHandlers()
        {
            CompositionTarget.Rendering += CompositionTargetRenderingHandler;

            RegisterButtonHandlers();

            RegisterViewEventHandlers();

            ViewModel.RequestCreateModels += RequestCreateModelsHandler;
            ViewModel.RequestRemoveModels += RequestRemoveModelsHandler;
            ViewModel.RequestViewRefresh += RequestViewRefreshHandler;
            ViewModel.RequestClickRay += GetClickRay;
            ViewModel.RequestCameraPosition += GetCameraPosition;
            ViewModel.RequestZoomToFit += ViewModel_RequestZoomToFit;

            ViewModel.UpdateUpstream();
            ViewModel.OnWatchExecution();

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
            watch_view.MouseDown += ViewModel.OnViewMouseDown;
            watch_view.MouseUp += WatchViewMouseUphandler;
            watch_view.MouseMove += ViewModel.OnViewMouseMove;
            watch_view.CameraChanged += WatchViewCameraChangedHandler;
          
        }

        private void WatchViewCameraChangedHandler(object sender, RoutedEventArgs e)
        {
            var view = sender as Viewport3DX;
            if (view != null)
            {
                e.Source = view.GetCameraPosition();
            }
            ViewModel.OnViewCameraChanged(sender, e);
        }

        private void WatchViewMouseUphandler(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OnViewMouseUp(sender, e);
            //Call update on completion of user manipulation of the scene
            runUpdateClipPlane = true;
        }

        private void UnRegisterViewEventHandlers()
        {
            watch_view.MouseDown -= ViewModel.OnViewMouseDown;
            watch_view.MouseUp -= WatchViewMouseUphandler;
            watch_view.MouseMove -= ViewModel.OnViewMouseMove;
            watch_view.CameraChanged -= WatchViewCameraChangedHandler;
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
            Loaded -= ViewLoadedHandler;
            Unloaded -= ViewUnloadedHandler;
        }

        private void ViewLoadedHandler(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as HelixWatch3DViewModel;

            if (ViewModel == null) return;

            RegisterEventHandlers();
        }

        private void ViewModel_RequestZoomToFit(BoundingBox bounds)
        {
            var preCamDir = watch_view.Camera.LookDirection;
            //TODO, Call the equivalent method in Helix on adoption of next release, remove these private helix definitions.

            if (watch_view.Camera is HelixToolkit.Wpf.SharpDX.PerspectiveCamera perspectiveCam)
            {
                ZoomExtents(perspectiveCam, (float)(watch_view.ActualWidth / watch_view.ActualHeight), bounds, out var pos,
                    out var look, out var up);
                perspectiveCam.AnimateTo(pos.ToPoint3D(), look.ToVector3D(), up.ToVector3D(), 0);
            }

            else if (watch_view.Camera is OrthographicCamera orthCam && watch_view.Camera.CameraInternal is OrthographicCameraCore orthoCamCore)
            {
                ZoomExtents(orthoCamCore, (float)(watch_view.ActualWidth / watch_view.ActualHeight), bounds, out var pos,
                    out var look, out var up,out var width
                    );
                orthCam.AnimateWidth(width,0);
                orthCam.AnimateTo(pos.ToPoint3D(), look.ToVector3D(), up.ToVector3D(), 0);
            }

            //if after a zoom the camera is in an undefined position or view direction, reset it.
            if (watch_view.Camera.Position.ToVector3().IsUndefined() ||
                watch_view.Camera.LookDirection.ToVector3().IsUndefined() ||
                watch_view.Camera.LookDirection.Length == 0)
            {
                watch_view.Camera.Position = prevCamera;
                watch_view.Camera.LookDirection = preCamDir;
            }
        }

        #region ZoomExtents

        //This implementation is found in the current dev branch of Helix-Toolkit (https://github.com/helix-toolkit/helix-toolkit/tree/develop) with the assumption it will be included in the 2.25.0 release
        //The PR including these changes "Re-implementing zoom extents in sharpdx versions" is found here -> https://github.com/helix-toolkit/helix-toolkit/pull/2003
        //Specifically the code included here is located in https://github.com/holance/helix-toolkit/blob/develop/Source/HelixToolkit.SharpDX.Shared/Extensions/CameraCoreExtensions.cs in this commit:
        //https://github.com/holance/helix-toolkit/commit/660c85ff2218eb318f810dd682986d1816c5ece5
        //This implementation is adjusted to remove the instance method pattern.
        //This section can be removed when we adopt the next release of helix-toolkit and call the ZoomExtents() directly.

        private static void ZoomExtents(HelixToolkit.Wpf.SharpDX.PerspectiveCamera camera, float aspectRatio, BoundingBox boundingBox, out Vector3 position, out Vector3 lookDir, out Vector3 upDir)
        {
            var cameraDir = Vector3.Normalize(camera.LookDirection.ToVector3());
            var cameraUp = Vector3.Normalize(camera.UpDirection.ToVector3());
            var cameraRight = Vector3.Cross(cameraDir, cameraUp);
            cameraUp = Vector3.Cross(cameraRight, cameraDir);

            var corners = boundingBox.GetCorners();

            var frustum = new BoundingFrustum(camera.CreateViewMatrix() * camera.CreateProjectionMatrix(aspectRatio));
            var leftNormal = -frustum.Left.Normal;
            var rightNormal = -frustum.Right.Normal;
            var topNormal = -frustum.Top.Normal;
            var bottomNormal = -frustum.Bottom.Normal;

            int leftMostPoint = -1, rightMostPoint = -1, topMostPoint = -1, bottomMostPoint = -1;
            for (int i = 0; i < corners.Length; i++)
            {
                if (leftMostPoint < 0 && IsOutermostPointInDirection(i, ref leftNormal, corners))
                {
                    leftMostPoint = i;
                }
                if (rightMostPoint < 0 && IsOutermostPointInDirection(i, ref rightNormal, corners))
                {
                    rightMostPoint = i;
                }
                if (topMostPoint < 0 && IsOutermostPointInDirection(i, ref topNormal, corners))
                {
                    topMostPoint = i;
                }
                if (bottomMostPoint < 0 && IsOutermostPointInDirection(i, ref bottomNormal, corners))
                {
                    bottomMostPoint = i;
                }
            }

            var plane1 = new Plane(corners[leftMostPoint], leftNormal);
            var plane2 = new Plane(corners[rightMostPoint], rightNormal);
            PlaneIntersectsPlane(ref plane1, ref plane2, out var horizontalIntersection);
            plane1 = new Plane(corners[topMostPoint], topNormal);
            plane2 = new Plane(corners[bottomMostPoint], bottomNormal);
            PlaneIntersectsPlane(ref plane1, ref plane2, out var verticalIntersection);
            FindClosestPointsOnTwoLines(ref horizontalIntersection, ref verticalIntersection, out var closestPointLine1, out var closestPointLine2);
            position = Vector3.Dot(closestPointLine1 - closestPointLine2, cameraDir) < 0 ? closestPointLine1 : closestPointLine2;
            upDir = cameraUp;
            var boundPlane = new Plane(boundingBox.Center, cameraDir);
            var lookRay = new Ray(position, cameraDir);
            boundPlane.Intersects(ref lookRay, out float dist);
            lookDir = cameraDir * dist;
        }

        private static void ZoomExtents(OrthographicCameraCore camera, float aspectRatio, BoundingBox boundingBox, out Vector3 position, out Vector3 lookDir, out Vector3 upDir, out float width)
        {
            float minX = float.PositiveInfinity, minY = float.PositiveInfinity, maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
            var corners = boundingBox.GetCorners();
            var view = camera.CreateViewMatrix();
            foreach (var p in corners)
            {
                var local = Vector3.TransformCoordinate(p, view);
                minX = Math.Min(minX, local.X);
                minY = Math.Min(minY, local.Y);
                maxX = Math.Max(maxX, local.X);
                maxY = Math.Max(maxY, local.Y);
            }
            width = aspectRatio > 1 ? Math.Max((maxX - minX), (maxY - minY) * aspectRatio) : Math.Max((maxX - minX) / aspectRatio, maxY - minY);
            position = boundingBox.Center - camera.LookDirection.Normalized() * width;
            lookDir = camera.LookDirection.Normalized() * width;
            upDir = camera.UpDirection;
        }

        private static bool IsOutermostPointInDirection(int pointIndex, ref Vector3 direction, Vector3[] corners)
        {
            Vector3 point = corners[pointIndex];
            for (int i = 0; i < corners.Length; i++)
            {
                if (i != pointIndex && Vector3.Dot(direction, corners[i] - point) > 0)
                    return false;
            }

            return true;
        }

        // Credit: http://wiki.unity3d.com/index.php/3d_Math_functions
        // Returns the edge points of the closest line segment between 2 lines
        private static void FindClosestPointsOnTwoLines(ref Ray line1, ref Ray line2, out Vector3 closestPointLine1, out Vector3 closestPointLine2)
        {
            Vector3 line1Direction = line1.Direction;
            Vector3 line2Direction = line2.Direction;

            float a = Vector3.Dot(line1Direction, line1Direction);
            float b = Vector3.Dot(line1Direction, line2Direction);
            float e = Vector3.Dot(line2Direction, line2Direction);

            float d = a * e - b * b;

            Vector3 r = line1.Position - line2.Position;
            float c = Vector3.Dot(line1Direction, r);
            float f = Vector3.Dot(line2Direction, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = line1.Position + line1Direction * s;
            closestPointLine2 = line2.Position + line2Direction * t;
        }

        private static bool PlaneIntersectsPlane(ref Plane p1, ref Plane p2, out Ray intersection)
        {
            var dir = Vector3.Cross(p1.Normal, p2.Normal);
            float det = Vector3.Dot(dir, dir);
            if (Math.Abs(det) > float.Epsilon)
            {
                var p = (Vector3.Cross(dir, p2.Normal) * p1.D + Vector3.Cross(p1.Normal, dir) * p2.D) / det;
                intersection = new Ray(p, dir);
                return true;
            }
            intersection = default;
            return false;
        }

        #endregion

        private void RequestViewRefreshHandler()
        {
            View.InvalidateRender();
            //Call update to the clipping plane after the scene items are updated
            runUpdateClipPlane = true;
        }

        private void RequestCreateModelsHandler(RenderPackageCache packages, bool forceAsyncCall = false)
        {
            if (!forceAsyncCall && CheckAccess())
            {
                ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => ViewModel.GenerateViewGeometryFromRenderPackagesAndRequestUpdate(packages)));
            }
        }

        private void RequestRemoveModelsHandler(NodeModel node)
        {
            if (CheckAccess())
            {
                ViewModel.DeleteGeometryForNode(node);
            }
            else
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => ViewModel.DeleteGeometryForNode(node)));
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
            // https://github.com/DynamoDS/Dynamo/issues/7295
            // This should not crash Dynamo when View is null
            try
            {
                //Do not call the clip plane update on the render loop if the camera is unchanged or
                //the user is manipulating the view with mouse.  Do run when queued by runUpdateClipPlane bool 
                if (runUpdateClipPlane || (!View.Camera.Position.Equals(prevCamera) && !View.IsMouseCaptured))
                {
                    ViewModel.UpdateNearClipPlane();
                    runUpdateClipPlane = false;
                }
                ViewModel.ComputeFrameUpdate();
                prevCamera = View.Camera.Position;
            }
            catch(Exception ex)
            {
                ViewModel.CurrentSpaceViewModel.DynamoViewModel.Model.Logger.Log(ex.ToString());
            }         
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

        private Point3D GetCameraPosition()
        {
            return View.GetCameraPosition();
        }
    }

    
}
