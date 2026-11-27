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

        /// <summary>Creates a new surface offset along the surface normal by <paramref name="distance"/>.</summary>
        /// <param name="distance">Signed offset distance along surface normal.</param>
        /// <returns>The offset surface.</returns>
        public Surface Offset(double distance) => new(NativeGeometry.SurfaceOffset(Handle, distance));

        /// <summary>Thickens this surface into a solid symmetrically on both sides.</summary>
        /// <param name="thickness">Total solid thickness.</param>
        /// <returns>The thickened solid.</returns>
        public Solid Thicken(double thickness) => Thicken(thickness, both_sides: true);

        /// <summary>Thickens this surface into a solid.</summary>
        /// <param name="thickness">Total thickness (positive along normal when single-sided).</param>
        /// <param name="both_sides">If true, thickens equally on both sides of the surface; if false, thickens on the normal side only.</param>
        /// <returns>The thickened solid.</returns>
        public Solid Thicken(double thickness, bool both_sides) =>
            new(NativeGeometry.SurfaceThicken(Handle, thickness, both_sides));

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

        /// <summary>Creates a lofted surface through cross-section curves guided by one or more rail/guide curves.</summary>
        /// <param name="crossSections">At least two profile curves.</param>
        /// <param name="guideCurves">Guide rail curves connecting the cross-sections.</param>
        /// <returns>The guided loft surface.</returns>
        public static Surface ByLoft(IEnumerable<Curve> crossSections, IEnumerable<Curve> guideCurves)
        {
            ArgumentNullException.ThrowIfNull(crossSections);
            var profiles = crossSections.Where(c => c != null).ToArray();
            if (profiles.Length < 2)
                throw new ArgumentException("Surface.ByLoft requires at least 2 cross-section curves.", nameof(crossSections));

            var guides = guideCurves?.Where(c => c != null).ToArray() ?? Array.Empty<Curve>();
            if (guides.Length >= 2 && profiles.Length == 2)
                return BySweep2Rails(guides[0], guides[1], profiles[0]);
            if (guides.Length == 1 && profiles.Length == 2)
            {
                // Sample guided intermediate sections between the two profiles along the guide curve.
                var g = guides[0];
                var p0 = profiles[0];
                var p1 = profiles[1];
                var g0 = g.StartPoint;
                var g1 = g.EndPoint;
                var flip = g0.DistanceTo(p1.StartPoint) < g0.DistanceTo(p0.StartPoint);
                var generated = new List<Curve>();
                try
                {
                    var all = new List<Curve> { p0 };
                    const int interior = 5;
                    for (var i = 1; i <= interior; i++)
                    {
                        var t = (double)i / (interior + 1);
                        var gt = flip ? 1.0 - t : t;
                        var gp = g.PointAtParameter(gt);
                        var gl = Point.ByCoordinates(
                            (1.0 - gt) * g0.X + gt * g1.X,
                            (1.0 - gt) * g0.Y + gt * g1.Y,
                            (1.0 - gt) * g0.Z + gt * g1.Z);
                        var shift = Vector.ByTwoPoints(gl, gp);
                        var linearDelta = Vector.ByTwoPoints(p0.StartPoint, p1.StartPoint).Scale(t);
                        var moved = (Curve)p0.Translate(linearDelta.Add(shift));
                        generated.Add(moved);
                        all.Add(moved);
                    }
                    all.Add(p1);
                    return ByLoft(all);
                }
                finally
                {
                    foreach (var c in generated) c.Dispose();
                }
            }
            return ByLoft(profiles);
        }

        /// <summary>Creates a surface by sweeping a profile curve along a path curve.</summary>
        /// <param name="profile">Curve to sweep.</param>
        /// <param name="path">Path curve to sweep along.</param>
        /// <returns>The swept surface.</returns>
        public static Surface BySweep(Curve profile, Curve path)
        {
            ArgumentNullException.ThrowIfNull(profile);
            ArgumentNullException.ThrowIfNull(path);
            var profileAtomic = profile.GetAtomicHandles();
            var pathAtomic = path.GetAtomicHandles();
            if (profileAtomic.Length != 1)
                throw new ArgumentException("Surface.BySweep requires a single-segment profile curve.", nameof(profile));
            if (pathAtomic.Length != 1)
                throw new ArgumentException("Surface.BySweep requires a single-segment path curve.", nameof(path));
            return new Surface(NativeGeometry.SurfaceBySweep(profileAtomic[0], pathAtomic[0]));
        }

        /// <summary>Creates a surface by sweeping a profile curve along a path curve.</summary>
        /// <param name="profile">Curve to sweep.</param>
        /// <param name="path">Path curve to sweep along.</param>
        /// <param name="cutEndOff">Optional end-cut option.</param>
        /// <returns>The swept surface.</returns>
        public static Surface BySweep(Curve profile, Curve path, bool cutEndOff)
        {
            _ = cutEndOff;
            return BySweep(profile, path);
        }

        /// <summary>Creates a surface by sweeping a profile curve along two rail curves.</summary>
        /// <param name="path">Primary rail curve to sweep along.</param>
        /// <param name="guideRail">Secondary rail curve guiding sweep orientation and width.</param>
        /// <param name="profile">Profile curve spanning between the rails.</param>
        /// <returns>The two-rail swept surface.</returns>
        public static Surface BySweep2Rails(Curve path, Curve guideRail, Curve profile)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(guideRail);
            ArgumentNullException.ThrowIfNull(profile);
            var pathAtomic = path.GetAtomicHandles();
            var guideAtomic = guideRail.GetAtomicHandles();
            var profileAtomic = profile.GetAtomicHandles();
            if (pathAtomic.Length != 1)
                throw new ArgumentException("Surface.BySweep2Rails requires a single-segment path curve.", nameof(path));
            if (guideAtomic.Length != 1)
                throw new ArgumentException("Surface.BySweep2Rails requires a single-segment guide rail curve.", nameof(guideRail));
            if (profileAtomic.Length != 1)
                throw new ArgumentException("Surface.BySweep2Rails requires a single-segment profile curve.", nameof(profile));
            return new Surface(NativeGeometry.SurfaceBySweep2Rails(pathAtomic[0], guideAtomic[0], profileAtomic[0]));
        }

        /// <summary>Creates a surface by revolving a profile curve around an axis.</summary>
        /// <param name="profile">Profile curve to revolve.</param>
        /// <param name="axisOrigin">Origin point of the axis of revolution.</param>
        /// <param name="axisDirection">Direction vector of the axis of revolution.</param>
        /// <param name="startAngle">Start angle in degrees.</param>
        /// <param name="sweepAngle">Sweep angle in degrees.</param>
        /// <returns>The revolved surface.</returns>
        public static Surface ByRevolve(
            Curve profile, Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle)
        {
            ArgumentNullException.ThrowIfNull(profile);
            ArgumentNullException.ThrowIfNull(axisOrigin);
            ArgumentNullException.ThrowIfNull(axisDirection);
            return new Surface(NativeGeometry.SurfaceByRevolve(
                profile.GetAtomicHandles(),
                axisOrigin.Position,
                axisDirection.Components,
                startAngle,
                sweepAngle));
        }

        /// <summary>Trims this surface with one or more closed edge loops lying on the surface.</summary>
        /// <param name="loops">Closed boundary loops (first loop is outer boundary; subsequent loops are inner holes).</param>
        /// <returns>The trimmed surface.</returns>
        public Surface TrimWithEdgeLoops(IEnumerable<PolyCurve> loops) => TrimWithEdgeLoops(loops, 0.0);

        /// <summary>Trims this surface with one or more closed edge loops lying on the surface.</summary>
        /// <param name="loops">Closed boundary loops (first loop is outer boundary; subsequent loops are inner holes).</param>
        /// <param name="tolerance">Optional loop coincidence tolerance.</param>
        /// <returns>The trimmed surface.</returns>
        public Surface TrimWithEdgeLoops(IEnumerable<PolyCurve> loops, double tolerance)
        {
            ArgumentNullException.ThrowIfNull(loops);
            _ = tolerance;
            var loopArray = loops.Where(l => l != null).ToArray();
            if (loopArray.Length == 0)
                throw new ArgumentException("Surface.TrimWithEdgeLoops requires at least one loop.", nameof(loops));

            var allHandles = new List<NativeGeometry.CurveHandle>();
            var counts = new uint[loopArray.Length];
            for (var i = 0; i < loopArray.Length; i++)
            {
                var atomic = loopArray[i].GetAtomicHandles();
                counts[i] = (uint)atomic.Length;
                allHandles.AddRange(atomic);
            }
            return new Surface(NativeGeometry.SurfaceTrimWithCurves(Handle, allHandles.ToArray(), counts));
        }

        /// <summary>Extracts an iso-parameter curve on this surface along U (isoDirection = 0) or V (isoDirection = 1).</summary>
        /// <param name="isoDirection">0 for a U parameter line (constant V); 1 for a V parameter line (constant U).</param>
        /// <param name="parameter">Normalized parameter in [0, 1] along the constant direction.</param>
        /// <returns>The extracted iso-parameter curve.</returns>
        public Curve GetIsoline(int isoDirection, double parameter)
        {
            if (isoDirection != 0 && isoDirection != 1)
                throw new ArgumentOutOfRangeException(nameof(isoDirection), "isoDirection must be 0 (U) or 1 (V).");
            return new Curve(NativeGeometry.SurfaceIsoCurve(Handle, isoDirection, parameter));
        }

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

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            return new Surface(NativeGeometry.BrepMirror(
                Handle,
                mirrorPlane.Origin.Position,
                mirrorPlane.XAxis.Components,
                mirrorPlane.YAxis.Components));
        }

        /// <inheritdoc />
        public override bool IsAlmostEqualTo(Geometry other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is not Surface s || GetType() != other.GetType())
                return false;
            var bb1 = BoundingBox;
            var bb2 = s.BoundingBox;
            return bb1.MinPoint.DistanceTo(bb2.MinPoint) <= 1e-4
                && bb1.MaxPoint.DistanceTo(bb2.MaxPoint) <= 1e-4
                && Math.Abs(Area - s.Area) <= Math.Max(1e-4, Math.Abs(Area) * 1e-5);
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
            WriteNativeMeshPreview(package, Handle, parameters);
        }
    }
}
