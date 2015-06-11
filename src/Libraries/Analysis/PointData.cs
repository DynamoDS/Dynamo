using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Analysis
{
    /// <summary>
    /// A class for storing structure point analysis data.
    /// </summary>
    public class PointData : IStructuredData<Point, double> //, IGraphicItem
    {
        /// <summary>
        /// A list of Points.
        /// </summary>
        public IEnumerable<Point> ValueLocations { get; internal set; }

        /// <summary>
        /// A dictionary of lists of double values.
        /// </summary>
        public IList<double> Values { get; internal set; }

        protected PointData(
            IEnumerable<Point> points, IList<double> values)
        {
            ValueLocations = points;
            Values = values;
        }

        /// <summary>
        /// Create a PointAnalysisData object.
        /// </summary>
        /// <param name="points">A list of Points.</param>
        /// <param name="values">A list of double values.</param>
        public static PointData ByPointsAndValues(IEnumerable<Point> points, IList<double> values)
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

            return new PointData(points, values);
        }
    }
}
