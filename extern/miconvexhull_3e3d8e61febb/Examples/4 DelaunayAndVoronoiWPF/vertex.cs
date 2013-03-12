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
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ExampleWithGraphics
{
    using MIConvexHull;
    using System.Windows.Media;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Vertex : Shape, IVertex
    {

        protected override Geometry DefiningGeometry
        {
            get
            {
                return new EllipseGeometry
                {
                    Center = new Point(Position[0], Position[1]),
                    RadiusX = 1.5,
                    RadiusY = 1.5
                };
            }
        }

        public Vertex(Brush fill = null)
        {
            Fill = fill ?? Brushes.Red;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        public Vertex(double x, double y, Brush fill = null)
            : this(fill)
        {
            Position = new double[] { x, y };
        }

        public Point ToPoint()
        {
            return new Point(Position[0], Position[1]);
        }

        /// <summary>
        /// Gets or sets the Z. Not used by MIConvexHull2D.
        /// </summary>
        /// <value>The Z position.</value>
        // private double Z { get; set; }

        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        public double[] Position { get; set; }
    }
}
