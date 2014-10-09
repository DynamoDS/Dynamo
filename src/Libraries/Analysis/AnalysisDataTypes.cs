using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

namespace Analysis
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
        /// Create a SurfaceAnalysisData object.
        /// </summary>
        /// <param name="surface">The surface which contains the calculation locations.</param>
        /// <param name="calculationLocations">A set of results keyed by the name of the result type.</param>
        public SurfaceAnalysisData(Surface surface, IEnumerable<UV> calculationLocations)
        {
            Surface = surface;
            CalculationLocations = CullCalculationLocations(surface, calculationLocations);
            Results = new Dictionary<string, IList<double>>();
        }

        /// <summary>
        /// Create a SurfaceAnalysisData object.
        /// </summary>
        /// <param name="surface">The surface which contains the calculation locations.</param>
        /// <param name="calculationLocations">UV coordinates on the surface.</param>
        /// <param name="results">A set of results keyed by the name of the result type.</param>
        public SurfaceAnalysisData(Surface surface, IEnumerable<UV> calculationLocations, Dictionary<string, IList<double>> results)
        {
            Surface = surface;
            CalculationLocations = CullCalculationLocations(surface, calculationLocations);
            Results = results;
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

        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        public VectorAnalysisData(IEnumerable<Point> points)
        {
            CalculationLocations = points;
            Results = new Dictionary<string, IList<Vector>>();
        }

        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        /// <param name="results">A set of results keyed by the name of the result type.</param>
        public VectorAnalysisData(IEnumerable<Point> points, Dictionary<string, IList<Vector>> results)
        {
            CalculationLocations = points;
            Results = results;
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

        /// <summary>
        /// Create a PointAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        public PointAnalysisData(IEnumerable<Point> points)
        {
            CalculationLocations = points;
            Results = new Dictionary<string, IList<double>>();
        }

        /// <summary>
        /// Create a PointAnalysisData object.
        /// </summary>
        /// <param name="points">A list of calculation locations.</param>
        /// <param name="results">A set of resutls keyed by the name of the result type.</param>
        public PointAnalysisData(IEnumerable<Point> points, Dictionary<string, IList<double>> results)
        {
            CalculationLocations = points;
            Results = results;
        }
    }
}
