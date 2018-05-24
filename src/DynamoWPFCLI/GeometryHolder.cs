using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string TriangleVertices { get; set; }

        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string TriangleNormals { get; set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        public string TriangleVertexColors { get; set; }

        // Base-64 encoded array of 32 bit floats, 2 per vertex.
        public string TriangleTextureCoordinates { get; set; }

        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string LineStripVertices { get; set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per line strip, giving the number of vertices in the strip.
        public string LineStripCounts { get; set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        public string LineStripColors { get; set; }

        // Base-64 encoded array of 32 bit floats, 3 per vertex.
        public string PointVertices { get; set; }

        // Base-64 encoded array of 32 bit unsigned integers, 1 per vertex, in RGBA format.
        public string PointVertexColors { get; set; }

        // Base-64 encoded array of 32 bit unsigned integers in RGBA format, definining a texture to apply to the triangles.
        public string Colors { get; set; }

        // Number of values per row in the `Colors` array.
        public string ColorsStride { get; set; }

        //  Whether or not the individual vertices should be colored using the data in the corresponding arrays.
        public bool RequiresPerVertexColoration { get; set; }
    }

    internal class GeometryData
    {
        /// <summary>
        /// Guid of the specified node
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// List of the graphic primitives that result object consist of.
        /// It is empty for nongraphic objects
        /// </summary>
        public IEnumerable<GraphicPrimitives> GeometryEntries { get; private set; }

        public GeometryData(string id)
        {
            Id = id;
            GeometryEntries = new List<GraphicPrimitives>();
        }

        public GeometryData(string id, IEnumerable<IRenderPackage> packages)
        {
            Id = id;
            GeneratePrimitives(packages);
        }

        private void GeneratePrimitives(IEnumerable<IRenderPackage> packages)
        {
            if (packages == null)
            {
                return;
            }

            var renderPackages = packages.ToList();
            if (!renderPackages.Any())
            {
                return;
            }

            var data = new List<GraphicPrimitives>();

            foreach (var package in renderPackages)
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

            GeometryEntries = data;
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