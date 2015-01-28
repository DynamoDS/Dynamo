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
            Surface.Tessellate(package, tol, maxGridLines);

            DebugTime(sw, "Ellapsed for tessellation.");

            int colorCount = 0;
            int uvCount = 0;

            for (int i = 0; i < package.TriangleVertices.Count; i += 3)
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
    }
}
