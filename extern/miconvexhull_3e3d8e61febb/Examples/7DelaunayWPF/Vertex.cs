using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIConvexHull;
using System.Windows.Media.Media3D;

namespace DelaunayWPF
{
    /// <summary>
    /// Represents a point in 3D space.
    /// </summary>
    class Vertex : IVertex
    {
        public Vertex(double x, double y, double z)
        {
            Position = new double[] { x, y, z };
        }

        public Point3D ToPoint3D() 
        {
            return new Point3D(Position[0], Position[1], Position[2]); 
        }

        public double[] Position { get; set; }
    }
}
