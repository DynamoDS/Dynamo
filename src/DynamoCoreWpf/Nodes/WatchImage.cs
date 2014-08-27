using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.Wpf;

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
        public WatchImageCore(WorkspaceModel ws) : base(ws)
        {
            InPortData.Add(new PortData("image", "image"));
            OutPortData.Add(new PortData("image", "image"));

            RegisterAllPorts();
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0])
            };

            return resultAst;
        }
        
        public override void UpdateRenderPackage(int maxTessDivisions)
        {
            //do nothing
            //a watch should not draw its outputs
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }

}
