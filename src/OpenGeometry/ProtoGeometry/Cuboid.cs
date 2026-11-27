using System;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A rectangular solid.</summary>
    public sealed class Cuboid : Solid
    {
        private Cuboid(NativeGeometry.SolidHandle handle, double width, double length, double height) : base(handle)
        {
            Width = width;
            Length = length;
            Height = height;
        }

        /// <summary>Gets the size along the cuboid's local X axis.</summary>
        public double Width { get; }

        /// <summary>Gets the size along the cuboid's local Y axis.</summary>
        public double Length { get; }

        /// <summary>Gets the size along the cuboid's local Z axis.</summary>
        public double Height { get; }

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

        /// <summary>Creates a cuboid centered at a CoordinateSystem's origin and aligned with its axes.</summary>
        /// <param name="cs">Local coordinate system.</param>
        /// <param name="width">Size along X.</param>
        /// <param name="length">Size along Y.</param>
        /// <param name="height">Size along Z.</param>
        /// <returns>The oriented cuboid.</returns>
        public static Cuboid ByLengths(CoordinateSystem cs, double width = 1, double length = 1, double height = 1)
        {
            ArgumentNullException.ThrowIfNull(cs);
            using var local = NativeGeometry.Box(
                new NativePoint(-width / 2, -length / 2, -height / 2), width, length, height);
            var handle = NativeGeometry.Transform(
                local, cs.Origin.Position, cs.XAxis.Components, cs.YAxis.Components, cs.ZAxis.Components);
            return new Cuboid(handle, width * cs.XAxis.Length, length * cs.YAxis.Length, height * cs.ZAxis.Length);
        }

        /// <summary>Creates an axis-aligned cuboid spanning two opposite corner points.</summary>
        /// <param name="lowPoint">First corner.</param>
        /// <param name="highPoint">Opposite corner.</param>
        /// <returns>The axis-aligned cuboid.</returns>
        public static Cuboid ByCorners(Point lowPoint, Point highPoint)
        {
            ArgumentNullException.ThrowIfNull(lowPoint);
            ArgumentNullException.ThrowIfNull(highPoint);
            var min = new NativePoint(
                Math.Min(lowPoint.X, highPoint.X),
                Math.Min(lowPoint.Y, highPoint.Y),
                Math.Min(lowPoint.Z, highPoint.Z));
            var width = Math.Abs(highPoint.X - lowPoint.X);
            var length = Math.Abs(highPoint.Y - lowPoint.Y);
            var height = Math.Abs(highPoint.Z - lowPoint.Z);
            return new Cuboid(NativeGeometry.Box(min, width, length, height), width, length, height);
        }

        private static Cuboid Create(NativePoint center, double width, double length, double height)
        {
            var corner = new NativePoint(center.X - width / 2, center.Y - length / 2, center.Z - height / 2);
            return new Cuboid(NativeGeometry.Box(corner, width, length, height), width, length, height);
        }
    }
}
