using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo;
using Dynamo.Connectors;
using Dynamo.Commands;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Connectors;
using Dynamo.Utilities;
using Dynamo.Revit;

using Value = Dynamo.FScheme.Value;
using Microsoft.FSharp.Collections;

namespace Dyamo.Nodes
{
    /// <summary>
    /// Create a custom node.
    /// </summary>
    [Dynamo.Nodes.NodeName("My Node")]
    [Dynamo.Nodes.NodeCategory(Dynamo.Nodes.BuiltinNodeCategories.SCRIPTING_CUSTOMNODES)]
    [Dynamo.Nodes.NodeDescription("A description for your node which will appear in the tooltip.")]
    public class CustomNode:dynRevitTransactionNodeWithOneOutput
    {
        public CustomNode()
        {
            //Define some input ports an input port will be created for 
            //each port data object you add to the InPortData collection
            InPortData.Add(new PortData("in", "The first port's description.", typeof(Value.Container)));

            //Define some output ports an output port will be created for 
            //the port data object you add to the OutPortData collection
            OutPortData.Add(new PortData("out", "The output value.", typeof(Value.Container)));

            //Setup all the ports on the node
            RegisterAllPorts();
        }

        public override void  SetupCustomUIElements(dynNodeView NodeUI)
        {
 	        //If you have custom UI elements which you want to
            //add to the node, set them up here.
            //Add custom UI elements to the NodeUI.InputGrid.Children collection
            //to have them appear in the center of the node.
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //Return a Value object
            return Value.NewNumber(0);
        }
    }
}
