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

/*
 * Why is this included?! The idea was in path-planning where you have 
 * to maneuver in full 3-D space, and you have 6 variables: x, y, z, and rotations
 * about x, y, and z. The resulting graph could be used by a path-planning search process.
 */
namespace TestEXE_for_MIConvexHull_Voronoi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MIConvexHull;

    static class Program
    {
        static void Main()
        {
            const int NumberOfVertices = 100;
            const double size = 1000;
            const int dimension = 6;

            var r = new Random();
            Console.WriteLine("Ready? Push Return/Enter to start.");
            Console.ReadLine();

            Console.WriteLine("Making " + NumberOfVertices + " random 6D vertices.");
            var vertices = new List<Vertex>();
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var location = new double[dimension];
                for (var j = 0; j < dimension; j++)
                    location[j] = size * r.NextDouble();
                vertices.Add(new Vertex(location));
            }
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            var voronoi = VoronoiMesh.Create(vertices);
            var interval = DateTime.Now - now;
            Console.WriteLine("Out of the {0} 6D vertices, there are {1} Voronoi cells and {2} edges.",
                NumberOfVertices, voronoi.Vertices.Count(), voronoi.Edges.Count());
            Console.WriteLine("time = " + interval);
            Console.ReadLine();
        }
    }
}
