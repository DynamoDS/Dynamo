using System.Collections.Generic;
using DSCoreNodesUI.Properties;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Scheduler;
using Dynamo.Visualization;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [NodeName("Watch Image")]
    [NodeDescription("WatchImageDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("WatchImageSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Dynamo.Nodes.WatchImageCore")]
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

        public override void RequestVisualUpdateAsync(
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory, bool forceUpdate = false)
        {
            //Do nothing
        }
    }

}