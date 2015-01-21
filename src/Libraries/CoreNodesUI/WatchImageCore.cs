using System.Collections.Generic;

using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Models;

using DSCoreNodesUI.Properties;


using ProtoCore.AST.AssociativeAST;
using Image = System.Windows.Controls.Image;

namespace Dynamo.Nodes
{

    [NodeName(/*NXLT*/"Watch Image")]
    [NodeDescription(/*NXLT*/"WatchImageDescription", typeof(Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("WatchImageSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    public class WatchImageCore : NodeModel
    {
        private Image image;

        public WatchImageCore()
        {
            InPortData.Add(new PortData(/*NXLT*/"image", Resources.PortDataImageToolTip));
            OutPortData.Add(new PortData(/*NXLT*/"image", Resources.PortDataImageToolTip));

            RegisterAllPorts(); 
            
            ShouldDisplayPreviewCore = false;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            yield return AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0]);
        }

        protected override void RequestVisualUpdateAsyncCore(
            IScheduler scheduler, EngineController engine, int maxTesselationDivisions)
        {
            //Do nothing
        }
    }

}