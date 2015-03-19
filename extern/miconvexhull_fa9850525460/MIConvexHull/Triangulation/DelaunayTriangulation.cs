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
    using System.Linq;
    using System;

    /// <summary>
    /// Calculation and representation of Delaunay triangulation.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public class DelaunayTriangulation<TVertex, TCell> : ITriangulation<TVertex, TCell>
        where TCell : TriangulationCell<TVertex, TCell>, new()
        where TVertex : IVertex
    {
        /// <summary>
        /// Cells of the triangulation.
        /// </summary>
        public IEnumerable<TCell> Cells { get; private set; }
        
        /// <summary>
        /// Creates the Delaunay triangulation of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig is used.</param>
        /// <returns></returns>
        public static DelaunayTriangulation<TVertex, TCell> Create(IList<TVertex> data, TriangulationComputationConfig config)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (data.Count == 0) return new DelaunayTriangulation<TVertex, TCell> { Cells = new TCell[0] };

            config = config ?? new TriangulationComputationConfig();       
            var cells = ConvexHullInternal.GetDelaunayTriangulation<TVertex, TCell>(data, config);

            return new DelaunayTriangulation<TVertex, TCell> { Cells = cells };
        }

        /// <summary>
        /// Can only be created using a factory method.
        /// </summary>
        private DelaunayTriangulation()
        {

        }
    }
}
