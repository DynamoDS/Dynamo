using System;
using Autodesk.DesignScript.Runtime;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A 3D cylindrical helix curve.</summary>
    public class Helix : Curve
    {
        internal Helix(
            NativeGeometry.CurveHandle handle,
            Point axisPoint,
            Vector axisDirection,
            double pitch,
            double radius,
            double angle)
            : base(handle)
        {
            AxisPoint = axisPoint;
            AxisDirection = axisDirection;
            Pitch = pitch;
            Radius = radius;
            Angle = angle;
        }

        /// <summary>Gets a point on the axis of the helix.</summary>
        public Point AxisPoint { get; }

        /// <summary>Gets the unit direction vector along the axis of the helix.</summary>
        public Vector AxisDirection { get; }

        /// <summary>Gets the axial distance advanced per full 360-degree turn.</summary>
        public double Pitch { get; }

        /// <summary>Gets the radial distance of the helix from its axis.</summary>
        public double Radius { get; }

        /// <summary>Gets the total sweep angle in degrees.</summary>
        public double Angle { get; }

        /// <summary>Creates a cylindrical helix around an axis starting at <paramref name="startPoint"/>.</summary>
        /// <param name="axisPoint">Point on the helix axis.</param>
        /// <param name="axisDirection">Direction vector of the helix axis.</param>
        /// <param name="startPoint">Starting point on the helix (defines radius and start phase).</param>
        /// <param name="pitch">Axial distance per 360-degree turn.</param>
        /// <param name="angleTurns">Total angular sweep in degrees (360 = one full turn).</param>
        /// <returns>The created helix curve.</returns>
        public static Helix ByAxis(
            [DefaultArgument("Autodesk.DesignScript.Geometry.Point.Origin();")] Point axisPoint,
            [DefaultArgument("Autodesk.DesignScript.Geometry.Vector.ZAxis();")] Vector axisDirection,
            [DefaultArgument("Autodesk.DesignScript.Geometry.Point.ByCoordinates(1, 0, 0);")] Point startPoint,
            double pitch = 1,
            double angleTurns = 360)
        {
            ArgumentNullException.ThrowIfNull(axisPoint);
            ArgumentNullException.ThrowIfNull(axisDirection);
            ArgumentNullException.ThrowIfNull(startPoint);

            var unitAxis = axisDirection.Normalized();
            var toStart = Vector.ByTwoPoints(axisPoint, startPoint);
            var proj = toStart.Dot(unitAxis);
            var basePt = axisPoint.Add(unitAxis.Scale(proj));
            var radius = basePt.DistanceTo(startPoint);

            var handle = NativeGeometry.Helix(
                axisPoint.Position, unitAxis.Components, startPoint.Position, pitch, angleTurns);
            return new Helix(handle, axisPoint, unitAxis, pitch, radius, angleTurns);
        }

        /// <inheritdoc />
        public override Curve Reverse() =>
            new Helix(NativeGeometry.CurveReverse(Handle), AxisPoint, AxisDirection.Reverse(), Pitch, Radius, -Angle);

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            var h = NativeGeometry.CurveTranslate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation));
            var newAxisPt = (Point)AxisPoint.Translate(xTranslation, yTranslation, zTranslation);
            return new Helix(h, newAxisPt, AxisDirection, Pitch, Radius, Angle);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var h = NativeGeometry.CurveRotate(Handle, origin.Position, axis.Components, degrees);
            var newAxisPt = (Point)AxisPoint.Rotate(origin, axis, degrees);
            var newDir = GeometryMath.RotateVector(AxisDirection, axis, degrees).Normalized();
            return new Helix(h, newAxisPt, newDir, Pitch, Radius, Angle);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            if (Math.Abs(xamount - yamount) <= 1e-6 && Math.Abs(xamount - zamount) <= 1e-6 && xamount > 0)
            {
                var h = NativeGeometry.CurveScale(Handle, basePoint.Position, xamount, yamount, zamount);
                var newAxisPt = (Point)AxisPoint.Scale(basePoint, xamount, yamount, zamount);
                return new Helix(h, newAxisPt, AxisDirection, Pitch * xamount, Radius * xamount, Angle);
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
                var newAxisPt = (Point)AxisPoint.Transform(cs);
                var newDir = cs.TransformVector(AxisDirection).Normalized();
                return new Helix(h, newAxisPt, newDir, Pitch * sx, Radius * sx, Angle);
            }
            return base.Transform(cs);
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            var h = NativeGeometry.CurveMirror(
                Handle, mirrorPlane.Origin.Position, mirrorPlane.XAxis.Components, mirrorPlane.YAxis.Components);
            var newAxisPt = (Point)AxisPoint.Mirror(mirrorPlane);
            var n = mirrorPlane.Normal.Normalized();
            var newDir = AxisDirection.Subtract(n.Scale(2.0 * AxisDirection.Dot(n))).Normalized();
            return new Helix(h, newAxisPt, newDir, Pitch, Radius, -Angle);
        }

        internal override uint PreviewSegmentCount => 128;
    }
}
