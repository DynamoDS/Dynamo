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
    /// Factory class for computing convex hulls.
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        /// Creates a convex hull of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <typeparam name="TFace"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig is used.</param>
        /// <returns></returns>
        public static ConvexHull<TVertex, TFace> Create<TVertex, TFace>(IList<TVertex> data, ConvexHullComputationConfig config = null)
            where TVertex : IVertex
            where TFace : ConvexFace<TVertex, TFace>, new()
        {
            return ConvexHull<TVertex, TFace>.Create(data, config);
        }

        /// <summary>
        /// Creates a convex hull of the input data.
        /// </summary>
        /// <typeparam name="TVertex"></typeparam>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig is used.</param>
        /// <returns></returns>
        public static ConvexHull<TVertex, DefaultConvexFace<TVertex>> Create<TVertex>(IList<TVertex> data, ConvexHullComputationConfig config = null)
            where TVertex : IVertex
        {
            return ConvexHull<TVertex, DefaultConvexFace<TVertex>>.Create(data, config);
        }

        /// <summary>
        /// Creates a convex hull of the input data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig is used.</param>
        /// <returns></returns>
        public static ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>> Create(IList<double[]> data, ConvexHullComputationConfig config = null)
        {
            var points = data.Select(p => new DefaultVertex { Position = p.ToArray() }).ToList();
            return ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>>.Create(points, config);
        }
    }

    /// <summary>
    /// Representation of a convex hull.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TFace"></typeparam>
    public class ConvexHull<TVertex, TFace>
        where TVertex : IVertex
        where TFace : ConvexFace<TVertex, TFace>, new()
    {
        /// <summary>
        /// Points of the convex hull.
        /// </summary>
        public IEnumerable<TVertex> Points { get; internal set; }

        /// <summary>
        /// Faces of the convex hull.
        /// </summary>
        public IEnumerable<TFace> Faces { get; internal set; }

        /// <summary>
        /// Creates the convex hull.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="config">If null, default ConvexHullComputationConfig is used.</param>
        /// <returns></returns>
        public static ConvexHull<TVertex, TFace> Create(IList<TVertex> data, ConvexHullComputationConfig config)
        {
            if (data == null) throw new ArgumentNullException("data");
            return ConvexHullInternal.GetConvexHull<TVertex, TFace>((IList<TVertex>)data, config);
        }

        /// <summary>
        /// Can only be created using a factory method.
        /// </summary>
        internal ConvexHull()
        {

        }
    }
}
