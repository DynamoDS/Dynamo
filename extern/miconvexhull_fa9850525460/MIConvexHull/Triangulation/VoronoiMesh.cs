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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A factory class for creating a Voronoi mesh.
    /// </summary>
    public static class VoronoiMesh
    {
        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<TVertex, TCell, TEdge> Create<TVertex, TCell, TEdge>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TCell : TriangulationCell<TVertex, TCell>, new()
            where TVertex : IVertex
            where TEdge : VoronoiEdge<TVertex, TCell>, new()
        {
            return VoronoiMesh<TVertex, TCell, TEdge>.Create(data, config);
        }

        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>> Create<TVertex>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TVertex : IVertex
        {
            return VoronoiMesh<TVertex, DefaultTriangulationCell<TVertex>, VoronoiEdge<TVertex, DefaultTriangulationCell<TVertex>>>.Create(data, config);
        }

        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<DefaultVertex, DefaultTriangulationCell<DefaultVertex>, VoronoiEdge<DefaultVertex, DefaultTriangulationCell<DefaultVertex>>>
            Create(IList<double[]> data, TriangulationComputationConfig config = null)
        {
            var points = data.Select(p => new DefaultVertex { Position = p.ToArray() }).ToList();
            return VoronoiMesh<DefaultVertex, DefaultTriangulationCell<DefaultVertex>, VoronoiEdge<DefaultVertex, DefaultTriangulationCell<DefaultVertex>>>.Create(points, config);
        }

        /// <summary>
        /// Create the voronoi mesh.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TCell"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        /// <returns></returns>
        public static VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>> Create<TVertex, TCell>(IList<TVertex> data, TriangulationComputationConfig config = null)
            where TVertex : IVertex
            where TCell : TriangulationCell<TVertex, TCell>, new()
        {
            return VoronoiMesh<TVertex, TCell, VoronoiEdge<TVertex, TCell>>.Create(data, config);
        }
    }

    /// <summary>
    /// A representation of a voronoi mesh.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    /// <typeparam name="TEdge"></typeparam>
    public class VoronoiMesh<TVertex, TCell, TEdge>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
        where TEdge : VoronoiEdge<TVertex, TCell>, new()
    {
        /// <summary>
        /// This is probably not needed, but might make things a tiny bit faster.
        /// </summary>
        class EdgeComparer : IEqualityComparer<TEdge>
        {
            public bool Equals(TEdge x, TEdge y)
            {
                return (x.Source == y.Source && x.Target == y.Target) || (x.Source == y.Target && x.Target == y.Source);
            }

            public int GetHashCode(TEdge obj)
            {
                return obj.Source.GetHashCode() ^ obj.Target.GetHashCode();
            }
        }

        /// <summary>
        /// Vertices of the diagram.
        /// </summary>
        public IEnumerable<TCell> Vertices { get; private set; }

        /// <summary>
        /// Edges connecting the cells. 
        /// The same information can be retrieved Cells' Adjacency.
        /// </summary>
        public IEnumerable<TEdge> Edges { get; private set; }

        /// <summary>
        /// Create a Voronoi diagram of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default TriangulationComputationConfig is used.</param>
        public static VoronoiMesh<TVertex, TCell, TEdge> Create(IList<TVertex> data, TriangulationComputationConfig config)
        {
            if (data == null) throw new ArgumentNullException("data");
            
            var t = DelaunayTriangulation<TVertex, TCell>.Create(data, config);
            var vertices = t.Cells;
            var edges = new HashSet<TEdge>(new EdgeComparer());

            foreach (var f in vertices)
            {
                for (int i = 0; i < f.Adjacency.Length; i++)
                {
                    var af = f.Adjacency[i];
                    if (af != null) edges.Add(new TEdge { Source = f, Target = af });
                }
            }

            return new VoronoiMesh<TVertex, TCell, TEdge>
            {
                Vertices = vertices,
                Edges = edges.ToList()
            };
        }

        /// <summary>
        /// Can only be created using a factory method.
        /// </summary>
        private VoronoiMesh()
        {

        }
    }
}
