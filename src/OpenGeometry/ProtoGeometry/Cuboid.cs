using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A rectangular solid.</summary>
    public sealed class Cuboid : Solid
    {
        private Cuboid(NativeGeometry.SolidHandle handle) : base(handle) { }

        /// <summary>Creates an axis-aligned cuboid centered at the world origin.</summary>
        /// <param name="width">Size along X.</param>
        /// <param name="length">Size along Y.</param>
        /// <param name="height">Size along Z.</param>
        /// <returns>The centered cuboid.</returns>
        public static Cuboid ByLengths(double width = 1, double length = 1, double height = 1)
        {
            return Create(default, width, length, height);
        }

        /// <summary>Creates an axis-aligned cuboid centered at a point.</summary>
        /// <param name="origin">The center of the cuboid.</param>
        /// <param name="width">Size along X.</param>
        /// <param name="length">Size along Y.</param>
        /// <param name="height">Size along Z.</param>
        /// <returns>The centered cuboid.</returns>
        public static Cuboid ByLengths(Point origin, double width = 1, double length = 1, double height = 1)
        {
            ArgumentNullException.ThrowIfNull(origin);
            return Create(origin.Position, width, length, height);
        }

        private static Cuboid Create(NativePoint center, double width, double length, double height)
        {
            var corner = new NativePoint(center.X - width / 2, center.Y - length / 2, center.Z - height / 2);
            return new Cuboid(NativeGeometry.Box(corner, width, length, height));
        }
    }
}
