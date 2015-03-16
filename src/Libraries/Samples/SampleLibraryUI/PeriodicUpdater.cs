using System;
using System.Collections.Generic;

using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

using SampleLibraryZeroTouch;

namespace SamplesLibraryUI
{
    [NodeName("Periodic Update")]
    [NodeCategory("Sample Nodes")]
    [NodeDescription("This node demonstrates the use of periodic update.")]
    [IsDesignScriptCompatible]
    public class PeriodicUpdater : NodeModel
    {
        private double t = 0.0;

        public PeriodicUpdater()
        {
            OutPortData.Add(new PortData("stuff", "The stuff that has been periodically updated."));
            RegisterAllPorts();

            // When EnablePeriodicUpdate is set to true, 
            // Periodic Run will be enabled in the Dynamo UI
            // and you will be able to set an interval at which
            // the graph updates.
            EnablePeriodicUpdate = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // Increment this parameter that will be fed into the
            // method that we periodically update.
            t += 0.1;
            if (t > Math.PI*2) t = 0.0;

            // The method that we update periodically is in the SampleLibraryZeroTouch assembly.
            // It's called by creating a DoubleNode in the AST representing the input parameter, t,
            // and a FunctionCallNode representing the call to the function PointField.ByParameter.

            var doubleNode = AstFactory.BuildDoubleNode(t);
            var funcNode = AstFactory.BuildFunctionCall(new Func<double, PointField>(PointField.ByParameter), new List<AssociativeNode>() { doubleNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode) };
        }
    }
}
