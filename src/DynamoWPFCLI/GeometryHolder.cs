using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Visualization;

namespace DynamoWPFCLI
{
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
        public IEnumerable<IGraphicPrimitives> GeometryEntries { get; private set; }

        public GeometryData(string id)
        {
            Id = id;
            GeometryEntries = new List<IGraphicPrimitives>();
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

            if (!packages.Any())
            {
                return;
            }

            var data = new List<IGraphicPrimitives>();

            foreach (var package in packages)
            {
                data.Add(new DefaultGraphicPrimitives(package));
            }

            GeometryEntries = data;
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