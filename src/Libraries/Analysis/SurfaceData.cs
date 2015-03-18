using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using DSCore;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured surface analysis data.
    /// </summary>
    public class SurfaceData : ISurfaceData<UV, double>, IGraphicItem
    {
        private Color[,] colorMap ;
        private const int COLOR_MAP_WIDTH = 100;
        private const int COLOR_MAP_HEIGHT = 100;

        /// <summary>
        /// The surface which contains the locations.
        /// </summary>
        public Surface Surface { get; set; }

        /// <summary>
        /// A list of UV locations on the surface.
        /// </summary>
        public IEnumerable<UV> ValueLocations { get; internal set; }

        /// <summary>
        /// A dictionary of lists of doubles.
        /// </summary>
        public IList<double> Values { get; internal set; }

        protected SurfaceData(
            Surface surface, IEnumerable<UV> valueLocations, IList<double> values)
        {
            Surface = surface;
            //CalculationLocations = CullCalculationLocations(surface, calculationLocations);
            ValueLocations = valueLocations;
            Values = values;

            colorMap = CreateColorMap();
        }

        /// <summary>
        /// Create a SurfaceData object without values.
        /// </summary>
        /// <param name="surface">The surface which contains the locations.</param>
        /// <param name="uvs">A list of UV locations on the surface.</param>
        /// <returns></returns>
        public static SurfaceData BySurfaceAndPoints(Surface surface, IEnumerable<UV> uvs)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (uvs == null)
            {
                throw new ArgumentNullException("uvs");
            }

            if (!uvs.Any())
            {
                throw new ArgumentException(AnalysisResources.EmptyUVsMessage);
            }

            return new SurfaceData(surface, uvs, null);
        }

        /// <summary>
        /// Create a SurfaceData object.
        /// </summary>
        /// <param name="surface">The surface which contains the locations.</param>
        /// <param name="uvs">A list of UV locations on the surface.</param>
        /// <param name="values">A list of double values.</param>
        public static SurfaceData BySurfacePointsAndValues(Surface surface, IEnumerable<UV> uvs, IList<double> values)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (uvs == null)
            {
                throw new ArgumentNullException("uvs");
            }

            if (!uvs.Any())
            {
                throw new ArgumentException(AnalysisResources.EmptyUVsMessage);
            }

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (!values.Any())
            {
                throw new ArgumentException("values", AnalysisResources.EmptyValuesMessage);
            }

            if (uvs.Count() != values.Count)
            {
                throw new ArgumentException(AnalysisResources.InputsNotEquivalentMessage);
            }

            return new SurfaceData(surface, uvs, values);
        }

        #region private methods

        /// <summary>
        /// Cull calculation locations that aren't within 1e-6 of the surface.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<UV> CullCalculationLocations(Surface surface, IEnumerable<UV> calculationLocations)
        {
            var pts = new List<UV>();

            foreach (var uv in calculationLocations)
            {
                var pt = surface.PointAtParameter(uv.U, uv.V);
                var dist = pt.DistanceTo(surface);
                if (dist < 1e-6 && dist > -1e-6)
                {
                    pts.Add(uv);
                }
            }

            return pts;
        }

        private Color[,] CreateColorMap()
        {
            if (Values == null) return null;

            // Find the minimum and the maximum for results
            var max = Values.Max();
            var min = Values.Min();

            var colorRange = Utils.CreateAnalyticalColorRange();

            // If the values are all the same, ex. max-min == 0.0,
            // then return the color at the top of the range. Otherwise,
            // return the color value at the specified parameter.
            var analysisColors = Values.Select(v => ((max-min) == 0.0 ? colorRange.GetColorAtParameter(1): colorRange.GetColorAtParameter((v - min) / (max - min)))).ToList();

            var colorRange2D = ColorRange2D.ByColorsAndParameters(analysisColors, ValueLocations.ToList());
            return colorRange2D.CreateColorMap(COLOR_MAP_WIDTH, COLOR_MAP_HEIGHT);
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            if (!Values.Any() || Values == null)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            // Use ASM's tesselation routine to tesselate
            // the surface. 
            //Surface.Tessellate(package, tol, maxGridLines);

            GridTesselate(Surface, package, 10, 10);

            DebugTime(sw, "Ellapsed for tessellation.");

            var colorCount = 0;
            var uvCount = 0;

            for (var i = 0; i < package.TriangleVertices.Count; i += 3)
            {
                var uvu = package.TriangleUVs[uvCount];
                var uvv = package.TriangleUVs[uvCount + 1];

                var uu = (int)(uvu * (COLOR_MAP_WIDTH - 1));
                var vv = (int)(uvv * (COLOR_MAP_HEIGHT - 1));
                var color = colorMap[uu,vv];

                package.TriangleVertexColors[colorCount] = color.Red;
                package.TriangleVertexColors[colorCount + 1] = color.Green;
                package.TriangleVertexColors[colorCount + 2] = color.Blue;
                package.TriangleVertexColors[colorCount + 3] = color.Alpha;

                colorCount += 4;
                uvCount += 2;
            }

            DebugTime(sw, "Ellapsed for setting colors on mesh.");
            sw.Stop();
        }

        private static void DebugTime(Stopwatch sw, string message)
        {
            sw.Stop();
            Debug.WriteLine("{0}:{1}", sw.Elapsed, message);
            sw.Reset();
            sw.Start();
        }

        private void GridTesselate(Surface surface, IRenderPackage package, int uDiv, int vDiv)
        {
            var points = new Point[uDiv+1, vDiv+1];
            var normals = new Vector[uDiv+1, vDiv+1];
            var uvs = new UV[uDiv+1, vDiv+1];

            // Lay down a grid of points
            for(var i=0; i <= uDiv; i+=1)
            {
                for (var j = 0; j <= vDiv; j += 1)
                {
                    var u = (double)i/uDiv;
                    var v = (double)j/vDiv;
                    var pt = Surface.PointAtParameter(u, v);
                    var uv = UV.ByCoordinates(u, v);
                    var n = Surface.NormalAtPoint(pt);

                    points[i, j] = pt;
                    normals[i, j] = n;
                    uvs[i, j] = uv;
                }
            }

            var simpleMesh = new SimpleMesh();

            for (var i = 0; i < uDiv; i += 1)
            {
                for (var j = 0; j < vDiv; j += 1)
                {
                    var a = points[i, j];
                    var b = points[i + 1, j];
                    var c = points[i + 1, j + 1];
                    var d = points[i, j + 1];

                    var an = normals[i, j];
                    var bn = normals[i + 1, j];
                    var cn = normals[i + 1, j + 1];
                    var dn = normals[i, j + 1];

                    var auv = uvs[i, j];
                    var buv = uvs[i + 1, j];
                    var cuv = uvs[i + 1, j + 1];
                    var duv = uvs[i, j + 1];

                    simpleMesh.AddTriangle(a, b, c, an, bn, cn, auv, buv, cuv);
                    simpleMesh.AddTriangle(a, c, d, an, cn, dn, auv, cuv, duv);
                }
            }

            // Cull triangles that aren't fully on the surface
            var badTris = simpleMesh.Triangles.Where(tri=>tri.DoesNotHaveAllPointsOnSurface(Surface)).ToArray();
            for (var i = badTris.Count() - 1; i >= 0; i--)
            {
                simpleMesh.RemoveTriangle(badTris[i]);
            }

            foreach (var tri in simpleMesh.Triangles)
            {
                foreach (var v in tri.Vertices)
                {
                    package.PushTriangleVertex(v.Point.X, v.Point.Y, v.Point.Z);
                    package.PushTriangleVertexNormal(v.Normal.X, v.Normal.Y, v.Normal.Z);
                    package.PushTriangleVertexUV(v.UV.U, v.UV.V);
                    package.PushTriangleVertexColor(0, 0, 0, 255);
                }
            }
            foreach (var edge in simpleMesh.Edges)
            {
                var a = edge.Start.Point;
                var b = edge.End.Point;
                package.PushLineStripVertex(a.X, a.Y, a.Z);
                package.PushLineStripVertex(b.X, b.Y, b.Z);

                if (edge.IsOpenEdge)
                {
                    package.PushLineStripVertexColor(255, 0, 0, 255);
                    package.PushLineStripVertexColor(255, 0, 0, 255);
                }
                else
                {
                    package.PushLineStripVertexColor(100, 100, 100, 255);
                    package.PushLineStripVertexColor(100, 100, 100, 255);
                }
                
                package.PushLineStripVertexCount(2);
            }
        }
    }

    public class SimpleMesh
    {
        public List<Triangle> Triangles { get; private set; }
        public List<Vertex> Vertices { get; private set; }
        public List<Edge> Edges { get; private set; }

        public SimpleMesh()
        {
            Triangles = new List<Triangle>();    
            Vertices = new List<Vertex>();
            Edges = new List<Edge>();
        }

        public void AddTriangle(Point ptA, Point ptB, Point ptC, 
            Vector nA, Vector nB, Vector nC, 
            UV uvA, UV uvB, UV uvC)
        {
            // Create vertices
            var a = FindOrCreateVertex(ptA, nA, uvA);
            var b = FindOrCreateVertex(ptB, nB, uvB);
            var c = FindOrCreateVertex(ptC, nC, uvC);

            // Create the triangle
            var tri = new Triangle(a, b, c);
            Triangles.Add(tri);

            // Create edges
            var ea = FindOrCreateEdge(a, b);
            var eb = FindOrCreateEdge(b, c);
            var ec = FindOrCreateEdge(c, a);

            // Link edges -> triangle
            ea.Triangles.Add(tri);
            eb.Triangles.Add(tri);
            ec.Triangles.Add(tri);

            // Link triangle -> edges
            tri.Edges.Add(ea);
            tri.Edges.Add(eb);
            tri.Edges.Add(ec);
        }

        public void RemoveTriangle(Triangle triangle)
        {
            // Unlink from edges and delete orphans
            foreach (var edge in triangle.Edges)
            {
                edge.Triangles.Remove(triangle);
                if (!edge.Triangles.Any())
                {
                    Edges.Remove(edge);
                }
            }

            // Unlink from vertices and delete orphans
            foreach (var vert in triangle.Vertices)
            {
                vert.Triangles.Remove(triangle);
                if (!vert.Triangles.Any())
                {
                    Vertices.Remove(vert);
                }
            }

            Triangles.Remove(triangle);
        }

        private Edge FindOrCreateEdge(Vertex a, Vertex b)
        {
            var foundEdge =
                Edges.FirstOrDefault(e => e.Start == a && e.End == b || e.Start == b && e.End == a);

            if (foundEdge != null) return foundEdge;

            foundEdge = new Edge(a, b);
            Edges.Add(foundEdge);

            return foundEdge;
        }

        private Vertex FindOrCreateVertex(Point pt, Vector normal, UV uv)
        {
            var foundVertex = Vertices.FirstOrDefault(v => v.Point.IsAlmostEqualTo(pt));
            if (foundVertex != null) return foundVertex;

            foundVertex = new Vertex(pt, normal, uv);
            Vertices.Add(foundVertex);

            return foundVertex;
        }

        public class Vertex
        {
            public Point Point { get; private set; }
            public Vector Normal { get; private set; }
            public UV UV { get; private set; }

            public List<Triangle> Triangles { get; private set; }
            public List<Edge> Edges { get; private set; }
 
            public Vertex(Point point, Vector normal, UV uv)
            {
                Triangles = new List<Triangle>();
                Edges = new List<Edge>();

                Point = point;
                Normal = normal;
                UV = uv;
            }
        }

        public class Edge
        {
            public List<Triangle> Triangles { get; private set; }
            public Vertex Start { get; private set; }
            public Vertex End { get; private set; }

            public bool IsOpenEdge
            {
                get { return Triangles.Count() == 1; }
            }

            public Edge(Vertex start, Vertex end)
            {
                Triangles = new List<Triangle>();
                
                Start = start;
                End = end;

                // Link vertices -> edge
                start.Edges.Add(this);
                end.Edges.Add(this);
            }
        }

        public class Triangle
        {
            public List<Vertex> Vertices { get; private set; }
            public List<Edge> Edges { get; private set; }

            public Triangle(Vertex a, Vertex b, Vertex c)
            {
                Vertices = new List<Vertex>();
                Edges = new List<Edge>();

                Vertices.Add(a);
                Vertices.Add(b);
                Vertices.Add(c);

                a.Triangles.Add(this);
                b.Triangles.Add(this);
                c.Triangles.Add(this);
            }
        }
    }

    public static class SimpleMeshExtensions
    {
        public static bool DoesNotHaveAllPointsOnSurface(this SimpleMesh.Triangle triangle, Surface surf)
        {
            return triangle.Vertices.All(v => !v.Point.DoesIntersect(surf));
        }
    }
}
