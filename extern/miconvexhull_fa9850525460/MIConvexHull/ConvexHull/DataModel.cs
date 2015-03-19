/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2014 David Sehnal, Matthew Campbell
 *
 *  This library is free software; you can redistribute it and/or modify it 
 *  under the terms of  the GNU Lesser General Public License as published by 
 *  the Free Software Foundation; either version 2.1 of the License, or 
 *  (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful, 
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of 
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser 
 *  General Public License for more details.
 *  
 *****************************************************************************/

namespace MIConvexHull
{
    using System.Collections.Generic;
    
    /// <summary>
    /// For deferred face addition.
    /// </summary>
    sealed class DeferredFace
    {
        /// <summary>
        /// The faces.
        /// </summary>
        public ConvexFaceInternal Face, Pivot, OldFace;

        /// <summary>
        /// The indices.
        /// </summary>
        public int FaceIndex, PivotIndex;
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

            uint hashCode = 23;

            unchecked
            {
                var vs = face.Vertices;
                int i, c = 0;
                for (i = 0; i < edgeIndex; i++)
                {
                    this.Vertices[c++] = vs[i];
                    hashCode += 31 * hashCode + (uint)vs[i];
                }
                for (i = edgeIndex + 1; i < vs.Length; i++)
                {
                    this.Vertices[c++] = vs[i];
                    hashCode += 31 * hashCode + (uint)vs[i];
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
            for (int i = 0; i < av.Length; i++)
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
            a.Face.AdjacentFaces[a.EdgeIndex] = b.Face.Index;
            b.Face.AdjacentFaces[b.EdgeIndex] = a.Face.Index;
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
        public ConvexFaceInternal(int dimension, int index, IndexBuffer beyondList)
        {
            Index = index;
            AdjacentFaces = new int[dimension];
            VerticesBeyond = beyondList;
            Normal = new double[dimension];
            Vertices = new int[dimension];
        }

        /// <summary>
        /// Index of the face inside the pool.
        /// </summary>
        public int Index;

        /// <summary>
        /// Gets or sets the adjacent face data.
        /// </summary>
        public int[] AdjacentFaces;

        /// <summary>
        /// Gets or sets the vertices beyond.
        /// </summary>
        public IndexBuffer VerticesBeyond;

        /// <summary>
        /// The furthest vertex.
        /// </summary>
        public int FurthestVertex;
                
        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        public int[] Vertices;
        
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

        //public int UnprocessedIndex;

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
