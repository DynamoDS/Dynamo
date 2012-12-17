namespace MIConvexHull
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ConvexHullInternal
    {
        bool Computed;
        readonly int Dimension;

        List<VertexWrap> InputVertices;
        List<VertexWrap> ConvexHull;
        FaceList UnprocessedFaces;
        List<ConvexFaceInternal> ConvexFaces;

        #region "Buffers"
        VertexWrap CurrentVertex;
        double MaxDistance;
        VertexWrap FurthestVertex;

        /// <summary>
        /// The centroid of the currently computed hull.
        /// </summary>
        double[] Center;

        // Buffers for normal computation.
        double[] ntX, ntY, ntZ;
        double[] nDNormalSolveVector;
        double[,] nDMatrix;
        double[][] jaggedNDMatrix;

        ConvexFaceInternal[] UpdateBuffer;
        int[] UpdateIndices;


        Stack<ConvexFaceInternal> TraverseStack;
        Stack<ConvexFaceInternal> RecycledFaceStack;
        Stack<FaceConnector> ConnectorStack;
        Stack<VertexBuffer> EmptyBufferStack;
        VertexBuffer EmptyBuffer; // this is used for VerticesBeyond for faces that are on the convex hull
        VertexBuffer BeyondBuffer;
        List<ConvexFaceInternal> AffectedFaceBuffer;

        const int ConnectorTableSize = 2017;
        ConnectorList[] ConnectorTable;

        #endregion

        /// <summary>
        /// Initialize buffers and lists.
        /// </summary>
        void Initialize()
        {
            ConvexHull = new List<VertexWrap>();
            UnprocessedFaces = new FaceList(); // new LinkedList<ConvexFaceInternal>();
            ConvexFaces = new List<ConvexFaceInternal>();

            Center = new double[Dimension];
            ntX = new double[Dimension];
            ntY = new double[Dimension];
            ntZ = new double[Dimension];
            TraverseStack = new Stack<ConvexFaceInternal>();
            UpdateBuffer = new ConvexFaceInternal[Dimension];
            UpdateIndices = new int[Dimension];
            RecycledFaceStack = new Stack<ConvexFaceInternal>();
            ConnectorStack = new Stack<FaceConnector>();
            EmptyBufferStack = new Stack<VertexBuffer>();
            EmptyBuffer = new VertexBuffer();
            AffectedFaceBuffer = new List<ConvexFaceInternal>();
            BeyondBuffer = new VertexBuffer();

            ConnectorTable = Enumerable.Range(0, ConnectorTableSize).Select(_ => new ConnectorList()).ToArray();

            nDNormalSolveVector = new double[Dimension];
            jaggedNDMatrix = new double[Dimension][];
            for (var i = 0; i < Dimension; i++)
            {
                nDNormalSolveVector[i] = 1.0;
                jaggedNDMatrix[i] = new double[Dimension];
            }
            nDMatrix = new double[Dimension, Dimension];
        }

        /// <summary>
        /// Check the dimensionality of the input data.
        /// </summary>
        int DetermineDimension()
        {
            var r = new Random();
            var VCount = InputVertices.Count;
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(InputVertices[r.Next(VCount)].Vertex.Position.Length);
            var dimension = dimensions.Min();
            if (dimension != dimensions.Max()) throw new ArgumentException("Invalid input data (non-uniform dimension).");
            return dimension;
        }

        /// <summary>
        /// Create the first faces from (dimension + 1) vertices.
        /// </summary>
        /// <returns></returns>
        ConvexFaceInternal[] InitiateFaceDatabase()
        {
            var faces = new ConvexFaceInternal[Dimension + 1];

            for (var i = 0; i < Dimension + 1; i++)
            {
                var vertices = ConvexHull.Where((_, j) => i != j).ToArray(); // Skips the i-th vertex
                var newFace = new ConvexFaceInternal(Dimension, new VertexBuffer());
                newFace.Vertices = vertices;
                Array.Sort(vertices, VertexWrapComparer.Instance);
                CalculateFacePlane(newFace);
                faces[i] = newFace;
            }

            // update the adjacency (check all pairs of faces)
            for (var i = 0; i < Dimension; i++)
            {
                for (var j = i + 1; j < Dimension + 1; j++) UpdateAdjacency(faces[i], faces[j]);
            }

            return faces;
        }
        
        /// <summary>
        /// Calculates the normal and offset of the hyper-plane given by the face's vertices.
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private void CalculateFacePlane(ConvexFaceInternal face)
        {
            var vertices = face.Vertices;
            var normal = face.Normal;
            FindNormalVector(vertices, normal);

            if (double.IsNaN(normal[0])) ThrowSingular();

            double offset = 0.0;
            double centerDistance = 0.0;
            var fi = vertices[0].PositionData;
            for (int i = 0; i < Dimension; i++)
            {
                double n = normal[i];
                offset += n * fi[i];
                centerDistance += n * Center[i];
            }
            face.Offset = -offset;
            centerDistance -= offset;

            if (centerDistance > 0)
            {
                for (int i = 0; i < Dimension; i++) normal[i] = -normal[i];
                face.Offset = offset;
                face.IsNormalFlipped = true;
            }
            else face.IsNormalFlipped = false;
        }
        
        /// <summary>
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is >= 0.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        /// <returns>The vertex is "over face" if the result is positive.</returns>
        double GetVertexDistance(VertexWrap v, ConvexFaceInternal f)
        {
            double[] normal = f.Normal;
            double[] p = v.PositionData;
            double distance = f.Offset;
            for (int i = 0; i < Dimension; i++) distance += normal[i] * p[i];
            return distance;
        }
        
        //unsafe double GetVertexDistance(VertexWrap v, ConvexFaceInternal f)
        //{
        //    fixed (double* pNormal = f.Normal)
        //    fixed (double* pP = v.PositionData)
        //    {
        //        double* normal = pNormal;
        //        double* p = pP;

        //        double distance = f.Offset;
        //        for (int i = 0; i < Dimension; i++)
        //        {
        //            distance += (*normal) * (*p);
        //            normal++;
        //            p++;
        //        }
        //        return distance;
        //    }
        //}

        /// <summary>
        /// Tags all faces seen from the current vertex with 1.
        /// </summary>
        /// <param name="currentFace"></param>
        void TagAffectedFaces(ConvexFaceInternal currentFace)
        {
            AffectedFaceBuffer.Clear();
            AffectedFaceBuffer.Add(currentFace);
            TraverseAffectedFaces(currentFace);
        }
        
        /// <summary>
        /// Recursively traverse all the relevant faces.
        /// </summary>
        void TraverseAffectedFaces(ConvexFaceInternal currentFace)
        {
            TraverseStack.Clear();
            TraverseStack.Push(currentFace);
            currentFace.Tag = 1;

            while (TraverseStack.Count > 0)
            {
                var top = TraverseStack.Pop();
                for (int i = 0; i < Dimension; i++)
                {
                    var adjFace = top.AdjacentFaces[i];

                    if (adjFace.Tag == 0 && GetVertexDistance(CurrentVertex, adjFace) >= 0)
                    {
                        AffectedFaceBuffer.Add(adjFace);
                        //TraverseAffectedFaces(adjFace);
                        adjFace.Tag = 1;
                        TraverseStack.Push(adjFace);
                    }
                }
            }
            
            ////for (int i = 0; i < Dimension; i++)
            ////{
            ////    var adjFace = currentFace.AdjacentFaces[i];

            ////    if (adjFace.Tag == 0 && GetVertexDistance(CurrentVertex, adjFace) >= 0)
            ////    {
            ////        AffectedFaceBuffer.Add(adjFace);
            ////        TraverseAffectedFaces(adjFace);
            ////    }
            ////}
        }

        /// <summary>
        /// Check if 2 faces are adjacent and if so, update their AdjacentFaces array.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="r"></param>
        void UpdateAdjacency(ConvexFaceInternal l, ConvexFaceInternal r)
        {
            var lv = l.Vertices;
            var rv = r.Vertices;
            int i;

            // reset marks on the 1st face
            for (i = 0; i < Dimension; i++) lv[i].Marked = false;

            // mark all vertices on the 2nd face
            for (i = 0; i < Dimension; i++) rv[i].Marked = true;

            // find the 1st false index
            for (i = 0; i < Dimension; i++) if (!lv[i].Marked) break;

            // no vertex was marked
            if (i == Dimension) return;

            // check if only 1 vertex wasn't marked
            for (int j = i + 1; j < Dimension; j++) if (!lv[j].Marked) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r;

            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < Dimension; i++) lv[i].Marked = false;
            for (i = 0; i < Dimension; i++)
            {
                if (rv[i].Marked) break;
            }
            r.AdjacentFaces[i] = l;
        }

        #region Memory stuff.
        
        /// <summary>
        /// Recycle face for future use.
        /// </summary>
        void RecycleFace(ConvexFaceInternal face)
        {
            for (int i = 0; i < Dimension; i++)
            {
                face.AdjacentFaces[i] = null;
            }
        }
        
        /// <summary>
        /// Get a fresh face.
        /// </summary>
        /// <returns></returns>
        ConvexFaceInternal GetNewFace()
        {
            return RecycledFaceStack.Count != 0
                    ? RecycledFaceStack.Pop()
                    : new ConvexFaceInternal(Dimension, EmptyBufferStack.Count != 0 ? EmptyBufferStack.Pop() : new VertexBuffer());
        }

        /// <summary>
        /// Get a new connector.
        /// </summary>
        /// <returns></returns>
        FaceConnector GetNewConnector()
        {
            return ConnectorStack.Count != 0
                    ? ConnectorStack.Pop()
                    : new FaceConnector(Dimension);
        }        
        #endregion

        /// <summary>
        /// Connect faces using a connector.
        /// </summary>
        /// <param name="connector"></param>
        void ConnectFace(FaceConnector connector)
        {
            var index = connector.HashCode % ConnectorTableSize;
            var list = ConnectorTable[index];

            int count = 0;
            for (var current = list.First; current != null; current = current.Next)
            {
                if (FaceConnector.AreConnectable(connector, current, Dimension))
                {
                    list.Remove(current);
                    FaceConnector.Connect(current, connector);
                    current.Face = null;
                    connector.Face = null;
                    ConnectorStack.Push(current);
                    ConnectorStack.Push(connector);
                    return;
                }
            }

            list.Add(connector);
        }

        /// <summary>
        /// Removes the faces "covered" by the current vertex and adds the newly created ones.
        /// </summary>
        private void CreateCone()
        {
            var oldFaces = AffectedFaceBuffer;

            var currentVertexIndex = CurrentVertex.Index;

            for (int fIndex = 0; fIndex < oldFaces.Count; fIndex++)
            {
                var oldFace = oldFaces[fIndex];

                // Find the faces that need to be updated
                int updateCount = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    var af = oldFace.AdjacentFaces[i];
                    if (af.Tag == 0) // Tag == 0 when oldFaces does not contain af
                    {
                        UpdateBuffer[updateCount] = af;
                        UpdateIndices[updateCount] = i;
                        ++updateCount;
                    }
                }

                // Recycle the face for future use
                if (updateCount == 0)
                {
                    // If the face is present in the unprocessed list, remove it 
                    UnprocessedFaces.Remove(oldFace);

                    RecycleFace(oldFace);
                    RecycledFaceStack.Push(oldFace);
                }

                for (int i = 0; i < updateCount; i++)
                {
                    var adjacentFace = UpdateBuffer[i];

                    int oldFaceAdjacentIndex = 0;
                    var adjFaceAdjacency = adjacentFace.AdjacentFaces;
                    for (int j = 0; j < Dimension; j++)
                    {
                        if (object.ReferenceEquals(oldFace, adjFaceAdjacency[j]))
                        {
                            oldFaceAdjacentIndex = j;
                            break;
                        }
                    }

                    var forbidden = UpdateIndices[i]; // Index of the face that corresponds to this adjacent face

                    ConvexFaceInternal newFace;

                    int oldVertexIndex;
                    VertexWrap[] vertices;

                    // Recycle the oldFace
                    if (i == updateCount - 1)
                    {
                        RecycleFace(oldFace);
                        newFace = oldFace;
                        vertices = newFace.Vertices;
                        oldVertexIndex = vertices[forbidden].Index;
                    }
                    else // Pop a face from the recycled stack or create a new one
                    {
                        newFace = GetNewFace();
                        vertices = newFace.Vertices;                        
                        for (int j = 0; j < Dimension; j++) vertices[j] = oldFace.Vertices[j];
                        oldVertexIndex = vertices[forbidden].Index;
                    }

                    int orderedPivotIndex;

                    // correct the ordering
                    if (currentVertexIndex < oldVertexIndex)
                    {
                        orderedPivotIndex = 0;
                        for (int j = forbidden - 1; j >= 0; j--)
                        {
                            if (vertices[j].Index > currentVertexIndex) vertices[j + 1] = vertices[j];
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
                            if (vertices[j].Index < currentVertexIndex) vertices[j - 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j - 1;
                                break;
                            }
                        }
                    }
                    
                    vertices[orderedPivotIndex] = CurrentVertex;

                    CalculateFacePlane(newFace);
                    newFace.AdjacentFaces[orderedPivotIndex] = adjacentFace;
                    adjacentFace.AdjacentFaces[oldFaceAdjacentIndex] = newFace;

                    // let there be a connection.
                    for (int j = 0; j < Dimension; j++)
                    {
                        if (j == orderedPivotIndex) continue;
                        var connector = GetNewConnector();
                        connector.Update(newFace, j, Dimension);
                        ConnectFace(connector);
                    }
                    
                    // This could slightly help...
                    if (adjacentFace.VerticesBeyond.Count < oldFace.VerticesBeyond.Count)
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
                        ConvexFaces.Add(newFace);
                        UnprocessedFaces.Remove(newFace);
                        EmptyBufferStack.Push(newFace.VerticesBeyond);
                        newFace.VerticesBeyond = EmptyBuffer;
                    }
                    else // Add the face to the list
                    {
                        UnprocessedFaces.Add(newFace);
                    }
                }
            }
        }

        /// <summary>
        /// Subtracts vectors x and y and stores the result to target.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="target"></param>
        void SubtractFast(double[] x, double[] y, double[] target)
        {
            for (int i = 0; i < Dimension; i++)
            {
                target[i] = x[i] - y[i];
            }
        }

        /// <summary>
        /// Finds 4D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector4D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);
            SubtractFast(vertices[2].PositionData, vertices[1].PositionData, ntY);
            SubtractFast(vertices[3].PositionData, vertices[2].PositionData, ntZ);

            var x = ntX;
            var y = ntY;
            var z = ntZ;

            // This was generated using Mathematica
            var nx = x[3] * (y[2] * z[1] - y[1] * z[2])
                   + x[2] * (y[1] * z[3] - y[3] * z[1])
                   + x[1] * (y[3] * z[2] - y[2] * z[3]);
            var ny = x[3] * (y[0] * z[2] - y[2] * z[0])
                   + x[2] * (y[3] * z[0] - y[0] * z[3])
                   + x[0] * (y[2] * z[3] - y[3] * z[2]);
            var nz = x[3] * (y[1] * z[0] - y[0] * z[1])
                   + x[1] * (y[0] * z[3] - y[3] * z[0])
                   + x[0] * (y[3] * z[1] - y[1] * z[3]);
            var nw = x[2] * (y[0] * z[1] - y[1] * z[0])
                   + x[1] * (y[2] * z[0] - y[0] * z[2])
                   + x[0] * (y[1] * z[2] - y[2] * z[1]);

            double norm = System.Math.Sqrt(nx * nx + ny * ny + nz * nz + nw * nw);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
            normal[3] = f * nw;
        }

        /// <summary>
        /// Finds 3D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector3D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);
            SubtractFast(vertices[2].PositionData, vertices[1].PositionData, ntY);

            var x = ntX;
            var y = ntY;

            var nx = x[1] * y[2] - x[2] * y[1];
            var ny = x[2] * y[0] - x[0] * y[2];
            var nz = x[0] * y[1] - x[1] * y[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
        }

        /// <summary>
        /// Finds 2D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector2D(VertexWrap[] vertices, double[] normal)
        {
            SubtractFast(vertices[1].PositionData, vertices[0].PositionData, ntX);

            var x = ntX;

            var nx = -x[1];
            var ny = x[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
        }

        /// <summary>
        /// Finds normal vector of a hyper-plane given by vertices.
        /// Stores the results to normalData.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normalData"></param>
        private void FindNormalVector(VertexWrap[] vertices, double[] normalData)
        {
            switch (Dimension)
            {
                case 2: FindNormalVector2D(vertices, normalData); break;
                case 3: FindNormalVector3D(vertices, normalData); break;
                case 4: FindNormalVector4D(vertices, normalData); break;
                default:
                    {
                        for (var i = 0; i < Dimension; i++) nDNormalSolveVector[i] = 1.0;
                        for (var i = 0; i < Dimension; i++)
                        {
                            var row = jaggedNDMatrix[i];
                            var pos = vertices[i].Vertex.Position;
                            for (int j = 0; j < Dimension; j++) row[j] = pos[j];
                        }
                        StarMath.gaussElimination(Dimension, jaggedNDMatrix, nDNormalSolveVector, normalData);
                        StarMath.normalizeInPlace(normalData, Dimension);
                        break;
                    }
            }
        }

        /// <summary>
        /// Check whether the vertex v is beyond the given face. If so, add it to beyondVertices.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="beyondVertices"></param>
        /// <param name="v"></param>
        void IsBeyond(ConvexFaceInternal face, VertexBuffer beyondVertices, VertexWrap v)
        {
            double distance = GetVertexDistance(v, face);
            if (distance >= 0)
            {
                if (distance > MaxDistance)
                {
                    MaxDistance = distance;
                    FurthestVertex = v;
                }
                beyondVertices.Add(v);
            }
        }

        /// <summary>
        /// Used in the "initialization" code.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face)
        {
            var beyondVertices = face.VerticesBeyond;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = null;

            int count = InputVertices.Count;
            for (int i = 0; i < count; i++) IsBeyond(face, beyondVertices, InputVertices[i]);

            face.FurthestVertex = FurthestVertex;
            //face.FurthestDistance = MaxDistance;
        }

        /// <summary>
        /// Used by update faces.
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face, VertexBuffer beyond, VertexBuffer beyond1)
        {
            var beyondVertices = BeyondBuffer;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = null;
            VertexWrap v;

            int count = beyond1.Count;
            for (int i = 0; i < count; i++) beyond1[i].Marked = true;
            CurrentVertex.Marked = false;
            count = beyond.Count;
            for (int i = 0; i < count; i++)
            {
                v = beyond[i];
                if (object.ReferenceEquals(v, CurrentVertex)) continue;
                v.Marked = false;
                IsBeyond(face, beyondVertices, v);
            }

            count = beyond1.Count;
            for (int i = 0; i < count; i++)
            {
                v = beyond1[i];
                if (v.Marked) IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = FurthestVertex;
            //face.FurthestDistance = MaxDistance;

            // Pull the old switch a roo
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
            for (int i = 0; i < Dimension; i++) Center[i] *= (ConvexHull.Count - 1);
            double f = 1.0 / ConvexHull.Count;
            for (int i = 0; i < Dimension; i++) Center[i] = f * (Center[i] + CurrentVertex.PositionData[i]);
        }

        /// <summary>
        /// Find the (dimension+1) initial points and create the simplexes.
        /// </summary>
        void InitConvexHull()
        {
            var extremes = FindExtremes();
            var initialPoints = FindInitialPoints(extremes);

            // Add the initial points to the convex hull.
            foreach (var vertex in initialPoints)
            {
                CurrentVertex = vertex;
                ConvexHull.Add(CurrentVertex);
                UpdateCenter();
                InputVertices.Remove(vertex);

                // Because of the AklTou heuristic.
                extremes.Remove(vertex);
            }

            // Create the initial simplexes.
            var faces = InitiateFaceDatabase();

            // Init the vertex beyond buffers.
            foreach (var face in faces)
            {
                FindBeyondVertices(face);
                if (face.VerticesBeyond.Count == 0) ConvexFaces.Add(face); // The face is on the hull
                else UnprocessedFaces.Add(face);
            }
        }

        /// <summary>
        /// Finds (dimension + 1) initial points.
        /// </summary>
        /// <param name="extremes"></param>
        /// <returns></returns>
        private List<VertexWrap> FindInitialPoints(List<VertexWrap> extremes)
        {
            List<VertexWrap> initialPoints = new List<VertexWrap>();// { extremes[0], extremes[1] };

            VertexWrap first = null, second = null;
            double maxDist = 0;
            for (int i = 0; i < extremes.Count - 1; i++)
            {
                var a = extremes[i];
                for (int j = i + 1; j < extremes.Count; j++)
                {
                    var b = extremes[j];
                    var dist = StarMath.norm2(StarMath.subtract(a.PositionData, b.PositionData, Dimension), Dimension, true);
                    if (dist > maxDist)
                    {
                        first = a;
                        second = b;
                        maxDist = dist;
                    }
                }
            }

            initialPoints.Add(first);
            initialPoints.Add(second);

            for (int i = 2; i <= Dimension; i++)
            {
                double maximum = 0.0001;
                VertexWrap maxPoint = null;
                for (int j = 0; j < extremes.Count; j++)
                {
                    var extreme = extremes[j];
                    if (initialPoints.Contains(extreme)) continue;

                    var val = GetSimplexVolume(extreme, initialPoints);

                    if (val > maximum)
                    {
                        maximum = val;
                        maxPoint = extreme;
                    }
                }
                if (maxPoint != null) initialPoints.Add(maxPoint);
                else
                {
                    int vCount = InputVertices.Count;
                    for (int j = 0; j < vCount; j++)
                    {
                        var point = InputVertices[j];
                        if (initialPoints.Contains(point)) continue;

                        var val = GetSimplexVolume(point, initialPoints);

                        if (val > maximum)
                        {
                            maximum = val;
                            maxPoint = point;
                        }
                    }

                    if (maxPoint != null) initialPoints.Add(maxPoint);
                    else ThrowSingular();
                }
            }
            return initialPoints;
        }

        /// <summary>
        /// Computes the volume of the (n=initialPoints.Count)D simplex defined by the
        /// pivot and initialPoints.
        /// This is computed as the determinant of the matrix | initialPoints[i] - pivot |
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="initialPoints"></param>
        /// <returns></returns>
        double GetSimplexVolume(VertexWrap pivot, List<VertexWrap> initialPoints)
        {
            var dim = initialPoints.Count;
            var m = nDMatrix;

            for (int i = 0; i < dim; i++)
            {
                var pts = initialPoints[i];
                for (int j = 0; j < dim; j++) m[i, j] = pts.PositionData[j] - pivot.PositionData[j];
            }

            return Math.Abs(StarMath.determinantDestructive(m, dim));
        }

        /// <summary>
        /// Finds the extremes in all dimensions.
        /// </summary>
        /// <returns></returns>
        private List<VertexWrap> FindExtremes()
        {
            var extremes = new List<VertexWrap>(2 * Dimension);

            int vCount = InputVertices.Count;
            for (int i = 0; i < Dimension; i++)
            {
                double min = double.MaxValue, max = double.MinValue;
                int minInd = 0, maxInd = 0;
                for (int j = 0; j < vCount; j++)
                {
                    var v = InputVertices[j].PositionData[i];
                    if (v < min)
                    {
                        min = v;
                        minInd = j;
                    }
                    if (v > max)
                    {
                        max = v;
                        maxInd = j;
                    }
                }

                if (minInd != maxInd)
                {
                    extremes.Add(InputVertices[minInd]);
                    extremes.Add(InputVertices[maxInd]);
                }
                else extremes.Add(InputVertices[minInd]);
            }
            return extremes;
        }

        /// <summary>
        /// The exception thrown if singular input data detected.
        /// </summary>
        void ThrowSingular()
        {
            throw new InvalidOperationException(
                    "ConvexHull: Singular input data (i.e. trying to triangulate a data that contain a regular lattice of points).\n"
                    + "Introducing some noise to the data might resolve the issue.");
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
                ConvexHull.Add(CurrentVertex);
                UpdateCenter();

                // The affected faces get tagged
                TagAffectedFaces(currentFace);

                // Create the cone from the currentVertex and the affected faces horizon.
                CreateCone();

                // Need to reset the tags
                int count = AffectedFaceBuffer.Count;
                for (int i = 0; i < count; i++) AffectedFaceBuffer[i].Tag = 0;
            }
        }

        /// <summary>
        /// Wraps the vertices and determines the dimension if it's unknown.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="dim"></param>
        private ConvexHullInternal(IEnumerable<IVertex> vertices)
        {
            InputVertices = new List<VertexWrap>(vertices.Select((v, i) => new VertexWrap { Vertex = v, PositionData = v.Position, Index = i }));
            Dimension = DetermineDimension();
            Initialize();
        }

        /// <summary>
        /// Finds the vertices on the convex hull and optionally converts them to the TVertex array.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="onlyCompute"></param>
        /// <returns></returns>
        private IEnumerable<TVertex> GetConvexHullInternal<TVertex>(bool onlyCompute = false) where TVertex : IVertex
        {
            if (Computed) return onlyCompute ? null : ConvexHull.Select(v => (TVertex)v.Vertex).ToArray();

            if (Dimension < 2) throw new ArgumentException("Dimension of the input must be 2 or greater.");

            FindConvexHull();
            Computed = true;
            return onlyCompute ? null : ConvexHull.Select(v => (TVertex)v.Vertex).ToArray();
        }

        /// <summary>
        /// Finds the convex hull and creates the TFace objects.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <returns></returns>
        private IEnumerable<TFace> GetConvexFacesInternal<TVertex, TFace>()
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            if (!Computed) GetConvexHullInternal<TVertex>(true);

            var faces = ConvexFaces;
            int cellCount = faces.Count;
            var cells = new TFace[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = faces[i];
                var vertices = new TVertex[Dimension];
                for (int j = 0; j < Dimension; j++) vertices[j] = (TVertex)face.Vertices[j].Vertex;
                cells[i] = new TFace
                {
                    Vertices = vertices,
                    Adjacency = new TFace[Dimension],
                    Normal = face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = faces[i];
                var cell = cells[i];
                for (int j = 0; j < Dimension; j++)
                {
                    if (face.AdjacentFaces[j] == null) continue;
                    cell.Adjacency[j] = cells[face.AdjacentFaces[j].Tag];
                }

                // Fix the vertex orientation.
                if (face.IsNormalFlipped)
                {
                    var tempVert = cell.Vertices[0];
                    cell.Vertices[0] = cell.Vertices[Dimension - 1];
                    cell.Vertices[Dimension - 1] = tempVert;

                    var tempAdj = cell.Adjacency[0];
                    cell.Adjacency[0] = cell.Adjacency[Dimension - 1];
                    cell.Adjacency[Dimension - 1] = tempAdj;
                }
            }

            return cells;
        }

        /// <summary>
        /// This is used by the Delaunay triangulation code.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static List<ConvexFaceInternal> GetConvexFacesInternal<TVertex, TFace>(IEnumerable<TVertex> data)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            ConvexHullInternal ch = new ConvexHullInternal(data.Cast<IVertex>());
            ch.GetConvexHullInternal<TVertex>(true);
            return ch.ConvexFaces;
        }

        /// <summary>
        /// This is called by the "ConvexHull" class.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static Tuple<IEnumerable<TVertex>, IEnumerable<TFace>> GetConvexHullAndFaces<TVertex, TFace>(IEnumerable<IVertex> data)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            ConvexHullInternal ch = new ConvexHullInternal(data);
            return Tuple.Create(
                ch.GetConvexHullInternal<TVertex>(),
                ch.GetConvexFacesInternal<TVertex, TFace>());
        }
    }
}
