using Dynamo.Models;
using Microsoft.FSharp.Collections;

//IMPORTANT!! In order for Dynamo to recognize your node, it must
//be in the Dynamo.Nodes workspace. 
namespace Dynamo.Nodes
{
    /// <summary>
    /// A node which has one output and returns a number
    /// </summary>
    [NodeName("Node with Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [NodeDescription("A description for your node which will appear in the tooltip.")]
    public class NodeWithNumber : NodeWithOneOutput
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public NodeWithNumber()
        {
            //Define some input ports an input port will be created for 
            //each port data object you add to the InPortData collection
            InPortData.Add(new PortData("in", "The first port's description.", typeof (FScheme.Value.Number)));

            //Define some output ports an output port will be created for 
            //the port data object you add to the OutPortData collection.
            OutPortData.Add(new PortData("out", "The output value.", typeof (FScheme.Value.Number)));

            //Setup all the ports on the node
            RegisterAllPorts();
        }

        /// <summary>
        /// Called after the construction of the node object, from the UI layer, to allow for the setup
        /// of additional UI elements (buttons, sliders, etc.) on the node. 
        /// </summary>
        /// <param name="nodeUI">The node ui view into which your custom elements will be added.</param>
        public void SetupCustomUIElements(object nodeUI)
        {
            //If you have custom UI elements which you want to
            //add to the node, set them up here.
            //Add custom UI elements to the NodeUI.InputGrid.Children collection
            //to have them appear in the center of the node.
        }

        /// <summary>
        /// The evaluation entry point for a node.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>An FScheme Value object.</returns>
        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            //let's say the thing coming in is a number.
            //unwrap the number from it's value here
            var number = ((FScheme.Value.Number) args[0]).Item;

            //do something with this value
            var sometOtherNumber = 42 + number;

            //Return a Value object
            return FScheme.Value.NewNumber(sometOtherNumber);
        }
    }

    /// <summary>
    /// A node which has one output and returns a list.
    /// </summary>
    [NodeName("Node With List")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [NodeDescription("A description for your node which will appear in the tooltip.")]
    public class NodeWithList : NodeWithOneOutput
    {
        public NodeWithList()
        {
            //Define some input ports an input port will be created for 
            //each port data object you add to the InPortData collection
            InPortData.Add(new PortData("in", "The first port's description.", typeof(FScheme.Value.Container)));

            //Define some output ports an output port will be created for 
            //the port data object you add to the OutPortData collection
            OutPortData.Add(new PortData("out", "The output value.", typeof(FScheme.Value.List)));

            //Setup all the ports on the node
            RegisterAllPorts();
        }

        public void SetupCustomUIElements(object nodeUI)
        {
            //If you have custom UI elements which you want to
            //add to the node, set them up here.
            //Add custom UI elements to the NodeUI.InputGrid.Children collection
            //to have them appear in the center of the node.
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            //let's say the thing coming in is a list
            //unwrap an enumerable from the 
            var list = ((FScheme.Value.List) args[0]).Item;

            //let's say that you want to pass out a list
            //start by setting up an empty list
            var result = FSharpList<FScheme.Value>.Empty;

            foreach (var thing in list)
            {
                //add some stuff to your list
                //a cons operation takes a 'head' and the 'tail' is your list
                //this means your list will come out backwards from how you put it in.
                FSharpList<FScheme.Value>.Cons(FScheme.Value.NewNumber(42), result);
            }

            //return your list wrapped in a value
            return FScheme.Value.NewList(result);
        }
    }
}