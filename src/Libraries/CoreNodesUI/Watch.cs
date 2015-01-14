using System.Collections.Generic;
using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

using VMDataBridge;

namespace Dynamo.Nodes
{
    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Visualize the output of node. ")]
    [NodeSearchTags("print", "output", "display")]
    [IsDesignScriptCompatible]
    public class Watch : NodeModel
    {
        private IdentifierNode astBeingWatched;
        public new object CachedValue { get; internal set; }

        public Watch()
        {
            InPortData.Add(new PortData("", "Node to evaluate."));
            OutPortData.Add(new PortData("", "Watch contents."));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            ShouldDisplayPreviewCore = false;
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return outputIndex == 0
                ? AstIdentifierForPreview
                : base.GetAstIdentifierForOutputIndex(outputIndex);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionObject(
                            new IdentifierListNode
                            {
                                LeftNode = AstFactory.BuildIdentifier("DataBridge"),
                                RightNode = AstFactory.BuildIdentifier("BridgeData")
                            },
                            2,
                            new[] { 0 },
                            new List<AssociativeNode>
                            {
                                AstFactory.BuildStringNode(GUID.ToString()),
                                AstFactory.BuildNullNode()
                            }))
                };
            }

            var resultAst = new[]
            {
                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), inputAstNodes[0])),
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0])
            };

            return resultAst;
        }

        protected override void RequestVisualUpdateAsyncCore(
            IScheduler scheduler, EngineController engine, int maxTesselationDivisions)
        {
            // Do nothing
        }
        
    }
}
