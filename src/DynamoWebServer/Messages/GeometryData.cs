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
            if (packages != null && packages.Any())
            {
                string pointVertices = encodeNumbers(packages, packageObj => packageObj.PointVertices);
                string lineStripVertices = encodeNumbers(packages, packageObj => packageObj.LineStripVertices);
                string triangleVertices = encodeNumbers(packages, packageObj => packageObj.TriangleVertices);
                string triangleNormals = encodeNumbers(packages, packageObj => packageObj.TriangleNormals);
                string lineStripCounts = encodeNumbers(packages, packageObj => packageObj.LineStripVertexCounts);
                string lineStripColors = encodeNumbers(packages, packageObj => packageObj.LineStripVertexColors);
                GraphicPrimitivesData = new GraphicPrimitives(pointVertices, lineStripVertices, triangleVertices, triangleNormals,
                    lineStripCounts, lineStripColors);
            }

        }

        private string encodeNumbers<T>(List<IRenderPackage> packages, Func<IRenderPackage, List<T>> getter)
        {
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (var package in packages)
                {
                    List<T> coordinates = getter(package);
                    foreach (T value in coordinates)
                    {
                        // we only need to handle doubles specifically
                        if (value is double)
                        {
                            writer.Write(Convert.ToSingle(value));
                        }
                        else {
                            if (value is int)
                            {
                                writer.Write(Convert.ToInt32(value));
                            }
                            else
                            {
                                // colors
                                if (value is byte)
                                {
                                    writer.Write(Convert.ToByte(value));
                                }
                            }
                        }
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
