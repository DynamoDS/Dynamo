using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using OpenGeometry;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>A polygon mesh composed of vertices and triangular or quadrilateral faces.</summary>
    public class Mesh : Geometry
    {
        private readonly Point[] vertices;
        private readonly IndexGroup[] faces;
        private readonly Vector[] normals;

        private Mesh(Point[] vertices, IndexGroup[] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
            normals = ComputeVertexNormals(vertices, faces);
        }

        /// <summary>Gets the vertex positions of this mesh.</summary>
        public Point[] VertexPositions => vertices.ToArray();

        /// <summary>Gets the unit vertex normals of this mesh.</summary>
        public Vector[] VertexNormals => normals.ToArray();

        /// <summary>Gets the face index groups of this mesh.</summary>
        public IndexGroup[] FaceIndices => faces.ToArray();

        /// <summary>Gets the number of vertices in this mesh.</summary>
        public int VertexCount => vertices.Length;

        /// <summary>Gets the number of triangles when all faces are triangulated.</summary>
        public int TriangleCount => faces.Sum(f => f.Count == 4 ? 2 : 1);

        /// <summary>Gets the number of unique undirected edges in this mesh.</summary>
        public int EdgeCount => CollectUniqueEdges().Count;

        /// <summary>Gets all vertex coordinates flattened as [x0, y0, z0, x1, y1, z1, ...].</summary>
        public double[] VerticesAsThreeNumbers
        {
            get
            {
                var result = new double[vertices.Length * 3];
                for (var i = 0; i < vertices.Length; i++)
                {
                    result[i * 3] = vertices[i].X;
                    result[i * 3 + 1] = vertices[i].Y;
                    result[i * 3 + 2] = vertices[i].Z;
                }
                return result;
            }
        }

        /// <summary>Gets all triangulated faces flattened as 9 coordinates per triangle.</summary>
        public double[] TrianglesAsNineNumbers
        {
            get
            {
                var tris = EnumerateTriangles().ToArray();
                var result = new double[tris.Length * 9];
                for (var i = 0; i < tris.Length; i++)
                {
                    var (a, b, c) = tris[i];
                    var pa = vertices[a];
                    var pb = vertices[b];
                    var pc = vertices[c];
                    var o = i * 9;
                    result[o] = pa.X; result[o + 1] = pa.Y; result[o + 2] = pa.Z;
                    result[o + 3] = pb.X; result[o + 4] = pb.Y; result[o + 5] = pb.Z;
                    result[o + 6] = pc.X; result[o + 7] = pc.Y; result[o + 8] = pc.Z;
                }
                return result;
            }
        }

        /// <summary>Gets all unique edges flattened as 6 coordinates [x0, y0, z0, x1, y1, z1] per edge.</summary>
        public double[] EdgesAsSixNumbers
        {
            get
            {
                var edges = CollectUniqueEdges();
                var result = new double[edges.Count * 6];
                var i = 0;
                foreach (var (u, v) in edges)
                {
                    var pu = vertices[u];
                    var pv = vertices[v];
                    var o = i * 6;
                    result[o] = pu.X; result[o + 1] = pu.Y; result[o + 2] = pu.Z;
                    result[o + 3] = pv.X; result[o + 4] = pv.Y; result[o + 5] = pv.Z;
                    i++;
                }
                return result;
            }
        }

        /// <summary>Gets the total surface area of all mesh faces.</summary>
        public double Area
        {
            get
            {
                var total = 0.0;
                foreach (var (a, b, c) in EnumerateTriangles())
                {
                    var ab = Vector.ByTwoPoints(vertices[a], vertices[b]);
                    var ac = Vector.ByTwoPoints(vertices[a], vertices[c]);
                    total += 0.5 * ab.Cross(ac).Length;
                }
                return total;
            }
        }

        /// <summary>Gets the enclosed volume of a closed, consistently oriented mesh via divergence summation.</summary>
        public double Volume
        {
            get
            {
                var sum = 0.0;
                foreach (var (a, b, c) in EnumerateTriangles())
                {
                    var va = vertices[a].AsVector();
                    var vb = vertices[b].AsVector();
                    var vc = vertices[c].AsVector();
                    sum += va.Dot(vb.Cross(vc)) / 6.0;
                }
                return Math.Abs(sum);
            }
        }

        /// <inheritdoc />
        public override BoundingBox BoundingBox => BoundingBox.ByGeometry(vertices);

        /// <summary>Returns each triangle of this mesh as a closed 3-vertex <see cref="Polygon"/>.</summary>
        public Polygon[] Triangles()
        {
            return EnumerateTriangles()
                .Select(t => Polygon.ByPoints(new[] { vertices[t.A], vertices[t.B], vertices[t.C] }))
                .ToArray();
        }

        /// <summary>Returns the unit normal vector for each triangle of this mesh.</summary>
        public Vector[] TriangleNormals()
        {
            var result = new List<Vector>();
            foreach (var (a, b, c) in EnumerateTriangles())
            {
                var ab = Vector.ByTwoPoints(vertices[a], vertices[b]);
                var ac = Vector.ByTwoPoints(vertices[a], vertices[c]);
                var cross = ab.Cross(ac);
                result.Add(cross.Length > 1e-12 ? cross.Normalized() : Vector.ZAxis());
            }
            return result.ToArray();
        }

        /// <summary>Returns the centroid point of each triangle of this mesh.</summary>
        public Point[] TriangleCentroids()
        {
            var result = new List<Point>();
            foreach (var (a, b, c) in EnumerateTriangles())
            {
                var pa = vertices[a];
                var pb = vertices[b];
                var pc = vertices[c];
                result.Add(Point.ByCoordinates(
                    (pa.X + pb.X + pc.X) / 3.0,
                    (pa.Y + pb.Y + pc.Y) / 3.0,
                    (pa.Z + pb.Z + pc.Z) / 3.0));
            }
            return result.ToArray();
        }

        /// <summary>Computes the Boolean union of this closed mesh with <paramref name="tool"/> via SMLib <c>SmPolyBrep</c>.</summary>
        /// <param name="tool">Second closed manifold mesh.</param>
        /// <returns>The united mesh.</returns>
        public Mesh BooleanUnion(Mesh tool) => RunPolyBoolean(tool, operation: 0);

        /// <summary>Computes the Boolean intersection of this closed mesh with <paramref name="tool"/> via SMLib <c>SmPolyBrep</c>.</summary>
        /// <param name="tool">Second closed manifold mesh.</param>
        /// <returns>The intersecting mesh.</returns>
        public Mesh BooleanIntersection(Mesh tool) => RunPolyBoolean(tool, operation: 1);

        /// <summary>Computes the Boolean difference (this minus <paramref name="tool"/>) via SMLib <c>SmPolyBrep</c>.</summary>
        /// <param name="tool">Mesh to subtract.</param>
        /// <returns>The difference mesh.</returns>
        public Mesh BooleanDifference(Mesh tool) => RunPolyBoolean(tool, operation: 2);

        /// <summary>Creates a mesh by tessellating a solid, surface, polysurface, or existing mesh.</summary>
        /// <param name="geometry">Geometry to convert into a mesh.</param>
        /// <param name="maxGridLines">Optional tessellation density parameter (reserved).</param>
        /// <param name="maxTessellation">Optional tessellation limit parameter (reserved).</param>
        /// <returns>The tessellated mesh with shared vertices.</returns>
        public static Mesh ByGeometry(Geometry geometry, double maxGridLines = 512, int maxTessellation = 512)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            _ = maxGridLines;
            _ = maxTessellation;

            if (geometry is Mesh existing)
                return new Mesh(existing.vertices.ToArray(), existing.faces.ToArray());

            if (geometry is Topology topo)
            {
                var rawVerts = NativeGeometry.Tessellate(topo.Handle, 0.0);
                return FromNativeTriangleVertices(rawVerts);
            }

            throw new NotSupportedException($"Mesh.ByGeometry is not supported for {geometry.GetType().Name}.");
        }

        /// <summary>Creates a mesh from vertex positions and face index groups.</summary>
        /// <param name="vertexPositions">Ordered vertex positions.</param>
        /// <param name="indices">Face index groups referencing <paramref name="vertexPositions"/>.</param>
        /// <returns>The created mesh.</returns>
        public static Mesh ByPointsFaceIndices(IEnumerable<Point> vertexPositions, IEnumerable<IndexGroup> indices)
        {
            ArgumentNullException.ThrowIfNull(vertexPositions);
            ArgumentNullException.ThrowIfNull(indices);

            var pts = vertexPositions.Where(p => p != null).ToArray();
            var idx = indices.ToArray();
            if (pts.Length < 3)
                throw new ArgumentException("A Mesh requires at least 3 vertices.", nameof(vertexPositions));
            if (idx.Length == 0)
                throw new ArgumentException("A Mesh requires at least 1 face.", nameof(indices));

            foreach (var g in idx)
            {
                if (g.Count != 3 && g.Count != 4)
                    throw new ArgumentException("Each IndexGroup must have 3 or 4 indices.", nameof(indices));
                if (g.A >= pts.Length || g.B >= pts.Length || g.C >= pts.Length || (g.Count == 4 && g.D >= pts.Length))
                    throw new ArgumentOutOfRangeException(nameof(indices), "Face vertex index exceeds vertex count.");
            }

            return new Mesh(pts, idx);
        }

        /// <summary>Creates a mesh from vertex positions and face index groups.</summary>
        public static Mesh ByPointsIndexGroups(IEnumerable<Point> vertexPositions, IEnumerable<IndexGroup> indices) =>
            ByPointsFaceIndices(vertexPositions, indices);

        /// <summary>Creates a triangulated mesh from vertex positions and flat triangle indices (length divisible by 3).</summary>
        /// <param name="points">Ordered vertex positions.</param>
        /// <param name="indices">Flat list of 0-based vertex indices (3 per triangle).</param>
        /// <returns>The created mesh.</returns>
        public static Mesh ByPointsIndices(IEnumerable<Point> points, IEnumerable<int> indices)
        {
            ArgumentNullException.ThrowIfNull(points);
            ArgumentNullException.ThrowIfNull(indices);
            var idxList = indices.ToArray();
            if (idxList.Length == 0 || idxList.Length % 3 != 0)
                throw new ArgumentException("Triangle index count must be a positive multiple of 3.", nameof(indices));

            var groups = new IndexGroup[idxList.Length / 3];
            for (var i = 0; i < groups.Length; i++)
            {
                var a = idxList[i * 3];
                var b = idxList[i * 3 + 1];
                var c = idxList[i * 3 + 2];
                if (a < 0 || b < 0 || c < 0)
                    throw new ArgumentOutOfRangeException(nameof(indices), "Vertex indices must be non-negative.");
                groups[i] = IndexGroup.ByIndices((uint)a, (uint)b, (uint)c);
            }
            return ByPointsFaceIndices(points, groups);
        }

        /// <summary>Creates a triangulated mesh from vertex positions and flat triangle indices.</summary>
        public static Mesh ByVerticesAndIndices(IEnumerable<Point> vertexPositions, IEnumerable<int> indices) =>
            ByPointsIndices(vertexPositions, indices);

        /// <summary>Creates a closed cuboid mesh centered at <paramref name="origin"/>.</summary>
        public static Mesh Cuboid(
            Point origin,
            double length = 1,
            double width = 1,
            double height = 1,
            int xDivisions = 1,
            int yDivisions = 1,
            int zDivisions = 1)
        {
            ArgumentNullException.ThrowIfNull(origin);
            _ = xDivisions;
            _ = yDivisions;
            _ = zDivisions;
            using var solid = Autodesk.DesignScript.Geometry.Cuboid.ByLengths(origin, length, width, height);
            return ByGeometry(solid);
        }

        /// <summary>Creates a closed sphere mesh centered at <paramref name="origin"/>.</summary>
        public static Mesh Sphere(Point origin, double radius = 1, int divisions = 2, bool icosphere = false)
        {
            ArgumentNullException.ThrowIfNull(origin);
            _ = divisions;
            _ = icosphere;
            using var solid = Autodesk.DesignScript.Geometry.Sphere.ByCenterPointRadius(origin, radius);
            return ByGeometry(solid);
        }

        /// <summary>Creates a cone or frustum mesh with base centered at <paramref name="origin"/> extending along +Z.</summary>
        public static Mesh Cone(
            Point origin,
            double baseRadius = 1,
            double topRadius = 0,
            double height = 1,
            int divisions = 32,
            bool cap = true)
        {
            ArgumentNullException.ThrowIfNull(origin);
            _ = divisions;
            _ = cap;
            var top = Point.ByCoordinates(origin.X, origin.Y, origin.Z + height);
            using var solid = Autodesk.DesignScript.Geometry.Cone.ByPointsRadii(origin, top, baseRadius, topRadius);
            return ByGeometry(solid);
        }

        /// <summary>Creates a planar subdivided quad mesh centered at <paramref name="origin"/> in the XY plane.</summary>
        public static Mesh Plane(
            Point origin, double xWidth = 1, double yWidth = 1, int xDivisions = 1, int yDivisions = 1)
        {
            ArgumentNullException.ThrowIfNull(origin);
            if (!double.IsFinite(xWidth) || xWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(xWidth));
            if (!double.IsFinite(yWidth) || yWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(yWidth));
            if (xDivisions < 1)
                throw new ArgumentOutOfRangeException(nameof(xDivisions));
            if (yDivisions < 1)
                throw new ArgumentOutOfRangeException(nameof(yDivisions));

            var nx = xDivisions + 1;
            var ny = yDivisions + 1;
            var pts = new Point[nx * ny];
            var x0 = origin.X - 0.5 * xWidth;
            var y0 = origin.Y - 0.5 * yWidth;
            for (var ix = 0; ix < nx; ix++)
            {
                var x = x0 + xWidth * ix / xDivisions;
                for (var iy = 0; iy < ny; iy++)
                {
                    var y = y0 + yWidth * iy / yDivisions;
                    pts[ix * ny + iy] = Point.ByCoordinates(x, y, origin.Z);
                }
            }

            var quads = new IndexGroup[xDivisions * yDivisions];
            var k = 0;
            for (var ix = 0; ix < xDivisions; ix++)
            {
                for (var iy = 0; iy < yDivisions; iy++)
                {
                    var i00 = (uint)(ix * ny + iy);
                    var i10 = (uint)((ix + 1) * ny + iy);
                    var i11 = (uint)((ix + 1) * ny + (iy + 1));
                    var i01 = (uint)(ix * ny + (iy + 1));
                    quads[k++] = IndexGroup.ByIndices(i00, i10, i11, i01);
                }
            }
            return new Mesh(pts, quads);
        }

        /// <inheritdoc />
        public override Geometry Translate(double xTranslation = 0, double yTranslation = 0, double zTranslation = 0)
        {
            var moved = vertices.Select(v => (Point)v.Translate(xTranslation, yTranslation, zTranslation)).ToArray();
            return new Mesh(moved, faces.ToArray());
        }

        /// <inheritdoc />
        public override Geometry Rotate(Point origin, Vector axis, double degrees)
        {
            var moved = vertices.Select(v => (Point)v.Rotate(origin, axis, degrees)).ToArray();
            return new Mesh(moved, faces.ToArray());
        }

        /// <inheritdoc />
        public override Geometry Scale(Point basePoint, double xamount, double yamount, double zamount)
        {
            var moved = vertices.Select(v => (Point)v.Scale(basePoint, xamount, yamount, zamount)).ToArray();
            return new Mesh(moved, faces.ToArray());
        }

        /// <inheritdoc />
        public override Geometry Transform(CoordinateSystem cs)
        {
            var moved = vertices.Select(v => (Point)v.Transform(cs)).ToArray();
            return new Mesh(moved, faces.ToArray());
        }

        internal override void WritePreview(IRenderPackage package, TessellationParameters parameters)
        {
            foreach (var (a, b, c) in EnumerateTriangles())
            {
                EmitVertex(package, a);
                EmitVertex(package, b);
                EmitVertex(package, c);
            }
        }

        private Mesh RunPolyBoolean(Mesh tool, int operation)
        {
            ArgumentNullException.ThrowIfNull(tool);
            var (vertsA, trisA) = ExportTriangles();
            var (vertsB, trisB) = tool.ExportTriangles();
            var rawResult = NativeGeometry.PolyBoolean(vertsA, trisA, vertsB, trisB, operation);
            return FromNativeTriangleVertices(rawResult);
        }

        private (NativePoint[] Vertices, uint[] Triangles) ExportTriangles()
        {
            var verts = new NativePoint[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
                verts[i] = vertices[i].Position;

            var tris = new List<uint>(TriangleCount * 3);
            foreach (var (a, b, c) in EnumerateTriangles())
            {
                tris.Add((uint)a);
                tris.Add((uint)b);
                tris.Add((uint)c);
            }
            return (verts, tris.ToArray());
        }

        private static Mesh FromNativeTriangleVertices(NativeVertex[] rawVerts)
        {
            if (rawVerts.Length < 3 || rawVerts.Length % 3 != 0)
                throw new InvalidOperationException("Native tessellation produced an invalid triangle stream.");

            var uniqueMap = new Dictionary<(long X, long Y, long Z), uint>();
            var uniquePts = new List<Point>();
            var faceList = new List<IndexGroup>(rawVerts.Length / 3);

            uint GetOrAddVertex(NativePoint p)
            {
                var key = (
                    (long)Math.Round(p.X * 1e6),
                    (long)Math.Round(p.Y * 1e6),
                    (long)Math.Round(p.Z * 1e6));
                if (uniqueMap.TryGetValue(key, out var existing))
                    return existing;
                var idx = (uint)uniquePts.Count;
                uniqueMap[key] = idx;
                uniquePts.Add(new Point(p));
                return idx;
            }

            for (var i = 0; i < rawVerts.Length; i += 3)
            {
                var a = GetOrAddVertex(rawVerts[i].Position);
                var b = GetOrAddVertex(rawVerts[i + 1].Position);
                var c = GetOrAddVertex(rawVerts[i + 2].Position);
                if (a != b && b != c && a != c)
                    faceList.Add(IndexGroup.ByIndices(a, b, c));
            }

            return new Mesh(uniquePts.ToArray(), faceList.ToArray());
        }

        private HashSet<(int U, int V)> CollectUniqueEdges()
        {
            var set = new HashSet<(int, int)>();
            void AddEdge(int i, int j) => set.Add(i < j ? (i, j) : (j, i));

            foreach (var f in faces)
            {
                if (f.Count == 3)
                {
                    AddEdge((int)f.A, (int)f.B);
                    AddEdge((int)f.B, (int)f.C);
                    AddEdge((int)f.C, (int)f.A);
                }
                else
                {
                    AddEdge((int)f.A, (int)f.B);
                    AddEdge((int)f.B, (int)f.C);
                    AddEdge((int)f.C, (int)f.D);
                    AddEdge((int)f.D, (int)f.A);
                }
            }
            return set;
        }

        private void EmitVertex(IRenderPackage package, int index)
        {
            var p = vertices[index];
            var n = normals[index];
            package.AddTriangleVertex(p.X, p.Y, p.Z);
            package.AddTriangleVertexNormal(n.X, n.Y, n.Z);
            package.AddTriangleVertexUV(0, 0);
        }

        private IEnumerable<(int A, int B, int C)> EnumerateTriangles()
        {
            foreach (var f in faces)
            {
                yield return ((int)f.A, (int)f.B, (int)f.C);
                if (f.Count == 4)
                    yield return ((int)f.A, (int)f.C, (int)f.D);
            }
        }

        private static Vector[] ComputeVertexNormals(Point[] verts, IndexGroup[] faceList)
        {
            var nx = new double[verts.Length];
            var ny = new double[verts.Length];
            var nz = new double[verts.Length];

            foreach (var f in faceList)
            {
                AccumulateTriangleNormal(verts, nx, ny, nz, (int)f.A, (int)f.B, (int)f.C);
                if (f.Count == 4)
                    AccumulateTriangleNormal(verts, nx, ny, nz, (int)f.A, (int)f.C, (int)f.D);
            }

            var result = new Vector[verts.Length];
            for (var i = 0; i < verts.Length; i++)
            {
                var len = Math.Sqrt(nx[i] * nx[i] + ny[i] * ny[i] + nz[i] * nz[i]);
                result[i] = len > 1e-12
                    ? Vector.ByCoordinates(nx[i] / len, ny[i] / len, nz[i] / len)
                    : Vector.ZAxis();
            }
            return result;
        }

        private static void AccumulateTriangleNormal(
            Point[] verts, double[] nx, double[] ny, double[] nz, int a, int b, int c)
        {
            var ab = Vector.ByTwoPoints(verts[a], verts[b]);
            var ac = Vector.ByTwoPoints(verts[a], verts[c]);
            var cross = ab.Cross(ac);
            nx[a] += cross.X; ny[a] += cross.Y; nz[a] += cross.Z;
            nx[b] += cross.X; ny[b] += cross.Y; nz[b] += cross.Z;
            nx[c] += cross.X; ny[c] += cross.Y; nz[c] += cross.Z;
        }
    }
}
