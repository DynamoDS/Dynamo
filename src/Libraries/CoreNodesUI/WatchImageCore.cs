using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;

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
            : base()
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

        protected override void RequestVisualUpdateAsyncCore(int maxTesselationDivisions)
        {
            return; // No visualization update is required for this node type.
        }
        
        public override void UpdateRenderPackage(int maxTessDivisions)
        {
            //do nothing
            //a watch should not draw its outputs
        }
    }

}