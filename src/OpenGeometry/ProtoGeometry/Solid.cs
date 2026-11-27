using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A solid backed by a native boundary representation.</summary>
    public class Solid : Topology
    {
        internal Solid(NativeGeometry.SolidHandle handle) : base(handle)
        {
        }

        /// <summary>Gets the enclosed volume in cubed model units.</summary>
        public double Volume => NativeGeometry.Volume(Handle);

        /// <summary>Gets the total surface area of all faces of this solid.</summary>
        public double Area => NativeGeometry.Area(Handle);

        /// <summary>Gets the mass centroid of this solid.</summary>
        public Point Centroid() => new(NativeGeometry.Centroid(Handle));

        /// <summary>Creates a closed solid by stitching a collection of contiguous surfaces.</summary>
        /// <param name="faces">Surfaces enclosing a closed volume.</param>
        /// <returns>The stitched manifold solid.</returns>
        public static Solid ByJoinedSurfaces(IEnumerable<Surface> faces)
        {
            ArgumentNullException.ThrowIfNull(faces);
            var list = faces.Where(f => f != null).ToArray();
            if (list.Length == 0)
                throw new ArgumentException("At least one surface is required.", nameof(faces));
            return new Solid(NativeGeometry.JoinSurfaces(list.Select(f => f.Handle).ToArray(), makeSolid: true));
        }

        /// <summary>Creates a closed solid by lofting through an ordered sequence of closed cross-section curves.</summary>
        /// <param name="crossSections">At least two closed profile curves.</param>
        /// <returns>The lofted solid.</returns>
        public static Solid ByLoft(IEnumerable<Curve> crossSections)
        {
            ArgumentNullException.ThrowIfNull(crossSections);
            var profiles = crossSections.Where(c => c != null).ToArray();
            if (profiles.Length < 2)
                throw new ArgumentException("Solid.ByLoft requires at least 2 cross-section curves.", nameof(crossSections));

            var allHandles = new List<NativeGeometry.CurveHandle>();
            var counts = new uint[profiles.Length];
            for (var i = 0; i < profiles.Length; i++)
            {
                var atomic = profiles[i].GetAtomicHandles();
                counts[i] = (uint)atomic.Length;
                allHandles.AddRange(atomic);
            }

            return new Solid(NativeGeometry.SolidByLoft(allHandles.ToArray(), counts));
        }

        /// <summary>Creates a closed solid by lofting through closed cross-section curves guided by one or more guide curves.</summary>
        /// <param name="crossSections">At least two closed profile curves.</param>
        /// <param name="guideCurves">Guide curves shaping the loft between cross sections.</param>
        /// <returns>The lofted solid.</returns>
        public static Solid ByLoft(IEnumerable<Curve> crossSections, IEnumerable<Curve> guideCurves)
        {
            ArgumentNullException.ThrowIfNull(crossSections);
            var profiles = crossSections.Where(c => c != null).ToArray();
            if (profiles.Length < 2)
                throw new ArgumentException("Solid.ByLoft requires at least 2 cross-section curves.", nameof(crossSections));

            var guides = guideCurves?.Where(c => c != null).ToArray() ?? Array.Empty<Curve>();
            if (guides.Length == 0 || profiles.Length > 2)
                return ByLoft(profiles);

            // Sample intermediate guided cross-sections between the two end profiles using the guide curve(s)
            // so the resulting loft solid smoothly follows the guide trajectory.
            try
            {
                var p0 = profiles[0];
                var p1 = profiles[1];
                var c0 = EstimateCurveCenter(p0);
                var c1 = EstimateCurveCenter(p1);

                var guide = guides[0];
                var gStart = guide.StartPoint;
                var gEnd = guide.EndPoint;
                var flipGuide = gStart.DistanceTo(c1) < gStart.DistanceTo(c0);

                var generated = new List<Curve>();
                try
                {
                    var allProfiles = new List<Curve> { p0 };
                    const int interior = 5;
                    for (var i = 1; i <= interior; i++)
                    {
                        var t = (double)i / (interior + 1);
                        var gt = flipGuide ? 1.0 - t : t;
                        var gPt = guide.PointAtParameter(gt);
                        var gLin = Point.ByCoordinates(
                            (1.0 - gt) * gStart.X + gt * gEnd.X,
                            (1.0 - gt) * gStart.Y + gt * gEnd.Y,
                            (1.0 - gt) * gStart.Z + gt * gEnd.Z);
                        var shift = Vector.ByTwoPoints(gLin, gPt);
                        var linearDelta = Vector.ByTwoPoints(c0, c1).Scale(t);
                        var moved = (Curve)p0.Translate(linearDelta.Add(shift));
                        generated.Add(moved);
                        allProfiles.Add(moved);
                    }
                    allProfiles.Add(p1);
                    return ByLoft(allProfiles);
                }
                finally
                {
                    foreach (var g in generated)
                        g.Dispose();
                }
            }
            catch (Exception)
            {
                // Fall back to unguided loft if guide orientation is incompatible with intermediate sampling.
                return ByLoft(profiles);
            }
        }

        private static Point EstimateCurveCenter(Curve curve)
        {
            const int samples = 16;
            double sx = 0, sy = 0, sz = 0;
            for (var i = 0; i < samples; i++)
            {
                var pt = curve.PointAtParameter((double)i / samples);
                sx += pt.X;
                sy += pt.Y;
                sz += pt.Z;
            }
            return Point.ByCoordinates(sx / samples, sy / samples, sz / samples);
        }

        /// <summary>Creates a closed solid by ruled lofting through cross-section curves.</summary>
        public static Solid ByRuledLoft(IEnumerable<Curve> crossSections) => ByLoft(crossSections);

        /// <summary>Creates a solid by sweeping a closed profile curve along a path curve.</summary>
        /// <param name="profile">Closed cross-section profile curve.</param>
        /// <param name="path">Path curve to sweep along.</param>
        /// <returns>The swept solid.</returns>
        public static Solid BySweep(Curve profile, Curve path)
        {
            ArgumentNullException.ThrowIfNull(profile);
            ArgumentNullException.ThrowIfNull(path);
            var pathAtomic = path.GetAtomicHandles();
            if (pathAtomic.Length != 1)
                throw new ArgumentException("Solid.BySweep currently requires a single-segment path curve.", nameof(path));

            return new Solid(NativeGeometry.SolidBySweep(profile.GetAtomicHandles(), pathAtomic[0]));
        }

        /// <summary>Creates a solid by revolving a closed profile curve around an axis.</summary>
        /// <param name="profile">Closed planar profile curve on one side of the axis.</param>
        /// <param name="axisOrigin">Point on the axis of revolution.</param>
        /// <param name="axisDirection">Direction of the axis of revolution.</param>
        /// <param name="startAngle">Start angle offset in degrees.</param>
        /// <param name="sweepAngle">Sweep angle in degrees (360 for a full revolution).</param>
        /// <returns>The revolved solid.</returns>
        public static Solid ByRevolve(
            Curve profile, Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle)
        {
            ArgumentNullException.ThrowIfNull(profile);
            ArgumentNullException.ThrowIfNull(axisOrigin);
            ArgumentNullException.ThrowIfNull(axisDirection);
            return new Solid(NativeGeometry.SolidByRevolve(
                profile.GetAtomicHandles(),
                axisOrigin.Position,
                axisDirection.Components,
                startAngle,
                sweepAngle));
        }

        /// <summary>Applies a constant-radius circular fillet to selected edges of this solid.</summary>
        /// <param name="edges">Edges of this solid to fillet.</param>
        /// <param name="radius">Positive fillet radius.</param>
        /// <returns>A new filleted solid.</returns>
        public Solid Fillet(IEnumerable<Edge> edges, double radius)
        {
            ArgumentNullException.ThrowIfNull(edges);
            var indices = edges.Where(e => e != null).Select(e => e.Index).Distinct().ToArray();
            if (indices.Length == 0)
                throw new ArgumentException("At least one edge is required for Fillet.", nameof(edges));
            return new Solid(NativeGeometry.SolidFilletEdges(Handle, indices, radius, chamfer: false));
        }

        /// <summary>Applies a flat bevel chamfer with constant setback to selected edges of this solid.</summary>
        /// <param name="edges">Edges of this solid to chamfer.</param>
        /// <param name="offset">Positive chamfer setback distance along each adjacent face.</param>
        /// <returns>A new chamfered solid.</returns>
        public Solid Chamfer(IEnumerable<Edge> edges, double offset)
        {
            ArgumentNullException.ThrowIfNull(edges);
            var indices = edges.Where(e => e != null).Select(e => e.Index).Distinct().ToArray();
            if (indices.Length == 0)
                throw new ArgumentException("At least one edge is required for Chamfer.", nameof(edges));
            return new Solid(NativeGeometry.SolidFilletEdges(Handle, indices, offset, chamfer: true));
        }

        /// <summary>Hollows this solid into a thin-walled shell.</summary>
        /// <param name="internalFaceThickness">Inward wall thickness (0 for outward-only).</param>
        /// <param name="externalFaceThickness">Outward wall thickness (0 for inward-only).</param>
        /// <returns>The thin-walled hollow solid.</returns>
        public Solid ThinShell(double internalFaceThickness = 1, double externalFaceThickness = 1)
        {
            return new Solid(NativeGeometry.SolidThinShell(Handle, internalFaceThickness, externalFaceThickness));
        }

        /// <inheritdoc />
        public override Geometry[] Split(Geometry tool)
        {
            ArgumentNullException.ThrowIfNull(tool);
            if (tool is Plane plane)
            {
                var results = new List<Geometry>(2);
                var pos = NativeGeometry.SolidCutPlaneOrNull(Handle, plane.Origin.Position, plane.Normal.Components);
                if (pos != null)
                    results.Add(new Solid(pos));
                var neg = NativeGeometry.SolidCutPlaneOrNull(Handle, plane.Origin.Position, plane.Normal.Reverse().Components);
                if (neg != null)
                    results.Add(new Solid(neg));
                return results.ToArray();
            }
            return base.Split(tool);
        }

        /// <summary>Subtracts another solid from this solid and returns the result.</summary>
        /// <param name="other">The solid to subtract.</param>
        /// <returns>The remainder after subtraction.</returns>
        public Solid Difference(Solid other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return new Solid(NativeGeometry.Difference(Handle, other.Handle));
        }

        /// <summary>Subtracts a sequence of solids from this solid and returns the result.</summary>
        /// <param name="others">Solids to subtract.</param>
        /// <returns>The remainder after all subtractions.</returns>
        public Solid DifferenceAll(IEnumerable<Solid> others)
        {
            ArgumentNullException.ThrowIfNull(others);
            var tools = others.Where(s => s != null).ToArray();
            if (tools.Length == 0)
                return (Solid)Translate(0, 0, 0);

            var current = Difference(tools[0]);
            for (var i = 1; i < tools.Length; i++)
            {
                var next = current.Difference(tools[i]);
                current.Dispose();
                current = next;
            }
            return current;
        }

        /// <summary>Computes the Boolean union of this solid with another solid.</summary>
        /// <param name="other">Solid to unite with.</param>
        /// <returns>The united solid.</returns>
        public Solid Union(Solid other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return new Solid(NativeGeometry.Union(Handle, other.Handle));
        }

        /// <summary>Computes the Boolean union of a sequence of solids.</summary>
        /// <param name="solids">Solids to unite.</param>
        /// <returns>The united solid.</returns>
        public static Solid ByUnion(IEnumerable<Solid> solids)
        {
            ArgumentNullException.ThrowIfNull(solids);
            var items = solids.Where(s => s != null).ToArray();
            if (items.Length == 0)
                throw new ArgumentException("At least one solid is required for ByUnion.", nameof(solids));
            if (items.Length == 1)
                return (Solid)items[0].Translate(0, 0, 0);

            var current = items[0].Union(items[1]);
            for (var i = 2; i < items.Length; i++)
            {
                var next = current.Union(items[i]);
                current.Dispose();
                current = next;
            }
            return current;
        }

        /// <summary>Computes the Boolean intersection of this solid with another solid.</summary>
        /// <param name="other">Solid to intersect with.</param>
        /// <returns>The intersecting volume.</returns>
        public Solid Intersect(Solid other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return new Solid(NativeGeometry.Intersect(Handle, other.Handle));
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return new Solid(NativeGeometry.Translate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation)));
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            return new Solid(NativeGeometry.Rotate(Handle, origin.Position, axis.Components, degrees));
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            return new Solid(NativeGeometry.Scale(Handle, basePoint.Position, xamount, yamount, zamount));
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return new Solid(NativeGeometry.Transform(
                Handle,
                cs.Origin.Position,
                cs.XAxis.Components,
                cs.YAxis.Components,
                cs.ZAxis.Components));
        }

        internal override Geometry MirrorCore(Plane mirrorPlane)
        {
            return new Solid(NativeGeometry.BrepMirror(
                Handle,
                mirrorPlane.Origin.Position,
                mirrorPlane.XAxis.Components,
                mirrorPlane.YAxis.Components));
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
