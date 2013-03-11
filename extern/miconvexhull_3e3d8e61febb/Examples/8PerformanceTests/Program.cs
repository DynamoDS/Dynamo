using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIConvexHull;
using System.IO;

namespace _8PerformanceTests
{
    class Program
    {
        static Random rnd = new Random();
        static DefaultVertex[] CreateRandomVertices(int dim, int numVert, double size)
        {
            var vertices = new DefaultVertex[numVert];
            for (var i = 0; i < numVert; i++)
            {
                double[] v = new double[dim];
                for (int j = 0; j < dim; j++) v[j] = size * rnd.NextDouble();
                vertices[i] = new DefaultVertex { Position = v };
            }
            ////File.WriteAllLines("i:/test/7Ddata.txt",
            ////    new string[] { "7 test", numVert.ToString() }
            ////    .Concat(vertices.Select(v => string.Join(" ", v.Position.Select(p => p.ToString()))))
            ////    .ToArray()
            ////    );
            return vertices;
        }

        static TimeSpan RunComputation<T>(Func<T> a)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var now = DateTime.Now;
            a();
            var interval = DateTime.Now - now;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return interval;
        }

        static TimeSpan TestNDConvexHull(int dim, int numVert, double size)
        {
            var vertices = CreateRandomVertices(dim, numVert, size);
            return RunComputation(() => 
                {
                    var h = ConvexHull.Create(vertices);
                   // Console.WriteLine(h.Points.Count());
                    return h;
                });
        }

        static TimeSpan TestNDDelau(int dim, int numVert, double size)
        {
            var vertices = CreateRandomVertices(dim, numVert, size);
            return RunComputation(() => 
            {
                var r = Triangulation.CreateDelaunay(vertices);
                Console.WriteLine(r.Cells.Count());
                return r;
            });
        }

        static TimeSpan TestNDVoronoi(int dim, int numVert, double size)
        {
            var vertices = CreateRandomVertices(dim, numVert, size);
            return RunComputation(() => VoronoiMesh.Create(vertices));
        }

        static void DoTest(int[] counts, int minD, int maxD, int nRuns, Func<int, int, double, TimeSpan> testFunc, string outputFile)
        {
            testFunc(2, 10, 10);
            var times = counts
                .Select(nV => new
                {
                    NumVertices = nV,
                    Times = Enumerable.Range(minD, maxD - minD + 1).Select(dim => new
                    {
                        Dimension = dim,
                        AvgTime =
                            Enumerable.Range(0, nRuns)
                            .Select(_ => testFunc(dim, nV, 1000))
                            .Average(t => t.TotalSeconds)
                    })
                });

            MemoryStream dataStream = new MemoryStream();
            var w = new StreamWriter(dataStream);

            Console.Write("Dim:\t");
            w.Write("Dimension/Number of vertices");
            for (int i = minD; i <= maxD; i++)
            {
                Console.Write("{0}\t", i);
                w.Write(",{0}", i);
            }
            Console.Write(Environment.NewLine);
            w.Write(Environment.NewLine);
            foreach (var r in times)
            {
                Console.Write(r.NumVertices.ToString() + "\t");
                w.Write(r.NumVertices);
                foreach (var t in r.Times)
                {
                    Console.Write("{0:0.000}s\t", t.AvgTime);
                    w.Write(",{0:0.000}", t.AvgTime);
                }
                Console.Write(Environment.NewLine);
                w.Write(Environment.NewLine);
            }

            w.Flush();

            using (var f = File.Open(outputFile, FileMode.Create))
            {
                dataStream.Seek(0, SeekOrigin.Begin);
                dataStream.WriteTo(f);
            }

            w.Dispose();
            dataStream.Dispose();
        }

        static void TestConvexHull()
        {
            var counts = new int[] { 100, 250, 500 };
            const int minDimension = 2;
            const int maxDimension = 8;
            const int nRuns = 1;

            Console.WriteLine("Convex Hull Test:");
            DoTest(counts, minDimension, maxDimension, nRuns, TestNDConvexHull, "convex.csv");
            Console.WriteLine("-----------------------------");
        }

        static void Test3DConvexHull()
        {
            var counts = new int[] { 1000000 };
            const int minDimension = 3;
            const int maxDimension = 3;
            const int nRuns = 3;

            Console.WriteLine("Convex Hull Test:");
            DoTest(counts, minDimension, maxDimension, nRuns, TestNDConvexHull, "convex3d.csv");
            Console.WriteLine("-----------------------------");
        }

        static void TestDelaunay()
        {
            var counts = new int[] { 350 };
            const int minDimension = 7;
            const int maxDimension = 7;
            const int nRuns = 1;
            
            Console.WriteLine("Delaunay Triangulation Test:");
            DoTest(counts, minDimension, maxDimension, nRuns, TestNDDelau, "delaunay6d.csv");
            Console.WriteLine("-----------------------------");
        }

        static void Test3DDelaunay()
        {
            var counts = new int[] { 100000 };
            const int minDimension = 3;
            const int maxDimension = 3;
            const int nRuns = 1;

            Console.WriteLine("Delaunay Triangulation Test:");
            DoTest(counts, minDimension, maxDimension, nRuns, TestNDDelau, "delaunay3d.csv");
            Console.WriteLine("-----------------------------");
        }
        
        static void TestVoronoi()
        {
            var counts = new int[] { 100, 1000, 10000, 100000 };
            const int minDimension = 2;
            const int maxDimension = 6;
            const int nRuns = 3;

            Console.WriteLine("Voronoi Test:");
            DoTest(counts, minDimension, maxDimension, nRuns, TestNDVoronoi, "voronoi.csv");
            Console.WriteLine("-----------------------------");
        }

        static void Main(string[] args)        
        {
            TestConvexHull();
            //TestDelaunay();
            //TestConvexHull();
            //TestVoronoi();
        }
    }
}
