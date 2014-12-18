using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

namespace Analysis
{
    /// <summary>
    /// A class for storing structure point analysis data.
    /// </summary>
    public class PointAnalysisData : IAnalysisData<Point, double>, IGraphicItem
    {
        private int valueIndex;

        /// <summary>
        /// A list of Points.
        /// </summary>
        public IEnumerable<Point> CalculationLocations { get; internal set; }

        /// <summary>
        /// A dictionary of lists of double values.
        /// </summary>
        public Dictionary<string, IList<double>> Values { get; internal set; }

        protected PointAnalysisData(
            IEnumerable<Point> points, Dictionary<string, IList<double>> results, int valueIndex)
        {
            CalculationLocations = points;
            Values = results;

            this.valueIndex = valueIndex;
        }

        /// <summary>
        /// Create a PointAnalysisData object.
        /// </summary>
        /// <param name="points">A list of Points.</param>
        public static PointAnalysisData ByPoints(IEnumerable<Point> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            var results = new Dictionary<string, IList<double>>();
            return new PointAnalysisData(points, results,0);
        }

        /// <summary>
        /// Create a PointAnalysisData object.
        /// </summary>
        /// <param name="points">A list of Points.</param>
        /// <param name="names">A list of value set names.</param>
        /// <param name="values">A list of lists of double values.</param>
        /// <param name="valueIndex">The index of the value set to visualize.</param>
        public static PointAnalysisData ByPointsAndValues(IEnumerable<Point> points, IList<string> names, IList<IList<double>> values, int valueIndex = 0)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (names == null)
            {
                throw new ArgumentNullException("resultNames");
            }

            if (values == null)
            {
                throw new ArgumentNullException("resultValues");
            }

            if (names.Count != values.Count)
            {
                throw new ArgumentException("The number of result names and result values must match.");
            }

            var results = new Dictionary<string, IList<double>>();
            for (var i = 0; i < names.Count; i++)
            {
                results.Add(names[i], values[i]);
            }

            return new PointAnalysisData(points, results, valueIndex);
        }

        /// <summary>
        /// Get the collection of values corresponding to a key.
        /// </summary>
        /// <param name="key">The name of the value set.</param>
        /// <returns>A list of doubles.</returns>
        public IList<double> GetValuesByKey(string key)
        {
            if (!Values.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Values[key];
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            if (!Values.Any() || valueIndex > Values.Count-1)
            {
                return;
            }

            var values = Values.ElementAt(valueIndex).Value;
            var min = values.Min();
            var max = values.Max();
            var normalizedValues = values.Select(v => (v - min)/(max - min));

            var colorRange = Utils.CreateAnalyticalColorRange();

            var data = CalculationLocations.Zip(
                normalizedValues,
                (p, v) => new Tuple<Point, double>(p, v));

            foreach (var d in data)
            {
                var pt = d.Item1;

                var color = colorRange.GetColorAtParameter(d.Item2);
                package.PushPointVertex(pt.X, pt.Y, pt.Z);
                package.PushPointVertexColor(color.Red, color.Green, color.Blue, color.Alpha);
            }
        }
    }
}
