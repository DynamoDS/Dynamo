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
            InitializePorts();

            this.PropertyChanged += ColorRange_PropertyChanged; 
            
            ShouldDisplayPreviewCore = false;
        }

        protected virtual void InitializePorts()
        {
            InPortData.Add(new PortData("colors", Resources.ColorRangePortDataColorsToolTip));
            InPortData.Add(new PortData("indices", Resources.ColorRangePortDataIndicesToolTip));
            InPortData.Add(new PortData("value", Resources.ColorRangePortDataValueToolTip));
            OutPortData.Add(new PortData("color", Resources.ColorRangePortDataResultToolTip));

            RegisterAllPorts();
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
            var buildColorRangeNode =
                AstFactory.BuildFunctionCall(
                    new Func<List<Color>, List<double>, ColorRange1D>(ColorRange1D.ByColorsAndParameters),
                    new List<AssociativeNode>(){inputAstNodes[0], inputAstNodes[1]});

            var functionCall =
                AstFactory.BuildFunctionCall(
                    new Func<ColorRange1D,double, Color>(ColorRange1D.GetColorAtParameter),
                    new List<AssociativeNode>(){buildColorRangeNode, inputAstNodes[2]});

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall),
            };
        }
    }
}
