using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;
using Point = Autodesk.DesignScript.Geometry.Point;
using Vector = Autodesk.DesignScript.Geometry.Vector;

namespace Dynamo.Manipulation
{
    /// <summary>
    /// Interface to define visuals of a manipulator, called Gizmo.
    /// </summary>
    public interface IGizmo : IDisposable
    {
        /// <summary>
        /// Unique name of the Gizmo
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Base position of the Gizmo
        /// </summary>
        Point Origin { get; }

        /// <summary>
        /// Performs hit test based on view projection ray and returns the
        /// object which is hit. The hit object could be an axis, plane or
        /// rotational arc etc.
        /// </summary>
        /// <param name="source">Source of the view projection ray.</param>
        /// <param name="direction">View projection direction.</param>
        /// <param name="hitObject">The object hit by the input ray.</param>
        /// <returns>True when an object is hit, else false.</returns>
        bool HitTest(Point source, Vector direction, out object hitObject);

        /// <summary>
        /// Computes new offset based on the hit point.
        /// </summary>
        /// <param name="newPosition">New position of mouse click.</param>
        /// <param name="viewDirection">view projection direction</param>
        /// <returns>Offset vector of hit point wrt it's origin.</returns>
        Vector GetOffset(Point newPosition, Vector viewDirection);

        /// <summary>
        /// Gets render package for all the drawables of this Gizmo.
        /// </summary>
        /// <returns>List of render packages.</returns>
        IEnumerable<IRenderPackage> GetDrawables();

        /// <summary>
        /// 
        /// </summary>
        void HideTransientGraphics();

        /// <summary>
        /// Highlight gizmo drawables or create transient geometry to highlight the gizmo during mouse over
        /// </summary>
        /// <returns></returns>
        void HighlightGizmo();

        /// <summary>
        /// Unhighlight gizmo drawables or delete all transient geometry used to highlight gizmo during mouse over 
        /// </summary>
        void UnhighlightGizmo();
    }

    internal abstract class Gizmo : IGizmo
    {
        private const double zDepth = 1.0;

        private Point3D? cameraPosition;

        protected IWatch3DViewModel BackgroundPreviewViewModel { get; private set; }

        protected IRenderPackageFactory RenderPackageFactory { get; private set; }

        protected Point PointOrigin { get; set; }

        public string Name { get; set; }

        public Point Origin
        {
            get
            {
                var cameraPos = cameraPosition != null ?
                    Point.ByCoordinates(cameraPosition.Value.X, cameraPosition.Value.Y, cameraPosition.Value.Z) : null;

                if (cameraPos == null) throw new Exception("camera position is null");

                var vec = Vector.ByTwoPoints(cameraPos, PointOrigin).Normalized();
                return cameraPos.Add(vec.Scale(zDepth));
            }
        }

        internal Gizmo(IWatch3DViewModel backgroundPreviewViewModel, IRenderPackageFactory factory, 
            Point3D cameraPosition, Point pointOrigin)
        {
            BackgroundPreviewViewModel = backgroundPreviewViewModel;
            BackgroundPreviewViewModel.ViewCameraChanged += OnViewCameraChanged;
            RenderPackageFactory = factory;

            PointOrigin = pointOrigin;
            //cameraPosition = BackgroundPreviewViewModel.GetCameraPosition();
            this.cameraPosition = cameraPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract void RedrawCore();

        private void Redraw()
        {
            RedrawCore();
            BackgroundPreviewViewModel.AddGeometryForRenderPackages(GetDrawables());
        }

        private void OnViewCameraChanged(object o, RoutedEventArgs routedEventArgs)
        {
            cameraPosition = routedEventArgs.Source as Point3D?;

            // Redraw Gizmos
            Redraw();
        }

        public abstract bool HitTest(Point source, Vector direction, out object hitObject);

        public abstract Vector GetOffset(Point newPosition, Vector viewDirection);

        public abstract IEnumerable<IRenderPackage> GetDrawables();

        public abstract void HighlightGizmo();

        public abstract void UnhighlightGizmo();

        public abstract void HideTransientGraphics();

        public void Dispose()
        {
            BackgroundPreviewViewModel.ViewCameraChanged -= OnViewCameraChanged;
        }
    }
}
