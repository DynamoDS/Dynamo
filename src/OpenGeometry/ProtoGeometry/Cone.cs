using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A solid circular cone or frustum.</summary>
    public sealed class Cone : Solid
    {
        internal Cone(
            NativeGeometry.SolidHandle handle,
            Point startPoint,
            Point endPoint,
            double startRadius,
            double endRadius)
            : base(handle)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            StartRadius = startRadius;
            EndRadius = endRadius;
            Height = startPoint.DistanceTo(endPoint);
        }

        /// <summary>Gets the center of the start cap.</summary>
        public Point StartPoint { get; }

        /// <summary>Gets the center of the end cap or apex.</summary>
        public Point EndPoint { get; }

        /// <summary>Gets the radius at the start point.</summary>
        public double StartRadius { get; }

        /// <summary>Gets the radius at the end point.</summary>
        public double EndRadius { get; }

        /// <summary>Gets the height along the cone axis.</summary>
        public double Height { get; }

        /// <summary>Creates a cone or truncated frustum between two points with start and end radii.</summary>
        /// <param name="startPoint">Center of the base.</param>
        /// <param name="endPoint">Center of the top or apex.</param>
        /// <param name="startRadius">Radius at startPoint.</param>
        /// <param name="endRadius">Radius at endPoint (0 for a pointed cone).</param>
        /// <returns>The created cone.</returns>
        public static Cone ByPointsRadii(
            Point startPoint, Point endPoint, double startRadius = 1, double endRadius = 0)
        {
            ArgumentNullException.ThrowIfNull(startPoint);
            ArgumentNullException.ThrowIfNull(endPoint);
            var handle = NativeGeometry.Cone(startPoint.Position, endPoint.Position, startRadius, endRadius);
            return new Cone(
                handle,
                Point.ByCoordinates(startPoint.X, startPoint.Y, startPoint.Z),
                Point.ByCoordinates(endPoint.X, endPoint.Y, endPoint.Z),
                startRadius,
                endRadius);
        }

        /// <summary>Creates a pointed cone from a base point, apex point, and base radius.</summary>
        public static Cone ByPointsRadius(Point startPoint, Point endPoint, double startRadius = 1)
        {
            return ByPointsRadii(startPoint, endPoint, startRadius, 0);
        }

        /// <summary>Creates a cone aligned with a CoordinateSystem's Z axis.</summary>
        public static Cone ByCoordinateSystemHeightRadii(
            CoordinateSystem cs, double height = 1, double startRadius = 1, double endRadius = 0)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var start = cs.Origin;
            var end = start.Add(cs.ZAxis.Normalized().Scale(height));
            return ByPointsRadii(start, end, startRadius, endRadius);
        }
    }
}
