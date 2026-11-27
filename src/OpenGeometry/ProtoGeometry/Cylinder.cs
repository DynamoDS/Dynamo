using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A capped circular cylinder.</summary>
    public sealed class Cylinder : Solid
    {
        internal Cylinder(NativeGeometry.SolidHandle handle, Point startPoint, Point endPoint, double radius)
            : base(handle)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Radius = radius;
            Height = startPoint.DistanceTo(endPoint);
        }

        /// <summary>Gets the center of the first cap.</summary>
        public Point StartPoint { get; }

        /// <summary>Gets the center of the second cap.</summary>
        public Point EndPoint { get; }

        /// <summary>Gets the cylinder radius.</summary>
        public double Radius { get; }

        /// <summary>Gets the cylinder height along its axis.</summary>
        public double Height { get; }

        /// <summary>Creates a cylinder between two cap centers.</summary>
        /// <param name="startPoint">Center of the first cap.</param>
        /// <param name="endPoint">Center of the second cap.</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <returns>The cylinder between the two points.</returns>
        public static Cylinder ByPointsRadius(Point startPoint, Point endPoint, double radius = 1)
        {
            ArgumentNullException.ThrowIfNull(startPoint);
            ArgumentNullException.ThrowIfNull(endPoint);
            var handle = NativeGeometry.Cylinder(startPoint.Position, endPoint.Position, radius);
            return new Cylinder(
                handle,
                Point.ByCoordinates(startPoint.X, startPoint.Y, startPoint.Z),
                Point.ByCoordinates(endPoint.X, endPoint.Y, endPoint.Z),
                radius);
        }

        /// <summary>Creates a cylinder aligned with a CoordinateSystem's Z axis.</summary>
        /// <param name="cs">Base coordinate system.</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <param name="height">Height along the Z axis.</param>
        /// <returns>The created cylinder.</returns>
        public static Cylinder ByRadiusHeight(CoordinateSystem cs, double radius = 1, double height = 1)
        {
            ArgumentNullException.ThrowIfNull(cs);
            var start = cs.Origin;
            var end = start.Add(cs.ZAxis.Normalized().Scale(height));
            return ByPointsRadius(start, end, radius);
        }
    }
}
