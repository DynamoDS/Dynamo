using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dynamo.Models;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;
using Autodesk.DesignScript.Interfaces;

namespace DynamoCLI
{
    internal class GeometryHolder
    {
        private NodeModel node;
        private ManualResetEvent done = new ManualResetEvent(false);
        private Dictionary<string, Object> groupData = new Dictionary<string, object>();
        private bool hasGeometry = false;

        public Object Geometry
        {
            get
            {
                Done.WaitOne();
                return groupData;
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
            node = nodeModel;
            nodeModel.RenderPackagesUpdated += NodeRenderPackagesUpdated;
            if (!nodeModel.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory, true))
            {
                Done.Set();
            }
        }

        public void Dispose()
        {
            node.RenderPackagesUpdated -= NodeRenderPackagesUpdated;
        }

        private void NodeRenderPackagesUpdated(NodeModel nodeModel, RenderPackageCache renderPackages)
        {
            if (renderPackages.Packages.Count() > 0)
            {
                List<double[]> verts = new List<double[]>();
                List<List<byte>> vertColors = new List<List<byte>>();
                List<double[]> normals = new List<double[]>();
                List<List<double>> points = new List<List<double>>();
                List<List<byte>> pointColors = new List<List<byte>>();
                List<List<double>> lines = new List<List<double>>();
                List<List<byte>> lineColors = new List<List<byte>>();

                foreach (IRenderPackage p in renderPackages.Packages)
                {
                    var meshVertices = p.MeshVertices.ToArray();
                    if (meshVertices.Length > 0)
                    {
                        verts.Add(meshVertices);
                    }

                    var meshVertexColors = p.MeshVertexColors.ToList();
                    if (meshVertexColors.Count > 0)
                    {
                        vertColors.Add(meshVertexColors);
                    }

                    var meshNormals = p.MeshNormals.ToArray();
                    if (meshNormals.Length > 0)
                    {
                        normals.Add(meshNormals);
                    }

                    var pointVertices = p.PointVertices.ToList();
                    if (pointVertices.Count > 0)
                    {
                        points.Add(pointVertices);
                    }

                    var pointVertexColors = p.PointVertexColors.ToList();
                    if (pointVertexColors.Count > 0)
                    {
                        pointColors.Add(pointVertexColors);
                    }

                    var lineStripVertices = p.LineStripVertices.ToList();
                    if (lineStripVertices.Count > 0)
                    {
                        lines.Add(lineStripVertices);
                    }

                    var lineStripVertexColors = p.LineStripVertexColors.ToList();
                    if (lineStripVertexColors.Count > 0)
                    {
                        lineColors.Add(lineStripVertexColors);
                    }
                }

                groupData.Add("name", nodeModel.GUID.ToString());
                groupData.Add("vertices", verts);
                groupData.Add("verticeColors", vertColors);
                groupData.Add("normals", normals);
                groupData.Add("points", points);
                groupData.Add("pointColors", pointColors);
                groupData.Add("lines", lines);
                groupData.Add("lineColors", lineColors);

                HasGeometry = true;
            }

            Done.Set();
        }
    }
}