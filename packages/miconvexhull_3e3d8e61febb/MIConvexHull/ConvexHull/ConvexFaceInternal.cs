namespace MIConvexHull
{
    using System.Collections.Generic;

    /// <summary>
    /// Wraps each IVertex to allow marking of nodes.
    /// </summary>
    sealed class VertexWrap
    {
        /// <summary>
        /// Ref. to the original vertex.
        /// </summary>
        public IVertex Vertex;

        /// <summary>
        /// Direct reference to PositionData makes IsVertexOverFace faster.
        /// </summary>
        public double[] PositionData;

        /// <summary>
        /// Vertex index.
        /// </summary>
        public int Index;

        /// <summary>
        /// Used mostly to enumerate unique vertices.
        /// </summary>
        public bool Marked;
    }

    /// <summary>
    /// Compare vertices based on their indices.
    /// </summary>
    class VertexWrapComparer : IComparer<VertexWrap>
    {
        public static readonly VertexWrapComparer Instance = new VertexWrapComparer();

        public int Compare(VertexWrap x, VertexWrap y)
        {
            return x.Index.CompareTo(y.Index);
        }
    }

    /// <summary>
    /// A helper class used to connect faces.
    /// </summary>
    sealed class FaceConnector
    {
        /// <summary>
        /// The face.
        /// </summary>
        public ConvexFaceInternal Face;

        /// <summary>
        /// The edge to be connected.
        /// </summary>
        public int EdgeIndex;

        /// <summary>
        /// The vertex indices.
        /// </summary>
        public int[] Vertices;

        /// <summary>
        /// The hash code computed from indices.
        /// </summary>
        public uint HashCode;

        /// <summary>
        /// Prev node in the list.
        /// </summary>
        public FaceConnector Previous;

        /// <summary>
        /// Next node in the list.
        /// </summary>
        public FaceConnector Next;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="dimension"></param>
        public FaceConnector(int dimension)
        {
            Vertices = new int[dimension - 1];
        }

        /// <summary>
        /// Updates the connector.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="edgeIndex"></param>
        /// <param name="dim"></param>
        public void Update(ConvexFaceInternal face, int edgeIndex, int dim)
        {
            this.Face = face;
            this.EdgeIndex = edgeIndex;

            uint hashCode = 31;

            var vs = face.Vertices;
            for (int i = 0, c = 0; i < dim; i++)
            {
                if (i != edgeIndex)
                {
                    var v = vs[i].Index;
                    this.Vertices[c++] = v;
                    hashCode += unchecked(23 * hashCode + (uint)v);
                }
            }

            this.HashCode = hashCode;
        }

        /// <summary>
        /// Can two faces be connected.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public static bool AreConnectable(FaceConnector a, FaceConnector b, int dim)
        {
            if (a.HashCode != b.HashCode) return false;

            var n = dim - 1;
            var av = a.Vertices;
            var bv = b.Vertices;
            for (int i = 0; i < n; i++)
            {
                if (av[i] != bv[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Connect two faces.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Connect(FaceConnector a, FaceConnector b)
        {
            a.Face.AdjacentFaces[a.EdgeIndex] = b.Face;
            b.Face.AdjacentFaces[b.EdgeIndex] = a.Face;
        }
    }

    /// <summary>
    /// This internal class manages the faces of the convex hull. It is a 
    /// separate class from the desired user class.
    /// </summary>
    sealed class ConvexFaceInternal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvexFaceInternal"/> class.
        /// </summary>
        public ConvexFaceInternal(int dimension, VertexBuffer beyondList)
        {
            AdjacentFaces = new ConvexFaceInternal[dimension];
            VerticesBeyond = beyondList;
            Normal = new double[dimension];
            Vertices = new VertexWrap[dimension];
        }

        /// <summary>
        /// Gets or sets the adjacent face data.
        /// </summary>
        public ConvexFaceInternal[] AdjacentFaces;

        /// <summary>
        /// Gets or sets the vertices beyond.
        /// </summary>
        public VertexBuffer VerticesBeyond;

        /// <summary>
        /// The furthest vertex.
        /// </summary>
        public VertexWrap FurthestVertex;

        /////// <summary>
        /////// Distance to the furthest vertex.
        /////// </summary>
        ////public double FurthestDistance;

        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        public VertexWrap[] Vertices;
        
        /// <summary>
        /// Gets or sets the normal vector.
        /// </summary>
        public double[] Normal;

        /// <summary>
        /// Is the normal flipped?
        /// </summary>
        public bool IsNormalFlipped;

        /// <summary>
        /// Face plane constant element.
        /// </summary>
        public double Offset;

        /// <summary>
        /// Used to traverse affected faces and create the Delaunay representation.
        /// </summary>
        public int Tag;

        /// <summary>
        /// Prev node in the list.
        /// </summary>
        public ConvexFaceInternal Previous;

        /// <summary>
        /// Next node in the list.
        /// </summary>
        public ConvexFaceInternal Next;

        /// <summary>
        /// Is it present in the list.
        /// </summary>
        public bool InList;
    }
}
