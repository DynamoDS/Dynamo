//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Dynamo.FSchemeInterop;

using Value = Dynamo.FScheme.Value;
using Dynamo.Revit;

using Autodesk.Revit;
using Autodesk.Revit.DB;

namespace Dynamo.Nodes
{
    [NodeName("Transient Geometry")]
    [NodeCategory(BuiltinNodeCategories.REVIT_VIEW)]
    [NodeDescription("Diplays geometry with Revit transient geometry.")]
    public class dynTransientGeometryNode : dynRevitTransactionNodeWithOneOutput
    {
        ElementId _keeperId = null;

        public dynTransientGeometryNode()
        {
            InPortData.Add(new PortData("IN", "Incoming geometry objects.", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Watch contents, passed through", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private void GetUpstreamGeometryObjects(List<GeometryObject> geometryObjects, Dictionary<int, Tuple<int, dynNodeModel>> inputs)
        {
            foreach (KeyValuePair<int, Tuple<int, dynNodeModel>> pair in inputs)
            {
                if (pair.Value == null)
                    continue;

                dynNodeModel node = pair.Value.Item2;
                dynGeometryBase geometryNode = node as dynGeometryBase;

                if (node.IsVisible && geometryNode != null)
                    geometryObjects.AddRange(geometryNode.GeometryObjects);

                if (node.IsUpstreamVisible)
                    GetUpstreamGeometryObjects(geometryObjects, node.Inputs);
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
                        GetUpstreamGeometryObjects(geometryObjects, innerNode.Inputs);
                    }
                }
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            List<GeometryObject> geometryObjects = new List<GeometryObject>();

            GetUpstreamGeometryObjects(geometryObjects, Inputs);

            if (_keeperId != null)
            {
                GeometryElement.SetForTransientDisplay(
                    dynRevitSettings.Doc.Document, ElementId.InvalidElementId,
                    new List<GeometryObject>(), ElementId.InvalidElementId);
                this.DeleteElement(_keeperId);
            }

            _keeperId = GeometryElement.SetForTransientDisplay(
                dynRevitSettings.Doc.Document, ElementId.InvalidElementId,
                geometryObjects, ElementId.InvalidElementId);

            this.Elements.Add(_keeperId);

            return Value.NewContainer(dynRevitSettings.Doc.Document.GetElement(_keeperId));
        }
    }
}