using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A point in Cartesian coordinates.</summary>
    public sealed class Point : Geometry
    {
        internal NativePoint Position { get; }

        private Point(NativePoint position)
        {
            Position = position;
        }

        /// <summary>Gets the X coordinate.</summary>
        public double X => Position.X;

        /// <summary>Gets the Y coordinate.</summary>
        public double Y => Position.Y;

        /// <summary>Gets the Z coordinate.</summary>
        public double Z => Position.Z;

        /// <summary>Creates a point from three Cartesian coordinates.</summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <returns>The point at the specified coordinates.</returns>
        public static Point ByCoordinates(double x = 0, double y = 0, double z = 0)
        {
            return new Point(new NativePoint(x, y, z));
        }

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            package.AddPointVertex(X, Y, Z);
        }
    }
}
