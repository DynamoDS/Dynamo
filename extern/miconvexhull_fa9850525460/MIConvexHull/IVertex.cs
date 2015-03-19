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
    /// An interface for a structure with nD position.
    /// </summary>
    public interface IVertex
    {
        /// <summary>
        /// Position of the vertex.
        /// </summary>
        double[] Position { get; }
    }

    /// <summary>
    /// "Default" vertex.
    /// </summary>
    public class DefaultVertex : IVertex
    {
        /// <summary>
        /// Position of the vertex.
        /// </summary>
        public double[] Position { get; set; }
    }

}
