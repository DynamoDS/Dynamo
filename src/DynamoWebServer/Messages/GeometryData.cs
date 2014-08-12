using Autodesk.DesignScript.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DynamoWebServer.Messages
{
    class GeometryData
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string NodeID { get; private set; }

        /// <summary>
        /// List of the graphic primitives that result object consist of.
        /// It is empty for nongraphic objects
        /// </summary>
        [DataMember]
        public GraphicPrimitives GraphicPrimitivesData { get; private set; }

        public GeometryData(string id, List<IRenderPackage> packages)
        {
            this.NodeID = id;
            GeneratePrimitives(packages);
        }

        private void GeneratePrimitives(List<IRenderPackage> packages)
        {
            if (packages == null || !packages.Any())
                return;

            string pointVertices = EncodeNumbers(packages, packageObj => packageObj.PointVertices);
            string lineStripVertices = EncodeNumbers(packages, packageObj => packageObj.LineStripVertices);
            string triangleVertices = EncodeNumbers(packages, packageObj => packageObj.TriangleVertices);
            string triangleNormals = EncodeNumbers(packages, packageObj => packageObj.TriangleNormals);
            string lineStripCounts = EncodeNumbers(packages, packageObj => packageObj.LineStripVertexCounts);
            string lineStripColors = EncodeNumbers(packages, packageObj => packageObj.LineStripVertexColors);
            GraphicPrimitivesData = new GraphicPrimitives(pointVertices, lineStripVertices, triangleVertices, triangleNormals,
                                                          lineStripCounts, lineStripColors);
        }

        private static string EncodeNumbers<T>(IEnumerable<IRenderPackage> packages,Func<IRenderPackage, List<T>> getter)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                if (typeof(T) == typeof(double))
                {
                    foreach (var package in packages)
                    {
                        List<T> coordinates = getter(package);
                        foreach (T value in coordinates)
                            writer.Write(Convert.ToSingle(value));
                    }
                }
                else
                {
                    foreach (var package in packages)
                    {
                        List<T> coordinates = getter(package);
                        foreach (T value in coordinates)
                            writer.Write(value as dynamic);
                    }
                }
            }

            return Convert.ToBase64String(stream.ToArray());
        }

        /// <summary>
        /// The class that represents data for drawing a graphic primitive 
        /// </summary>
        public class GraphicPrimitives
        {
            [DataMember]
            public string PointVertices { get; private set; }

            [DataMember]
            public string LineStripVertices { get; private set; }

            [DataMember]
            public string TriangleVertices { get; private set; }

            [DataMember]
            public string TriangleNormals { get; private set; }

            [DataMember]
            public string LineStripCounts { get; private set; }

            [DataMember]
            public string LineStripColors { get; private set; }

            public GraphicPrimitives(string pointVertices, string lineStripes, string triangleVertices,
                string triangleNormals, string lineStripCounts, string lineStripColors)
            {
                PointVertices = pointVertices;
                LineStripVertices = lineStripes;
                TriangleVertices = triangleVertices;
                TriangleNormals = triangleNormals;
                LineStripCounts = lineStripCounts;
                LineStripColors = lineStripColors;
            }
        }

    }
}
