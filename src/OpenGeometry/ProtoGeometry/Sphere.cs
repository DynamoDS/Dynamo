using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A solid sphere.</summary>
    public sealed class Sphere : Solid
    {
        private Sphere(NativeGeometry.SolidHandle handle, Point centerPoint, double radius)
            : base(handle)
        {
            CenterPoint = centerPoint;
            Radius = radius;
        }

        /// <summary>Gets the center point of the sphere.</summary>
        public Point CenterPoint { get; }

        /// <summary>Gets the radius of the sphere.</summary>
        public double Radius { get; }

        /// <summary>Creates a solid sphere from a center point and radius.</summary>
        /// <param name="centerPoint">Center of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <returns>The created sphere.</returns>
        public static Sphere ByCenterPointRadius(Point centerPoint, double radius = 1)
        {
            ArgumentNullException.ThrowIfNull(centerPoint);
            var handle = NativeGeometry.Sphere(centerPoint.Position, radius);
            return new Sphere(
                handle,
                Point.ByCoordinates(centerPoint.X, centerPoint.Y, centerPoint.Z),
                radius);
        }
    }
}
