using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Autodesk.DesignScript.Geometry;

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

        public ColorRange()
        {
            InPortData.Add(new PortData("colors", "A list of colors to include in the range."));
            InPortData.Add(new PortData("indices", "A list of values between 0.0 and 1.0 which position the colors along the range."));
            InPortData.Add(new PortData("value", "A list of values between 0 and 1. These values are used to look up the color within the range."));
            OutPortData.Add(new PortData("color", "The selected color."));

            RegisterAllPorts();


            ArgumentLacing = LacingStrategy.Disabled;

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
                    new Func<IList<Color>, IList<double>, double, Color>(Color.BuildColorFrom1DRange),
                    inputAstNodes);
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }

    //[IsDesignScriptCompatible]
    //[NodeName("Color Range 2D")]
    //[NodeCategory("Core.Color.Create")]
    //[NodeDescription("Get a color given a color range.")]
    //public class ColorRange2D : NodeModel
    //{
    //    public event EventHandler RequestChangeColorRange;
    //    protected virtual void OnRequestChangeColorRange(object sender, EventArgs e)
    //    {
    //        if (RequestChangeColorRange != null)
    //            RequestChangeColorRange(sender, e);
    //    }

    //    public ColorRange2D(WorkspaceModel workspace)
    //        : base(workspace)
    //    {
    //        InPortData.Add(new PortData("colors", "A list of colors to include in the range."));
    //        InPortData.Add(new PortData("parameters", "A list of UVs between (0.0,0.0) and (1.0,1.0) which position the colors within the range."));
    //        InPortData.Add(new PortData("values", "A list of UVs between in the (0.0) to (1.0,1.0) domain. These values are used to look up the color within the range."));
    //        OutPortData.Add(new PortData("color", "The selected color."));

    //        RegisterAllPorts();

    //        this.PropertyChanged += ColorRange_PropertyChanged;

    //        ArgumentLacing = LacingStrategy.Disabled;
    //    }

    //    void ColorRange_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //    {
    //        if (e.PropertyName != "IsUpdated")
    //            return;

    //        if (InPorts.Any(x => x.Connectors.Count == 0))
    //            return;

    //        OnRequestChangeColorRange(this, EventArgs.Empty);
    //    }

    //    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    //    {
    //        var functionCall =
    //            AstFactory.BuildFunctionCall(
    //                new Func<IList<Color>, IList<UV>, UV, Color>(Color.BuildColorFrom2DRange),
    //                inputAstNodes);
    //        return new[]
    //        {
    //            AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
    //        };
    //    }
    //}

}
