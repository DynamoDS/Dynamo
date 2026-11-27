using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A stitched shell or collection of contiguous surfaces backed by an SMLib BRep.</summary>
    public class PolySurface : Topology
    {
        internal PolySurface(NativeGeometry.SolidHandle handle) : base(handle)
        {
        }

        /// <summary>Gets the number of surfaces (faces) in this polysurface.</summary>
        public int SurfaceCount => checked((int)NativeGeometry.TopologyCounts(Handle).Faces);

        /// <summary>Gets the number of edges in this polysurface.</summary>
        public int EdgeCount => checked((int)NativeGeometry.TopologyCounts(Handle).Edges);

        /// <summary>Gets the number of vertices in this polysurface.</summary>
        public int VertexCount => checked((int)NativeGeometry.TopologyCounts(Handle).Vertices);

        /// <summary>Gets the total surface area of all faces in this polysurface.</summary>
        public double Area => NativeGeometry.Area(Handle);

        /// <summary>Returns standalone <see cref="Surface"/> copies for each face in this polysurface.</summary>
        public Surface[] Surfaces() => Faces.Select(f => f.SurfaceGeometry()).ToArray();

        /// <summary>Creates a polysurface by joining a collection of surfaces.</summary>
        /// <param name="surfaces">Surfaces to join.</param>
        /// <returns>The joined polysurface.</returns>
        public static PolySurface ByJoinedSurfaces(IEnumerable<Surface> surfaces)
        {
            ArgumentNullException.ThrowIfNull(surfaces);
            var list = surfaces.Where(s => s != null).ToArray();
            if (list.Length == 0)
                throw new ArgumentException("At least one surface is required.", nameof(surfaces));
            return new PolySurface(NativeGeometry.JoinSurfaces(list.Select(s => s.Handle).ToArray(), makeSolid: false));
        }

        /// <summary>Creates a polysurface by extracting the boundary faces of a solid.</summary>
        /// <param name="solid">Solid to convert to a polysurface.</param>
        /// <returns>The polysurface representing the solid's boundary.</returns>
        public static PolySurface BySolid(Solid solid)
        {
            ArgumentNullException.ThrowIfNull(solid);
            return new PolySurface(NativeGeometry.Translate(solid.Handle, new NativePoint(0, 0, 0)));
        }

        /// <summary>Converts a closed polysurface into a manifold <see cref="Solid"/>.</summary>
        public Solid Solid()
        {
            var surfaces = Surfaces();
            try
            {
                return Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces(surfaces);
            }
            finally
            {
                foreach (var s in surfaces)
                    s.Dispose();
            }
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            return new PolySurface(NativeGeometry.Translate(Handle, new NativePoint(xTranslation, yTranslation, zTranslation)));
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            ArgumentNullException.ThrowIfNull(origin);
            ArgumentNullException.ThrowIfNull(axis);
            return new PolySurface(NativeGeometry.Rotate(Handle, origin.Position, axis.Components, degrees));
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            ArgumentNullException.ThrowIfNull(basePoint);
            return new PolySurface(NativeGeometry.Scale(Handle, basePoint.Position, xamount, yamount, zamount));
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            ArgumentNullException.ThrowIfNull(cs);
            return new PolySurface(NativeGeometry.Transform(
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
