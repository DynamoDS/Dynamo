using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>An open planar elliptical arc curve.</summary>
    public class EllipseArc : Curve
    {
        internal EllipseArc(
            NativeGeometry.CurveHandle handle,
            Plane plane,
            Vector majorAxis,
            Vector minorAxis,
            double startAngle,
            double sweepAngle,
            double xRadius,
            double yRadius)
            : base(handle)
        {
            Plane = plane;
            MajorAxis = majorAxis;
            MinorAxis = minorAxis;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
            XRadius = xRadius;
            YRadius = yRadius;
        }

        internal double XRadius { get; }
        internal double YRadius { get; }

        /// <summary>Gets the plane containing this elliptical arc.</summary>
        public Plane Plane { get; }

        /// <summary>Gets the center point of the ellipse.</summary>
        public Point CenterPoint => Plane.Origin;

        /// <inheritdoc />
        public override Vector Normal => Plane.Normal;

        /// <summary>Gets the major axis vector (scaled by the major semi-axis length).</summary>
        public Vector MajorAxis { get; }

        /// <summary>Gets the minor axis vector (scaled by the minor semi-axis length).</summary>
        public Vector MinorAxis { get; }

        /// <summary>Gets the start angle in degrees measured from the plane X axis.</summary>
        public double StartAngle { get; }

        /// <summary>Gets the angular sweep in degrees.</summary>
        public double SweepAngle { get; }

        /// <summary>Creates an elliptical arc on <paramref name="plane"/> with the specified radii and angular span.</summary>
        /// <param name="plane">Plane containing the ellipse arc.</param>
        /// <param name="xRadius">Radius in the plane X direction.</param>
        /// <param name="yRadius">Radius in the plane Y direction.</param>
        /// <param name="startAngle">Start angle in degrees from the positive X axis of <paramref name="plane"/>.</param>
        /// <param name="sweepAngle">Angular sweep in degrees from <paramref name="startAngle"/>.</param>
        /// <returns>The created elliptical arc.</returns>
        public static EllipseArc ByPlaneRadiiAngles(
            Plane plane, double xRadius, double yRadius, double startAngle, double sweepAngle)
        {
            ArgumentNullException.ThrowIfNull(plane);
            var ux = plane.XAxis.Normalized();
            var uy = plane.YAxis.Normalized();
            var handle = NativeGeometry.EllipseArc(
                plane.Origin.Position, ux.Components, uy.Components, xRadius, yRadius, startAngle, sweepAngle);
            var vx = ux.Scale(xRadius);
            var vy = uy.Scale(yRadius);
            var (major, minor) = xRadius >= yRadius ? (vx, vy) : (vy, vx);
            return new EllipseArc(handle, plane, major, minor, startAngle, sweepAngle, xRadius, yRadius);
        }

        /// <inheritdoc />
        public override Curve Reverse()
        {
            var h = NativeGeometry.CurveReverse(Handle);
            var flippedPlane = Plane.ByOriginNormalXAxis(Plane.Origin, Plane.Normal.Reverse(), Plane.XAxis);
            var newStartAngle = -(StartAngle + SweepAngle);
            return new EllipseArc(h, flippedPlane, MajorAxis, MinorAxis, newStartAngle, SweepAngle, XRadius, YRadius);
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            var h = NativeGeometry.CurveTranslate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation));
            var newPlane = (Plane)Plane.Translate(xTranslation, yTranslation, zTranslation);
            return new EllipseArc(h, newPlane, MajorAxis, MinorAxis, StartAngle, SweepAngle, XRadius, YRadius);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var h = NativeGeometry.CurveRotate(Handle, origin.Position, axis.Components, degrees);
            var newPlane = (Plane)Plane.Rotate(origin, axis, degrees);
            var newMajor = GeometryMath.RotateVector(MajorAxis, axis, degrees);
            var newMinor = GeometryMath.RotateVector(MinorAxis, axis, degrees);
            return new EllipseArc(h, newPlane, newMajor, newMinor, StartAngle, SweepAngle, XRadius, YRadius);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            if (Math.Abs(xamount - yamount) <= 1e-6 && Math.Abs(xamount - zamount) <= 1e-6 && xamount > 0)
            {
                var h = NativeGeometry.CurveScale(Handle, basePoint.Position, xamount, yamount, zamount);
                var newOrigin = (Point)Plane.Origin.Scale(basePoint, xamount, yamount, zamount);
                var newPlane = Plane.ByOriginNormalXAxis(newOrigin, Plane.Normal, Plane.XAxis);
                return new EllipseArc(
                    h, newPlane, MajorAxis.Scale(xamount), MinorAxis.Scale(xamount), StartAngle, SweepAngle, XRadius * xamount, YRadius * xamount);
            }
            return base.Scale(basePoint, xamount, yamount, zamount);
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var sx = cs.XScaleFactor;
            var sy = cs.YScaleFactor;
            var sz = cs.ZScaleFactor;
            var isUniformOrtho = Math.Abs(sx - sy) <= 1e-6 && Math.Abs(sx - sz) <= 1e-6
                && cs.XAxis.IsPerpendicular(cs.YAxis) && cs.XAxis.IsPerpendicular(cs.ZAxis) && cs.YAxis.IsPerpendicular(cs.ZAxis);
            if (isUniformOrtho)
            {
                var h = NativeGeometry.CurveTransform(
                    Handle, cs.Origin.Position, cs.XAxis.Components, cs.YAxis.Components, cs.ZAxis.Components);
                var newPlane = (Plane)Plane.Transform(cs);
                var newMajor = cs.TransformVector(MajorAxis);
                var newMinor = cs.TransformVector(MinorAxis);
                return new EllipseArc(h, newPlane, newMajor, newMinor, StartAngle, SweepAngle, XRadius * sx, YRadius * sx);
            }
            return base.Transform(cs);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            var h = NativeGeometry.CurveMirror(
                Handle, mirrorPlane.Origin.Position, mirrorPlane.XAxis.Components, mirrorPlane.YAxis.Components);
            var n = mirrorPlane.Normal.Normalized();
            var newOrigin = (Point)Plane.Origin.Mirror(mirrorPlane);
            var newX = Plane.XAxis.Subtract(n.Scale(2.0 * Plane.XAxis.Dot(n))).Normalized();
            var reflectedN = Plane.Normal.Subtract(n.Scale(2.0 * Plane.Normal.Dot(n))).Normalized();
            var newMajor = MajorAxis.Subtract(n.Scale(2.0 * MajorAxis.Dot(n)));
            var newMinor = MinorAxis.Subtract(n.Scale(2.0 * MinorAxis.Dot(n)));
            // Reflection flips orientation in the plane: negate normal so sweep angle stays oriented with curve.
            var newPlane = Plane.ByOriginNormalXAxis(newOrigin, reflectedN.Reverse(), newX);
            return new EllipseArc(h, newPlane, newMajor, newMinor, StartAngle, SweepAngle, XRadius, YRadius);
        }
    }
}
