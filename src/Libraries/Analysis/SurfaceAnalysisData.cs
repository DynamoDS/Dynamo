using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

using DSCore;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured surface analysis data.
    /// </summary>
    public class SurfaceAnalysisData : ISurfaceAnalysisData<UV, double>, IGraphicItem
    {
        private int valueIndex;
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
        public IEnumerable<UV> CalculationLocations { get; set; }

        /// <summary>
        /// A dictionary of lists of doubles.
        /// </summary>
        public Dictionary<string, IList<double>> Values { get; set; }

        /// <summary>
        /// Get the collection of values corresponding to a key.
        /// </summary>
        /// <param name="key">The name of the value set.</param>
        /// <returns>A list of doubles.</returns>
        public IList<double> GetValuesByKey(string key)
        {
            if (!Values.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Values[key];
        }

        protected SurfaceAnalysisData(
            Surface surface, IEnumerable<UV> calculationLocations, Dictionary<string, IList<double>> values, int valueIndex)
        {
            Surface = surface;
            //CalculationLocations = CullCalculationLocations(surface, calculationLocations);
            CalculationLocations = calculationLocations;
            Values = values;

            this.valueIndex = valueIndex;
            colorMap = CreateColorMap();
            
        }

        /// <summary>
        /// Create a SurfaceAnalysisData object.
        /// </summary>
        /// <param name="surface">The surface which contains the locations.</param>
        /// <param name="uvs">A list of UV locations on the surface.</param>
        public static SurfaceAnalysisData BySurfaceAndPoints(Surface surface, IEnumerable<UV> uvs)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (uvs == null)
            {
                throw new ArgumentNullException("points");
            }

            if (!uvs.Any())
            {
                throw new ArgumentException("The specified points list does not contain any points.");    
            }

            return new SurfaceAnalysisData(
                surface,
                uvs,
                new Dictionary<string, IList<double>>(), 0);
        }

        /// <summary>
        /// Create a SurfaceAnalysisData object.
        /// </summary>
        /// <param name="surface">The surface which contains the locations.</param>
        /// <param name="uvs">A list of UV locations on the surface.</param>
        /// <param name="names">A list of value set names.</param>
        /// <param name="values">A list of lists of double values.</param>
        /// <param name="valueIndex">The index of the value set to visualize.</param>
        public static SurfaceAnalysisData BySurfacePointsAndValues(Surface surface, IEnumerable<UV> uvs, IList<string> names, IList<IList<double>> values, int valueIndex = 0)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }

            if (uvs == null)
            {
                throw new ArgumentNullException("points");
            }

            if (!uvs.Any())
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

            if (valueIndex > values.Count() - 1 || valueIndex < 0)
            {
                throw new ArgumentException("You must specify a results index no larger than the number of available results.");
            }

            var results = new Dictionary<string, IList<double>>();
            for (var i = 0; i < names.Count; i++)
            {
                results.Add(names[i], values[i]);
            }

            return new SurfaceAnalysisData(surface, uvs, results, valueIndex);
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
            var values = Values.ElementAt(valueIndex).Value;
            var max = values.Max();
            var min = values.Min();

            var colorRange = Utils.CreateAnalyticalColorRange();

            var analysisColors = values.Select(v => colorRange.GetColorAtParameter((v - min) / (max - min))).ToList();
            var colorRange2D = ColorRange2D.ByColorsAndParameters(analysisColors, CalculationLocations.ToList());
            return colorRange2D.CreateColorMap(COLOR_MAP_WIDTH, COLOR_MAP_HEIGHT);
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            if (!Values.Any() || valueIndex > Values.Count - 1)
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
}
