using System;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>An infinite oriented plane defined by an origin, normal, and local axes.</summary>
    public sealed class Plane : Geometry
    {
        private Plane(Point origin, Vector xAxis, Vector yAxis, Vector normal)
        {
            Origin = origin;
            XAxis = xAxis;
            YAxis = yAxis;
            Normal = normal;
        }

        /// <summary>Gets the origin point on the plane.</summary>
        public Point Origin { get; }

        /// <summary>Gets the unit normal vector of the plane.</summary>
        public Vector Normal { get; }

        /// <summary>Gets the unit X axis lying in the plane.</summary>
        public Vector XAxis { get; }

        /// <summary>Gets the unit Y axis lying in the plane.</summary>
        public Vector YAxis { get; }

        /// <summary>Gets the world XY plane at the origin.</summary>
        public static Plane XY() => new(Point.Origin(), Vector.XAxis(), Vector.YAxis(), Vector.ZAxis());

        /// <summary>Gets the world YZ plane at the origin.</summary>
        public static Plane YZ() => new(Point.Origin(), Vector.YAxis(), Vector.ZAxis(), Vector.XAxis());

        /// <summary>Gets the world XZ plane at the origin.</summary>
        public static Plane XZ() => new(Point.Origin(), Vector.XAxis(), Vector.ZAxis(), Vector.ByCoordinates(0, -1, 0));

        /// <summary>Creates a plane from an origin point and normal vector.</summary>
        /// <param name="origin">Origin point.</param>
        /// <param name="normal">Plane normal vector.</param>
        /// <returns>The oriented plane.</returns>
        public static Plane ByOriginNormal(Point origin, Vector normal)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(normal);
            var n = normal.Normalized();
            var refAxis = Math.Abs(n.Z) < 0.9 ? Vector.ZAxis() : Vector.YAxis();
            var x = refAxis.Cross(n).Normalized();
            var y = n.Cross(x).Normalized();
            return new Plane(Point.ByCoordinates(origin.X, origin.Y, origin.Z), x, y, n);
        }

        /// <summary>Creates a plane from an origin, normal, and X axis direction.</summary>
        /// <param name="origin">Origin point.</param>
        /// <param name="normal">Plane normal vector.</param>
        /// <param name="xAxis">Reference X axis direction.</param>
        /// <returns>The oriented plane.</returns>
        public static Plane ByOriginNormalXAxis(Point origin, Vector normal, Vector xAxis)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(normal);
            ArgumentNullException.ThrowIfNull(xAxis);
            var n = normal.Normalized();
            var projX = xAxis.Subtract(n.Scale(xAxis.Dot(n)));
            var x = projX.Normalized();
            var y = n.Cross(x).Normalized();
            return new Plane(Point.ByCoordinates(origin.X, origin.Y, origin.Z), x, y, n);
        }

        /// <summary>Creates a plane passing through three non-collinear points.</summary>
        /// <param name="p1">First point (origin).</param>
        /// <param name="p2">Second point defining the X direction.</param>
        /// <param name="p3">Third point defining the plane.</param>
        /// <returns>The plane through the three points.</returns>
        public static Plane ByThreePoints(Point p1, Point p2, Point p3)
        {
            ArgumentNullException.ThrowIfNull(p1);
            ArgumentNullException.ThrowIfNull(p2);
            ArgumentNullException.ThrowIfNull(p3);
            var v1 = Vector.ByTwoPoints(p1, p2);
            var v2 = Vector.ByTwoPoints(p1, p3);
            var n = v1.Cross(v2).Normalized();
            return ByOriginNormalXAxis(p1, n, v1);
        }

        /// <summary>Creates a parallel plane offset along its normal.</summary>
        /// <param name="distance">Signed offset distance along the normal.</param>
        /// <returns>The offset plane.</returns>
        public Plane Offset(double distance)
        {
            if (!double.IsFinite(distance))
                throw new ArgumentException("Offset distance must be finite.");
            return new Plane(Origin.Add(Normal.Scale(distance)), XAxis, YAxis, Normal);
        }

        /// <inheritdoc />
        public override BoundingBox BoundingBox => BoundingBox.ByCorners(Origin, Origin);

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return new Plane(
                Point.ByCoordinates(Origin.X + xTranslation, Origin.Y + yTranslation, Origin.Z + zTranslation),
                XAxis, YAxis, Normal);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var newOrigin = (Point)Origin.Rotate(origin, axis, degrees);
            var newX = GeometryMath.RotateVector(XAxis, axis, degrees).Normalized();
            var newN = GeometryMath.RotateVector(Normal, axis, degrees).Normalized();
            return ByOriginNormalXAxis(newOrigin, newN, newX);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            var newOrigin = (Point)Origin.Scale(basePoint, xamount, yamount, zamount);
            return new Plane(newOrigin, XAxis, YAxis, Normal);
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var newOrigin = Point.ByCartesianCoordinates(cs, Origin.X, Origin.Y, Origin.Z);
            var newX = cs.TransformVector(XAxis).Normalized();
            var newY = cs.TransformVector(YAxis).Normalized();
            var newN = newX.Cross(newY).Normalized();
            return ByOriginNormalXAxis(newOrigin, newN, newX);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            var n = mirrorPlane.Normal.Normalized();
            var newOrigin = (Point)Origin.Mirror(mirrorPlane);
            var newNormal = Normal.Subtract(n.Scale(2.0 * Normal.Dot(n))).Normalized();
            var newX = XAxis.Subtract(n.Scale(2.0 * XAxis.Dot(n))).Normalized();
            return ByOriginNormalXAxis(newOrigin, newNormal, newX);
        }

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            var s = 2.5;
            var cs = CoordinateSystem.ByPlane(this);
            var a = Point.ByCartesianCoordinates(cs, s, s, 0);
            var b = Point.ByCartesianCoordinates(cs, -s, s, 0);
            var c = Point.ByCartesianCoordinates(cs, -s, -s, 0);
            var d = Point.ByCartesianCoordinates(cs, s, -s, 0);

            package.AddTriangleVertex(a.X, a.Y, a.Z);
            package.AddTriangleVertex(b.X, b.Y, b.Z);
            package.AddTriangleVertex(c.X, c.Y, c.Z);
            package.AddTriangleVertex(c.X, c.Y, c.Z);
            package.AddTriangleVertex(d.X, d.Y, d.Z);
            package.AddTriangleVertex(a.X, a.Y, a.Z);

            for (var i = 0; i < 6; i++)
            {
                package.AddTriangleVertexNormal(Normal.X, Normal.Y, Normal.Z);
                package.AddTriangleVertexUV(0, 0);
            }
        }
    }
}
