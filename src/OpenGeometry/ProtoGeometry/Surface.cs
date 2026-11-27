using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A bounded 3D parametric surface backed by a single-face SMLib sheet BRep.</summary>
    public class Surface : Topology
    {
        internal Surface(NativeGeometry.SolidHandle handle) : base(handle)
        {
        }

        /// <summary>Gets the surface area within trim boundaries.</summary>
        public double Area => NativeGeometry.Area(Handle);

        /// <summary>Gets the total perimeter length of all bounding edges.</summary>
        public double Perimeter => Edges.Sum(e => e.Length);

        /// <summary>Returns the bounding curves around the perimeter of this surface.</summary>
        public Curve[] PerimeterCurves() => Edges.Select(e => e.CurveGeometry).ToArray();

        /// <summary>Evaluates a 3D point on this surface at normalized parameters (<paramref name="u"/>, <paramref name="v"/>) in [0, 1] x [0, 1].</summary>
        public Point PointAtParameter(double u, double v)
        {
            var (pt, _) = NativeGeometry.SurfaceEvaluate(Handle, u, v);
            return new Point(pt);
        }

        /// <summary>Evaluates the unit normal vector on this surface at normalized parameters (<paramref name="u"/>, <paramref name="v"/>) in [0, 1] x [0, 1].</summary>
        public Vector NormalAtParameter(double u, double v)
        {
            var (_, nrm) = NativeGeometry.SurfaceEvaluate(Handle, u, v);
            return new Vector(nrm);
        }

        /// <summary>Creates a surface bounded by an ordered list of perimeter points (4 points creates a bilinear patch; 3 or 5+ planar points creates a planar face).</summary>
        /// <param name="points">Ordered boundary points.</param>
        /// <returns>The created surface.</returns>
        public static Surface ByPerimeterPoints(IEnumerable<Point> points)
        {
            ArgumentNullException.ThrowIfNull(points);
            var pts = points.Where(p => p != null).ToList();
            if (pts.Count > 1 && pts[0].DistanceTo(pts[^1]) <= 1e-9)
                pts.RemoveAt(pts.Count - 1);
            if (pts.Count < 3)
                throw new ArgumentException("Surface.ByPerimeterPoints requires at least 3 points.", nameof(points));

            if (pts.Count == 4)
            {
                var handle = NativeGeometry.SurfaceByFourPoints(
                    pts[0].Position, pts[1].Position, pts[2].Position, pts[3].Position);
                return new Surface(handle);
            }

            using var poly = Polygon.ByPoints(pts);
            return ByPatch(poly);
        }

        /// <summary>Creates a planar trimmed surface bounded by a closed curve or polycurve.</summary>
        /// <param name="closedCurve">Closed planar boundary curve.</param>
        /// <returns>The planar surface.</returns>
        public static Surface ByPatch(Curve closedCurve)
        {
            ArgumentNullException.ThrowIfNull(closedCurve);
            var handles = closedCurve.GetAtomicHandles();
            return new Surface(NativeGeometry.SurfaceByPatch(handles));
        }

        /// <summary>Creates a lofted surface passing through an ordered sequence of cross-section curves.</summary>
        /// <param name="crossSections">At least two profile curves.</param>
        /// <returns>The lofted surface.</returns>
        public static Surface ByLoft(IEnumerable<Curve> crossSections)
        {
            ArgumentNullException.ThrowIfNull(crossSections);
            var profiles = crossSections.Where(c => c != null).ToArray();
            if (profiles.Length < 2)
                throw new ArgumentException("Surface.ByLoft requires at least 2 cross-section curves.", nameof(crossSections));

            var handles = new NativeGeometry.CurveHandle[profiles.Length];
            for (var i = 0; i < profiles.Length; i++)
            {
                var atomic = profiles[i].GetAtomicHandles();
                if (atomic.Length != 1)
                    throw new ArgumentException("Surface.ByLoft requires single-segment curves for each cross-section.", nameof(crossSections));
                handles[i] = atomic[0];
            }

            return new Surface(NativeGeometry.SurfaceByLoft(handles));
        }

        /// <summary>Creates a ruled loft surface linearly interpolating between cross-section curves.</summary>
        public static Surface ByRuledLoft(IEnumerable<Curve> crossSections) => ByLoft(crossSections);

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return new Surface(NativeGeometry.Translate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation)));
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            return new Surface(NativeGeometry.Rotate(Handle, origin.Position, axis.Components, degrees));
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            return new Surface(NativeGeometry.Scale(Handle, basePoint.Position, xamount, yamount, zamount));
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return new Surface(NativeGeometry.Transform(
                Handle,
                cs.Origin.Position,
                cs.XAxis.Components,
                cs.YAxis.Components,
                cs.ZAxis.Components));
        }

        /// <inheritdoc />
        public override double DistanceTo(Geometry other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other is Point pt)
                return NativeGeometry.ClosestPoint(Handle, pt.Position).Distance;
            if (other is Topology otherTopo)
                return NativeGeometry.Distance(Handle, otherTopo.Handle);
            return base.DistanceTo(other);
        }

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            var chordTolerance = parameters.Tolerance > 0 ? parameters.Tolerance : 0;
            foreach (var vertex in NativeGeometry.Tessellate(Handle, chordTolerance))
            {
                var position = vertex.Position;
                var normal = vertex.Normal;
                package.AddTriangleVertex(position.X, position.Y, position.Z);
                package.AddTriangleVertexNormal(normal.X, normal.Y, normal.Z);
                package.AddTriangleVertexUV(0, 0);
            }
        }
    }
}
