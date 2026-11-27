using System;
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
            Point axisPoint, Vector axisDirection, Point startPoint, double pitch, double angleTurns)
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

        internal override uint PreviewSegmentCount => 128;
    }
}
