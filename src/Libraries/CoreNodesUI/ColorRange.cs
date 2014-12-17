using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DSCore;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [IsDesignScriptCompatible]
    [NodeName("Color Range")]
    [NodeCategory("Core.Color.Create")]
    [NodeDescription("Get a color given a color range.")]
    public class ColorRange : NodeModel
    {
        public event EventHandler RequestChangeColorRange;
        protected virtual void OnRequestChangeColorRange(object sender, EventArgs e)
        {
            if (RequestChangeColorRange != null)
                RequestChangeColorRange(sender, e);
        }

        public ColorRange(WorkspaceModel workspace)
            : base(workspace)
        {
            InPortData.Add(new PortData("start", "The start color."));
            InPortData.Add(new PortData("end", "The end color."));
            InPortData.Add(new PortData("value", "The value between 0 and 1 of the selected color."));
            OutPortData.Add(new PortData("color", "The selected color."));

            RegisterAllPorts();

            this.PropertyChanged += ColorRange_PropertyChanged;
        }

        void ColorRange_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsUpdated")
                return;

            if (InPorts.Any(x => x.Connectors.Count == 0))
                return;

            OnRequestChangeColorRange(this, EventArgs.Empty);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //var functionCall = AstFactory.BuildFunctionCall("Color", "BuildColorFromRange", inputAstNodes);
            var functionCall =
                AstFactory.BuildFunctionCall(
                    new Func<Color, Color, double, Color>(Color.BuildColorFromRange),
                    inputAstNodes);
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }

        protected override bool ShouldDisplayPreviewCore
        {
            get
            {
                return false; // Previews are not shown for this node type.
            }
        }

    }
}
