using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured vector analysis data.
    /// </summary>
    public class VectorAnalysisData : IAnalysisData<Point, Vector>, IGraphicItem
    {
        private int valueIndex;

        /// <summary>
        /// A list of calculation locations.
        /// </summary>
        public IEnumerable<Point> CalculationLocations { get; internal set; }

        /// <summary>
        /// A dictionary of results.
        /// </summary>
        public Dictionary<string, IList<Vector>> Values { get; internal set; }

        protected VectorAnalysisData(IEnumerable<Point> points, Dictionary<string, IList<Vector>> values, int valueIndex)
        {
            CalculationLocations = points;
            Values = values;

            this.valueIndex = valueIndex;
        }

        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="points">A list of Points.</param>
        public static VectorAnalysisData ByPoints(IEnumerable<Point> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            var results = new Dictionary<string, IList<Vector>>();
            return new VectorAnalysisData(points, results, 0);
        }

        /// <summary>
        /// Create a VectorAnalysisData object.
        /// </summary>
        /// <param name="names">A list of value set names.</param>
        /// <param name="values">A list of lists of Vector values.</param>
        /// <param name="valueIndex">The index of the value set to visualize.</param>
        public static VectorAnalysisData ByPointsAndValues(
            IEnumerable<Point> points, IList<string> names, IList<IList<Vector>> values, int valueIndex = 0)
        {

            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (names.Count != values.Count)
            {
                throw new ArgumentException("The number of result names and result values must match.");
            }

            var results = new Dictionary<string, IList<Vector>>();
            for (var i = 0; i < names.Count; i++)
            {
                results.Add(names[i], values[i]);
            }

            return new VectorAnalysisData(points, results, valueIndex);
        }

        /// <summary>
        /// Get the collection of values corresponding to a key.
        /// </summary>
        /// <param name="key">The name of the value set.</param>
        /// <returns>A list of Vectors.</returns>
        public IList<Vector> GetValuesByKey(string key)
        {
            if (!Values.ContainsKey(key))
            {
                throw new Exception("The requested result collection does not exist.");
            }

            return Values[key];
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            if (!Values.Any() || valueIndex > Values.Count - 1)
            {
                return;
            }

            var data = Values.ElementAt(valueIndex).Value.Zip(CalculationLocations, (v, p) => new Tuple<Vector, Point>(v, p));

            foreach (var d in data)
            {
                DrawVector(d, package);
            }
        }

        private void DrawVector(Tuple<Vector, Point> data, IRenderPackage package)
        {
            var p = data.Item2;
            var v = data.Item1;

            package.PushLineStripVertex(p.X, p.Y, p.Z);
            package.PushLineStripVertexColor(120, 120, 120, 255);

            var o = p.Add(v);

            package.PushLineStripVertex(o.X, o.Y, o.Z);
            package.PushLineStripVertexColor(120, 120, 120, 255);

            package.PushLineStripVertexCount(2);
        }
    }
}
