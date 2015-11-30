using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;

namespace Dynamo.Manipulation
{
    /// <summary>
    /// Interface to define visuals of a manipulator, called Gizmo.
    /// </summary>
    public interface IGizmo
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
        /// 
        /// </summary>
        Point CameraPosition { get; }

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
        /// <param name="factory">Render package factory</param>
        /// <returns>List of render packages.</returns>
        IEnumerable<IRenderPackage> GetDrawables(IRenderPackageFactory factory);

        /// <summary>
        /// Highlight gizmo drawables or create transient geometry to highlight the gizmo during mouse over
        /// </summary>
        /// <param name="backgroundPreviewViewModel"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        void HighlightGizmo(IWatch3DViewModel backgroundPreviewViewModel, IRenderPackageFactory factory);

        /// <summary>
        /// Unhighlight gizmo drawables or delete all transient geometry used to highlight gizmo during mouse over 
        /// </summary>
        /// <param name="backgroundPreviewViewModel"></param>
        void UnhighlightGizmo(IWatch3DViewModel backgroundPreviewViewModel);
    }

    internal abstract class Gizmo : IGizmo
    {
        private const double zDepth = 1.0;

        public string Name { get; set; }

        protected IWatch3DViewModel BackgroundPreviewViewModel { get; private set; }
        protected Point origin;

        public Point Origin
        {
            get
            {
                var vec = Vector.ByTwoPoints(CameraPosition, origin).Normalized();
                return CameraPosition.Add(vec.Scale(zDepth));
            }
        }

        public Point CameraPosition { get; protected set; }

        internal Gizmo(IWatch3DViewModel backgroundPreviewViewModel, Point origin, Point cameraPosition)
        {
            BackgroundPreviewViewModel = backgroundPreviewViewModel;
            this.origin = origin;
            CameraPosition = cameraPosition;
        }

        public abstract bool HitTest(Point source, Vector direction, out object hitObject);

        public abstract Vector GetOffset(Point newPosition, Vector viewDirection);

        public abstract IEnumerable<IRenderPackage> GetDrawables(IRenderPackageFactory factory);

        public abstract void HighlightGizmo(IWatch3DViewModel backgroundPreviewViewModel, IRenderPackageFactory factory);

        public abstract void UnhighlightGizmo(IWatch3DViewModel backgroundPreviewViewModel);

    }
}
