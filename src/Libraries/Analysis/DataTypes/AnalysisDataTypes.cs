using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;

namespace Analysis.DataTypes
{
    /// <summary>
    /// A class for storing structured surface analysis data.
    /// </summary>
    public class SurfaceAnalysisData : ISurfaceAnalysisData<UV, double>
    {
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
        /// <summary>
        /// Gets result by a specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<double> GetResultByKey(string key)
        {
            if (!Results.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Results[key];
        }

        protected SurfaceAnalysisData(
            Surface surface, IEnumerable<UV> calculationLocations, Dictionary<string,IList<double>> results)
        {
            Surface = surface;
            CalculationLocations = CullCalculationLocations(surface, calculationLocations);
            Results = results;
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
                new Dictionary<string, IList<double>>());
        }

        /// <summary>
        /// Create a SurfaceAnalysisData object.
        /// </summary>
        /// <param name="surface">The surface which contains the calculation locations.</param>
        /// <param name="points">A list of UV calculation locations on the surface.</param>
        /// <param name="resultNames">A list of result names.</param>
        /// <param name="resultValues">A list of lists of result values.</param>
        public static SurfaceAnalysisData BySurfacePointsAndResults(Surface surface, IEnumerable<UV> points, IList<string> resultNames, IList<IList<double>> resultValues)
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

            return new SurfaceAnalysisData(surface, points, results);
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
    }

    /// <summary>
    /// A class for storing structured vector analysis data.
    /// </summary>
    public class VectorAnalysisData : IAnalysisData<Point, Vector>
    {
        /// <summary>
        /// A list of calculation locations.
        /// </summary>
        public IEnumerable<Point> CalculationLocations { get; internal set; }

        /// <summary>
        /// A dictionary of results.
        /// </summary>
        public Dictionary<string, IList<Vector>> Results { get; internal set; }

        protected VectorAnalysisData(IEnumerable<Point> points, Dictionary<string,IList<Vector>> results)
        {
            CalculationLocations = points;
            Results = results;
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
            return new VectorAnalysisData(points, results);
        }
 
        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        /// <param name="resultNames">A list of result names.</param>
        /// <param name="resultValues">A list of lists of result values.</param>
        public static VectorAnalysisData ByPointsAndResults(
            IEnumerable<Point> points, IList<string> resultNames, IList<IList<Vector>> resultValues)
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

            return new VectorAnalysisData(points, results);
        }
        /// <summary>
        /// Gets result by a specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<Vector> GetResultByKey(string key)
        {
            if (!Results.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Results[key];
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
        /// <summary>
        /// Gets result by a specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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
