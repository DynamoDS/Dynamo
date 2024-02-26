using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using Dynamo.Wpf.ViewModels.Watch3D;

using Autodesk.GeometryPrimitives.Dynamo.Geometry;
using Vector = Autodesk.GeometryPrimitives.Dynamo.Math.Vector3d;
using Matrix = Autodesk.GeometryPrimitives.Dynamo.Math.Matrix3d;

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
        /// Hit tolerance
        /// </summary>
        double tolerance = 0.15;


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
            ReferenceCoordinateSystem = Matrix.Identity;
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
            ReferenceCoordinateSystem = Matrix.Identity;
            UpdateGeometry(axis1, axis2, null, size);
        }

        /// <summary>
        /// Constructs a 3D gizmo, can be manipulated in all three directions.
        /// </summary>
        /// <param name="manipulator"></param>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="axis3">Third axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        public TranslationGizmo(NodeManipulator manipulator, Vector axis1, Vector axis2, Vector axis3, double size)
            : base(manipulator)
        {
            ReferenceCoordinateSystem = Matrix.Identity;
            UpdateGeometry(axis1, axis2, axis3, size);
        }

        /// <summary>
        /// Constructs a 3D gizmo, can be manipulated in all three directions.
        /// </summary>
        /// <param name="axis1">First axis of freedom</param>
        /// <param name="axis2">Second axis of freedom</param>
        /// <param name="axis3">Third axis of freedom</param>
        /// <param name="size">Visual size of the Gizmo</param>
        internal void UpdateGeometry(Vector axis1, Vector axis2, Vector axis3, double size)
        {
            if (axis1 == null) throw new ArgumentNullException(nameof(axis1));

            //Reset the dataset, but don't reset the cached hitAxis or hitPlane.
            //hitAxis and hitPlane are used to compute the offset for a move.
            axes.Clear();
            planes.Clear();
            
            scale = size;

            axes.Add(axis1);
            if (axis2 != null)
            {
                axes.Add(axis2);
                planes.Add(new Plane(Origin.Position, axis1 * axis2, axis1));
            }
            if (axis3 != null)
            {
                axes.Add(axis3);
                if (axis2 != null)
                {
                    planes.Add(new Plane(Origin.Position, axis2 * axis3, axis2));
                }
                planes.Add(new Plane(Origin.Position, axis3 * axis1, axis3));
            }

            if (axes.Count == 1 && hitAxis != null)
            {
                hitAxis = axes.First();
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Returns default color for a given axis
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
            var tol = 0.0001;
            var xAxis = new Vector(ReferenceCoordinateSystem[0, 0], ReferenceCoordinateSystem[0, 1], ReferenceCoordinateSystem[0, 2]);
            if ((axis * xAxis).IsNull(tol))
                return Axes.xAxis;

            var yAxis = new Vector(ReferenceCoordinateSystem[1, 0], ReferenceCoordinateSystem[1, 1], ReferenceCoordinateSystem[1, 2]);
            if ((axis * yAxis).IsNull(tol))
                return Axes.yAxis;

            var zAxis = new Vector(ReferenceCoordinateSystem[2, 0], ReferenceCoordinateSystem[2, 1], ReferenceCoordinateSystem[2, 2]);
            if ((axis * zAxis).IsNull(tol))
                return Axes.zAxis;

            return Axes.randomAxis;
        }

        private static double DistanceTo(Line line, Point pt)
        {
            var p = pt.Position;
            var sp = line.Position;
            var dir = line.Direction;

            // closest_dist = |Vector(p - sp) x dir| / |dir|
            var num = ((p - sp) * dir).Magnitude;
            var den = dir.Magnitude;
            return num/den;
        }

        private static double DistanceTo(Line line, Line ray)
        {
            var a1 = line.Position;
            var a2 = ray.Position;
            var b1 = line.Direction;
            var b2 = ray.Direction;

            // closest_dist = |(a1 - a2) % (b1 * b2) / |b1 * b2||
            return Math.Abs((a1 - a2) % (b1 * b2).Unit);
        }

        private static Point Intersect(Line line, Plane plane)
        {
            var d = line.Direction;
            var sp = line.Position;
            var n = plane.Normal;
            var p = plane.Origin;

            // intersection point (ip) = sp + t * d
            // equation of plane: n % (ip - p) = 0
            // substituting the intersection point into the plane equation:
            if(n% d == 0) return null; // line is parallel to the plane

            var t = (n % (p - sp)) / (n % d);
            var ip = sp + d * t;

            return new Point(ip);
        }

        // TODO: Implement this method
        private Point ClosestPointTo(Line line, Line ray)
        {
            var p1 = line.Position;
            var p2 = ray.Position;
            var d1 = line.Direction;
            var d2 = ray.Direction;

            var d1Crossd2 = d1 * d2;
            var sqrMagnitude = Math.Pow(d1Crossd2.Magnitude, 2);

            var planarFactor = (p2 - p1) % d1Crossd2;

            // Check if the lines are coplanar and not parallel
            if (Math.Abs(planarFactor) < tolerance && sqrMagnitude > tolerance)
            {
                var t1 = ((p2 - p1) * d2) % d1Crossd2 / sqrMagnitude;
                //var t2 = ((p2 - p1) * d1) % d1Crossd2 / Math.Pow(d1Crossd2.Magnitude, 2);

                return new Point(p1 + d1 * t1);
            }
            return Origin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private object HitTest(Point source, Vector direction)
        {

            var ray = GetRayGeometry(source, direction);
            //First hit test for position
            if (DistanceTo(ray, Origin) < tolerance)
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
                var pt = Intersect(ray, plane);
                if (pt == null) continue;

                var vec = new Vector(pt.Position.X - Origin.Position.X, pt.Position.Y - Origin.Position.Y,
                    pt.Position.Z - Origin.Position.Z);
                var dot1 = plane.UAxis % vec;

                var planeYAxis = plane.Normal * plane.UAxis;
                var dot2 = planeYAxis % vec;
                if (dot1 > 0 && dot2 > 0 && dot1 < scale / 2 && dot2 < scale / 2)
                {
                    return plane; //specific plane is hit
                }
            }

            foreach (var axis in axes)
            {
                var vec = axis.Unit;
                vec.Scale(scale);
                var line = new Line(Origin.Position, vec);

                if (DistanceTo(line, ray) < tolerance)
                {
                    return axis; //specific axis is hit.
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
            double size = (ManipulatorOrigin.Position - source.Position).Magnitude * 100;
            var vec = direction.Unit;
            vec.Scale(size);
            return new Line(source.Position, vec);
        }

        #endregion

        #region interface methods

        /// <summary>
        /// Reference coordinate system for the Gizmo
        /// </summary>
        public Matrix ReferenceCoordinateSystem { get; set; }

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
            var ray = GetRayGeometry(newPosition, viewDirection);
            if (hitPlane != null)
            {
                var testPlane = new Plane(ManipulatorOrigin.Position, hitPlane.Normal, hitPlane.UAxis);
                hitPoint = Intersect(ray, testPlane);
            }
            else if (hitAxis != null)
            {
                var axisLine = RayExtensions.ToOriginCenteredLine(ManipulatorOrigin, hitAxis);
                hitPoint = ClosestPointTo(axisLine, ray);
            }
            if (hitPoint == null)
            {
                return new Vector(0, 0, 0);
            }

            return new Vector(hitPoint.Position.X - ManipulatorOrigin.Position.X, hitPoint.Position.Y - ManipulatorOrigin.Position.Y, hitPoint.Position.Z - ManipulatorOrigin.Position.Z);
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
                plane => new Plane(Origin.Position, plane.Normal, plane.UAxis)).ToList();

            planes.Clear();

            planes.AddRange(newPlanes);
        }


        public override void DeleteTransientGraphics()
        {
            var identifier = $"{RenderDescriptions.AxisLine}_{Name}";
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
                DrawAxisLine(ref package, hitPlane.UAxis, "xAxisLine");
                drawables.Add(package);

                package = RenderPackageFactory.CreateRenderPackage();
                DrawAxisLine(ref package, hitPlane.Normal * hitPlane.UAxis, "yAxisLine");
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
            package.Description = $"{RenderDescriptions.AxisLine}_{Name}_{name}";

            var line = RayExtensions.ToOriginCenteredLine(Origin, axis);
            var color = GetAxisColor(GetAlignedAxis(axis));
            package.AddLineStripVertexCount(2);
            package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
            package.AddLineStripVertex(line.Position.X, line.Position.Y, line.Position.Z);
            package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
            var endPoint = line.Position + line.Direction;
            package.AddLineStripVertex(endPoint.X, endPoint.Y, endPoint.Z);
        }

        /// <summary>
        /// Draws plane at half the scale.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="plane"></param>
        /// <param name="name"></param>
        private void DrawPlane(ref IRenderPackage package, Plane plane, Planes name)
        {
            package.Description = $"{RenderDescriptions.ManipulatorPlane}_{Name}_{name}";

            var xAxis = plane.UAxis;
            xAxis.Scale(scale / 3);

            var yAxis = plane.Normal * plane.UAxis;
            yAxis.Scale(scale / 3);

            var p1 = Origin.Position + xAxis;
            var p2 = p1 + yAxis;
            var p3 = Origin.Position + yAxis;
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

        /// <summary>
        /// Draws axis as 3D arrow based on scale factor.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="axis"></param>
        private void DrawAxis(ref IRenderPackage package, Vector axis)
        {
            var axisType = GetAlignedAxis(axis);
            package.Description = $"{RenderDescriptions.ManipulatorAxis}_{Name}_{axisType}";

            var axis1 = new Vector(axis.X, axis.Y, axis.Z);
            axis1.Scale(axisOriginOffset);
            var axisStart = Origin.Position + axis1;

            var axis2 = new Vector(axis.X, axis.Y, axis.Z);
            axis2.Scale(scale);
            var axisEnd = Origin.Position + axis2;
            var color = GetAxisColor(axisType);
            package.AddLineStripVertexCount(2);
            package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
            package.AddLineStripVertex(axisStart.X, axisStart.Y, axisStart.Z);
            package.AddLineStripVertexColor(color.R, color.G, color.B, color.A);
            package.AddLineStripVertex(axisEnd.X, axisEnd.Y, axisEnd.Z);
        }

        #endregion
    }
}
