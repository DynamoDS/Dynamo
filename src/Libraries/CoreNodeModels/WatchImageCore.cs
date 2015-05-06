using System.Collections.Generic;

using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Interfaces;

using DSCoreNodesUI.Properties;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    [NodeName("Watch Image")]
    [NodeDescription("WatchImageDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("WatchImageSearchTags", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
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

        protected override void RequestVisualUpdateAsyncCore(
            IScheduler scheduler, EngineController engine, IRenderPackageFactory factory)
        {
            //Do nothing
        }
    }

}