using System;
using System.Collections.Generic;
using System.Linq;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>An axis-aligned 3D bounding box.</summary>
    public sealed class BoundingBox : IDisposable
    {
        private BoundingBox(Point minPoint, Point maxPoint, CoordinateSystem contextCoordinateSystem = null)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
            ContextCoordinateSystem = contextCoordinateSystem ?? CoordinateSystem.Identity();
        }

        /// <summary>Gets the minimum corner of the bounding box.</summary>
        public Point MinPoint { get; }

        /// <summary>Gets the maximum corner of the bounding box.</summary>
        public Point MaxPoint { get; }

        /// <summary>Gets the coordinate system in which this bounding box is oriented.</summary>
        public CoordinateSystem ContextCoordinateSystem { get; }

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

        /// <summary>Computes an oriented minimum-volume bounding box enclosing a collection of geometries.</summary>
        /// <param name="geom">Geometries to enclose.</param>
        /// <returns>An oriented bounding box whose <see cref="ContextCoordinateSystem"/> aligns with the principal axes.</returns>
        public static BoundingBox ByMinimumVolume(IEnumerable<Geometry> geom)
        {
            ArgumentNullException.ThrowIfNull(geom);
            var list = geom.Where(g => g != null).ToArray();
            if (list.Length == 0)
                throw new ArgumentException("At least one geometry object is required.", nameof(geom));

            var samplePoints = new List<Point>();
            foreach (var g in list)
            {
                if (g is Point pt)
                {
                    samplePoints.Add(pt);
                }
                else
                {
                    var bb = g.BoundingBox;
                    samplePoints.Add(bb.MinPoint);
                    samplePoints.Add(bb.MaxPoint);
                    samplePoints.Add(Point.ByCoordinates(bb.MinPoint.X, bb.MaxPoint.Y, bb.MinPoint.Z));
                    samplePoints.Add(Point.ByCoordinates(bb.MaxPoint.X, bb.MinPoint.Y, bb.MaxPoint.Z));
                }
            }

            if (samplePoints.Count < 3)
                return ByGeometry(list);

            double cx = samplePoints.Average(p => p.X);
            double cy = samplePoints.Average(p => p.Y);
            double cz = samplePoints.Average(p => p.Z);

            var cov = new double[3, 3];
            foreach (var p in samplePoints)
            {
                var dx = p.X - cx;
                var dy = p.Y - cy;
                var dz = p.Z - cz;
                cov[0, 0] += dx * dx; cov[0, 1] += dx * dy; cov[0, 2] += dx * dz;
                cov[1, 1] += dy * dy; cov[1, 2] += dy * dz;
                cov[2, 2] += dz * dz;
            }
            cov[1, 0] = cov[0, 1];
            cov[2, 0] = cov[0, 2];
            cov[2, 1] = cov[1, 2];

            var eig = new double[3, 3]
            {
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 1 }
            };
            for (var iter = 0; iter < 30; iter++)
            {
                int p = 0, q = 1;
                var maxOff = Math.Abs(cov[0, 1]);
                if (Math.Abs(cov[0, 2]) > maxOff) { maxOff = Math.Abs(cov[0, 2]); p = 0; q = 2; }
                if (Math.Abs(cov[1, 2]) > maxOff) { maxOff = Math.Abs(cov[1, 2]); p = 1; q = 2; }
                if (maxOff <= 1e-14)
                    break;

                var tau = (cov[q, q] - cov[p, p]) / (2.0 * cov[p, q]);
                var t = tau >= 0
                    ? 1.0 / (tau + Math.Sqrt(1.0 + tau * tau))
                    : -1.0 / (-tau + Math.Sqrt(1.0 + tau * tau));
                var c = 1.0 / Math.Sqrt(1.0 + t * t);
                var s = t * c;

                var app = cov[p, p];
                var aqq = cov[q, q];
                var apq = cov[p, q];
                cov[p, p] = app - t * apq;
                cov[q, q] = aqq + t * apq;
                cov[p, q] = cov[q, p] = 0.0;

                for (var r = 0; r < 3; r++)
                {
                    if (r != p && r != q)
                    {
                        var arp = cov[r, p];
                        var arq = cov[r, q];
                        cov[r, p] = cov[p, r] = c * arp - s * arq;
                        cov[r, q] = cov[q, r] = s * arp + c * arq;
                    }
                    var vrp = eig[r, p];
                    var vrq = eig[r, q];
                    eig[r, p] = c * vrp - s * vrq;
                    eig[r, q] = s * vrp + c * vrq;
                }
            }

            var ax0 = Vector.ByCoordinates(eig[0, 0], eig[1, 0], eig[2, 0]).Normalized();
            var ax1 = Vector.ByCoordinates(eig[0, 1], eig[1, 1], eig[2, 1]).Normalized();
            var ax2 = ax0.Cross(ax1).Normalized();
            ax1 = ax2.Cross(ax0).Normalized();

            var centroid = Point.ByCoordinates(cx, cy, cz);
            double minU = double.PositiveInfinity, maxU = double.NegativeInfinity;
            double minV = double.PositiveInfinity, maxV = double.NegativeInfinity;
            double minW = double.PositiveInfinity, maxW = double.NegativeInfinity;
            foreach (var p in samplePoints)
            {
                var rel = Vector.ByTwoPoints(centroid, p);
                var u = rel.Dot(ax0);
                var v = rel.Dot(ax1);
                var w = rel.Dot(ax2);
                minU = Math.Min(minU, u); maxU = Math.Max(maxU, u);
                minV = Math.Min(minV, v); maxV = Math.Max(maxV, v);
                minW = Math.Min(minW, w); maxW = Math.Max(maxW, w);
            }

            var boxOrigin = centroid.Add(ax0.Scale(minU)).Add(ax1.Scale(minV)).Add(ax2.Scale(minW));
            var cs = CoordinateSystem.ByOriginVectors(boxOrigin, ax0, ax1, ax2);
            var localMin = Point.ByCoordinates(0, 0, 0);
            var localMax = Point.ByCoordinates(maxU - minU, maxV - minV, maxW - minW);
            return new BoundingBox(localMin, localMax, cs);
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
        public string ToJson()
        {
            if (!ContextCoordinateSystem.IsEqualTo(CoordinateSystem.Identity()))
                throw new NotSupportedException("JSON serialization is not supported for oriented BoundingBox instances.");
            return GeometryJsonSerializer.SerializeBoundingBox(this);
        }

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
