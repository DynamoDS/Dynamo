using System;
using System.Collections.Generic;

using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Interfaces;

using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace Dynamo.Nodes
{
    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("WatchNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("WatchNodeSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class Watch : NodeModel
    {
        public event Action<Object> EvaluationComplete;
        public new object CachedValue;

        /// <summary>
        ///     Has the Watch node been run once?  If not, the CachedValue
        ///     is technically not accurate.
        /// </summary>
        public bool HasRunOnce { get; private set; }

        public Watch()
        {
            InPortData.Add(new PortData("", "Node to evaluate."));
            OutPortData.Add(new PortData("", "Watch contents."));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            ShouldDisplayPreviewCore = false;
            HasRunOnce = false;
        }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            DataBridge.Instance.RegisterCallback(GUID.ToString(), OnEvaluationComplete);
        }

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        private void OnEvaluationComplete(object obj)
        {
            this.CachedValue = obj;
            this.HasRunOnce = true;

            if (EvaluationComplete != null)
            {
                EvaluationComplete(obj);
            }
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
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory)
        {
            // Do nothing
        }
    }
}
