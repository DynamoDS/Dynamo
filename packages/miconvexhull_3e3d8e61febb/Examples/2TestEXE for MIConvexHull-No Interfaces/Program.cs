using System;
using System.Linq;
using MIConvexHull;

namespace TestEXE_for_MIConvexHull_No_Interfaces
{
    class Program
    {
        static void Main()
        {            
            const int NumberOfVertices = 30000;
            const double size = 1000;
            const int dimension = 3;

            var r = new Random();
            Console.WriteLine("Ready? Push Return/Enter to start.");
            Console.ReadLine();

            Console.WriteLine("Making " + NumberOfVertices + " random vertices.");
            /* our inputs are simply in the form of an array of double arrays */
            var vertices = new double[NumberOfVertices][];
            for (var i = 0; i < NumberOfVertices; i++)
            {
                var location = new double[dimension];
                for (var j = 0; j < dimension; j++)
                    location[j] = size * r.NextDouble();
                vertices[i] = location;
            }
            Console.WriteLine("Running...");
            var now = DateTime.Now;
            var convexHull = ConvexHull.Create(vertices);

            double[][] hullPoints = convexHull.Points.Select(p => p.Position).ToArray();

            var interval = DateTime.Now - now;
            Console.WriteLine("Out of the {0} 2D vertices, there are {1} on the convex hull.", NumberOfVertices, hullPoints.Length);
            Console.WriteLine("time = " + interval);
            Console.ReadLine();
        }
    }
}
