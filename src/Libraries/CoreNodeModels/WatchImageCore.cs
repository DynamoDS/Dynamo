using System.Collections.Generic;
using CoreNodeModels.Properties;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Scheduler;
using Dynamo.Visualization;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    [NodeName("Watch Image")]
    [NodeDescription("WatchImageDescription", typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("WatchImageSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortTypes("var")]
    [AlsoKnownAs("Dynamo.Nodes.WatchImageCore", "DSCoreNodesUI.WatchImageCore")]
    public class WatchImageCore : NodeModel
    {
        [JsonConstructor]
        private WatchImageCore(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ShouldDisplayPreviewCore = false;
        }

        public WatchImageCore()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("image", Resources.PortDataImageToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("image", Resources.PortDataImageToolTip)));

            RegisterAllPorts(); 
            
            ShouldDisplayPreviewCore = false;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            yield return AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0]);
        }

        public override bool RequestVisualUpdateAsync(
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory, bool forceUpdate = false)
        {
            //Do nothing
            return false;
        }
    }

}