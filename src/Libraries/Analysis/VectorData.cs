using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured vector analysis data.
    /// </summary>
    public class VectorData : IStructuredData<Point, Vector>
    {
        private const byte VectorColor = 120;

        /// <summary>
        /// A list of calculation locations.
        /// </summary>
        public IEnumerable<Point> ValueLocations { get; internal set; }

        /// <summary>
        /// A dictionary of results.
        /// </summary>
        public IList<Vector> Values { get; internal set; }

        protected VectorData(IEnumerable<Point> points, IList<Vector> values)
        {
            ValueLocations = points;
            Values = values;
        }

        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="points">A list of Points.</param>
        /// <param name="values">A list of Vector values.</param>
        public static VectorData ByPointsAndValues(
            IEnumerable<Point> points, IList<Vector> values)
        {

            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (!points.Any())
            {
                throw new ArgumentException("points", AnalysisResources.EmptyPointsMessage);
            }

            if (!values.Any())
            {
                throw new ArgumentException("values", AnalysisResources.EmptyValuesMessage);
            }

            if (points.Count() != values.Count)
            {
                throw new ArgumentException(AnalysisResources.InputsNotEquivalentMessage);
            }

            return new VectorData(points, values);
        }
    }
}
