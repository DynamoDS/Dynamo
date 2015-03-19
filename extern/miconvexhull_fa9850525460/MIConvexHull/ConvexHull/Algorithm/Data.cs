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

    /*
     * This part of the implementation defines the data used by the algorithm.
     */
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// Corresponds to the dimension of the data.
        /// 
        /// When the "lifted" hull is computed, Dimension is automatically incremented by one.
        /// </summary>
        internal readonly int Dimension;

        /// <summary>
        /// Are we on a paraboloid?
        /// </summary>
        readonly bool IsLifted;

        /// <summary>
        /// Explained in ConvexHullComputationConfig.
        /// </summary>
        readonly double PlaneDistanceTolerance;

        /*
         * Representation of the input vertices.
         * 
         * - In the algorithm, a vertex is represented by its index in the Vertices array.
         *   This makes the algorithm a lot faster (up to 30%) than using object reference everywhere.
         * - Positions are stored as a single array of values. Coordinates for vertex with index i
         *   are stored at indices <i * Dimension, (i + 1) * Dimension)
         * - VertexMarks are used by the algorithm to help identify a set of vertices that is "above" (or "beyond") 
         *   a specific face.
         * - VertexAdded helps with handling of degenerate data to prevent duplicate addition of the same vertex to the hull.
         */
        IVertex[] Vertices;
        double[] Positions;
        bool[] VertexMarks;
        bool[] VertexAdded;

        /*
         * The triangulation faces are represented in a single pool for objects that are being reused.
         * This allows for represent the faces as integers and significantly speeds up many computations.
         * - AffectedFaceFlags are used to mark affected faces/
         */
        internal ConvexFaceInternal[] FacePool;
        internal bool[] AffectedFaceFlags;

        /// <summary>
        /// A list of vertices that form the convex hull.
        /// </summary>
        IndexBuffer ConvexHull;

        /// <summary>
        /// A list of faces that that are not a part of the final convex hull and still need to be processed.
        /// </summary>
        FaceList UnprocessedFaces;

        /// <summary>
        /// A list of faces that form the convex hull.
        /// </summary>
        IndexBuffer ConvexFaces;

        /// <summary>
        /// The vertex that is currently being processed.
        /// </summary>
        int CurrentVertex;

        /// <summary>
        /// A helper variable to determine the furthest vertex for a particular convex face.
        /// </summary>
        double MaxDistance;

        /// <summary>
        /// A helper variable to help determine the index of the vertex that is furthest from the face that is currently being processed.
        /// </summary>
        int FurthestVertex;

        /// <summary>
        /// The centroid of the currently computed hull.
        /// </summary>
        double[] Center;

        /*
         * Helper arrays to store faces for adjacency update.
         * This is just to prevent unnecessary allocations.
         */
        int[] UpdateBuffer;
        int[] UpdateIndices;

        /// <summary>
        /// Used to determine which faces need to be updated at each step of the algorithm.
        /// </summary>
        IndexBuffer TraverseStack;

        /// <summary>
        /// Used for VerticesBeyond for faces that are on the convex hull.
        /// </summary>
        IndexBuffer EmptyBuffer;

        /// <summary>
        /// Used to determine which vertices are "above" (or "beyond") a face
        /// </summary>
        IndexBuffer BeyondBuffer;

        /// <summary>
        /// Stores faces that are visible from the current vertex.
        /// </summary>
        IndexBuffer AffectedFaceBuffer;

        /// <summary>
        /// Stores faces that form a "cone" created by adding new vertex.
        /// </summary>
        SimpleList<DeferredFace> ConeFaceBuffer;

        /// <summary>
        /// Stores a list of "singular" (or "generate", "planar", etc.) vertices that cannot be part of the hull.
        /// </summary>
        HashSet<int> SingularVertices;

        /// <summary>
        /// The connector table helps to determine the adjacency of convex faces.
        /// Hashing is used instead of pairwise comparison. This significantly speeds up the computations,
        /// especially for higher dimensions.
        /// </summary>
        ConnectorList[] ConnectorTable;
        const int ConnectorTableSize = 2017;

        /// <summary>
        /// Manages the memory allocations and storage of unused objects.
        /// Saves the garbage collector a lot of work.
        /// </summary>
        ObjectManager ObjectManager;

        /// <summary>
        /// Helper class for handling math related stuff.
        /// </summary>
        MathHelper MathHelper;

        /// <summary>
        /// Initialize buffers and lists.
        /// </summary>
        /// <param name="config"></param>
        void InitializeData(ConvexHullComputationConfig config)
        {
            ConvexHull = new IndexBuffer();
            UnprocessedFaces = new FaceList();
            ConvexFaces = new IndexBuffer();
            
            FacePool = new ConvexFaceInternal[(Dimension + 1) * 10]; // must be initialized before object manager
            AffectedFaceFlags = new bool[(Dimension + 1) * 10];
            ObjectManager = new MIConvexHull.ObjectManager(this);

            Center = new double[Dimension];
            TraverseStack = new IndexBuffer();
            UpdateBuffer = new int[Dimension];
            UpdateIndices = new int[Dimension];
            EmptyBuffer = new IndexBuffer();
            AffectedFaceBuffer = new IndexBuffer();
            ConeFaceBuffer = new SimpleList<DeferredFace>();
            SingularVertices = new HashSet<int>();
            BeyondBuffer = new IndexBuffer();

            ConnectorTable = new ConnectorList[ConnectorTableSize];
            for (int i = 0; i < ConnectorTableSize; i++) ConnectorTable[i] = new ConnectorList();
            
            VertexMarks = new bool[Vertices.Length];
            VertexAdded = new bool[Vertices.Length];
            InitializePositions(config);

            MathHelper = new MIConvexHull.MathHelper(Dimension, Positions);
        }

        /// <summary>
        /// Initialize the vertex positions based on the translation type from config.
        /// </summary>
        /// <param name="config"></param>
        void InitializePositions(ConvexHullComputationConfig config)
        {
            Positions = new double[Vertices.Length * Dimension];
            int index = 0;
            if (IsLifted)
            {
                var origDim = Dimension - 1;
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType)
                {
                    case PointTranslationType.None:
                        foreach (var v in Vertices)
                        {
                            double lifted = 0.0;
                            for (int i = 0; i < origDim; i++)
                            {
                                var t = v.Position[i];
                                Positions[index++] = t;
                                lifted += t * t;
                            }
                            Positions[index++] = lifted;
                        }
                        break;
                    case PointTranslationType.TranslateInternal:
                        foreach (var v in Vertices)
                        {
                            double lifted = 0.0;
                            for (int i = 0; i < origDim; i++)
                            {
                                var t = v.Position[i] + tf();
                                Positions[index++] = t;
                                lifted += t * t;
                            }
                            Positions[index++] = lifted;
                        }
                        break;
                }
            }
            else
            {
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType)
                {
                    case PointTranslationType.None:
                        foreach (var v in Vertices)
                        {
                            for (int i = 0; i < Dimension; i++) Positions[index++] = v.Position[i];
                        }
                        break;
                    case PointTranslationType.TranslateInternal:
                        foreach (var v in Vertices)
                        {
                            for (int i = 0; i < Dimension; i++) Positions[index++] = v.Position[i] + tf();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Get a vertex coordinate. Only used in the initialize functions,
        /// in other places it part v * Dimension + i is inlined.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        double GetCoordinate(int v, int i)
        {
            return Positions[v * Dimension + i];
        }

        /// <summary>
        /// Check if the vertex was already added and if not, add it.
        /// </summary>
        /// <param name="i"></param>
        void AddConvexVertex(int i)
        {
            if (!VertexAdded[i])
            {
                ConvexHull.Add(i);
                VertexAdded[i] = true;
            }
        }
    }
}
