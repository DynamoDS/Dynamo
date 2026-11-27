using System;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A point in Cartesian coordinates.</summary>
    public sealed class Point : Geometry
    {
        internal NativePoint Position { get; }

        internal Point(NativePoint position)
        {
            Position = position;
        }

        /// <summary>Gets the X coordinate.</summary>
        public double X => Position.X;

        /// <summary>Gets the Y coordinate.</summary>
        public double Y => Position.Y;

        /// <summary>Gets the Z coordinate.</summary>
        public double Z => Position.Z;

        /// <summary>Gets the world origin point (0, 0, 0).</summary>
        public static Point Origin() => new(new NativePoint(0, 0, 0));

        /// <summary>Creates a point in the XY plane from two Cartesian coordinates.</summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>The point at (x, y, 0).</returns>
        public static Point ByCoordinates(double x = 0, double y = 0)
        {
            return ByCoordinates(x, y, 0);
        }

        /// <summary>Creates a point from three Cartesian coordinates.</summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <returns>The point at the specified coordinates.</returns>
        public static Point ByCoordinates(double x = 0, double y = 0, double z = 0)
        {
            if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(z))
                throw new ArgumentException("Point coordinates must be finite.");
            return new Point(new NativePoint(x, y, z));
        }

        /// <summary>Creates a point from local Cartesian coordinates in a CoordinateSystem.</summary>
        /// <param name="cs">Reference coordinate system.</param>
        /// <param name="x">Local X coordinate.</param>
        /// <param name="y">Local Y coordinate.</param>
        /// <param name="z">Local Z coordinate.</param>
        /// <returns>The point in world coordinates.</returns>
        public static Point ByCartesianCoordinates(CoordinateSystem cs, double x = 0, double y = 0, double z = 0)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return new Point(cs.TransformPoint(new NativePoint(x, y, z)));
        }

        /// <summary>Translates this point by adding a vector.</summary>
        /// <param name="vectorToAdd">Displacement vector.</param>
        /// <returns>The displaced point.</returns>
        public Point Add(Vector vectorToAdd)
        {
            ArgumentNullException.ThrowIfNull(vectorToAdd);
            return new Point(new NativePoint(X + vectorToAdd.X, Y + vectorToAdd.Y, Z + vectorToAdd.Z));
        }

        /// <summary>Translates this point by subtracting a vector.</summary>
        /// <param name="vectorToSubtract">Displacement vector to subtract.</param>
        /// <returns>The displaced point.</returns>
        public Point Subtract(Vector vectorToSubtract)
        {
            ArgumentNullException.ThrowIfNull(vectorToSubtract);
            return new Point(new NativePoint(X - vectorToSubtract.X, Y - vectorToSubtract.Y, Z - vectorToSubtract.Z));
        }

        /// <summary>Converts this point to a vector from the world origin.</summary>
        public Vector AsVector() => Vector.ByCoordinates(X, Y, Z);

        /// <summary>Computes the Euclidean distance to another point.</summary>
        /// <param name="other">Other point.</param>
        /// <returns>Euclidean distance.</returns>
        public double DistanceTo(Point other)
        {
            ArgumentNullException.ThrowIfNull(other);
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <inheritdoc />
        public override BoundingBox BoundingBox => BoundingBox.ByCorners(this, this);

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return ByCoordinates(X + xTranslation, Y + yTranslation, Z + zTranslation);
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            var offset = Vector.ByTwoPoints(origin, this);
            var rotated = GeometryMath.RotateVector(offset, axis, degrees);
            return origin.Add(rotated);
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            return ByCoordinates(
                basePoint.X + (X - basePoint.X) * xamount,
                basePoint.Y + (Y - basePoint.Y) * yamount,
                basePoint.Z + (Z - basePoint.Z) * zamount);
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return new Point(cs.TransformPoint(Position));
        }

        /// <inheritdoc />
        public override double DistanceTo(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other is Point pt)
                return DistanceTo(pt);
            if (other is Topology or Curve)
                return other.DistanceTo(this);
            return base.DistanceTo(other);
        }

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            package.AddPointVertex(X, Y, Z);
        }

        /// <inheritdoc />
        public override string ToString() => $"Point(X = {X:0.###}, Y = {Y:0.###}, Z = {Z:0.###})";
    }
}
