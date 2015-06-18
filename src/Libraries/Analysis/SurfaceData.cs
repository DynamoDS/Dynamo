using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DSCore;
using MIConvexHull;
using Tessellation.Adapters;
using Math = System.Math;

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
            package.RequiresPerVertexColoration = true;

            if (!Values.Any() || Values == null)
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();

            DelaunayTesselate(Surface, package, 30, 30, ValueLocations);

            DebugTime(sw, "Ellapsed for tessellation.");

            var colorCount = 0;
            var uvCount = 0;

            var uvs = package.MeshTextureCoordinates.ToList();
            var colors = package.MeshVertexColors.ToList();

            var newColors = new byte[colors.Count];

            for (var i = 0; i < colors.Count; i += 4)
            {
                var uvu = uvs[uvCount];
                var uvv = uvs[uvCount + 1];

                var uu = (int)(uvu * (COLOR_MAP_WIDTH - 1));
                var vv = (int)(uvv * (COLOR_MAP_HEIGHT - 1));
                var color = colorMap[uu,vv];

                newColors[colorCount] = color.Red;
                newColors[colorCount + 1] = color.Green;
                newColors[colorCount + 2] = color.Blue;
                newColors[colorCount + 3] = color.Alpha;

                colorCount += 4;
                uvCount += 2;
            }

            package.ApplyMeshVertexColors(newColors);

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

        /// <summary>
        /// This method uses the MIConvexHull Delaunay triangulator to create a 
        /// Delaunay tessellation of a given u and v distribution on the surface.
        /// It adds to this tessellation all the points on the surface's trim
        /// curves. Triangles are then deleted from the tessellation based on whether
        /// the sum of their vertices' distances from the surface are greater than a
        /// threshold.
        /// </summary>
        /// <param name="surface">The surface on which to apply the tessellation.</param>
        /// <param name="package">The IRenderPackage object into which the graphics data will be pushed.</param>
        /// <param name="uDiv">The number of divisions of the grid on the surface in the U direction.</param>
        /// <param name="vDiv">The number of divisions of the grid on the surface in the V direction.</param>
        private static void DelaunayTesselate(Surface surface, IRenderPackage package, int uDiv, int vDiv, IEnumerable<UV> additionalUVs)
        {
            var uvs = new List<UV>();
            for (var i = 0; i <= uDiv; i += 1)
            {
                for (var j = 0; j <= vDiv; j += 1)
                {
                    var u = (double)i / uDiv;
                    var v = (double)j / vDiv;
                    var uv = UV.ByCoordinates(u, v);
                    uvs.Add(uv);
                }
            }

            uvs.AddRange(additionalUVs);

            var curves = surface.PerimeterCurves();
            var coords = GetEdgeCoordinates(curves, 100, surface);
            curves.ForEach(c=>c.Dispose());

            var verts = uvs.Select(Vertex2.FromUV).Concat(coords.Select(Vertex2.FromUV)).ToList();
            var config = new TriangulationComputationConfig
            {
                PointTranslationType = PointTranslationType.TranslateInternal,
                PlaneDistanceTolerance = 0.000001,
                // the translation radius should be lower than PlaneDistanceTolerance / 2
                PointTranslationGenerator = TriangulationComputationConfig.RandomShiftByRadius(0.0000001, 0)
            };

            var triangulation = DelaunayTriangulation<Vertex2, Cell2>.Create(verts, config);

            foreach (var cell in triangulation.Cells)
            {
                var v1 = cell.Vertices[0].AsVector();
                var pt1 = surface.PointAtParameter(v1.X, v1.Y);
                var n1 = surface.NormalAtParameter(v1.X, v1.Y);

                var v2 = cell.Vertices[1].AsVector();
                var pt2 = surface.PointAtParameter(v2.X, v2.Y);
                var n2 = surface.NormalAtParameter(v2.X, v2.Y);

                var v3 = cell.Vertices[2].AsVector();
                var pt3 = surface.PointAtParameter(v3.X, v3.Y);
                var n3 = surface.NormalAtParameter(v3.X, v3.Y);

                // Calculate the aggregate distance of all vertex 
                // locations from the surface. Triangles not on the surface
                // will have a higher aggregate value.
                var sumDist = pt1.DistanceTo(surface) + pt2.DistanceTo(surface) + pt3.DistanceTo(surface);
                if (sumDist > 0.05)
                {
                    continue;
                }

                package.AddTriangleVertex(pt1.X, pt1.Y, pt1.Z);
                package.AddTriangleVertexNormal(n1.X, n1.Y, n1.Z);
                package.AddTriangleVertexUV(v1.X, v1.Y);
                package.AddTriangleVertexColor(0, 0, 0, 255);

                package.AddTriangleVertex(pt2.X, pt2.Y, pt2.Z);
                package.AddTriangleVertexNormal(n2.X, n2.Y, n2.Z);
                package.AddTriangleVertexUV(v2.X, v2.Y);
                package.AddTriangleVertexColor(0, 0, 0, 255);

                package.AddTriangleVertex(pt3.X, pt3.Y, pt3.Z);
                package.AddTriangleVertexNormal(n3.X, n3.Y, n3.Z);
                package.AddTriangleVertexUV(v3.X, v3.Y);
                package.AddTriangleVertexColor(0, 0, 0, 255);

                package.AddLineStripVertex(pt1.X, pt1.Y, pt1.Z);
                package.AddLineStripVertex(pt2.X, pt2.Y, pt2.Z);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertexCount(2);

                package.AddLineStripVertex(pt2.X, pt2.Y, pt2.Z);
                package.AddLineStripVertex(pt3.X, pt3.Y, pt3.Z);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertexCount(2);

                package.AddLineStripVertex(pt3.X, pt3.Y, pt3.Z);
                package.AddLineStripVertex(pt1.X, pt1.Y, pt1.Z);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertexColor(100, 100, 100, 255);
                package.AddLineStripVertexCount(2);

                v1.Dispose(); v2.Dispose(); v3.Dispose();
                pt1.Dispose(); pt2.Dispose(); pt3.Dispose();
                n1.Dispose(); n2.Dispose(); n3.Dispose();
            }
        }

        private static List<UV> GetEdgeCoordinates(IEnumerable<Curve> curves, int div, Surface surface)
        {
            var points = new List<UV>();
            foreach (var curve in curves)
            {
                var line = curve as Line;
                UV start, end;

                if (line != null)
                {
                    start = surface.UVParameterAtPoint(line.PointAtParameter(0));
                    end = surface.UVParameterAtPoint(line.PointAtParameter(1));
                    points.Add(start);
                    points.Add(end);
                }
                else
                {
                    var step = 1.0 / div;
                    for (var t = 0.0; t < 1.0; t += step)
                    {
                        // Create a line segment
                        var a = curve.PointAtParameter(t);
                        var b = curve.PointAtParameter(t + step);
                        start = surface.UVParameterAtPoint(a);
                        end = surface.UVParameterAtPoint(b);
                        if (start.IsAlmostEqualTo(end))
                            continue;
                        points.Add(start);

                        // We don't always add the end point to
                        // reduce the number of duplicate points.
                        // We add it here, only if we're at the
                        // end of the curve.
                        if (t + step == 1.0)
                        {
                            points.Add(end);
                        }
                    }
                }
            }
            return points;
        }
    }

    public static class AnalysisExtensions
    {
        public static bool IsAlmostEqualTo(this UV a, UV b)
        {
            return Math.Abs(a.U - b.U) < 1.0e-6 && Math.Abs(a.V - b.V) < 1.0e-6;
        }
    }
}
