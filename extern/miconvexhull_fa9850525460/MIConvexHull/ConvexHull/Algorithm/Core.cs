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
    /*
     * Main part of the algorithm 
     * Basic idea:
     * - Create the initial hull (done in Initialize.cs)
     * 
     *   For each face there are "vertices beyond" which are "visible" from it. 
     *   If there are no such vertices, the face is on the hull.
     *   
     * - While there is at least one face with at least one "vertex beyond":
     *   * Pick the furthest beyond vertex
     *   * For this vertex:
     *     > find all faces that are visible from it (TagAffectedFaces)
     *     > remove them and replace them with a "cone" created by the vertex and the boundary
     *       of the affected faces, and for each new face, compute "beyond vertices" 
     *       (CreateCone + CommitCone)
     * 
     * + Implement it in way that is fast, but hard to understand and maintain.
     */
    internal partial class ConvexHullInternal
    {                
        /// <summary>
        /// Tags all faces seen from the current vertex with 1.
        /// </summary>
        /// <param name="currentFace"></param>
        void TagAffectedFaces(ConvexFaceInternal currentFace)
        {
            AffectedFaceBuffer.Clear();
            AffectedFaceBuffer.Add(currentFace.Index);
            TraverseAffectedFaces(currentFace.Index);
        }
        
        /// <summary>
        /// Recursively traverse all the relevant faces.
        /// </summary>
        void TraverseAffectedFaces(int currentFace)
        {
            TraverseStack.Clear();
            TraverseStack.Push(currentFace);
            AffectedFaceFlags[currentFace] = true;

            while (TraverseStack.Count > 0)
            {
                var top = FacePool[TraverseStack.Pop()];
                for (int i = 0; i < Dimension; i++)
                {
                    var adjFace = top.AdjacentFaces[i];

                    if (!AffectedFaceFlags[adjFace] && MathHelper.GetVertexDistance(CurrentVertex, FacePool[adjFace]) >= PlaneDistanceTolerance)
                    {
                        AffectedFaceBuffer.Add(adjFace);
                        AffectedFaceFlags[adjFace] = true;
                        TraverseStack.Push(adjFace);
                    }
                }
            }
        }
                
        /// <summary>
        /// Creates a new deferred face.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="faceIndex"></param>
        /// <param name="pivot"></param>
        /// <param name="pivotIndex"></param>
        /// <param name="oldFace"></param>
        /// <returns></returns>
        DeferredFace MakeDeferredFace(ConvexFaceInternal face, int faceIndex, ConvexFaceInternal pivot, int pivotIndex, ConvexFaceInternal oldFace)
        {
            var ret = ObjectManager.GetDeferredFace();
            
            ret.Face = face;
            ret.FaceIndex = faceIndex;
            ret.Pivot = pivot;
            ret.PivotIndex = pivotIndex;
            ret.OldFace = oldFace;

            return ret;
        }

        /// <summary>
        /// Connect faces using a connector.
        /// </summary>
        /// <param name="connector"></param>
        void ConnectFace(FaceConnector connector)
        {
            var index = connector.HashCode % ConnectorTableSize;
            var list = ConnectorTable[index];

            for (var current = list.First; current != null; current = current.Next)
            {
                if (FaceConnector.AreConnectable(connector, current, Dimension))
                {
                    list.Remove(current);
                    FaceConnector.Connect(current, connector);
                    current.Face = null;
                    connector.Face = null;
                    ObjectManager.DepositConnector(current);
                    ObjectManager.DepositConnector(connector);
                    return;
                }
            }

            list.Add(connector);
        }

        /// <summary>
        /// Removes the faces "covered" by the current vertex and adds the newly created ones.
        /// </summary>
        private bool CreateCone()
        {
            var currentVertexIndex = CurrentVertex;
            ConeFaceBuffer.Clear();

            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var oldFaceIndex = AffectedFaceBuffer[fIndex];
                var oldFace = FacePool[oldFaceIndex];

                // Find the faces that need to be updated
                int updateCount = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    var af = oldFace.AdjacentFaces[i];
                    if (!AffectedFaceFlags[af]) // Tag == false when oldFaces does not contain af
                    {
                        UpdateBuffer[updateCount] = af;
                        UpdateIndices[updateCount] = i;
                        ++updateCount;
                    }
                }

                for (int i = 0; i < updateCount; i++)
                {
                    var adjacentFace = FacePool[UpdateBuffer[i]];

                    int oldFaceAdjacentIndex = 0;
                    var adjFaceAdjacency = adjacentFace.AdjacentFaces;
                    for (int j = 0; j < adjFaceAdjacency.Length; j++)
                    {
                        if (oldFaceIndex == adjFaceAdjacency[j])
                        {
                            oldFaceAdjacentIndex = j;
                            break;
                        }
                    }

                    var forbidden = UpdateIndices[i]; // Index of the face that corresponds to this adjacent face

                    ConvexFaceInternal newFace;

                    int oldVertexIndex;
                    int[] vertices;

                    var newFaceIndex = ObjectManager.GetFace();
                    newFace = FacePool[newFaceIndex];
                    vertices = newFace.Vertices;
                    for (int j = 0; j < Dimension; j++) vertices[j] = oldFace.Vertices[j];
                    oldVertexIndex = vertices[forbidden];

                    int orderedPivotIndex;

                    // correct the ordering
                    if (currentVertexIndex < oldVertexIndex)
                    {
                        orderedPivotIndex = 0;
                        for (int j = forbidden - 1; j >= 0; j--)
                        {
                            if (vertices[j] > currentVertexIndex) vertices[j + 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        orderedPivotIndex = Dimension - 1;
                        for (int j = forbidden + 1; j < Dimension; j++)
                        {
                            if (vertices[j] < currentVertexIndex) vertices[j - 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j - 1;
                                break;
                            }
                        }
                    }
                    
                    vertices[orderedPivotIndex] = CurrentVertex;

                    if (!MathHelper.CalculateFacePlane(newFace, Center))
                    {
                        return false;
                    }

                    ConeFaceBuffer.Add(MakeDeferredFace(newFace, orderedPivotIndex, adjacentFace, oldFaceAdjacentIndex, oldFace));
                }
            }
            
            return true;
        }

        /// <summary>
        /// Commits a cone and adds a vertex to the convex hull.
        /// </summary>
        void CommitCone()
        {
            // Add the current vertex.
            AddConvexVertex(CurrentVertex);
            
            // Fill the adjacency.
            for (int i = 0; i < ConeFaceBuffer.Count; i++)
            {
                var face = ConeFaceBuffer[i];

                var newFace = face.Face;
                var adjacentFace = face.Pivot;
                var oldFace = face.OldFace;
                var orderedPivotIndex = face.FaceIndex;

                newFace.AdjacentFaces[orderedPivotIndex] = adjacentFace.Index;
                adjacentFace.AdjacentFaces[face.PivotIndex] = newFace.Index;

                // let there be a connection.
                for (int j = 0; j < Dimension; j++)
                {
                    if (j == orderedPivotIndex) continue;
                    var connector = ObjectManager.GetConnector();
                    connector.Update(newFace, j, Dimension);
                    ConnectFace(connector);
                }
                
                // the id adjacent face on the hull? If so, we can use simple method to find beyond vertices.
                if (adjacentFace.VerticesBeyond.Count == 0)
                {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond);
                }
                // it is slightly more effective if the face with the lower number of beyond vertices comes first.
                else if (adjacentFace.VerticesBeyond.Count < oldFace.VerticesBeyond.Count)
                {
                    FindBeyondVertices(newFace, adjacentFace.VerticesBeyond, oldFace.VerticesBeyond);
                }
                else
                {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond, adjacentFace.VerticesBeyond);
                }

                // This face will definitely lie on the hull
                if (newFace.VerticesBeyond.Count == 0)
                {
                    ConvexFaces.Add(newFace.Index);
                    UnprocessedFaces.Remove(newFace);
                    ObjectManager.DepositVertexBuffer(newFace.VerticesBeyond);
                    newFace.VerticesBeyond = EmptyBuffer;
                }
                else // Add the face to the list
                {
                    UnprocessedFaces.Add(newFace);
                }

                // recycle the object.
                ObjectManager.DepositDeferredFace(face);
            }

            // Recycle the affected faces.
            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var face = AffectedFaceBuffer[fIndex];
                UnprocessedFaces.Remove(FacePool[face]);
                ObjectManager.DepositFace(face);                
            }
        }
        
        /// <summary>
        /// Check whether the vertex v is beyond the given face. If so, add it to beyondVertices.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="beyondVertices"></param>
        /// <param name="v"></param>
        void IsBeyond(ConvexFaceInternal face, IndexBuffer beyondVertices, int v)
        {
            double distance = MathHelper.GetVertexDistance(v, face);
            if (distance >= PlaneDistanceTolerance)
            {
                if (distance > MaxDistance)
                {
                    // If it's within the tolerance distance, use the lex. larger point
                    if (distance - MaxDistance < PlaneDistanceTolerance)
                    {
                        if (LexCompare(v, FurthestVertex) > 0)
                        {
                            MaxDistance = distance;
                            FurthestVertex = v;
                        }
                    }
                    else
                    {
                        MaxDistance = distance;
                        FurthestVertex = v;
                    }
                }
                beyondVertices.Add(v);
            }
        }
        
        /// <summary>
        /// Used by update faces.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond, IndexBuffer beyond1)
        {
            var beyondVertices = BeyondBuffer;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = 0;
            int v;

            for (int i = 0; i < beyond1.Count; i++) VertexMarks[beyond1[i]] = true;
            VertexMarks[CurrentVertex] = false;
            for (int i = 0; i < beyond.Count; i++)
            {
                v = beyond[i];
                if (v == CurrentVertex) continue;
                VertexMarks[v] = false;
                IsBeyond(face, beyondVertices, v);
            }

            for (int i = 0; i < beyond1.Count; i++)
            {
                v = beyond1[i];
                if (VertexMarks[v]) IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = FurthestVertex;

            // Pull the old switch a roo (switch the face beyond buffers)
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            BeyondBuffer = temp;
        }

        void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond)
        {
            var beyondVertices = BeyondBuffer;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = 0;
            int v;

            for (int i = 0; i < beyond.Count; i++)
            {
                v = beyond[i];
                if (v == CurrentVertex) continue;
                IsBeyond(face, beyondVertices, v);
            }
            
            face.FurthestVertex = FurthestVertex;

            // Pull the old switch a roo (switch the face beyond buffers)
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            BeyondBuffer = temp;
        }
                        
        /// <summary>
        /// Recalculates the centroid of the current hull.
        /// </summary>
        void UpdateCenter()
        {
            var count = ConvexHull.Count + 1;
            for (int i = 0; i < Dimension; i++) Center[i] *= (count - 1);
            double f = 1.0 / count;
            var co = CurrentVertex * Dimension;
            for (int i = 0; i < Dimension; i++) Center[i] = f * (Center[i] + Positions[co + i]);
        }

        /// <summary>
        /// Removes the last vertex from the center.
        /// </summary>
        void RollbackCenter()
        {
            var count = ConvexHull.Count + 1;
            for (int i = 0; i < Dimension; i++) Center[i] *= count;
            double f = 1.0 / (count - 1);
            var co = CurrentVertex * Dimension;
            for (int i = 0; i < Dimension; i++) Center[i] = f * (Center[i] - Positions[co + i]);
        }

        /// <summary>
        /// Handles singular vertex.
        /// </summary>
        void HandleSingular()
        {
            RollbackCenter();
            SingularVertices.Add(CurrentVertex);

            // This means that all the affected faces must be on the hull and that all their "vertices beyond" are singular.
            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var face = FacePool[AffectedFaceBuffer[fIndex]];
                var vb = face.VerticesBeyond;
                for (int i = 0; i < vb.Count; i++)
                {
                    SingularVertices.Add(vb[i]);
                }

                ConvexFaces.Add(face.Index);
                UnprocessedFaces.Remove(face);
                ObjectManager.DepositVertexBuffer(face.VerticesBeyond);
                face.VerticesBeyond = EmptyBuffer;
            }
        }

        /// <summary>
        /// Fins the convex hull.
        /// </summary>
        void FindConvexHull()
        {
            // Find the (dimension+1) initial points and create the simplexes.
            InitConvexHull();

            // Expand the convex hull and faces.
            while (UnprocessedFaces.First != null)
            {
                var currentFace = UnprocessedFaces.First;
                CurrentVertex = currentFace.FurthestVertex;
                                                
                UpdateCenter();

                // The affected faces get tagged
                TagAffectedFaces(currentFace);

                // Create the cone from the currentVertex and the affected faces horizon.
                if (!SingularVertices.Contains(CurrentVertex) && CreateCone()) CommitCone();
                else HandleSingular();

                // Need to reset the tags
                int count = AffectedFaceBuffer.Count;
                for (int i = 0; i < count; i++) AffectedFaceFlags[AffectedFaceBuffer[i]] = false;
            }
        }
    }
}
