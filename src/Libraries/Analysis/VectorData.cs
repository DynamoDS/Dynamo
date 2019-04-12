using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured vector analysis data.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class VectorData : IStructuredData<Point, Vector>
    {
        private const byte VectorColor = 120;

        /// <summary>
        /// A list of calculation locations.
        /// </summary>
        [NodeObsolete("ValueLocationObsolete", typeof(Properties.Resources))]
        public IEnumerable<Point> ValueLocations { get; internal set; }

        /// <summary>
        /// A dictionary of results.
        /// </summary>
        [NodeObsolete("ValuesObsolete",typeof(Properties.Resources))]
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
        [NodeObsolete("VectorValueObsolete", typeof(Properties.Resources))]
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
