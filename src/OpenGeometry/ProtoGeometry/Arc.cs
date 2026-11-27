using System;
using Autodesk.DesignScript.Runtime;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A circular arc in 3D space.</summary>
    public class Arc : Curve
    {
        internal Arc(
            NativeGeometry.CurveHandle handle,
            Point center,
            double radius,
            double startAngle,
            double sweepAngle,
            Vector normal)
            : base(handle)
        {
            CenterPoint = center;
            Radius = radius;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
            Normal = normal;
        }

        /// <summary>Gets the center point of the arc.</summary>
        public Point CenterPoint { get; }

        /// <summary>Gets the radius of the arc.</summary>
        public double Radius { get; }

        /// <summary>Gets the start angle of the arc in degrees.</summary>
        public double StartAngle { get; }

        /// <summary>Gets the sweep angle of the arc in degrees.</summary>
        public double SweepAngle { get; }

        /// <summary>Gets the unit normal of the plane containing the arc.</summary>
        public override Vector Normal { get; }

        /// <summary>Creates a circular arc from center, radius, start and end angles (in degrees), and plane normal.</summary>
        public static Arc ByCenterPointRadiusAngle(
            Point center, double radius, double startAngle, double endAngle, Vector normal)
        {
            ArgumentNullException.ThrowIfNull(center);
            ArgumentNullException.ThrowIfNull(normal);
            var plane = Plane.ByOriginNormal(center, normal);
            var handle = NativeGeometry.Arc(
                plane.Origin.Position,
                plane.XAxis.Components,
                plane.YAxis.Components,
                radius,
                startAngle,
                endAngle);
            return new Arc(handle, center, radius, startAngle, endAngle - startAngle, plane.Normal);
        }

        /// <summary>Creates a circular arc from center point, start point, sweep angle in degrees, and plane normal.</summary>
        /// <param name="centerPoint">Center of the arc.</param>
        /// <param name="startPoint">Starting point on the arc.</param>
        /// <param name="sweepAngle">Angular sweep in degrees around <paramref name="normal"/>.</param>
        /// <param name="normal">Normal vector of the plane containing the arc.</param>
        /// <returns>The created circular arc.</returns>
        public static Arc ByCenterPointStartPointSweepAngle(
            [DefaultArgument("Autodesk.DesignScript.Geometry.Point.Origin();")] Point centerPoint,
            [DefaultArgument("Autodesk.DesignScript.Geometry.Point.ByCoordinates(1, 0, 0);")] Point startPoint,
            [DefaultArgument("90;")] double sweepAngle,
            [DefaultArgument("Autodesk.DesignScript.Geometry.Vector.ZAxis();")] Vector normal)
        {
            ArgumentNullException.ThrowIfNull(centerPoint);
            ArgumentNullException.ThrowIfNull(startPoint);
            ArgumentNullException.ThrowIfNull(normal);
            if (!double.IsFinite(sweepAngle) || Math.Abs(sweepAngle) <= 1e-9 || Math.Abs(sweepAngle) > 360.0 + 1e-6)
                throw new ArgumentOutOfRangeException(nameof(sweepAngle), "Sweep angle must be finite, non-zero, and within [-360, 360] degrees.");

            var plane = Plane.ByOriginNormal(centerPoint, normal);
            var radial = Vector.ByTwoPoints(centerPoint, startPoint);
            var projected = radial.Subtract(plane.Normal.Scale(radial.Dot(plane.Normal)));
            var radius = projected.Length;
            if (radius <= 1e-9)
                throw new ArgumentException("Start point must not lie on the arc axis.", nameof(startPoint));

            var startAngle = Math.Atan2(projected.Dot(plane.YAxis), projected.Dot(plane.XAxis)) * (180.0 / Math.PI);
            if (startAngle < 0)
                startAngle += 360.0;

            var handle = NativeGeometry.Arc(
                plane.Origin.Position,
                plane.XAxis.Components,
                plane.YAxis.Components,
                radius,
                startAngle,
                startAngle + sweepAngle);
            return new Arc(handle, centerPoint, radius, startAngle, sweepAngle, plane.Normal);
        }

        /// <summary>Creates a circular arc starting at <paramref name="firstPoint"/>, passing through <paramref name="secondPoint"/>, and ending at <paramref name="thirdPoint"/>.</summary>
        public static Arc ByThreePoints(Point firstPoint, Point secondPoint, Point thirdPoint)
        {
            ArgumentNullException.ThrowIfNull(firstPoint);
            ArgumentNullException.ThrowIfNull(secondPoint);
            ArgumentNullException.ThrowIfNull(thirdPoint);

            var (center, radius, plane) = Circle.ComputeCircumcircle(firstPoint, secondPoint, thirdPoint);
            // plane.XAxis points from center to firstPoint (angle 0).
            var v2 = Vector.ByTwoPoints(center, secondPoint);
            var v3 = Vector.ByTwoPoints(center, thirdPoint);

            var ang2 = Math.Atan2(v2.Dot(plane.YAxis), v2.Dot(plane.XAxis)) * (180.0 / Math.PI);
            var ang3 = Math.Atan2(v3.Dot(plane.YAxis), v3.Dot(plane.XAxis)) * (180.0 / Math.PI);
            if (ang2 < 0) ang2 += 360.0;
            if (ang3 < 0) ang3 += 360.0;

            // Ensure the sweep passes through secondPoint; flip plane normal if needed.
            if (ang2 > ang3)
            {
                plane = Plane.ByOriginNormalXAxis(center, plane.Normal.Reverse(), plane.XAxis);
                ang3 = 360.0 - ang3;
            }

            var handle = NativeGeometry.Arc(
                plane.Origin.Position,
                plane.XAxis.Components,
                plane.YAxis.Components,
                radius,
                0.0,
                ang3);
            return new Arc(handle, center, radius, 0.0, ang3, plane.Normal);
        }

        /// <inheritdoc />
        public override Curve Reverse()
        {
            var reversedHandle = NativeGeometry.CurveReverse(Handle);
            var newNormal = Normal.Reverse();
            var newStart = EndPoint;
            var plane = Plane.ByOriginNormal(CenterPoint, newNormal);
            var radial = Vector.ByTwoPoints(CenterPoint, newStart);
            var newStartAngle = Math.Atan2(radial.Dot(plane.YAxis), radial.Dot(plane.XAxis)) * (180.0 / Math.PI);
            if (newStartAngle < 0)
                newStartAngle += 360.0;
            return new Arc(reversedHandle, CenterPoint, Radius, newStartAngle, SweepAngle, newNormal);
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            var h = NativeGeometry.CurveTranslate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation));
            var newCenter = (Point)CenterPoint.Translate(xTranslation, yTranslation, zTranslation);
            return new Arc(h, newCenter, Radius, StartAngle, SweepAngle, Normal);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var h = NativeGeometry.CurveRotate(Handle, origin.Position, axis.Components, degrees);
            var newCenter = (Point)CenterPoint.Rotate(origin, axis, degrees);
            var newNormal = GeometryMath.RotateVector(Normal, axis, degrees).Normalized();
            var newStart = (Point)StartPoint.Rotate(origin, axis, degrees);
            return BuildFromHandleAndFrame(h, newCenter, Radius, SweepAngle, newNormal, newStart);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            if (Math.Abs(xamount - yamount) <= 1e-6 && Math.Abs(xamount - zamount) <= 1e-6 && xamount > 0)
            {
                var h = NativeGeometry.CurveScale(Handle, basePoint.Position, xamount, yamount, zamount);
                var newCenter = (Point)CenterPoint.Scale(basePoint, xamount, yamount, zamount);
                return new Arc(h, newCenter, Radius * xamount, StartAngle, SweepAngle, Normal);
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
                var newCenter = (Point)CenterPoint.Transform(cs);
                var newStart = (Point)StartPoint.Transform(cs);
                var newNormal = cs.TransformVector(Normal).Normalized();
                if (cs.Determinant < 0)
                    newNormal = newNormal.Reverse();
                return BuildFromHandleAndFrame(h, newCenter, Radius * sx, SweepAngle, newNormal, newStart);
            }
            return base.Transform(cs);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            var h = NativeGeometry.CurveMirror(
                Handle, mirrorPlane.Origin.Position, mirrorPlane.XAxis.Components, mirrorPlane.YAxis.Components);
            var newCenter = (Point)CenterPoint.Mirror(mirrorPlane);
            var newStart = (Point)StartPoint.Mirror(mirrorPlane);
            var n = mirrorPlane.Normal.Normalized();
            var reflectedNormal = Normal.Subtract(n.Scale(2.0 * Normal.Dot(n))).Normalized();
            // Reflection reverses angular orientation in the plane, so flip normal to preserve positive sweep direction.
            var orientedNormal = reflectedNormal.Reverse();
            return BuildFromHandleAndFrame(h, newCenter, Radius, SweepAngle, orientedNormal, newStart);
        }

        private static Arc BuildFromHandleAndFrame(
            NativeGeometry.CurveHandle handle, Point center, double radius, double sweepAngle, Vector normal, Point startPoint)
        {
            var plane = Plane.ByOriginNormal(center, normal);
            var radial = Vector.ByTwoPoints(center, startPoint);
            var startAngle = Math.Atan2(radial.Dot(plane.YAxis), radial.Dot(plane.XAxis)) * (180.0 / Math.PI);
            if (startAngle < 0)
                startAngle += 360.0;
            return new Arc(handle, center, radius, startAngle, sweepAngle, plane.Normal);
        }
    }
}
