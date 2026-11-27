using System;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A 3D Euclidean vector.</summary>
    public sealed class Vector : IGraphicItem, IDisposable
    {
        internal NativePoint Components { get; }

        internal Vector(NativePoint components)
        {
            Components = components;
        }

        /// <summary>Gets the X component.</summary>
        public double X => Components.X;

        /// <summary>Gets the Y component.</summary>
        public double Y => Components.Y;

        /// <summary>Gets the Z component.</summary>
        public double Z => Components.Z;

        /// <summary>Gets the Euclidean length of this vector.</summary>
        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>Gets the canonical world X axis (1, 0, 0).</summary>
        public static Vector XAxis() => new(new NativePoint(1, 0, 0));

        /// <summary>Gets the canonical world Y axis (0, 1, 0).</summary>
        public static Vector YAxis() => new(new NativePoint(0, 1, 0));

        /// <summary>Gets the canonical world Z axis (0, 0, 1).</summary>
        public static Vector ZAxis() => new(new NativePoint(0, 0, 1));

        /// <summary>Creates a vector from Cartesian components.</summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <returns>The created vector.</returns>
        public static Vector ByCoordinates(double x = 0, double y = 0, double z = 0)
        {
            if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(z))
                throw new ArgumentException("Vector components must be finite.");
            return new Vector(new NativePoint(x, y, z));
        }

        /// <summary>Creates a vector from two points.</summary>
        /// <param name="start">Start point.</param>
        /// <param name="end">End point.</param>
        /// <returns>The vector from start to end.</returns>
        public static Vector ByTwoPoints(Point start, Point end)
        {
            ArgumentNullException.ThrowIfNull(start);
            ArgumentNullException.ThrowIfNull(end);
            return new Vector(new NativePoint(end.X - start.X, end.Y - start.Y, end.Z - start.Z));
        }

        /// <summary>Returns a unit-length vector in the same direction.</summary>
        public Vector Normalized()
        {
            var len = Length;
            if (len <= 1e-12)
                throw new InvalidOperationException("Cannot normalize a zero-length vector.");
            return new Vector(new NativePoint(X / len, Y / len, Z / len));
        }

        /// <summary>Scales this vector uniformly by a scalar factor.</summary>
        /// <param name="scale_factor">Scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public Vector Scale(double scale_factor)
        {
            if (!double.IsFinite(scale_factor))
                throw new ArgumentException("Scale factor must be finite.");
            return new Vector(new NativePoint(X * scale_factor, Y * scale_factor, Z * scale_factor));
        }

        /// <summary>Adds another vector to this vector.</summary>
        /// <param name="vectorToAdd">Vector to add.</param>
        /// <returns>The summed vector.</returns>
        public Vector Add(Vector vectorToAdd)
        {
            ArgumentNullException.ThrowIfNull(vectorToAdd);
            return new Vector(new NativePoint(X + vectorToAdd.X, Y + vectorToAdd.Y, Z + vectorToAdd.Z));
        }

        /// <summary>Subtracts another vector from this vector.</summary>
        /// <param name="vectorToSubtract">Vector to subtract.</param>
        /// <returns>The difference vector.</returns>
        public Vector Subtract(Vector vectorToSubtract)
        {
            ArgumentNullException.ThrowIfNull(vectorToSubtract);
            return new Vector(new NativePoint(X - vectorToSubtract.X, Y - vectorToSubtract.Y, Z - vectorToSubtract.Z));
        }

        /// <summary>Returns the negated vector.</summary>
        public Vector Reverse() => new(new NativePoint(-X, -Y, -Z));

        /// <summary>Computes the dot product with another vector.</summary>
        /// <param name="otherVector">The other vector.</param>
        /// <returns>The scalar dot product.</returns>
        public double Dot(Vector otherVector)
        {
            ArgumentNullException.ThrowIfNull(otherVector);
            return X * otherVector.X + Y * otherVector.Y + Z * otherVector.Z;
        }

        /// <summary>Computes the cross product with another vector.</summary>
        /// <param name="otherVector">The other vector.</param>
        /// <returns>The orthogonal cross product vector.</returns>
        public Vector Cross(Vector otherVector)
        {
            ArgumentNullException.ThrowIfNull(otherVector);
            return new Vector(new NativePoint(
                Y * otherVector.Z - Z * otherVector.Y,
                Z * otherVector.X - X * otherVector.Z,
                X * otherVector.Y - Y * otherVector.X));
        }

        /// <summary>Computes the angle between this vector and another in degrees.</summary>
        /// <param name="otherVector">The other vector.</param>
        /// <returns>Angle in degrees in [0, 180].</returns>
        public double AngleWithVector(Vector otherVector)
        {
            ArgumentNullException.ThrowIfNull(otherVector);
            var denom = Length * otherVector.Length;
            if (denom <= 1e-12)
                throw new ArgumentException("Cannot compute angle with a zero-length vector.");
            var cos = Math.Clamp(Dot(otherVector) / denom, -1.0, 1.0);
            return Math.Acos(cos) * (180.0 / Math.PI);
        }

        /// <summary>Returns true if this vector is parallel to another vector.</summary>
        /// <param name="otherVector">The other vector.</param>
        /// <returns>True when parallel within tolerance.</returns>
        public bool IsParallel(Vector otherVector)
        {
            ArgumentNullException.ThrowIfNull(otherVector);
            var denom = Length * otherVector.Length;
            if (denom <= 1e-12)
                return false;
            return Cross(otherVector).Length <= 1e-6 * denom;
        }

        /// <summary>Returns true if this vector is perpendicular to another vector.</summary>
        /// <param name="otherVector">The other vector.</param>
        /// <returns>True when perpendicular within tolerance.</returns>
        public bool IsPerpendicular(Vector otherVector)
        {
            ArgumentNullException.ThrowIfNull(otherVector);
            var denom = Length * otherVector.Length;
            if (denom <= 1e-12)
                return false;
            return Math.Abs(Dot(otherVector)) <= 1e-6 * denom;
        }

        /// <summary>Returns true if this vector is almost equal to another vector within tolerance.</summary>
        /// <param name="other">The vector to compare against.</param>
        /// <returns>True when component distance is within tolerance.</returns>
        public bool IsAlmostEqualTo(Vector other)
        {
            if (other is null)
                return false;
            return Subtract(other).Length <= 1e-6;
        }

        /// <summary>Converts this vector to a point with the same coordinates.</summary>
        public Point AsPoint() => Point.ByCoordinates(X, Y, Z);

        /// <summary>Serializes this vector to JSON.</summary>
        /// <returns>A JSON representation of this vector.</returns>
        public string ToJson() => GeometryJsonSerializer.SerializeVector(this);

        /// <summary>Deserializes a vector from JSON.</summary>
        /// <param name="json">JSON string representing a vector.</param>
        /// <returns>The deserialized vector.</returns>
        public static Vector FromJson(string json) => GeometryJsonSerializer.DeserializeVector(json);

        /// <summary>Vectors are directional quantities without a fixed origin and emit no standalone preview geometry.</summary>
        /// <param name="package">Target render package.</param>
        /// <param name="parameters">Tessellation parameters.</param>
        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(package);
            ArgumentNullException.ThrowIfNull(parameters);
        }

        /// <summary>Releases resources owned by this vector.</summary>
        public void Dispose() { }

        /// <inheritdoc />
        public override string ToString() => $"Vector(X = {X:0.###}, Y = {Y:0.###}, Z = {Z:0.###})";
    }
}
