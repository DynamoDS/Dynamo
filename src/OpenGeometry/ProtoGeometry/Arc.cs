using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A circular arc in 3D space.</summary>
    public class Arc : Curve
    {
        private Arc(
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
        public Vector Normal { get; }

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
    }
}
