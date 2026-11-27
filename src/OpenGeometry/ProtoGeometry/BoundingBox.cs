using System;
using System.Collections.Generic;
using System.Linq;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>An axis-aligned 3D bounding box.</summary>
    public sealed class BoundingBox : IDisposable
    {
        private BoundingBox(Point minPoint, Point maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }

        /// <summary>Gets the minimum corner of the bounding box.</summary>
        public Point MinPoint { get; }

        /// <summary>Gets the maximum corner of the bounding box.</summary>
        public Point MaxPoint { get; }

        /// <summary>Creates an axis-aligned bounding box spanning two corner points.</summary>
        /// <param name="min">First corner.</param>
        /// <param name="max">Opposite corner.</param>
        /// <returns>The axis-aligned bounding box.</returns>
        public static BoundingBox ByCorners(Point min, Point max)
        {
            ArgumentNullException.ThrowIfNull(min);
            ArgumentNullException.ThrowIfNull(max);
            return new BoundingBox(
                Point.ByCoordinates(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y), Math.Min(min.Z, max.Z)),
                Point.ByCoordinates(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y), Math.Max(min.Z, max.Z)));
        }

        /// <summary>Computes the bounding box of a single geometry object.</summary>
        public static BoundingBox ByGeometry(Geometry geom)
        {
            ArgumentNullException.ThrowIfNull(geom);
            return geom.BoundingBox;
        }

        /// <summary>Computes the combined bounding box of multiple geometry objects.</summary>
        public static BoundingBox ByGeometry(IEnumerable<Geometry> geom)
        {
            ArgumentNullException.ThrowIfNull(geom);
            var boxes = geom.Where(g => g != null).Select(g => g.BoundingBox).ToArray();
            if (boxes.Length == 0)
                throw new ArgumentException("At least one geometry object is required.", nameof(geom));

            double minX = boxes.Min(b => b.MinPoint.X);
            double minY = boxes.Min(b => b.MinPoint.Y);
            double minZ = boxes.Min(b => b.MinPoint.Z);
            double maxX = boxes.Max(b => b.MaxPoint.X);
            double maxY = boxes.Max(b => b.MaxPoint.Y);
            double maxZ = boxes.Max(b => b.MaxPoint.Z);
            return new BoundingBox(Point.ByCoordinates(minX, minY, minZ), Point.ByCoordinates(maxX, maxY, maxZ));
        }

        /// <summary>Returns true when this bounding box has zero extent in any dimension.</summary>
        public bool IsEmpty() =>
            MaxPoint.X - MinPoint.X <= 1e-12
            || MaxPoint.Y - MinPoint.Y <= 1e-12
            || MaxPoint.Z - MinPoint.Z <= 1e-12;

        /// <summary>Returns true if the point lies inside or on the boundary of this bounding box.</summary>
        public bool Contains(Point point)
        {
            ArgumentNullException.ThrowIfNull(point);
            const double tol = 1e-9;
            return point.X >= MinPoint.X - tol && point.X <= MaxPoint.X + tol
                && point.Y >= MinPoint.Y - tol && point.Y <= MaxPoint.Y + tol
                && point.Z >= MinPoint.Z - tol && point.Z <= MaxPoint.Z + tol;
        }

        /// <summary>Returns true if this bounding box overlaps another bounding box.</summary>
        public bool Intersects(BoundingBox other)
        {
            ArgumentNullException.ThrowIfNull(other);
            const double tol = 1e-9;
            return MinPoint.X <= other.MaxPoint.X + tol && MaxPoint.X >= other.MinPoint.X - tol
                && MinPoint.Y <= other.MaxPoint.Y + tol && MaxPoint.Y >= other.MinPoint.Y - tol
                && MinPoint.Z <= other.MaxPoint.Z + tol && MaxPoint.Z >= other.MinPoint.Z - tol;
        }

        /// <summary>Converts this bounding box into a solid cuboid.</summary>
        public Cuboid ToCuboid() => Cuboid.ByCorners(MinPoint, MaxPoint);

        /// <summary>Serializes this bounding box to JSON.</summary>
        /// <returns>A JSON representation of this bounding box.</returns>
        public string ToJson() => GeometryJsonSerializer.SerializeBoundingBox(this);

        /// <summary>Deserializes a bounding box from JSON.</summary>
        /// <param name="json">JSON string representing a bounding box.</param>
        /// <returns>The deserialized bounding box.</returns>
        public static BoundingBox FromJson(string json) => GeometryJsonSerializer.DeserializeBoundingBox(json);

        /// <summary>Releases resources owned by this bounding box.</summary>
        public void Dispose() { }

        /// <inheritdoc />
        public override string ToString() => $"BoundingBox(MinPoint = {MinPoint}, MaxPoint = {MaxPoint})";
    }
}
