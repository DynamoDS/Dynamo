using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Revit;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    /// <summary>
    ///     Create a custom node.
    /// </summary>
    [NodeName("My Node")]
    [NodeCategory(BuiltinNodeCategories.SCRIPTING_CUSTOMNODES)]
    [NodeDescription("A description for your node which will appear in the tooltip.")]
    public class CustomNode : dynRevitTransactionNodeWithOneOutput
    {
        public CustomNode()
        {
            //Define some input ports an input port will be created for 
            //each port data object you add to the InPortData collection
            InPortData.Add(new PortData("in", "The first port's description.", typeof (FScheme.Value.Container)));

            //Define some output ports an output port will be created for 
            //the port data object you add to the OutPortData collection
            OutPortData.Add(new PortData("out", "The output value.", typeof (FScheme.Value.Container)));

            //Setup all the ports on the node
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //If you have custom UI elements which you want to
            //add to the node, set them up here.
            //Add custom UI elements to the NodeUI.InputGrid.Children collection
            //to have them appear in the center of the node.
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            //Return a Value object
            return FScheme.Value.NewNumber(0);
        }
    }
}