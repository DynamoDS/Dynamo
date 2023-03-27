using System;
using System.Collections.Generic;
using CoreNodeModels.Properties;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Scheduler;
using Dynamo.Visualization;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;

namespace CoreNodeModels
{
    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("WatchNodeDescription", typeof(Resources))]
    [NodeSearchTags("WatchNodeSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortTypes("var")]
    [AlsoKnownAs("Dynamo.Nodes.Watch", "DSCoreNodesUI.Watch")]
    public class Watch : NodeModel
    {
        public event Action<Object> EvaluationComplete;

        [JsonIgnore]
        public new object CachedValue;

        /// <summary>
        ///     Has the Watch node been run once?  If not, the CachedValue
        ///     is technically not accurate.
        /// </summary>
        [JsonIgnore]
        public bool HasRunOnce { get; private set; }

        [JsonConstructor]
        private Watch(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            ShouldDisplayPreviewCore = false;
            HasRunOnce = false;
        }

        /// <summary>
        ///     Watch node's Width
        public double WatchWidth { get; set; }
        /// <summary>
        ///     Watch node's Height
        public double WatchHeight { get; set; }

        //Stores the custom sizes for each watch node.
        public static Dictionary<Guid, Tuple<double,double>> NodeSizes = new Dictionary<Guid, Tuple<double, double>>();

        public void SetWatchSize(double w, double h)
        {
            WatchWidth = w;
            WatchHeight = h;
        }

        public Watch()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("", Resources.WatchPortDataInputToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", Resources.WatchPortDataResultToolTip)));

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
