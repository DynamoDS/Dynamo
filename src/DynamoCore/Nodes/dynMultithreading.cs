using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Xml;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Future")]
    [NodeDescription("Runs the given thunk (0-argument function) in a separate thread.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    public class Future : NodeWithOneOutput
    {
        public Future()
        {
            InPortData.Add(new PortData("thunk", "Function to evaluate in a new thread.",
                typeof (FScheme.Value.Function)));
            OutPortData.Add(new PortData("receipt", "Receipt to this future evaluation.", typeof (object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var thunk = ((FScheme.Value.Function) args[0]).Item;
            var task = FScheme.MakeFuture(thunk);

            return FScheme.Value.NewContainer(task);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    [NodeName("Now")]
    [NodeDescription("Fetches the result of a future evaluation, waiting for completion if necessary.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    public class Now : NodeWithOneOutput
    {
        public Now()
        {
            InPortData.Add(new PortData("receipt", "Receipt to a future evaluation.", typeof (object)));
            OutPortData.Add(new PortData("result", "Result of the future evaluation.", typeof (object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var task = (Task<FScheme.Value>) ((FScheme.Value.Container) args[0]).Item;

            return FScheme.Redeem(task);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    [NodeName("Create Thunk")]
    [NodeDescription("Wraps the attached upstream workflow in a 0-argument function.")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    public class Thunk : NodeWithOneOutput
    {
        public Thunk()
        {
            InPortData.Add(new PortData("body", "Body of the thunk.", typeof(object)));
            OutPortData.Add(new PortData("thunk", "Thunk that will evaluate the given body when executed.", typeof(FScheme.Value.Function)));

            RegisterAllPorts();
        }

        protected internal override INode Build(Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> outputs;
            if (preBuilt.TryGetValue(this, out outputs))
                return outputs[outPort];

            Tuple<int, NodeModel> input;
            if (TryGetInput(0, out input))
                return new AnonymousFunctionNode(input.Item2.Build(preBuilt, input.Item1));

            return base.Build(preBuilt, outPort);
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return FScheme.Value.NewFunction(Utils.ConvertToFSchemeFunc(_ => args[0]));
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }
}
