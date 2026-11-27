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
