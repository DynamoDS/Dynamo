using DSCoreNodes;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Domain 2D")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("Create a two dimensional domain specifying the Minimum and Maximum UVs.")]
    public class dynDomain : dynNodeWithOneOutput
    {
        public dynDomain()
        {
            InPortData.Add(new PortData("min", "The minimum UV of the domain.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("max", "The maximum UV of the domain.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("domain", "A domain.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var min = (Autodesk.LibG.Vector)((FScheme.Value.Container)args[0]).Item;
            var max = (Autodesk.LibG.Vector)((FScheme.Value.Container)args[1]).Item;

            return FScheme.Value.NewContainer(Domain2D.ByMinimumAndMaximum(min, max));
        }
    }

    [NodeName("Domain")]
    [NodeCategory(BuiltinNodeCategories.REVIT)]
    [NodeDescription("Create a domain specifying the Minimum and Maximum UVs.")]
    public class dynDomain1 : dynNodeWithOneOutput
    {
        public dynDomain1()
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
