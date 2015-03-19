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
namespace ExampleWithGraphics
{
    using MIConvexHull;
    using Petzold.Media3D;
    using System.Windows.Media.Media3D;
    using System.Windows.Media;
    /// <summary>
    /// A vertex is a simple class that stores the postion of a point, node or vertex.
    /// </summary>
    public class Vertex : ModelVisual3D, IVertex
    {
        static readonly Material material = new DiffuseMaterial(Brushes.Black);
        static readonly SphereMesh mesh = new SphereMesh { Slices = 6, Stacks = 4, Radius = 0.5 };

        static readonly Material hullMaterial = new DiffuseMaterial(Brushes.Yellow);
        static readonly SphereMesh hullMesh = new SphereMesh { Slices = 6, Stacks = 4, Radius = 1.0 };

        /// <summary>
        /// Initializes a new instance of the <see cref="Vertex"/> class.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        /// <param name="isHull"></param>
        public Vertex(double x, double y, double z, bool isHull = false)
        {
            Content = new GeometryModel3D
            {
                Geometry = isHull ? hullMesh.Geometry : mesh.Geometry,
                Material = isHull ? hullMaterial : material,
                Transform = new TranslateTransform3D(x, y, z)
            };
            Position = new double[] { x, y, z };
        }

        public Vertex AsHullVertex()
        {
            return new Vertex(Position[0], Position[1], Position[2], true);
        }

        public Point3D Center { get { return new Point3D(Position[0], Position[1], Position[2]); } }
        
        /// <summary>
        /// Gets or sets the coordinates.
        /// </summary>
        /// <value>The coordinates.</value>
        public double[] Position
        {
            get;
            set;
        }
    }
}
