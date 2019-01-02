using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;

namespace Dynamo.Manipulation
{

    /// <summary>
    /// Translation Gizmo, that handles translation.
    /// </summary>
    class TranslationGizmo : Gizmo
    {
        /// <summary>
        /// Defines type of planes.
        /// </summary>
        enum Planes
        {
            xyPlane,
            yzPlane,
            zxPlane,
        }

        /// <summary>
        /// Defines type of axis.
        /// </summary>
        enum Axes
        {
            xAxis,
            yAxis,
            zAxis,
            randomAxis, //Arbitrary axis
        }

        #region Private members

        /// <summary>
        /// An offset distance from the gizmo Origin
        /// at which to place the axes in their respective directions
        /// </summary>
        private const double axisOriginOffset = 0.2;

        /// <summary>
        /// List of axis available for manipulation
        /// </summary>
        private readonly List<Vector> axes = new List<Vector>();

        /// <summary>
        /// List of planes available for manipulation
        /// </summary>
        private readonly List<Plane> planes = new List<Plane>();

        /// <summary>
        /// Scale to draw the gizmo
        /// </summary>
        private double scale = 1.0;

        /// <summary>
        /// Hit Data
        /// </summary>
        private Vector hitAxis = null;
        private Plane hitPlane = null;

        #endregion

        #region public methods and constructors

        /// <summary>
        /// Constructs a linear gizmo, can be moved in one direction only.
        /// </summary>
        /// <param name="manipulator"></param>
        /// <param name="axis1">Axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public TranslationGizmo(NodeManipulator manipulator, Vector axis1, double size)
            : base(manipulator) 
        {
            ReferenceCoordinateSystem = CoordinateSystem.Identity();
            UpdateGeometry(axis1, null, null, size);
        }

        /// <summary>
        /// Constructs planar gizmo, can be manipulated in two directions.
        /// </summary>
        /// <param name="manipulator"></param>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public TranslationGizmo(NodeManipulator manipulator, Vector axis1, Vector axis2, double size)
            : base(manipulator)
        {
            ReferenceCoordinateSystem = CoordinateSystem.Identity();
            UpdateGeometry(axis1, axis2, null, size);
        }

        /// <summary>
        /// Construcs a 3D gizmo, can be manipulated in all three directions.
        /// </summary>
        /// <param name="manipulator"></param>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="axis3">Third axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public TranslationGizmo(NodeManipulator manipulator, Vector axis1, Vector axis2, Vector axis3, double size)
            : base(manipulator)
        {
            ReferenceCoordinateSystem = CoordinateSystem.Identity();
            UpdateGeometry(axis1, axis2, axis3, size);
        }

        /// <summary>
        /// Construcs a 3D gizmo, can be manipulated in all three directions.
        /// </summary>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="axis3">Third axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        internal void UpdateGeometry(Vector axis1, Vector axis2, Vector axis3, double size)
        {
            if (axis1 == null) throw new ArgumentNullException("axis1");

            //Reset the dataset, but don't reset the cached hitAxis or hitPlane.
            //hitAxis and hitPlane are used to compute the offset for a move.
            axes.Clear();
            planes.Clear();
            
            scale = size;

            axes.Add(axis1);
            if (axis2 != null)
            {
                axes.Add(axis2);
                planes.Add(Plane.ByOriginXAxisYAxis(Origin, axis1, axis2));
            }
            if (axis3 != null)
            {
                axes.Add(axis3);
                if (axis2 != null)
                {
                    planes.Add(Plane.ByOriginXAxisYAxis(Origin, axis2, axis3));
                }
                planes.Add(Plane.ByOriginXAxisYAxis(Origin, axis3, axis1));
            }

            if (axes.Count == 1 && hitAxis != null)
            {
                hitAxis = axes.First();
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Returs default color for a given axis
        /// </summary>
        /// <param name="axis">Axes</param>
        /// <returns>Color</returns>
        private Color GetAxisColor(Axes axis)
        {
            var col = Convert.ToByte(255);
            switch (axis)
            {
                case Axes.xAxis:
                    return Color.FromRgb(col, 0, 0);
                case Axes.yAxis:
                    return Color.FromRgb(0, col, 0);
                case Axes.zAxis:
                    return Color.FromRgb(0, 0, col);
                case Axes.randomAxis:
                    break;
                default:
                    break;
            }
            
            const byte colR = 0;
            var colG = Convert.ToByte(158);
            var colB = Convert.ToByte(255);
            return Color.FromRgb(colR, colG, colB);
        }

        /// <summary>
        /// Returns Axis enum along which given vector is aligned.
        /// </summary>
        /// <param name="axis">Axis vector</param>
        /// <returns>Axis</returns>
        private Axes GetAlignedAxis(Vector axis)
        {
            if (axis.IsParallel(ReferenceCoordinateSystem.XAxis))
                return Axes.xAxis;
            if (axis.IsParallel(ReferenceCoordinateSystem.YAxis))
                return Axes.yAxis;
            if (axis.IsParallel(ReferenceCoordinateSystem.ZAxis))
                return Axes.zAxis;

            return Axes.randomAxis;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private object HitTest(Point source, Vector direction)
        {
            double tolerance = 0.15; //Hit tolerance
            
            using (var ray = GetRayGeometry(source, direction))
            {
                //First hit test for position
                if (ray.DistanceTo(Origin) < tolerance)
                {
                    if (planes.Any())
                    {
                        return planes.First(); //Xy or first available plane is hit
                    }
                    return axes.First(); //xAxis or first axis is hit
                }

                foreach (var plane in planes)
                {
                    // plane needs to be up-to-date at this time with the current value of Origin
                    using (var pt = plane.Intersect(ray).FirstOrDefault() as Point)
                    {
                        if (pt == null) continue;

                        using (var vec = Vector.ByTwoPoints(Origin, pt))
                        {
                            var dot1 = plane.XAxis.Dot(vec);
                            var dot2 = plane.YAxis.Dot(vec);
                            if (dot1 > 0 && dot2 > 0 && dot1 < scale/2 && dot2 < scale/2)
                            {
                                return plane; //specific plane is hit
                            }
                        }
                    }
                }

                foreach (var axis in axes)
                {
                    using (var line = Line.ByStartPointDirectionLength(Origin, axis, scale))
                    {
                        if (line.DistanceTo(ray) < tolerance)
                        {
                            return axis; //specific axis is hit.
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private Line GetRayGeometry(Point source, Vector direction)
        {
            double size = ManipulatorOrigin.DistanceTo(source) * 100;
            return Line.ByStartPointDirectionLength(source, direction, size);
        }

        #endregion

        #region interface methods

        /// <summary>
        /// Reference coordinate system for the Gizmo
        /// </summary>
        public CoordinateSystem ReferenceCoordinateSystem { get; set; }

        /// <summary>
        /// Performs hit test on Gizmo to find out hit object. The returned 
        /// hitObject could be either axis vector or a plane.
        /// </summary>
        /// <param name="source">Mouse click source for hit test</param>
        /// <param name="direction">View projection direction</param>
        /// <param name="hitObject">object hit</param>
        /// <returns>True if Gizmo was hit successfully</returns>
        public override bool HitTest(Point source, Vector direction, out object hitObject)
        {
            hitAxis = null;
            hitPlane = null; //reset hit objects
            hitObject = HitTest(source, direction);
            hitAxis = hitObject as Vector;
            if (hitAxis == null)
            {
                hitPlane = hitObject as Plane;
            }

            return hitObject != null;
        }

        /// <summary>
        /// Computes move vector, based on new position of mouse and view direction.
        /// </summary>
        /// <param name="newPosition">New mouse position</param>
        /// <param name="viewDirection">view direction</param>
        /// <returns>Offset vector wrt manipulator origin</returns>
        public override Vector GetOffset(Point newPosition, Vector viewDirection)
        {
            Point hitPoint = Origin;
            using (var ray = GetRayGeometry(newPosition, viewDirection))
            {
                if (hitPlane != null)
                {
                    using (var testPlane = Plane.ByOriginXAxisYAxis(ManipulatorOrigin, hitPlane.XAxis, hitPlane.YAxis))
                    {
                        hitPoint = testPlane.Intersect(ray).FirstOrDefault() as Point;
                    }
                }
                else if (hitAxis != null)
                {
                    using (var axisLine = RayExtensions.ToOriginCenteredLine(ManipulatorOrigin, hitAxis))
                    {
                        hitPoint = axisLine.ClosestPointTo(ray);
                    }
                }
            }
            if (hitPoint == null)
            {
                return Vector.ByCoordinates(0, 0, 0);
            }

            return Vector.ByTwoPoints(ManipulatorOrigin, hitPoint);
        }

        /// <summary>
        /// Returns drawables to render this Gizmo
        /// </summary>
        /// <returns>List of render package</returns>
        public override RenderPackageCache GetDrawables()
        {
            var drawables = new RenderPackageCache();
            foreach (Vector axis in axes)
            {
                IRenderPackage package = RenderPackageFactory.CreateRenderPackage();
                DrawAxis(ref package, axis);
                drawables.Add(package);
            }

            var p = Planes.xyPlane;
            foreach (Plane plane in planes)
            {
                IRenderPackage package = RenderPackageFactory.CreateRenderPackage();
                DrawPlane(ref package, plane, p++);
                drawables.Add(package);
            }
            drawables.Add(GetDrawablesForTransientGraphics());

            return drawables;
        }

        public override void UpdateGizmoGraphics()
        {
            // Update gizmo geometry wrt to current Origin
            var newPlanes = planes.Select(
                plane => Plane.ByOriginXAxisYAxis(Origin, plane.XAxis, plane.YAxis)).ToList();

            planes.Clear();

            planes.AddRange(newPlanes);
        }


        public override void DeleteTransientGraphics()
        {
            var identifier = string.Format("{0}_{1}", RenderDescriptions.AxisLine, Name);
            BackgroundPreviewViewModel.DeleteGeometryForIdentifier(identifier);
        }

        #endregion

        #region Helper methods to draw the gizmo

        /// <summary>
        /// Returns drawables for transient geometry associated with Gizmo
        /// </summary>
        /// <returns></returns>
        public override RenderPackageCache GetDrawablesForTransientGraphics()
        {
            var drawables = new RenderPackageCache();
            if (null != hitAxis)
            {
                IRenderPackage package = RenderPackageFactory.CreateRenderPackage();
                DrawAxisLine(ref package, hitAxis, "xAxisLine");
                drawables.Add(package);
            }
            if (null != hitPlane)
            {
                IRenderPackage package = RenderPackageFactory.CreateRenderPackage();
                DrawAxisLine(ref package, hitPlane.XAxis, "xAxisLine");
                drawables.Add(package);

                package = RenderPackageFactory.CreateRenderPackage();
                DrawAxisLine(ref package, hitPlane.YAxis, "yAxisLine");
                drawables.Add(package);
            }

            return drawables;
        }

        /// <summary>
        /// Draws axis line
        /// </summary>
        /// <param name="package"></param>
        /// <param name="axis"></param>
        /// <param name="name"></param>
        private void DrawAxisLine(ref IRenderPackage package, Vector axis, string name)
        {
            package.Description = string.Format("{0}_{1}_{2}", RenderDescriptions.AxisLine, Name, name);
            using (var line = RayExtensions.ToOriginCenteredLine(Origin, axis))
            {
                var color = GetAxisColor(GetAlignedAxis(axis));
                package.AddLineStripVertexCount(2);
                package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                package.AddLineStripVertex(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z);
                package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                package.AddLineStripVertex(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z);
            }
        }

        /// <summary>
        /// Draws plane at half the scale.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="plane"></param>
        /// <param name="name"></param>
        private void DrawPlane(ref IRenderPackage package, Plane plane, Planes name)
        {
            package.Description = string.Format("{0}_{1}_{2}", RenderDescriptions.ManipulatorPlane, Name, name);
            using (var vec1 = plane.XAxis.Scale(scale/3))
            using (var vec2 = plane.YAxis.Scale(scale/3))
            using (var vec3 = plane.YAxis.Scale(scale/3))
            {
                using (var p1 = Origin.Add(vec1))
                using (var p2 = p1.Add(vec2))
                using (var p3 = Origin.Add(vec3))
                {
                    var axis = plane.Normal;
                    var color = GetAxisColor(GetAlignedAxis(axis));

                    package.AddLineStripVertexCount(3);
                    package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                    package.AddLineStripVertex(p1.X, p1.Y, p1.Z);

                    package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                    package.AddLineStripVertex(p2.X, p2.Y, p2.Z);

                    package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                    package.AddLineStripVertex(p3.X, p3.Y, p3.Z);
                    
                }
            }
        }

        /// <summary>
        /// Draws axis as 3D arrow based on scale factor.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="axis"></param>
        private void DrawAxis(ref IRenderPackage package, Vector axis)
        {
            var axisType = GetAlignedAxis(axis);
            package.Description = string.Format("{0}_{1}_{2}", RenderDescriptions.ManipulatorAxis, Name, axisType);

            using (var axisStart = Origin.Add(axis.Scale(axisOriginOffset)))
            using (var axisEnd = Origin.Add(axis.Scale(scale)))
            {
                var color = GetAxisColor(axisType);
                package.AddLineStripVertexCount(2);
                package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                package.AddLineStripVertex(axisStart.X, axisStart.Y, axisStart.Z);
                package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
                package.AddLineStripVertex(axisEnd.X, axisEnd.Y, axisEnd.Z);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            axes.ForEach(x => x.Dispose());
            planes.ForEach(x => x.Dispose());

            if(ReferenceCoordinateSystem != null) ReferenceCoordinateSystem.Dispose();

            base.Dispose(disposing);
        }
    }
}
