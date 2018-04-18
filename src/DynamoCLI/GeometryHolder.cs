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
                if (!nodeModel.RequestVisualUpdateAsync(model.Scheduler, model.EngineController, factory))
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
                        verts.Add(p.MeshVertices.ToArray());
                        vertColors.Add(p.MeshVertexColors.ToList());
                        normals.Add(p.MeshNormals.ToArray());
                        points.Add(p.PointVertices.ToList());
                        pointColors.Add(p.PointVertexColors.ToList());
                        lines.Add(p.LineStripVertices.ToList());
                        lineColors.Add(p.LineStripVertexColors.ToList());
                    }

                    Dictionary<string, Object> groupData = new Dictionary<string, object>();
                    groupData.Add("name", node.GUID.ToString());
                    groupData.Add("transactionType", "update");
                    groupData.Add("displayPreview", node.ShouldDisplayPreview);
                    groupData.Add("vertices", verts);
                    groupData.Add("verticeColors", vertColors);
                    groupData.Add("normals", normals);
                    groupData.Add("points", points);
                    groupData.Add("pointColors", pointColors);
                    groupData.Add("lines", lines);
                    groupData.Add("lineColors", lineColors);

                    GeometryJson = JsonConvert.SerializeObject(groupData);
                    HasGeometry = true;
                }

                Done.Set();
            }
        }
    }
}
