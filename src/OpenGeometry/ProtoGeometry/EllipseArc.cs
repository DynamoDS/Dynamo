using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>An open planar elliptical arc curve.</summary>
    public class EllipseArc : Curve
    {
        private EllipseArc(
            NativeGeometry.CurveHandle handle,
            Plane plane,
            Vector majorAxis,
            Vector minorAxis,
            double startAngle,
            double sweepAngle)
            : base(handle)
        {
            Plane = plane;
            MajorAxis = majorAxis;
            MinorAxis = minorAxis;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }

        /// <summary>Gets the plane containing this elliptical arc.</summary>
        public Plane Plane { get; }

        /// <summary>Gets the center point of the ellipse.</summary>
        public Point CenterPoint => Plane.Origin;

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
            return new EllipseArc(handle, plane, major, minor, startAngle, sweepAngle);
        }
    }
}
