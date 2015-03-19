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
     * Code here transforms the result to its final form 
     */
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// This is called by the "ConvexHull" class.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig.GetDefault() is used.</param>
        /// <returns></returns>
        internal static ConvexHull<TVertex, TFace> GetConvexHull<TVertex, TFace>(IList<TVertex> data, ConvexHullComputationConfig config)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            config = config ?? new ConvexHullComputationConfig();

            var vertices = new IVertex[data.Count];
            for (int i = 0; i < data.Count; i++) vertices[i] = data[i];
            ConvexHullInternal ch = new ConvexHullInternal(vertices, false, config);
            ch.FindConvexHull();

            var hull = new TVertex[ch.ConvexHull.Count];
            for (int i = 0; i < hull.Length; i++)
            {
                hull[i] = (TVertex)ch.Vertices[ch.ConvexHull[i]];
            }

            return new ConvexHull<TVertex, TFace> { Points = hull, Faces = ch.GetConvexFaces<TVertex, TFace>() };
        }
        
        /// <summary>
        /// Finds the convex hull and creates the TFace objects.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <returns></returns>
        TFace[] GetConvexFaces<TVertex, TFace>()
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            var faces = ConvexFaces;
            int cellCount = faces.Count;
            var cells = new TFace[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = FacePool[faces[i]];
                var vertices = new TVertex[Dimension];
                for (int j = 0; j < Dimension; j++)
                {
                    vertices[j] = (TVertex)this.Vertices[face.Vertices[j]];
                }

                cells[i] = new TFace
                {
                    Vertices = vertices,
                    Adjacency = new TFace[Dimension],
                    Normal = IsLifted ? null : face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = FacePool[faces[i]];
                var cell = cells[i];
                for (int j = 0; j < Dimension; j++)
                {
                    if (face.AdjacentFaces[j] < 0) continue;
                    cell.Adjacency[j] = cells[FacePool[face.AdjacentFaces[j]].Tag];
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
    }
}
