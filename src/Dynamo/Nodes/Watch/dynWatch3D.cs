//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using Microsoft.FSharp.Collections;
using Dynamo.Connectors;
using Value = Dynamo.FScheme.Value;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    [NodeName("Watch 3D")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Shows a dynamic preview of geometry.")]
    [AlsoKnownAs("Dynamo.Nodes.dyn3DPreview")]
    public partial class dynWatch3D : dynNodeWithOneOutput
    {
        private PointsVisual3D _points;
        private LinesVisual3D _lines;
        private List<MeshVisual3D> _meshes = new List<MeshVisual3D>();

        public Point3DCollection Points { get; set; }
        public Point3DCollection Lines { get; set; }
        public List<Mesh3D> Meshes { get; set; }

        List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
        
        private bool _requiresRedraw = false;
        private bool _isRendering = false;

        public dynWatch3D()
        {
            InPortData.Add(new PortData("", "Incoming geometry objects.", typeof(object)));
            OutPortData.Add(new PortData("", "Watch contents, passed through", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private void GetUpstreamIDrawable(List<IDrawable> drawables, Dictionary<int, Tuple<int, dynNodeModel>> inputs)
        {
            foreach (KeyValuePair<int, Tuple<int, dynNodeModel>> pair in inputs)
            {
                if (pair.Value == null)
                    continue;

                dynNodeModel node = pair.Value.Item2;
                IDrawable drawable = node as IDrawable;

                if (node.IsVisible && drawable != null)
                    drawables.Add(drawable);

                if (node.IsUpstreamVisible)
                    GetUpstreamIDrawable(drawables, node.Inputs);
                else
                    continue; // don't bother checking if function

                //if the node is function then get all the 
                //drawables inside that node. only do this if the
                //node's workspace is the home space to avoid infinite
                //recursion in the case of custom nodes in custom nodes
                if (node is dynFunction && node.WorkSpace == dynSettings.Controller.DynamoModel.HomeSpace)
                {
                    dynFunction func = (dynFunction)node;
                    IEnumerable<dynNodeModel> topElements = func.Definition.Workspace.GetTopMostNodes();
                    foreach (dynNodeModel innerNode in topElements)
                    {
                        GetUpstreamIDrawable(drawables, innerNode.Inputs);
                    }
                }
            }
        }

        MeshVisual3D MakeMeshVisual3D(Mesh3D mesh)
        {
            MeshVisual3D vismesh = new MeshVisual3D { Content = new GeometryModel3D { Geometry = mesh.ToMeshGeometry3D(), Material = Materials.White } };
            return vismesh;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            _requiresRedraw = true;

            return input;
        }
    }
}
