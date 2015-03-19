/******************************************************************************
 *
 *    MIConvexHull, Copyright (C) 2015 David Sehnal, Matthew Campbell
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
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A helper class mostly for normal computation. If convex hulls are computed
    /// in higher dimensions, it might be a good idea to add a specific
    /// FindNormalVectorND function.
    /// </summary>
    class MathHelper
    {
        readonly int Dimension;

        double[] PositionData;

        double[] ntX, ntY, ntZ;
        double[] nDNormalHelperVector;
        double[] nDMatrix;
        int[] matrixPivots;

        #region Normals
        // Modified from Math.NET
        // Copyright (c) 2009-2013 Math.NET
        static void LUFactor(double[] data, int order, int[] ipiv, double[] vecLUcolj)
        {
            // Initialize the pivot matrix to the identity permutation.
            for (var i = 0; i < order; i++)
            {
                ipiv[i] = i;
            }

            // Outer loop.
            for (var j = 0; j < order; j++)
            {
                var indexj = j * order;
                var indexjj = indexj + j;

                // Make a copy of the j-th column to localize references.
                for (var i = 0; i < order; i++)
                {
                    vecLUcolj[i] = data[indexj + i];
                }

                // Apply previous transformations.
                for (var i = 0; i < order; i++)
                {
                    // Most of the time is spent in the following dot product.
                    var kmax = Math.Min(i, j);
                    var s = 0.0;
                    for (var k = 0; k < kmax; k++)
                    {
                        s += data[(k * order) + i] * vecLUcolj[k];
                    }

                    data[indexj + i] = vecLUcolj[i] -= s;
                }

                // Find pivot and exchange if necessary.
                var p = j;
                for (var i = j + 1; i < order; i++)
                {
                    if (Math.Abs(vecLUcolj[i]) > Math.Abs(vecLUcolj[p]))
                    {
                        p = i;
                    }
                }

                if (p != j)
                {
                    for (var k = 0; k < order; k++)
                    {
                        var indexk = k * order;
                        var indexkp = indexk + p;
                        var indexkj = indexk + j;
                        var temp = data[indexkp];
                        data[indexkp] = data[indexkj];
                        data[indexkj] = temp;
                    }

                    ipiv[j] = p;
                }

                // Compute multipliers.
                if (j < order & data[indexjj] != 0.0)
                {
                    for (var i = j + 1; i < order; i++)
                    {
                        data[indexj + i] /= data[indexjj];
                    }
                }
            }
        }

        void FindNormal(int[] vertices, double[] normal)
        {
            var iPiv = matrixPivots;
            var data = nDMatrix;

            double norm = 0.0;
            // Solve determinants by replacing x-th column by all 1.
            for (int x = 0; x < Dimension; x++)
            {
                for (int i = 0; i < Dimension; i++)
                {
                    var offset = vertices[i] * Dimension;
                    for (int j = 0; j < Dimension; j++)
                    {
                        data[Dimension * j + i] = j == x ? 1.0 : PositionData[offset + j];
                    }
                }
                LUFactor(data, Dimension, iPiv, nDNormalHelperVector);
                var coord = 1.0;
                for (int i = 0; i < Dimension; i++)
                {                    
                    if (iPiv[i] != i) coord *= -data[Dimension * i + i];
                    else coord *= data[Dimension * i + i];
                }
                normal[x] = coord;
                norm += coord * coord;
            }

            // Normalize the result
            double f = 1.0 / Math.Sqrt(norm);
            for (int i = 0; i < normal.Length; i++) normal[i] *= f;
        }

        /// <summary>
        /// Squared length of the vector.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double LengthSquared(double[] x)
        {
            double norm = 0;
            for (int i = 0; i < x.Length; i++)
            {
                var t = x[i];
                norm += t * t;
            }
            return norm;
        }

        /// <summary>
        /// Subtracts vectors x and y and stores the result to target.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="target"></param>
        public void SubtractFast(int x, int y, double[] target)
        {
            int u = x * Dimension, v = y * Dimension;
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = PositionData[u + i] - PositionData[v + i];
            }
        }

        /// <summary>
        /// Finds 4D normal vector.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normal"></param>
        void FindNormalVector4D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);
            SubtractFast(vertices[2], vertices[1], ntY);
            SubtractFast(vertices[3], vertices[2], ntZ);

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
        void FindNormalVector3D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);
            SubtractFast(vertices[2], vertices[1], ntY);

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
        void FindNormalVector2D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);

            var x = ntX;

            var nx = -x[1];
            var ny = x[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
        }

        void FindNormalVectorND(int[] vertices, double[] normal)
        {
            /*
             * We need to solve the matrix A n = B where
             *  - A contains coordinates of vertices as columns
             *  - B is vector with all 1
             *   
             * To do this, we apply "modified" Cramer's rule: n_i = Det(A_i) / Det(A) where
             * A_i is created from A by replacing i-th column by B. The modification comes
             * from ignoring the factor 1/Det(A). Because:
             *  - It would get "lost" during the final normalization step anyway.
             *  - More importantly, allows us to compute normals for singlular A matrices
             *    (i.e. matrices with zero determinat).
             */

            var iPiv = matrixPivots;
            var data = nDMatrix;
            double norm = 0.0;

            // Solve determinants by replacing x-th column by all 1.
            for (int x = 0; x < Dimension; x++)
            {
                for (int i = 0; i < Dimension; i++)
                {
                    var offset = vertices[i] * Dimension;
                    for (int j = 0; j < Dimension; j++)
                    {
                        // maybe I got the i/j mixed up here regarding the representation Math.net uses...
                        // ...but it does not matter since Det(A) = Det(Transpose(A)).
                        data[Dimension * i + j] = j == x ? 1.0 : PositionData[offset + j];
                    }
                }
                LUFactor(data, Dimension, iPiv, nDNormalHelperVector);
                var coord = 1.0;
                for (int i = 0; i < Dimension; i++)
                {
                    if (iPiv[i] != i) coord *= -data[Dimension * i + i]; // the determinant sign changes on row swap.
                    else coord *= data[Dimension * i + i];
                }
                normal[x] = coord;
                norm += coord * coord;
            }

            // Normalize the result
            double f = 1.0 / Math.Sqrt(norm);
            for (int i = 0; i < normal.Length; i++) normal[i] *= f;
        }

        /// <summary>
        /// Finds normal vector of a hyper-plane given by vertices.
        /// Stores the results to normalData.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normalData"></param>
        public void FindNormalVector(int[] vertices, double[] normalData)
        {
            switch (Dimension)
            {
                case 2: FindNormalVector2D(vertices, normalData); break;
                case 3: FindNormalVector3D(vertices, normalData); break;
                case 4: FindNormalVector4D(vertices, normalData); break;
                default: FindNormalVectorND(vertices, normalData); break;
            }
        }
        #endregion

        /// <summary>
        /// Calculates the normal and offset of the hyper-plane given by the face's vertices.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public bool CalculateFacePlane(ConvexFaceInternal face, double[] center)
        {
            var vertices = face.Vertices;
            var normal = face.Normal;
            FindNormalVector(vertices, normal);

            if (double.IsNaN(normal[0]))
            {
                return false;
            }

            double offset = 0.0;
            double centerDistance = 0.0;
            var fi = vertices[0] * Dimension;
            for (int i = 0; i < Dimension; i++)
            {
                double n = normal[i];
                offset += n * PositionData[fi + i];
                centerDistance += n * center[i];
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

            return true;
        }

        /// <summary>
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is > Constants.PlaneDistanceTolerance.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="f"></param>
        /// <returns>The vertex is "over face" if the result is positive.</returns>
        public double GetVertexDistance(int v, ConvexFaceInternal f)
        {
            double[] normal = f.Normal;
            int x = v * Dimension;
            double distance = f.Offset;
            for (int i = 0; i < normal.Length; i++) distance += normal[i] * PositionData[x + i];
            return distance;
        }

        #region Simplex Volume
        /// <summary>
        /// Helper class with "buffers" for computing simplex volume.
        /// </summary>
        public class SimplexVolumeBuffer
        {
            public int Dimension;
            public double[] Data;
            public double[] Helper;
            public int[] Pivots;

            public SimplexVolumeBuffer(int dimension)
            {
                Dimension = dimension;
                Data = new double[dimension * dimension];
                Helper = new double[dimension];
                Pivots = new int[dimension];
            }
        }

        /// <summary>
        /// Computes the volume of an n-dimensional simplex.
        /// Buffer needs to be array of shape Dimension x Dimension.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="vertices"></param>
        /// <param name="buffer">Helper for the calculation to avoid unnecessary allocations.</param>
        /// <returns></returns>
        public static double GetSimplexVolume(ConvexFaceInternal cell, IList<IVertex> vertices, SimplexVolumeBuffer buffer)
        {
            var xs = cell.Vertices;
            var pivot = vertices[xs[0]].Position;
            var data = buffer.Data;
            var dim = buffer.Dimension;
            double f = 1.0;
            for (int i = 1; i < xs.Length; i++)
            {
                f *= i + 1;
                var point = vertices[xs[i]].Position;
                for (int j = 0; j < point.Length; j++) data[j * dim + i - 1] = point[j] - pivot[j];
            }

            return Math.Abs(DeterminantDestructive(buffer)) / f;
        }

        static double DeterminantDestructive(SimplexVolumeBuffer buff)
        {
            var A = buff.Data;
            switch (buff.Dimension)
            {
                case 0: return 0.0;
                case 1: return A[0];
                case 2: return (A[0] * A[3]) - (A[1] * A[2]);
                case 3: return (A[0] * A[4] * A[8]) + (A[1] * A[5] * A[6]) + (A[2] * A[3] * A[7])
                             - (A[0] * A[5] * A[7]) - (A[1] * A[3] * A[8]) - (A[2] * A[4] * A[6]);
                default:
                    {
                        var iPiv = buff.Pivots;
                        var dim = buff.Dimension;
                        LUFactor(A, dim, iPiv, buff.Helper);
                        var det = 1.0;
                        for (int i = 0; i < iPiv.Length; i++)
                        {
                            det *= A[dim * i + i];
                            if (iPiv[i] != i) det *= -1; // the determinant sign changes on row swap.
                        }
                        return det;
                    }
            }
        }

        #endregion

        public MathHelper(int dimension, double[] positions)
        {
            this.PositionData = positions;
            this.Dimension = dimension;

            ntX = new double[Dimension];
            ntY = new double[Dimension];
            ntZ = new double[Dimension];

            nDNormalHelperVector = new double[Dimension];
            nDMatrix = new double[Dimension * Dimension];
            matrixPivots = new int[Dimension];
        }
    }
}
