﻿using System;
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

        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
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

            package.AddLineStripVertex(p.X, p.Y, p.Z);
            package.AddLineStripVertexColor(VectorColor, VectorColor, VectorColor, 255);

            var o = p.Add(v);

            package.AddLineStripVertex(o.X, o.Y, o.Z);
            package.AddLineStripVertexColor(VectorColor, VectorColor, VectorColor, 255);

            package.AddLineStripVertexCount(2);
        }
    }
}
