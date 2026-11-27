using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A capped circular cylinder.</summary>
    public sealed class Cylinder : Solid
    {
        private Cylinder(NativeGeometry.SolidHandle handle) : base(handle) { }

        /// <summary>Creates a cylinder between two cap centers.</summary>
        /// <param name="startPoint">Center of the first cap.</param>
        /// <param name="endPoint">Center of the second cap.</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <returns>The cylinder between the two points.</returns>
        public static Cylinder ByPointsRadius(Point startPoint, Point endPoint, double radius)
        {
            ArgumentNullException.ThrowIfNull(startPoint);
            ArgumentNullException.ThrowIfNull(endPoint);
            return new Cylinder(NativeGeometry.Cylinder(startPoint.Position, endPoint.Position, radius));
        }
    }
}
