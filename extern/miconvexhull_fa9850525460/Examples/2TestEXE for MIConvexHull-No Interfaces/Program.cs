using System;
using System.Linq;
using MIConvexHull;
using System.Collections.Generic;

namespace TestEXE_for_MIConvexHull_No_Interfaces
{
    class Program
    {
        class VectorComparer : IComparer<double[]>
        {
            public int Compare(double[] x, double[] y)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var comp = x[i].CompareTo(y[i]);
                    if (comp != 0) return comp;
                }
                return 0;
            }
        }

        static void TestHypercube(int dim)
        {
            var hypercube = new double[1 << dim][];
            for (int i = 0; i < 1 << dim; i++)
            {
                hypercube[i] = Enumerable.Range(0, dim).Select(j => ((i >> j) & 1) == 1 ? 10.0 : 0.0).ToArray();
            }
            var hull = ConvexHull.Create(hypercube);
            foreach (var n in hull.Faces.Select(f => f.Normal).OrderBy(v => v, new VectorComparer()))
            {
                Console.WriteLine(string.Join(" ", n.Select(x => x.ToString("0.0"))));
            }
        }

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

            Console.WriteLine();
            Console.WriteLine("Push Return/Enter to test 5D hypercube hull and print its normals.");
            Console.ReadLine();
            TestHypercube(5);
        }
    }
}
