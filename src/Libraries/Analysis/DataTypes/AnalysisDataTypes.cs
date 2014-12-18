using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using DSCore;

namespace Analysis.DataTypes
{
    /// <summary>
    /// A class for storing structured surface analysis data.
    /// </summary>
    public class SurfaceAnalysisData : ISurfaceAnalysisData<UV, double>, IGraphicItem
    {
        private int resultIndex;
        private Color[,] colorMap ;
        private const int COLOR_MAP_WIDTH = 100;
        private const int COLOR_MAP_HEIGHT = 100;

        /// <summary>
        /// The surface which contains the calculation locations.
        /// </summary>
        public Surface Surface { get; set; }

        /// <summary>
        /// A list of calculation locations.
        /// </summary>
        public IEnumerable<UV> CalculationLocations { get; set; }

        /// <summary>
        /// A dictionary of results.
        /// </summary>
        public Dictionary<string, IList<double>> Results { get; set; }

        public IList<double> GetResultByKey(string key)
        {
            if (!Results.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Results[key];
        }

        protected SurfaceAnalysisData(
            Surface surface, IEnumerable<UV> calculationLocations, Dictionary<string,IList<double>> results, int resultIndex)
        {
            Surface = surface;
            //CalculationLocations = CullCalculationLocations(surface, calculationLocations);
            CalculationLocations = calculationLocations;
            Results = results;

            this.resultIndex = resultIndex;
            colorMap = CreateColorMap();
            
        }

        /// <summary>
        /// Create a SurfaceAnalysisData object.
        /// </summary>
        /// <param name="surface">The surface which contains the calculation locations.</param>
        /// <param name="points">A list of UV calculation locations on the surface.</param>
        public static SurfaceAnalysisData BySurfaceAndPoints(Surface surface, IEnumerable<UV> points)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (!points.Any())
            {
                throw new ArgumentException("The specified points list does not contain any points.");    
            }

            return new SurfaceAnalysisData(
                surface,
                points,
                new Dictionary<string, IList<double>>(), 0);
        }

        /// <summary>
        /// Create a SurfaceAnalysisData object.
        /// </summary>
        /// <param name="surface">The surface which contains the calculation locations.</param>
        /// <param name="points">A list of UV calculation locations on the surface.</param>
        /// <param name="names">A list of result names.</param>
        /// <param name="values">A list of lists of result values.</param>
        /// <param name="resultIndex">The index of the results to visualize.</param>
        public static SurfaceAnalysisData BySurfacePointsAndResults(Surface surface, IEnumerable<UV> points, IList<string> names, IList<IList<double>> values, int resultIndex = 0)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (!points.Any())
            {
                throw new ArgumentException("The specified points list does not contain any points.");
            }

            if (names == null)
            {
                throw new ArgumentNullException("resultNames");
            }

            if (values == null)
            {
                throw new ArgumentNullException("resultValues");
            }

            if (names.Count != values.Count)
            {
                throw new ArgumentException("The number of result names and result values must match.");
            }

            if (resultIndex > values.Count() - 1 || resultIndex < 0)
            {
                throw new ArgumentException("You must specify a results index no larger than the number of available results.");
            }

            var results = new Dictionary<string, IList<double>>();
            for (var i = 0; i < names.Count; i++)
            {
                results.Add(names[i], values[i]);
            }

            return new SurfaceAnalysisData(surface, points, results, resultIndex);
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
            // Find the minimum and the maximum for results
            var values = Results.ElementAt(resultIndex).Value;
            var max = values.Max();
            var min = values.Min();

            // Build some colors for a range
            var c1 = Color.ByARGB(255, 255, 0, 0);
            var c2 = Color.ByARGB(255, 255, 255, 0);
            var c3 = Color.ByARGB(255, 0, 0, 255);
            var colors = new[] { c1, c2, c3 };
            var parameters = new[] { 0.0, 0.5, 1.0 };

            var colorRange = ColorRange1D.ByColorsAndParameters(colors, parameters);
            var analysisColors = values.Select(v => colorRange.GetColorAtParameter((v - min) / (max - min))).ToList();
            var colorRange2D = ColorRange2D.ByColorsAndParameters(analysisColors, CalculationLocations.ToList());
            return colorRange2D.CreateColorMap(COLOR_MAP_WIDTH, COLOR_MAP_HEIGHT);
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            if (!Results.Any())
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            // Use ASM's tesselation routine to tesselate
            // the surface. 
            Surface.Tessellate(package, tol, maxGridLines);

            DebugTime(sw, "Ellapsed for tessellation.");

            int colorCount = 0;

            for (int i = 0; i < package.TriangleVertices.Count; i += 3)
            {
                var vx = package.TriangleVertices[i];
                var vy = package.TriangleVertices[i + 1];
                var vz = package.TriangleVertices[i + 2];

                // Get the triangle vertex
                var v = Point.ByCoordinates(vx, vy, vz);
                var uv = Surface.UVParameterAtPoint(v);

                var uu = (int)(uv.U*(COLOR_MAP_WIDTH-1));
                var vv = (int)(uv.V*(COLOR_MAP_HEIGHT-1));
                var color = colorMap[uu,vv];

                package.TriangleVertexColors[colorCount] = color.Red;
                package.TriangleVertexColors[colorCount + 1] = color.Green;
                package.TriangleVertexColors[colorCount + 2] = color.Blue;
                package.TriangleVertexColors[colorCount + 3] = color.Alpha;

                colorCount += 4;
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
    }

    /// <summary>
    /// A class for storing structured vector analysis data.
    /// </summary>
    public class VectorAnalysisData : IAnalysisData<Point, Vector>, IGraphicItem
    {
        private int resultIndex;

        /// <summary>
        /// A list of calculation locations.
        /// </summary>
        public IEnumerable<Point> CalculationLocations { get; internal set; }

        /// <summary>
        /// A dictionary of results.
        /// </summary>
        public Dictionary<string, IList<Vector>> Results { get; internal set; }

        protected VectorAnalysisData(IEnumerable<Point> points, Dictionary<string,IList<Vector>> results, int resultIndex)
        {
            CalculationLocations = points;
            Results = results;

            this.resultIndex = resultIndex;
        }

        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        public static VectorAnalysisData ByPoints(IEnumerable<Point> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            var results = new Dictionary<string, IList<Vector>>();
            return new VectorAnalysisData(points, results, 0);
        }
 
        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        /// <param name="resultNames">A list of result names.</param>
        /// <param name="resultValues">A list of lists of result values.</param>
        public static VectorAnalysisData ByPointsAndResults(
            IEnumerable<Point> points, IList<string> resultNames, IList<IList<Vector>> resultValues, int resultIndex = 0)
        {

            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (resultNames == null)
            {
                throw new ArgumentNullException("resultNames");
            }

            if (resultValues == null)
            {
                throw new ArgumentNullException("resultValues");
            }

            if (resultNames.Count != resultValues.Count)
            {
                throw new ArgumentException("The number of result names and result values must match.");
            }

            var results = new Dictionary<string, IList<Vector>>();
            for (var i = 0; i < resultNames.Count; i++)
            {
                results.Add(resultNames[i], resultValues[i]);
            }

            return new VectorAnalysisData(points, results, resultIndex);
        }

        public IList<Vector> GetResultByKey(string key)
        {
            if (!Results.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Results[key];
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            // Create vectors 50% gray scaled at the point.
            if (!Results.Any())
            {
                return;
            }

            var data = Results.ElementAt(resultIndex).Value.Zip(CalculationLocations, (v,p)=>new Tuple<Vector,Point>(v,p));

            foreach (var d in data)
            {
                DrawVector(d, package);
            }
        }

        private void DrawVector(Tuple<Vector, Point> data, IRenderPackage package)
        {
            var p = data.Item2;
            var v = data.Item1;

            package.PushLineStripVertex(p.X,p.Y,p.Z);
            package.PushLineStripVertexColor(120,120,120,255);
            
            var o = p.Add(v);

            package.PushLineStripVertex(o.X, o.Y, o.Z);
            package.PushLineStripVertexColor(120, 120, 120, 255);

            package.PushLineStripVertexCount(2);
        }
    }

    /// <summary>
    /// A class for storing structure point analysis data.
    /// </summary>
    public class PointAnalysisData : IAnalysisData<Point, double>
    {
        /// <summary>
        /// A list of calculation locations.
        /// </summary>
        public IEnumerable<Point> CalculationLocations { get; internal set; }

        /// <summary>
        /// A dictionary of results.
        /// </summary>
        public Dictionary<string, IList<double>> Results { get; internal set; }

        protected PointAnalysisData(
            IEnumerable<Point> points, Dictionary<string, IList<double>> results)
        {
            CalculationLocations = points;
            Results = results;
        }

        /// <summary>
        /// Create a PointAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        public static PointAnalysisData ByPoints(IEnumerable<Point> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            var results = new Dictionary<string, IList<double>>();
            return new PointAnalysisData(points, results);
        }

        /// <summary>
        /// Create a PointAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        /// <param name="results">A set of resutls keyed by the name of the result type.</param>
        public static PointAnalysisData ByPointsAndResults(IEnumerable<Point> points, IList<string> resultNames, IList<IList<double>> resultValues)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (resultNames == null)
            {
                throw new ArgumentNullException("resultNames");
            }

            if (resultValues == null)
            {
                throw new ArgumentNullException("resultValues");
            }

            if (resultNames.Count != resultValues.Count)
            {
                throw new ArgumentException("The number of result names and result values must match.");
            }

            var results = new Dictionary<string, IList<double>>();
            for (var i = 0; i < resultNames.Count; i++)
            {
                results.Add(resultNames[i], resultValues[i]);
            }

            return new PointAnalysisData(points, results);
        }

        public IList<double> GetResultByKey(string key)
        {
            if (!Results.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Results[key];
        }

    }
}
