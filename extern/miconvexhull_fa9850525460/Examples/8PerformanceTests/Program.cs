using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MIConvexHull;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace _8PerformanceTests
{
    class Program
    {
        static Random rnd = new Random(0);
        static DefaultVertex[] CreateRandomVertices(int dim, int numVert, double size)
        {
            var vertices = new DefaultVertex[numVert];
            for (var i = 0; i < numVert; i++)
            {
                double[] v = new double[dim];
                for (int j = 0; j < dim; j++) v[j] = size * rnd.NextDouble();
                vertices[i] = new DefaultVertex { Position = v };
            }
            return vertices;
        }

        static TimeSpan RunComputation<T>(Func<T> a)
        {
            GC.Collect();
            var sw = Stopwatch.StartNew();
            a();
            sw.Stop();
            GC.Collect();
            return sw.Elapsed;
        }

        static TimeSpan TestNDConvexHull(int dim, int numVert, double size)
        {
            var vertices = CreateRandomVertices(dim, numVert, size);
            return RunComputation(() => ConvexHull.Create(vertices));
        }

        static TimeSpan TestNDDelau(int dim, int numVert, double size)
        {
            var vertices = CreateRandomVertices(dim, numVert, size);
            return RunComputation(() => Triangulation.CreateDelaunay(vertices));
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
            //Console.ReadLine();
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
            var counts = new int[] { 1000, 10000, 100000, 1000000/*, 10000000*/ };
            const int minDimension = 3;
            const int maxDimension = 3;
            const int nRuns = 3;

            Console.WriteLine("Convex Hull Test:");
            DoTest(counts, minDimension, maxDimension, nRuns, TestNDConvexHull, "convex3d.csv");
            Console.WriteLine("-----------------------------");
        }

        static void TestDelaunay()
        {
            var counts = new int[] { 100, 250, 500 };
            const int minDimension = 4;
            const int maxDimension = 6;
            const int nRuns = 1;
            
            Console.WriteLine("Delaunay Triangulation Test:");
            DoTest(counts, minDimension, maxDimension, nRuns, TestNDDelau, "delaunay6d.csv");
            Console.WriteLine("-----------------------------");
        }

        static void Test3DDelaunay()
        {
            var counts = new int[] { 1000, 10000, 25000, 50000 };
            const int minDimension = 3;
            const int maxDimension = 3;
            const int nRuns = 3;

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
            //TestConvexHull();
            Test3DDelaunay();
            //Test3DConvexHull();
            //TestVoronoi();
        }
    }
}
