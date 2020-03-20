using System;
using System.Windows;
using System.Windows.Media.Media3D;
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
        /// Returns render package for all the drawables of this Gizmo.
        /// </summary>
        /// <returns>List of render packages.</returns>
        RenderPackageCache GetDrawables();

        RenderPackageCache GetDrawablesForTransientGraphics();

        /// <summary>
        /// Delete any transient graphics associated with the Gizmo
        /// such as those used in highlights, etc.
        /// </summary>
        void DeleteTransientGraphics();

        /// <summary>
        /// Highlight gizmo drawables or create transient geometry to highlight the gizmo during mouse over
        /// </summary>
        /// <returns></returns>
        void HighlightGizmo();

        /// <summary>
        /// Unhighlight gizmo drawables or delete all transient geometry used to highlight gizmo during mouse over 
        /// </summary>
        void UnhighlightGizmo();

        /// <summary>
        /// Update graphics widgets associated with the gizmo whenever it needs to be updated
        /// for example, after a view transformation
        /// </summary>
        void UpdateGizmoGraphics();
    }

    internal abstract class Gizmo : IGizmo
    {
        private const double zDepth = 20.0;

        private readonly NodeManipulator manipulator;

        private Point3D? cameraPosition;

        protected IWatch3DViewModel BackgroundPreviewViewModel
        {
            get { return manipulator.BackgroundPreviewViewModel; }
        }

        protected IRenderPackageFactory RenderPackageFactory
        {
            get { return manipulator.RenderPackageFactory; }
        }

        /// <summary>
        /// Physical location of the manipulator lying on the geometry being manipulated
        /// </summary>
        protected Point ManipulatorOrigin
        {
            get { return manipulator.Origin; }
        }

        private string name = "gizmo";
        public string Name 
        { 
            get { return name; }
            set
            {
                if (!name.Contains(value))
                {
                    name = string.Format("{0}_{1}", name, value);
                }
            } 
        }

        private Point origin;
        /// <summary>
        /// Base position of the Gizmo which is at a fixed distance from the camera
        /// so that it is redrawn at the same size to the viewer
        /// </summary>
        protected Point Origin
        {
            get
            {
                if(origin != null) origin.Dispose();

                using (var cameraPos = cameraPosition != null
                    ? Point.ByCoordinates(cameraPosition.Value.X, cameraPosition.Value.Y, cameraPosition.Value.Z)
                    : null)
                {

                    if (cameraPos == null)
                    {
                        // cameraPos will be null if HelixWatch3DViewModel is not initialized
                        // this happens on an out of memory exception in SharpDX probably due to 
                        // DynamoEffectsManager not being disposed off promptly.
                        // TODO: revisit to fix this properly later
                        // For the time being return a default position instead of throwing an exception
                        // to the effect that camerPos should not be null
                        return Point.ByCoordinates(ManipulatorOrigin.X, ManipulatorOrigin.Y, ManipulatorOrigin.Z);
                    }

                    using (var vec = Vector.ByTwoPoints(cameraPos, ManipulatorOrigin).Normalized())
                    {
                        origin = cameraPos.Add(vec.Scale(zDepth));
                    }
                }
                return origin;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        internal Gizmo(NodeManipulator manipulator)
        {
            this.manipulator = manipulator;

            // Append node AST identifier to gizmo name
            // so that it gets added to package description
            Name = manipulator.Node.AstIdentifierBase;
            
            BackgroundPreviewViewModel.ViewCameraChanged += OnViewCameraChanged;

            cameraPosition = manipulator.CameraPosition;
        }

        private void Redraw()
        {
            if (manipulator.IsEnabled())
            {
                BackgroundPreviewViewModel.AddGeometryForRenderPackages(GetDrawables());
            }
        }

        private void OnViewCameraChanged(object o, RoutedEventArgs routedEventArgs)
        {
            var newCameraPosition = routedEventArgs.Source as Point3D?;
            // A change of behavior in helix when upgrading from v2015 is that the CameraChanged event gets fired
            // several times, once for each individual property of Camera that changed.
            // In order to avoid rendering the Gizmo several times, which leads to choppy camera rotation, we limit the
            // rendering of the Gizmo to only be done when the position of the camera changed.
            if (cameraPosition != newCameraPosition)
            {
                cameraPosition = newCameraPosition;
                // Redraw Gizmos
                Redraw();
            }
        }

        public abstract bool HitTest(Point source, Vector direction, out object hitObject);

        public abstract Vector GetOffset(Point newPosition, Vector viewDirection);

        public abstract RenderPackageCache GetDrawables();

        public abstract RenderPackageCache GetDrawablesForTransientGraphics();

        public abstract void UpdateGizmoGraphics();

        public abstract void DeleteTransientGraphics();

        public void HighlightGizmo()
        {
            var drawables = GetDrawablesForTransientGraphics();
            BackgroundPreviewViewModel.AddGeometryForRenderPackages(drawables);
        }

        public void UnhighlightGizmo()
        {
            // Delete all transient geometry used to highlight gizmo
            DeleteTransientGraphics();
        }

        public void Dispose()
        {
            Dispose(true);

            if(origin != null) origin.Dispose();

            BackgroundPreviewViewModel.ViewCameraChanged -= OnViewCameraChanged;
        }
    }
}
