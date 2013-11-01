using DSCoreNodes;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Domain")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_SURFACE_UV)]
    [NodeDescription("Create a domain specifying the Minimum and Maximum UVs.")]
    public class Domain : NodeWithOneOutput
    {
        public Domain()
        {
            InPortData.Add(new PortData("min", "The minimum of the domain.", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("max", "The maximum of the domain.", typeof(FScheme.Value.Number)));
            OutPortData.Add(new PortData("domain", "A domain.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var min = ((FScheme.Value.Number)args[0]).Item;
            var max = ((FScheme.Value.Number)args[1]).Item;

            return FScheme.Value.NewContainer(DSCoreNodes.Domain.ByMinimumAndMaximum(min, max));
        }
    }
}
