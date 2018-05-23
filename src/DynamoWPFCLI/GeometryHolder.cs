using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using Dynamo.Models;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;
using Autodesk.DesignScript.Interfaces;

namespace DynamoWPFCLI
{
    internal class GraphicPrimitives
    {
        /// <summary>
        /// The class that represents data for drawing a graphic primitive 
        /// </summary>
        [DataMember]
        public string Colors { get; set; }

        [DataMember] 
        public string TriangleTextureCoordinates { get; set; }

        [DataMember] 
        public string ColorsStride { get; set; }

        [DataMember] 
        public string PointVertices { get; set; }

        [DataMember] 
        public string PointVertexColors { get; set; }

        [DataMember] 
        public string TriangleVertices { get; set; }

        [DataMember] 
        public string TriangleNormals { get; set; }

        [DataMember] 
        public string TriangleVertexColors { get; set; }

        [DataMember] 
        public string LineStripVertices { get; set; }

        [DataMember] 
        public string LineStripCounts { get; set; }

        [DataMember] 
        public string LineStripColors { get; set; }

        [DataMember] 
        public bool RequiresPerVertexColoration { get; set; }
    }

    internal class GeometryData
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        [DataMember]
        public string Id { get; private set; }

        /// <summary>
        /// List of the graphic primitives that result object consist of.
        /// It is empty for nongraphic objects
        /// </summary>
        [DataMember]
        public IEnumerable<GraphicPrimitives> GeometryEntries { get; private set; }

        public GeometryData(string id)
        {
            this.Id = id;
        }

        public GeometryData(string id, IEnumerable<IRenderPackage> packages)
        {
            this.Id = id;
            GeneratePrimitives(packages);
        }

        private void GeneratePrimitives(IEnumerable<IRenderPackage> packages)
        {
            if (packages == null || !packages.Any())
                return;

            var data = new List<GraphicPrimitives>();

            foreach (var package in packages)
            {
                string triangleTextureCoordinates = String.Empty,
                    colorsStride = String.Empty,
                    colors = String.Empty;

                if (package.Colors != null)
                {
                    colors = EncodeNumbers(package.Colors);
                    triangleTextureCoordinates = EncodeNumbers(package.MeshTextureCoordinates);
                    colorsStride = package.ColorsStride.ToString();
                }

                string pointVertices = EncodeNumbers(package.PointVertices);
                string pointVertexColors = EncodeNumbers(package.PointVertexColors);

                string triangleVertices = EncodeNumbers(package.MeshVertices);
                string triangleNormals = EncodeNumbers(package.MeshNormals);
                string triangleVertexColors = EncodeNumbers(package.MeshVertexColors);

                string lineStripVertices = EncodeNumbers(package.LineStripVertices);
                string lineStripCounts = EncodeNumbers(package.LineStripVertexCounts);
                string lineStripColors = EncodeNumbers(package.LineStripVertexColors);

                data.Add(new GraphicPrimitives()
                {
                    Colors = colors,
                    TriangleTextureCoordinates = triangleTextureCoordinates,
                    ColorsStride = colorsStride,
                    PointVertexColors = pointVertexColors,
                    PointVertices = pointVertices,
                    TriangleNormals = triangleNormals,
                    TriangleVertexColors = triangleVertexColors,
                    TriangleVertices = triangleVertices,
                    LineStripColors = lineStripColors,
                    LineStripCounts = lineStripCounts,
                    LineStripVertices = lineStripVertices,
                    RequiresPerVertexColoration = package.RequiresPerVertexColoration,
                });
            }

            this.GeometryEntries = data;
        }

        private static string EncodeNumbers<T>(IEnumerable<T> coordinates)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                if (typeof(T) == typeof(double))
                {
                    foreach (T value in coordinates)
                        writer.Write(Convert.ToSingle(value));
                }
                else
                {
                    foreach (T value in coordinates)
                        writer.Write(value as dynamic);
                }
            }

            return Convert.ToBase64String(stream.ToArray());
        }

    }
    internal class GeometryHolder
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        /// <summary>
        /// List of the graphic primitives that result object consist of.
        /// It is empty for nongraphic objects
        /// </summary>
        private GeometryData data;

        private ManualResetEvent done = new ManualResetEvent(false);
        private bool hasGeometry = false;

        public Object Geometry
        {
            get
            {
                Done.WaitOne();
                return data;
            }
        }

        public ManualResetEvent Done
        {
            get { return done; }

            private set { done = value; }
        }

        public bool HasGeometry
        {
            get
            {
                Done.WaitOne();
                return hasGeometry;
            }
            private set { hasGeometry = value; }
        }

        public GeometryHolder(DynamoModel model, IRenderPackageFactory factory, NodeModel nodeModel)
        {
            data = new GeometryData(nodeModel.GUID.ToString());

            // Schedule the generation of render packages for this node. NodeRenderPackagesUpdated will be
            // called with the render packages when they are ready. The node will be set do 'Done' if the 
            // sheduling for some reason is not successful (usually becuase the node have no geometry or is inivisible)
            nodeModel.RenderPackagesUpdated += NodeRenderPackagesUpdated;
            if (!nodeModel.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory, true))
            {
                // The node has no geometry so we are 'Done'
                Done.Set();
            }
        }

        private void NodeRenderPackagesUpdated(NodeModel nodeModel, RenderPackageCache renderPackages)
        {
            if (renderPackages.Packages.Any())
            {
                data = new GeometryData(nodeModel.GUID.ToString(), renderPackages.Packages);

                // We have geometry
                HasGeometry = true;
            }

            // We are 'Done'
            Done.Set();
        }
    }
}