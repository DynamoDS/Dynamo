using System;
using System.Linq;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>Base class for topological boundary-representation geometry (solids, surfaces, and polysurfaces).</summary>
    public abstract class Topology : Geometry
    {
        internal NativeGeometry.SolidHandle Handle { get; }

        internal Topology(NativeGeometry.SolidHandle handle)
        {
            Handle = handle;
        }

        /// <summary>Gets all topological faces in this body.</summary>
        public Face[] Faces
        {
            get
            {
                var (faceCount, _, _) = NativeGeometry.TopologyCounts(Handle);
                var result = new Face[faceCount];
                for (uint i = 0; i < faceCount; i++)
                    result[i] = new Face(this, i);
                return result;
            }
        }

        /// <summary>Gets all topological edges in this body.</summary>
        public Edge[] Edges
        {
            get
            {
                var (_, edgeCount, _) = NativeGeometry.TopologyCounts(Handle);
                var result = new Edge[edgeCount];
                for (uint i = 0; i < edgeCount; i++)
                    result[i] = new Edge(this, i);
                return result;
            }
        }

        /// <summary>Gets all topological vertices in this body.</summary>
        public Vertex[] Vertices
        {
            get
            {
                var (_, _, vertexCount) = NativeGeometry.TopologyCounts(Handle);
                var result = new Vertex[vertexCount];
                for (uint i = 0; i < vertexCount; i++)
                    result[i] = new Vertex(this, i);
                return result;
            }
        }

        /// <inheritdoc />
        public override BoundingBox BoundingBox
        {
            get
            {
                var (min, max) = NativeGeometry.BoundingBox(Handle);
                return BoundingBox.ByCorners(new Point(min), new Point(max));
            }
        }

        internal override void ReleaseResources()
        {
            Handle.Dispose();
        }
    }

    /// <summary>A topological face belonging to a <see cref="Topology"/> body.</summary>
    public sealed class Face : IDisposable
    {
        private readonly Topology owner;
        internal uint Index { get; }

        internal Face(Topology owner, uint index)
        {
            this.owner = owner;
            Index = index;
        }

        /// <summary>Gets the surface area of this face within its trim boundaries.</summary>
        public double Area => NativeGeometry.FaceInfo(owner.Handle, Index).Area;

        /// <summary>Gets the axis-aligned bounding box of this face.</summary>
        public BoundingBox BoundingBox
        {
            get
            {
                var (_, min, max) = NativeGeometry.FaceInfo(owner.Handle, Index);
                return BoundingBox.ByCorners(new Point(min), new Point(max));
            }
        }

        /// <summary>Gets the edges bounding this face.</summary>
        public Edge[] Edges =>
            NativeGeometry.FaceEdges(owner.Handle, Index).Select(i => new Edge(owner, i)).ToArray();

        /// <summary>Gets the vertices on this face's boundary.</summary>
        public Vertex[] Vertices =>
            NativeGeometry.FaceVertices(owner.Handle, Index).Select(i => new Vertex(owner, i)).ToArray();

        /// <summary>Extracts a standalone trimmed <see cref="Surface"/> copy of this face.</summary>
        public Surface SurfaceGeometry() => new(NativeGeometry.FaceToSurface(owner.Handle, Index));

        /// <inheritdoc />
        public void Dispose()
        {
            // Topology handles are borrowed views owned by the parent Topology body.
        }
    }

    /// <summary>A topological edge belonging to a <see cref="Topology"/> body.</summary>
    public sealed class Edge : IDisposable
    {
        private readonly Topology owner;
        internal uint Index { get; }

        internal Edge(Topology owner, uint index)
        {
            this.owner = owner;
            Index = index;
        }

        /// <summary>Gets a standalone <see cref="Curve"/> copy of this edge's 3D curve.</summary>
        public Curve CurveGeometry => new(NativeGeometry.EdgeCurve(owner.Handle, Index));

        /// <summary>Gets the vertex at the start of this edge.</summary>
        public Vertex StartVertex
        {
            get
            {
                var (startIdx, _, _) = NativeGeometry.EdgeInfo(owner.Handle, Index);
                return startIdx == uint.MaxValue ? null : new Vertex(owner, startIdx);
            }
        }

        /// <summary>Gets the vertex at the end of this edge.</summary>
        public Vertex EndVertex
        {
            get
            {
                var (_, endIdx, _) = NativeGeometry.EdgeInfo(owner.Handle, Index);
                return endIdx == uint.MaxValue ? null : new Vertex(owner, endIdx);
            }
        }

        /// <summary>Gets the faces adjacent to this edge.</summary>
        public Face[] AdjacentFaces =>
            NativeGeometry.EdgeFaces(owner.Handle, Index).Select(i => new Face(owner, i)).ToArray();

        /// <summary>Gets the 3D arc length of this edge.</summary>
        public double Length => NativeGeometry.EdgeInfo(owner.Handle, Index).Length;

        /// <inheritdoc />
        public void Dispose()
        {
            // Topology handles are borrowed views owned by the parent Topology body.
        }
    }

    /// <summary>A topological vertex belonging to a <see cref="Topology"/> body.</summary>
    public sealed class Vertex : IDisposable
    {
        private readonly Topology owner;
        internal uint Index { get; }

        internal Vertex(Topology owner, uint index)
        {
            this.owner = owner;
            Index = index;
        }

        /// <summary>Gets the 3D <see cref="Point"/> position of this vertex.</summary>
        public Point PointGeometry => new(NativeGeometry.VertexPoint(owner.Handle, Index));

        /// <summary>Gets the edges incident to this vertex.</summary>
        public Edge[] AdjacentEdges =>
            NativeGeometry.VertexEdges(owner.Handle, Index).Select(i => new Edge(owner, i)).ToArray();

        /// <summary>Gets the faces incident to this vertex.</summary>
        public Face[] AdjacentFaces =>
            NativeGeometry.VertexFaces(owner.Handle, Index).Select(i => new Face(owner, i)).ToArray();

        /// <inheritdoc />
        public void Dispose()
        {
            // Topology handles are borrowed views owned by the parent Topology body.
        }
    }
}
