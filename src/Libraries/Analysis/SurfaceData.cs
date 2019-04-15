using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured surface analysis data.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class SurfaceData : ISurfaceData<UV, double>
    {
        /// <summary>
        /// The surface which contains the locations.
        /// </summary>
        [NodeObsolete("SurfaceObsolete", typeof(Properties.Resources))]
        public Surface Surface { get; set; }

        /// <summary>
        /// A list of UV locations on the surface.
        /// </summary>
        [NodeObsolete("ValueLocationsObsolete", typeof(Properties.Resources))]
        public IEnumerable<UV> ValueLocations { get; internal set; }

        /// <summary>
        /// A dictionary of lists of doubles.
        /// </summary>
        [NodeObsolete("ValueSurfaceObsolete", typeof(Properties.Resources))]
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
        [NodeObsolete("BySurfacePointObsolete", typeof(Properties.Resources))]
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
        [NodeObsolete("BySurfacePointsAndValues", typeof(Properties.Resources))]
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
        //public double[][] GetValueMap(int width, int height)
        //{
        //    const int MinPointCount = 3;
        //    const double DefaultPointSpacing = 1 / ((double)MinPointCount - 1);

        //    if (!Values.Any())
        //    {
        //        return new double[0][];
        //    }

        //    var min = Values.Min();
        //    var max = Values.Max();

        //    var domain = max - min;

        //    if (min == max)
        //    {
        //        domain = Math.Abs(max);
        //    }

        //    // Normalize the values in the 0->1 range
        //    var normalizedValues = Values.Select(v => (v - min)/ domain);

        //    // Create points at locations.
        //    var zip = ValueLocations.Zip(normalizedValues, (uv, value) => Point.ByCoordinates(uv.U,uv.V,value));

        //    // Group the points by their x coordinate. Then order,
        //    // the inner groups by y. 
        //    var pointGroups = zip.
        //        GroupBy(pt => pt.X).
        //        OrderBy(group => group.First().X).
        //        Select(group=>group.OrderBy(pt=>pt.Y));

        //    // x - Value location provided
        //    // o - Value location added to 'square' the grid
        //    //
        //    // o-o-o----o-o
        //    // ----x-------
        //    // --x------x--
        //    // o-o-o----o-o

        //    // Project groups to a List<List<Tuple<UV,double>>
        //    var pointLists = pointGroups.Select(g => g.ToList()).ToList();

        //    // Square any rows that aren't square
        //    foreach (var pointList in pointLists)
        //    {
        //        var vMin = pointList.First().Y;
        //        var vMax = pointList.Last().Y;

        //        var u = pointList.First().X;
        //        var v = pointList.First().Y;

        //        // If 0,0 0,1 1,0 1,1 continue
        //        if (u == 0.0 && (v == 0.0 || v==1.0) ||
        //            u==1.0 && (v==0.0||v==1.0))
        //        {
        //            continue;
        //        }

        //        if (vMin != 0.0)
        //        {
        //            pointList.Insert(0, Point.ByCoordinates(u, 0.0, 0.0)); 
        //        }

        //        if (vMax != 1.0)
        //        {
        //            pointList.Add(Point.ByCoordinates(u,1.0,0.0));
        //        }

        //        // The column should now have start and end points,
        //        // but what if that's all that was added? 
        //        if (pointList.Count() == 2)
        //        {
        //            pointList.Add(Point.ByCoordinates(u,0.5,0.0));
        //        }
        //    }

        //    // Check whether we have 'end' rows at u=0 and u=1
        //    // If not, add them with the correct number of points
        //    if (!pointLists.Any(list=>list.Any(item=>item.X == 0.0)))
        //    {
        //        var startList = new List<Point>();
        //        for (var i = 0; i < MinPointCount; i++)
        //        {
        //            var v = i * DefaultPointSpacing;
        //            startList.Add(Point.ByCoordinates(0.0,v,0.0));
        //        }
        //        pointLists.Insert(0, startList);
        //    }

        //    if (!pointLists.Any(list => list.Any(item => item.X == 1.0)))
        //    {
        //        var endList = new List<Point>();
        //        for (var i = 0; i < MinPointCount; i++)
        //        {
        //            var v = i * DefaultPointSpacing;
        //            endList.Add(Point.ByCoordinates(1.0,v,0.0));
        //        }
        //        pointLists.Add(endList);
        //    }

        //    // Project the IGrouping back into an [][] array
        //    // this is required for the Surface.ByPoints method
        //    var pointArr = pointLists.Select(l=>l.ToArray()).ToArray();

        //    //var startUTangents = new List<Vector>();
        //    //var startVTangents = new List<Vector>();
        //    //var endUTangents = new List<Vector>();
        //    //var endVTangents = new List<Vector>();

        //    //for (int i = 0; i < pointArr.Length; i++)
        //    //{
        //    //    startUTangents.Add(Vector.XAxis());
        //    //    endUTangents.Add(Vector.XAxis());

        //    //    if (i == 0)
        //    //    {
        //    //        for (int j = 0; j < pointArr[i].Count(); j++)
        //    //        {
        //    //            startVTangents.Add(Vector.YAxis());
        //    //        }
        //    //    }
        //    //    else if (i == pointArr.Length - 1)
        //    //    {
        //    //        for (int j = 0; j < pointArr[i].Count(); j++)
        //    //        {
        //    //            endVTangents.Add(Vector.YAxis());
        //    //        }
        //    //    }
        //    //}

        //    //gradientSurface = NurbsSurface.ByPoints(pointArr);
        //    //gradientSurface = NurbsSurface.ByPointsTangents(pointArr, startUTangents, startVTangents, endUTangents, endVTangents);

        //    curves = new List<Curve>();
        //    foreach(var points in pointArr)
        //    {
        //        var cpts = new List<Point>();
        //        var weights = new List<double>();
        //        var knots = new List<double>();

        //        Point prevPt = null;
        //        Point currPt = null;

        //        for (var i = 0; i < points.Length; i++)
        //        {
        //            currPt = points[i];

        //            if (i == 1 || i == points.Length-1)
        //            {
        //                cpts.Add(Point.ByCoordinates(currPt.X, prevPt.Y + (currPt.Y - prevPt.Y) / 2, currPt.Z));
        //            }

        //            cpts.Add(currPt);

        //            prevPt = currPt;
        //        }

        //        for (var i = 0; i < cpts.Count; i++)
        //        {
        //            if (i == 0 || i == cpts.Count - 1)
        //            {
        //                weights.Add(2);
        //            }
        //            else
        //            {
        //                weights.Add(1);
        //            }
        //        }

        //        for (var i = 0; i <= cpts.Count - 3; i++)
        //        {
        //            if (i == 0 || i == cpts.Count - 3)
        //            {
        //                knots.AddRange(Enumerable.Repeat((double)i, 4));
        //            }
        //            else
        //            {
        //                knots.Add(i);
        //            }
        //        }

        //        // Create a nurbs curve;
        //        curves.Add(NurbsCurve.ByControlPointsWeightsKnots(cpts, weights.ToArray(), knots.ToArray()));
        //    }

        //    gradientSurface = NurbsSurface.ByLoft(curves);

        //    var result = new double[width][];

        //    var wStep = 1 / ((double)width);
        //    var hStep = 1 / ((double)height);

        //    for (var i = 0; i < width; i++)
        //    {
        //        result[i] = new double[height];
        //        for (var j = 0; j < height; j++)
        //        {
        //            var projPt = Point.ByCoordinates(i * wStep, j * hStep, 3.0);
        //            var proj = gradientSurface.ProjectInputOnto(projPt, Vector.ZAxis().Reverse());
        //            var ptEval = proj.First() as Point;

        //            // Clamp the points to 0 and 1
        //            if (ptEval.Z > 1.0)
        //            {
        //                result[i][j] = 1.0;
        //            }
        //            else if (ptEval.Z < 0.0)
        //            {
        //                result[i][j] = 0.0;
        //            }
        //            else
        //            {
        //                result[i][j] = ptEval.Z;
        //            }
        //        }
        //    }

        //    gradientSurface.Dispose();

        //    return null;
        //}
    }

    [IsVisibleInDynamoLibrary(false)]
    public static class AnalysisExtensions
    {
        /// <summary>
        /// Takes two planes (of type UV) and returns True if the difference between the two planes is smaller than 1.0e-6 and returns False otherwise.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsAlmostEqualTo(this UV a, UV b)
        {
            return Math.Abs(a.U - b.U) < 1.0e-6 && Math.Abs(a.V - b.V) < 1.0e-6;
        }
    }
}
