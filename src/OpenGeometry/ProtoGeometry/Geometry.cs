using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>Base type for geometry that can appear in Dynamo's preview.</summary>
    public abstract class Geometry : IGraphicItem, IDisposable
    {
        internal Geometry() { }

        /// <summary>Gets the axis-aligned bounding box enclosing this geometry.</summary>
        public abstract BoundingBox BoundingBox { get; }

        /// <summary>Translates this geometry by a vector and returns a new geometry.</summary>
        /// <param name="direction">Translation displacement vector.</param>
        /// <returns>The translated geometry.</returns>
        public Geometry Translate(Vector direction)
        {
            ArgumentNullException.ThrowIfNull(direction);
            return Translate(direction.X, direction.Y, direction.Z);
        }

        /// <summary>Translates this geometry along a direction by a given distance.</summary>
        /// <param name="direction">Direction vector.</param>
        /// <param name="distance">Signed distance along the normalized direction.</param>
        /// <returns>The translated geometry.</returns>
        public Geometry Translate(Vector direction, double distance)
        {
            ArgumentNullException.ThrowIfNull(direction);
            var delta = direction.Normalized().Scale(distance);
            return Translate(delta.X, delta.Y, delta.Z);
        }

        /// <summary>Translates this geometry by Cartesian offsets.</summary>
        /// <param name="xTranslation">Offset along X.</param>
        /// <param name="yTranslation">Offset along Y.</param>
        /// <param name="zTranslation">Offset along Z.</param>
        /// <returns>The translated geometry.</returns>
        public abstract Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0);

        /// <summary>Rotates this geometry around an axis passing through an origin point.</summary>
        /// <param name="origin">Point on the axis of rotation.</param>
        /// <param name="axis">Axis of rotation.</param>
        /// <param name="degrees">Rotation angle in degrees.</param>
        /// <returns>The rotated geometry.</returns>
        public abstract Geometry Rotate(Point origin, Vector axis, double degrees);

        /// <summary>Rotates this geometry in the given base plane about its normal.</summary>
        /// <param name="basePlane">Base plane whose origin and normal define the axis.</param>
        /// <param name="degrees">Rotation angle in degrees.</param>
        /// <returns>The rotated geometry.</returns>
        public Geometry Rotate(Plane basePlane, double degrees)
        {
            ArgumentNullException.ThrowIfNull(basePlane);
            return Rotate(basePlane.Origin, basePlane.Normal, degrees);
        }

        /// <summary>Scales this geometry uniformly about the world origin.</summary>
        /// <param name="amount">Positive uniform scale factor.</param>
        /// <returns>The scaled geometry.</returns>
        public Geometry Scale(double amount) => Scale(Point.Origin(), amount, amount, amount);

        /// <summary>Scales this geometry non-uniformly about the world origin.</summary>
        /// <param name="xamount">X scale factor.</param>
        /// <param name="yamount">Y scale factor.</param>
        /// <param name="zamount">Z scale factor.</param>
        /// <returns>The scaled geometry.</returns>
        public Geometry Scale(double xamount, double yamount, double zamount) =>
            Scale(Point.Origin(), xamount, yamount, zamount);

        /// <summary>Scales this geometry about a base point.</summary>
        /// <param name="basePoint">Pivot point.</param>
        /// <param name="xamount">X scale factor.</param>
        /// <param name="yamount">Y scale factor.</param>
        /// <param name="zamount">Z scale factor.</param>
        /// <returns>The scaled geometry.</returns>
        public abstract Geometry Scale(Point basePoint, double xamount, double yamount, double zamount);

        /// <summary>Transforms this geometry by a coordinate system.</summary>
        /// <param name="cs">Target coordinate system.</param>
        /// <returns>The transformed geometry.</returns>
        public abstract Geometry Transform(CoordinateSystem cs);

        /// <summary>Transforms this geometry from one coordinate system to another.</summary>
        /// <param name="fromCoordinateSystem">Source coordinate system.</param>
        /// <param name="contextCoordinateSystem">Target coordinate system.</param>
        /// <returns>The transformed geometry.</returns>
        public Geometry Transform(CoordinateSystem fromCoordinateSystem, CoordinateSystem contextCoordinateSystem)
        {
            ArgumentNullException.ThrowIfNull(fromCoordinateSystem);
            ArgumentNullException.ThrowIfNull(contextCoordinateSystem);
            return Transform(fromCoordinateSystem.Inverse().Compose(contextCoordinateSystem));
        }

        /// <summary>Reflects this geometry across <paramref name="mirrorPlane"/>.</summary>
        /// <param name="mirrorPlane">Plane of reflection.</param>
        /// <returns>The mirrored geometry.</returns>
        public Geometry Mirror(Plane mirrorPlane)
        {
            ArgumentNullException.ThrowIfNull(mirrorPlane);
            return MirrorCore(mirrorPlane);
        }

        internal virtual Geometry MirrorCore(Plane mirrorPlane) =>
            throw new NotSupportedException($"Mirror is not supported for {GetType().Name}.");

        /// <summary>Computes the shortest Euclidean distance to another geometry.</summary>
        /// <param name="other">Other geometry.</param>
        /// <returns>Minimum distance.</returns>
        public virtual double DistanceTo(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            throw new NotSupportedException($"DistanceTo between {GetType().Name} and {other.GetType().Name} is not yet supported.");
        }

        /// <summary>Splits this geometry using another geometry tool (for example, splitting a solid with a plane).</summary>
        /// <param name="tool">Splitting tool geometry.</param>
        /// <returns>The resulting split geometry pieces.</returns>
        public virtual Geometry[] Split(Geometry tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            throw new NotSupportedException($"Split of {GetType().Name} with {tool.GetType().Name} is not yet supported.");
        }

        /// <summary>Splits this geometry by <paramref name="tool"/> and removes the resulting piece closest to <paramref name="pick"/>.</summary>
        /// <param name="tool">Trimming tool geometry.</param>
        /// <param name="pick">Point indicating the side/piece to remove.</param>
        /// <returns>The remaining geometry pieces.</returns>
        public virtual Geometry[] Trim(Geometry tool, Point pick)
        {
            ArgumentNullException.ThrowIfNull(tool);
            ArgumentNullException.ThrowIfNull(pick);

            var pieces = Split(tool);
            if (pieces.Length <= 1)
                return pieces;

            var removeIndex = 0;
            var bestDist = double.PositiveInfinity;
            for (var i = 0; i < pieces.Length; i++)
            {
                var d = pieces[i].DistanceTo(pick);
                if (d < bestDist)
                {
                    bestDist = d;
                    removeIndex = i;
                }
            }

            var kept = new List<Geometry>(pieces.Length - 1);
            for (var i = 0; i < pieces.Length; i++)
            {
                if (i == removeIndex)
                    pieces[i].Dispose();
                else
                    kept.Add(pieces[i]);
            }
            return kept.ToArray();
        }

        /// <summary>Computes the geometric intersection of this geometry with <paramref name="other"/>.</summary>
        /// <param name="other">Other geometry to intersect with.</param>
        /// <returns>Intersection points, curves, or solids (empty if disjoint).</returns>
        public virtual Geometry[] Intersect(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return GeometryIntersections.Intersect(this, other);
        }

        /// <summary>Computes the combined geometric intersections of this geometry with each geometry in <paramref name="others"/>.</summary>
        /// <param name="others">Geometries to intersect with.</param>
        /// <returns>All resulting intersection geometries.</returns>
        public Geometry[] IntersectAll(IEnumerable<Geometry> others)
        {
            ArgumentNullException.ThrowIfNull(others);
            var results = new List<Geometry>();
            foreach (var item in others)
            {
                if (item != null)
                    results.AddRange(Intersect(item));
            }
            return results.ToArray();
        }

        /// <summary>Determines whether this geometry intersects <paramref name="other"/>.</summary>
        /// <param name="other">Other geometry.</param>
        /// <returns>True if at least one intersection entity exists; otherwise false.</returns>
        public virtual bool DoesIntersect(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            var hits = Intersect(other);
            var any = hits.Length > 0;
            foreach (var hit in hits)
                hit.Dispose();
            return any;
        }

        /// <summary>SAT export is unsupported in OpenGeometry (tracked compatibility gap).</summary>
        public string ExportToSAT(string filePath) => throw NativeGeometry.UnsupportedFormat("SAT export");

        /// <summary>SAB serialization is unsupported in OpenGeometry (tracked compatibility gap).</summary>
        public byte[] SerializeAsSAB() => throw NativeGeometry.UnsupportedFormat("SAB serialization");

        /// <summary>SolidDef serialization is unsupported in OpenGeometry (tracked compatibility gap).</summary>
        public string ToSolidDef() => throw NativeGeometry.UnsupportedFormat("SolidDef serialization");

        /// <summary>Releases resources owned by this object.</summary>
        public void Dispose()
        {
            ReleaseResources();
        }

        /// <summary>Populates a Dynamo render package with tessellated preview geometry.</summary>
        /// <param name="package">Target render package.</param>
        /// <param name="parameters">Tessellation quality parameters.</param>
        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            ArgumentNullException.ThrowIfNull(package);
            ArgumentNullException.ThrowIfNull(parameters);
            WritePreview(package, parameters);
        }

        internal abstract void WritePreview(IRenderPackage package, TessellationParameters parameters);

        internal virtual void ReleaseResources() { }
    }

    /// <summary>Helper extension methods on geometry arrays matching Dynamo's ProtoGeometry conventions.</summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class GeometryExtension
    {
        /// <summary>Executes an action on each element of an array.</summary>
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            if (array == null || action == null)
                return;
            foreach (var item in array)
                action(item);
        }

        /// <summary>Executes an action on each element of a jagged 2D array.</summary>
        public static void ForEach<T>(this T[][] array, Action<T> action)
        {
            if (array == null || action == null)
                return;
            foreach (var row in array)
            {
                if (row == null)
                    continue;
                foreach (var item in row)
                    action(item);
            }
        }
    }

    internal static class GeometryMath
    {
        internal static Vector RotateVector(Vector vector, Vector axis, double degrees)
        {
            var k = axis.Normalized();
            var radians = degrees * (Math.PI / 180.0);
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);
            // Rodrigues' rotation formula: v*cos + (k x v)*sin + k*(k.v)*(1 - cos)
            var term1 = vector.Scale(cos);
            var term2 = k.Cross(vector).Scale(sin);
            var term3 = k.Scale(k.Dot(vector) * (1.0 - cos));
            return term1.Add(term2).Add(term3);
        }
    }

    internal static class GeometryIntersections
    {
        private const double Tolerance = 1e-6;

        internal static Geometry[] Intersect(Geometry a, Geometry b)
        {
            if (a is Point pa)
                return IntersectPointWith(pa, b);
            if (b is Point pb)
                return IntersectPointWith(pb, a);

            if (a is Curve ca && b is Curve cb)
                return IntersectCurves(ca, cb);

            if (a is Curve c1 && b is Topology t1)
                return IntersectCurveWithTopology(c1, t1);
            if (a is Topology t2 && b is Curve c2)
                return IntersectCurveWithTopology(c2, t2);

            if (a is Curve c3 && b is Plane p1)
                return IntersectCurveWithPlane(c3, p1);
            if (a is Plane p2 && b is Curve c4)
                return IntersectCurveWithPlane(c4, p2);

            if (a is Topology t3 && b is Plane p3)
                return PackList(NativeGeometry.IntersectBrepPlane(t3.Handle, p3.Origin.Position, p3.Normal.Components));
            if (a is Plane p4 && b is Topology t4)
                return PackList(NativeGeometry.IntersectBrepPlane(t4.Handle, p4.Origin.Position, p4.Normal.Components));

            if (a is Topology t5 && b is Topology t6)
                return PackList(NativeGeometry.IntersectBreps(t5.Handle, t6.Handle));

            throw new NotSupportedException($"Intersect between {a.GetType().Name} and {b.GetType().Name} is not yet supported.");
        }

        private static Geometry[] IntersectPointWith(Point pt, Geometry other)
        {
            if (other is Plane plane)
            {
                var d = Math.Abs(Vector.ByTwoPoints(plane.Origin, pt).Dot(plane.Normal.Normalized()));
                return d <= Tolerance ? new Geometry[] { Point.ByCoordinates(pt.X, pt.Y, pt.Z) } : Array.Empty<Geometry>();
            }
            return pt.DistanceTo(other) <= Tolerance
                ? new Geometry[] { Point.ByCoordinates(pt.X, pt.Y, pt.Z) }
                : Array.Empty<Geometry>();
        }

        private static Geometry[] IntersectCurves(Curve a, Curve b)
        {
            var results = new List<Geometry>();
            foreach (var ha in a.GetAtomicHandles())
            {
                foreach (var hb in b.GetAtomicHandles())
                {
                    var (pts, crvs) = NativeGeometry.IntersectCurves(ha, hb);
                    AppendPacked(results, pts, crvs);
                }
            }
            return DeduplicatePoints(results);
        }

        private static Geometry[] IntersectCurveWithTopology(Curve curve, Topology topo)
        {
            var results = new List<Geometry>();
            foreach (var h in curve.GetAtomicHandles())
            {
                var (pts, crvs) = NativeGeometry.IntersectCurveBrep(h, topo.Handle);
                AppendPacked(results, pts, crvs);
            }
            return DeduplicatePoints(results);
        }

        private static Geometry[] IntersectCurveWithPlane(Curve curve, Plane plane)
        {
            using var sheet = CreatePlaneSheetForCurve(curve, plane);
            return IntersectCurveWithTopology(curve, sheet);
        }

        internal static Surface CreatePlaneSheetForCurve(Curve curve, Plane plane)
        {
            var bb = curve.BoundingBox;
            var corners = new[]
            {
                bb.MinPoint,
                bb.MaxPoint,
                Point.ByCoordinates(bb.MinPoint.X, bb.MaxPoint.Y, bb.MinPoint.Z),
                Point.ByCoordinates(bb.MaxPoint.X, bb.MinPoint.Y, bb.MaxPoint.Z)
            };
            var ux = plane.XAxis.Normalized();
            var uy = plane.YAxis.Normalized();
            var minU = double.PositiveInfinity;
            var maxU = double.NegativeInfinity;
            var minV = double.PositiveInfinity;
            var maxV = double.NegativeInfinity;
            foreach (var c in corners)
            {
                var rel = Vector.ByTwoPoints(plane.Origin, c);
                var u = rel.Dot(ux);
                var v = rel.Dot(uy);
                minU = Math.Min(minU, u);
                maxU = Math.Max(maxU, u);
                minV = Math.Min(minV, v);
                maxV = Math.Max(maxV, v);
            }
            var pad = Math.Max(10.0, Math.Max(maxU - minU, maxV - minV) * 2.0);
            minU -= pad; maxU += pad;
            minV -= pad; maxV += pad;

            var p0 = plane.Origin.Add(ux.Scale(minU)).Add(uy.Scale(minV));
            var p1 = plane.Origin.Add(ux.Scale(maxU)).Add(uy.Scale(minV));
            var p2 = plane.Origin.Add(ux.Scale(maxU)).Add(uy.Scale(maxV));
            var p3 = plane.Origin.Add(ux.Scale(minU)).Add(uy.Scale(maxV));
            return Surface.ByPerimeterPoints(new[] { p0, p1, p2, p3 });
        }

        private static Geometry[] PackList((NativePoint[] Points, NativeGeometry.CurveHandle[] Curves) data)
        {
            var results = new List<Geometry>(data.Points.Length + data.Curves.Length);
            AppendPacked(results, data.Points, data.Curves);
            return DeduplicatePoints(results);
        }

        private static void AppendPacked(
            List<Geometry> results, NativePoint[] points, NativeGeometry.CurveHandle[] curves)
        {
            foreach (var p in points)
                results.Add(new Point(p));
            foreach (var c in curves)
                results.Add(new Curve(c));
        }

        private static Geometry[] DeduplicatePoints(List<Geometry> items)
        {
            var unique = new List<Geometry>(items.Count);
            foreach (var item in items)
            {
                if (item is Point pt)
                {
                    var dup = unique.OfType<Point>().Any(existing => existing.DistanceTo(pt) <= 1e-5);
                    if (dup)
                        pt.Dispose();
                    else
                        unique.Add(pt);
                }
                else
                {
                    unique.Add(item);
                }
            }
            return unique.ToArray();
        }
    }
}
