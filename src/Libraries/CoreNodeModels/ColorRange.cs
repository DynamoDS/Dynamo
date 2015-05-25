using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using DSCore;
using Dynamo.Models;
using DSCoreNodesUI.Properties;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    [IsDesignScriptCompatible]
    [NodeName("Color Range")]
    [NodeCategory("Core.Color.Create")]
    [NodeDescription("ColorRangeDescription",typeof(DSCoreNodesUI.Properties.Resources))]
    public class ColorRange : NodeModel
    {
        public event EventHandler RequestChangeColorRange;
        protected virtual void OnRequestChangeColorRange(object sender, EventArgs e)
        {
            if (RequestChangeColorRange != null)
                RequestChangeColorRange(sender, e);
        }

        public ColorRange()
        {
            InPortData.Add(new PortData("colors", Resources.ColorRangePortDataColorsToolTip));
            InPortData.Add(new PortData("indices", Resources.ColorRangePortDataIndicesToolTip));
            InPortData.Add(new PortData("value", Resources.ColorRangePortDataValueToolTip));
            OutPortData.Add(new PortData("color",  Resources.ColorRangePortDataResultToolTip));

            RegisterAllPorts();

            this.PropertyChanged += ColorRange_PropertyChanged; 
            
            ShouldDisplayPreviewCore = false;
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
            var functionCall =
                AstFactory.BuildFunctionCall(
                    new Func<List<Color>, List<double>, double, Color>(Color.BuildColorFrom1DRange),
                    inputAstNodes);
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }
}
