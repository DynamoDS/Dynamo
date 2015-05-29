using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured surface analysis data.
    /// </summary>
    public class SurfaceData : ISurfaceData<UV, double>
    {
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

        #endregion

        private static void DebugTime(Stopwatch sw, string message)
        {
            sw.Stop();
            Debug.WriteLine("{0}:{1}", sw.Elapsed, message);
            sw.Reset();
            sw.Start();
        }

        /// <summary>
        /// This method constructs a surface through the UV locations,
        /// converting UVs to Points in the 0,0->1,1 domain. This surface is used
        /// to calculate gradient values at positions on a grid specified by width
        /// and height. The Z coordinate of the created points is set to the normalized value 
        /// associated with that UV locaton. Points are then found on the surface from UVs,
        /// in a grid of the width and height provided. The Z component of the 
        /// points is then stored in the map.
        /// </summary>
        /// <param name="width">The width of the value map.</param>
        /// <param name="height"></param>
        /// <returns></returns>
        public double[][] GetValueMap(int width, int height)
        {
            const int MinPointCount = 3;
            const double DefaultPointSpacing = 1 / ((double)MinPointCount - 1);

            if (!Values.Any())
            {
                return new double[0][];
            }

            var min = Values.Min();
            var max = Values.Max();

            var domain = max - min;

            if (min == max)
            {
                domain = Math.Abs(max);
            }

            // Normalize the values in the 0->1 range
            var normalizedValues = Values.Select(v => ((v - min) == 0.0 ? v : (v-min)) / domain);

            // Create points at locations.
            var zip = ValueLocations.Zip(normalizedValues, (uv, value) => Point.ByCoordinates(uv.U,uv.V,value));

            // Group the values by their U coordinate. Then order,
            // the inner groups by V. 
            var pointGroups = zip.
                GroupBy(pt => pt.X).
                OrderBy(group => group.First().X).
                Select(group=>group.OrderBy(pt=>pt.Y));

            // x - Value location provided
            // o - Value location added to 'square' the grid
            //
            // o-o-o----o-o
            // ----x-------
            // --x------x--
            // o-o-o----o-o

            // Project groups to a List<List<Tuple<UV,double>>
            var pointLists = pointGroups.Select(g => g.ToList()).ToList();

            // Square any rows that aren't square
            foreach (var pointList in pointLists)
            {
                var vMin = pointList.First().Y;
                var vMax = pointList.Last().Y;

                var u = pointList.First().X;

                if (vMin != 0.0)
                {
                    pointList.Insert(0, Point.ByCoordinates(u, 0.0, 0.0)); 
                }

                if (vMax != 1.0)
                {
                    pointList.Add(Point.ByCoordinates(u,1.0,0.0));
                }

                // The column should now have start and end points,
                // but what if that's all that was added? 
                if (pointList.Count() == 2)
                {
                    pointList.Add(Point.ByCoordinates(u,0.5,0.0));
                }
            }

            // Check whether we have 'end' rows at u=0 and u=1
            // If not, add them with the correct number of points
            if (!pointLists.Any(list=>list.Any(item=>item.X == 0.0)))
            {
                var startList = new List<Point>();
                for (var i = 0; i < MinPointCount; i++)
                {
                    var v = i * DefaultPointSpacing;
                    startList.Add(Point.ByCoordinates(0.0,v,0.0));
                }
                pointLists.Insert(0, startList);
            }

            if (!pointLists.Any(list => list.Any(item => item.X == 1.0)))
            {
                var endList = new List<Point>();
                for (var i = 0; i < MinPointCount; i++)
                {
                    var v = i * DefaultPointSpacing;
                    endList.Add(Point.ByCoordinates(1.0,v,0.0));
                }
                pointLists.Add(endList);
            }

            // Project the IGrouping back into an [][] array
            // this is required for the Surface.ByPoints method
            var pointArr = pointLists.Select(l=>l.ToArray()).ToArray();

            var surf = NurbsSurface.ByPoints(pointArr, 3, 3);

            var result = new double[width][];

            var wStep = 1/(double) width;
            var hStep = 1/(double) height;

            for (var i = 0; i < width; i++)
            {
                result[i] = new double[height];
                for (var j = 0; j < height; j++)
                {
                    var ptEval = surf.PointAtParameter(i * wStep, j * hStep);
                    result[i][j] = ptEval.Z;
                }
            }

            return result;
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
