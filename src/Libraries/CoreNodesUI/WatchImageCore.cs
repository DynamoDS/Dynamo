using System.Collections.Generic;

using Dynamo.Core.Threading;
using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;
using Image = System.Windows.Controls.Image;

namespace Dynamo.Nodes
{
    [NodeName("Watch Image")]
    [NodeDescription("Previews an image")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeSearchTags("image")]
    [IsDesignScriptCompatible]
    public class WatchImageCore : NodeModel
    {
        private Image image;

        public WatchImageCore()
        {
            InPortData.Add(new PortData("image", "image"));
            OutPortData.Add(new PortData("image", "image"));

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