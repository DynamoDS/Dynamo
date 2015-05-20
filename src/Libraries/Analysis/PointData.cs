using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Analysis
{
    /// <summary>
    /// A class for storing structure point analysis data.
    /// </summary>
    public class PointData : IStructuredData<Point, double>, IGraphicItem
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

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            if (!Values.Any() || Values == null)
            {
                return;
            }

            var min = Values.Min();
            var max = Values.Max();
            var normalizedValues = Values.Select(v => (v - min)/(max - min));

            var colorRange = Utils.CreateAnalyticalColorRange();

            var data = ValueLocations.Zip(
                normalizedValues,
                (p, v) => new Tuple<Point, double>(p, v));

            foreach (var d in data)
            {
                var pt = d.Item1;

                var color = colorRange.GetColorAtParameter(d.Item2);
                package.AddPointVertex(pt.X, pt.Y, pt.Z);
                package.AddPointVertexColor(color.Red, color.Green, color.Blue, color.Alpha);
            }
        }
    }
}
