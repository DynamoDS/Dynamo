using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dynamo.Models;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;
using Autodesk.DesignScript.Interfaces;
using Newtonsoft.Json;

namespace DynamoCLI
{
    public partial class CommandLineRunner
    {
        internal class GeometryHolder
        {
            private NodeModel node;
            private ManualResetEvent done = new ManualResetEvent(false);
            private string geometryJson = "";
            private bool hasGeometry = false;

            public string GeometryJson
            {
                get
                {
                    Done.WaitOne();
                    return geometryJson;
                }
                private set => geometryJson = value;
            }

            public ManualResetEvent Done { get => done; private set => done = value; }
            public bool HasGeometry
            {
                get
                {
                    Done.WaitOne();
                    return hasGeometry;
                }
                private set => hasGeometry = value;
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
                    List<double[]> normals = new List<double[]>();
                    List<List<double>> points = new List<List<double>>();
                    List<List<double>> lines = new List<List<double>>();

                    foreach (IRenderPackage p in renderPackages.Packages)
                    {
                        if (p.MeshVertexCount == 0)
                        {
                            continue;
                        }
                        verts.Add(p.MeshVertices.ToArray());
                        normals.Add(p.MeshNormals.ToArray());
                        points.Add(p.PointVertices.ToList());
                        lines.Add(p.LineStripVertices.ToList());
                    }
                    if (verts.Count > 0 || normals.Count > 0 || points.Count > 0 || lines.Count > 0)
                    {
                        Dictionary<string, Object> groupData = new Dictionary<string, object>();
                        groupData.Add("name", node.GUID.ToString());
                        groupData.Add("transactionType", "update");
                        groupData.Add("displayPreview", node.ShouldDisplayPreview);
                        groupData.Add("vertices", verts);
                        groupData.Add("normals", normals);
                        groupData.Add("points", points);
                        groupData.Add("lines", lines);

                        GeometryJson = JsonConvert.SerializeObject(groupData);
                        HasGeometry = true;
                    }

                }

                Done.Set();
            }

        }
    }
}
