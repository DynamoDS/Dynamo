/*************************************************************************
 *     This file & class is part of the MIConvexHull Library Project. 
 *     Copyright 2010 Matthew Ira Campbell, PhD.
 *
 *     MIConvexHull is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *  
 *     MIConvexHull is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *  
 *     You should have received a copy of the GNU General Public License
 *     along with MIConvexHull.  If not, see <http://www.gnu.org/licenses/>.
 *     
 *     Please find further details and contact information on GraphSynth
 *     at http://miconvexhull.codeplex.com
 *************************************************************************/
namespace TestEXE_for_MIConvexHull_Voronoi
{
    using MIConvexHull;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Vertex : IVertex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public Vertex(double[] location)
        {
            Position = location;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// **** You must have a constructor that takes 0 arguments for 
        /// **** both the IVertexConvHull and IFaceConvHull inherited
        /// **** classes! ******
        /// </summary>
        public Vertex()
        {
        }


        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        public double[] Position { get; set; }
    }
}
