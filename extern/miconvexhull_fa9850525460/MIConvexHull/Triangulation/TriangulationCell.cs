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
    /// <summary>
    /// Representation of the triangulation cell. Pretty much the same as ConvexFace,
    /// just wanted to distinguish the two.
    /// To declare your own face type, use class Face : DelaunayFace(of Vertex, of Face)
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    /// <typeparam name="TCell"></typeparam>
    public abstract class TriangulationCell<TVertex, TCell> : ConvexFace<TVertex, TCell>
        where TVertex : IVertex
        where TCell : ConvexFace<TVertex, TCell>
    {

    }

    /// <summary>
    /// Default triangulation cell.
    /// </summary>
    /// <typeparam name="TVertex"></typeparam>
    public class DefaultTriangulationCell<TVertex> : TriangulationCell<TVertex, DefaultTriangulationCell<TVertex>>
        where TVertex : IVertex
    {
    }
}
