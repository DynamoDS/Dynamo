using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A closed planar ellipse curve.</summary>
    public class Ellipse : Curve
    {
        internal Ellipse(NativeGeometry.CurveHandle handle, Point center, Vector majorAxis, Vector minorAxis)
            : base(handle)
        {
            CenterPoint = center;
            MajorAxis = majorAxis;
            MinorAxis = minorAxis;
        }

        /// <summary>Gets the center point of the ellipse.</summary>
        public Point CenterPoint { get; }

        /// <summary>Gets the major axis vector (scaled by the major semi-axis length).</summary>
        public Vector MajorAxis { get; }

        /// <summary>Gets the minor axis vector (scaled by the minor semi-axis length).</summary>
        public Vector MinorAxis { get; }

        /// <inheritdoc />
        public override Vector Normal
        {
            get
            {
                var radial = Vector.ByTwoPoints(CenterPoint, PointAtParameter(0.0));
                var cross = radial.Cross(TangentAtParameter(0.0));
                return cross.Length > 1e-9 ? cross.Normalized() : MajorAxis.Cross(MinorAxis).Normalized();
            }
        }

        /// <summary>Creates an ellipse in the world XY plane centered at <paramref name="origin"/>.</summary>
        /// <param name="origin">Origin point of the ellipse.</param>
        /// <param name="xAxisRadius">Semi-axis radius along world X.</param>
        /// <param name="yAxisRadius">Semi-axis radius along world Y.</param>
        /// <returns>The created ellipse.</returns>
        public static Ellipse ByOriginRadii(Point origin, double xAxisRadius, double yAxisRadius)
        {
            ArgumentNullException.ThrowIfNull(origin);
            return ByPlaneRadii(Plane.ByOriginNormal(origin, Vector.ZAxis()), xAxisRadius, yAxisRadius);
        }

        /// <summary>Creates an ellipse centered at <paramref name="origin"/> using orthogonal axis vectors whose lengths define the semi-axes.</summary>
        /// <param name="origin">Origin point of the ellipse.</param>
        /// <param name="xAxisRadius">First axis vector (direction and semi-axis length).</param>
        /// <param name="yAxisRadius">Second axis vector (direction and semi-axis length, orthogonal to the first).</param>
        /// <returns>The created ellipse.</returns>
        public static Ellipse ByOriginVectors(Point origin, Vector xAxisRadius, Vector yAxisRadius)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(xAxisRadius);
            ArgumentNullException.ThrowIfNull(yAxisRadius);

            var rx = xAxisRadius.Length;
            var ry = yAxisRadius.Length;
            var ux = xAxisRadius.Normalized();
            var uy = yAxisRadius.Normalized();
            var handle = NativeGeometry.Ellipse(origin.Position, ux.Components, uy.Components, rx, ry);
            var (major, minor) = rx >= ry ? (xAxisRadius, yAxisRadius) : (yAxisRadius, xAxisRadius);
            return new Ellipse(handle, origin, major, minor);
        }

        /// <summary>Creates an ellipse in the XY plane of a coordinate system.</summary>
        /// <param name="origin">Origin coordinate system of the ellipse.</param>
        /// <param name="xAxisRadius">Radius along the coordinate system X axis.</param>
        /// <param name="yAxisRadius">Radius along the coordinate system Y axis.</param>
        /// <returns>The created ellipse.</returns>
        public static Ellipse ByCoordinateSystemRadii(CoordinateSystem origin, double xAxisRadius, double yAxisRadius)
        {
            ArgumentNullException.ThrowIfNull(origin);
            var ux = origin.XAxis.Normalized();
            var uy = origin.YAxis.Normalized();
            var handle = NativeGeometry.Ellipse(origin.Origin.Position, ux.Components, uy.Components, xAxisRadius, yAxisRadius);
            var vx = ux.Scale(xAxisRadius);
            var vy = uy.Scale(yAxisRadius);
            var (major, minor) = xAxisRadius >= yAxisRadius ? (vx, vy) : (vy, vx);
            return new Ellipse(handle, origin.Origin, major, minor);
        }

        /// <summary>Creates an ellipse lying on <paramref name="plane"/> with the specified X and Y radii.</summary>
        /// <param name="plane">Plane containing the ellipse.</param>
        /// <param name="xAxisRadius">Radius along the plane X axis.</param>
        /// <param name="yAxisRadius">Radius along the plane Y axis.</param>
        /// <returns>The created ellipse.</returns>
        public static Ellipse ByPlaneRadii(Plane plane, double xAxisRadius, double yAxisRadius)
        {
            ArgumentNullException.ThrowIfNull(plane);
            var ux = plane.XAxis.Normalized();
            var uy = plane.YAxis.Normalized();
            var handle = NativeGeometry.Ellipse(plane.Origin.Position, ux.Components, uy.Components, xAxisRadius, yAxisRadius);
            var vx = ux.Scale(xAxisRadius);
            var vy = uy.Scale(yAxisRadius);
            var (major, minor) = xAxisRadius >= yAxisRadius ? (vx, vy) : (vy, vx);
            return new Ellipse(handle, plane.Origin, major, minor);
        }

        /// <inheritdoc />
        public override Curve Reverse() =>
            new Ellipse(NativeGeometry.CurveReverse(Handle), CenterPoint, MajorAxis, MinorAxis);

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            var h = NativeGeometry.CurveTranslate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation));
            var newCenter = (Point)CenterPoint.Translate(xTranslation, yTranslation, zTranslation);
            return new Ellipse(h, newCenter, MajorAxis, MinorAxis);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var h = NativeGeometry.CurveRotate(Handle, origin.Position, axis.Components, degrees);
            var newCenter = (Point)CenterPoint.Rotate(origin, axis, degrees);
            var newMajor = GeometryMath.RotateVector(MajorAxis, axis, degrees);
            var newMinor = GeometryMath.RotateVector(MinorAxis, axis, degrees);
            return new Ellipse(h, newCenter, newMajor, newMinor);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            var h = NativeGeometry.CurveScale(Handle, basePoint.Position, xamount, yamount, zamount);
            var newCenter = (Point)CenterPoint.Scale(basePoint, xamount, yamount, zamount);
            var scaledA = Vector.ByCoordinates(MajorAxis.X * xamount, MajorAxis.Y * yamount, MajorAxis.Z * zamount);
            var scaledB = Vector.ByCoordinates(MinorAxis.X * xamount, MinorAxis.Y * yamount, MinorAxis.Z * zamount);
            if (scaledA.IsPerpendicular(scaledB))
            {
                var (major, minor) = scaledA.Length >= scaledB.Length ? (scaledA, scaledB) : (scaledB, scaledA);
                return new Ellipse(h, newCenter, major, minor);
            }
            return new Curve(h);
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var h = NativeGeometry.CurveTransform(
                Handle, cs.Origin.Position, cs.XAxis.Components, cs.YAxis.Components, cs.ZAxis.Components);
            var newCenter = (Point)CenterPoint.Transform(cs);
            var newA = cs.TransformVector(MajorAxis);
            var newB = cs.TransformVector(MinorAxis);
            if (newA.IsPerpendicular(newB))
            {
                var (major, minor) = newA.Length >= newB.Length ? (newA, newB) : (newB, newA);
                return new Ellipse(h, newCenter, major, minor);
            }
            return new Curve(h);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            var h = NativeGeometry.CurveMirror(
                Handle, mirrorPlane.Origin.Position, mirrorPlane.XAxis.Components, mirrorPlane.YAxis.Components);
            var newCenter = (Point)CenterPoint.Mirror(mirrorPlane);
            var n = mirrorPlane.Normal.Normalized();
            var newMajor = MajorAxis.Subtract(n.Scale(2.0 * MajorAxis.Dot(n)));
            var newMinor = MinorAxis.Subtract(n.Scale(2.0 * MinorAxis.Dot(n)));
            return new Ellipse(h, newCenter, newMajor, newMinor);
        }
    }
}
