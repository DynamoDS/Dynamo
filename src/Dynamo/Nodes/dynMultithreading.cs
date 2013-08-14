using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;

namespace Dynamo.Nodes
{
    [NodeName("Future")]
    [NodeDescription("Runs the given thunk (0-argument function) in a separate thread.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    public class dynFuture : dynNodeWithOneOutput
    {
        public dynFuture()
        {
            InPortData.Add(new PortData("thunk", "Function to evaluate in a new thread.",
                typeof (FScheme.Value.Function)));
            OutPortData.Add(new PortData("receipt", "Receipt to this future evaluation.", typeof (object)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var thunk = ((FScheme.Value.Function) args[0]).Item;
            var task = FScheme.MakeFuture(thunk);

            return FScheme.Value.NewContainer(task);
        }
    }

    [NodeName("Now")]
    [NodeDescription("Fetches the result of a future evaluation, waiting for completion if necessary.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    public class dynNow : dynNodeWithOneOutput
    {
        public dynNow()
        {
            InPortData.Add(new PortData("receipt", "Receipt to a future evaluation.", typeof (object)));
            OutPortData.Add(new PortData("result", "Result of the future evaluation.", typeof (object)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var task = (Task<FScheme.Value>) ((FScheme.Value.Container) args[0]).Item;

            return FScheme.Redeem(task);
        }
    }

    [NodeName("Create Thunk")]
    [NodeDescription("Wraps the attached upstream workflow in a 0-argument function.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    public class dynThunk : dynNodeWithOneOutput
    {
        public dynThunk()
        {
            InPortData.Add(new PortData("body", "Body of the thunk.", typeof(object)));
            OutPortData.Add(new PortData("thunk", "Thunk that will evaluate the given body when executed.", typeof(FScheme.Value.Function)));

            RegisterAllPorts();
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> outputs;
            if (preBuilt.TryGetValue(this, out outputs))
                return outputs[outPort];

            Tuple<int, dynNodeModel> input;
            if (TryGetInput(0, out input))
                return new AnonymousFunctionNode(input.Item2.Build(preBuilt, input.Item1));

            return base.Build(preBuilt, outPort);
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewFunction(Utils.ConvertToFSchemeFunc(_ => args[0]));
        }
    }
}
