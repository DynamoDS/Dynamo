using System.Collections.Generic;
using Microsoft.FSharp.Collections;

namespace Dynamo.Models
{
    public abstract class NodeWithOneOutput : NodeModel
    {
        public override void Evaluate(FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public abstract FScheme.Value Evaluate(FSharpList<FScheme.Value> args);
    }
}