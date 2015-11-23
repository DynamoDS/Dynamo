using System;
using System.Collections.Generic;
using DSCoreNodesUI.Properties;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Scheduler;
using Dynamo.Visualization;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace DSCoreNodesUI
{
    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("WatchNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("WatchNodeSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.Watch")]
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
            InPortData.Add(new PortData("", Resources.WatchPortDataInputToolTip));
            OutPortData.Add(new PortData("", Resources.WatchPortDataResultToolTip));

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

        public override bool RequestVisualUpdateAsync(
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory, bool forceUpdate = false)
        {
            // Do nothing
            return false;
        }
    }
}
