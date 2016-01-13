using System.Collections.Generic;
using CoreNodeModels.Properties;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Scheduler;
using Dynamo.Visualization;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels
{
    [NodeName("Watch Image")]
    [NodeDescription("WatchImageDescription", typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("WatchImageSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.WatchImageCore", "DSCoreNodesUI.WatchImageCore")]
    public class WatchImageCore : NodeModel
    {
        public WatchImageCore()
        {
            InPortData.Add(new PortData("image", Resources.PortDataImageToolTip));
            OutPortData.Add(new PortData("image", Resources.PortDataImageToolTip));

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