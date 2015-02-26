using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

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

            // Setting the EnablePeriodicUpdate flag will enable periodic updating
            // in the view.
            EnablePeriodicUpdate = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            t += 0.1;

            if (t > Math.PI*2) t = 0.0;

            var doubleNode = AstFactory.BuildDoubleNode(t);
            var funcNode = AstFactory.BuildFunctionCall(new Func<double, PointField>(PointField.ByParameter), new List<AssociativeNode>() { doubleNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), funcNode) };
        }
    }
}
