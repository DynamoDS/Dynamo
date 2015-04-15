using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Analysis
{
    /// <summary>
    /// A class for storing structured vector analysis data.
    /// </summary>
    public class VectorData : IStructuredData<Point, Vector>, IGraphicItem
    {
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

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            if (!Values.Any() || Values == null)
            {
                return;
            }

            var data = Values.Zip(ValueLocations, (v, p) => new Tuple<Vector, Point>(v, p));

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
