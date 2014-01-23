using System;
using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Create a custom node.
    /// </summary>
    [NodeName("Node with Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [NodeDescription("A description for your node which will appear in the tooltip.")]
    public class CustomNode : NodeModel, IWpfNode
    {
        public CustomNode()
        {
            //Define some input ports an input port will be created for 
            //each port data object you add to the InPortData collection
            InPortData.Add(new PortData("in", "The first port's description.", typeof (FScheme.Value.Number)));

            //Define some output ports an output port will be created for 
            //the port data object you add to the OutPortData collection
            OutPortData.Add(new PortData("out", "The output value.", typeof (FScheme.Value.Number)));

            //Setup all the ports on the node
            RegisterAllPorts();
        }

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //If you have custom UI elements which you want to
            //add to the node, set them up here.
            //Add custom UI elements to the NodeUI.InputGrid.Children collection
            //to have them appear in the center of the node.
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException("FILL ME IN");
        }
    }
}