using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels;
using DSCore;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;

namespace GeometryUIWpf
{
    [IsVisibleInDynamoLibrary(true)]
    [NodeName("PanelSurfaceBoundaryConditions")]
    [NodeCategory("Geometry.PanelSurface")]
    [OutPortNames(">")]
    [IsDesignScriptCompatible]
    public class PanelSurfaceBoundaryConditionDropDown : EnumBase<PanelSurfaceBoundaryCondition>
    {
        /// <summary>
        /// Overrides the default behavior to serialize internal enumeration id 
        /// instead of the name of the enum constant.
        /// </summary>
        /// <param name="item">Selected DynamoDropDownItem</param>
        /// <returns>A string representing the internal enum id.</returns>
        protected override string GetSelectedStringFromItem(DynamoDropDownItem item)
        {
            return item == null ? string.Empty : item.Item.ToString();
        }

        /// <summary>
        /// Builds the output AST.
        /// </summary>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //Some of the legacy categories which were not working before will now be out of index.
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
                return new[] { AstFactory.BuildNullNode() };

            var selection =
                AstFactory.BuildStringNode(Items[SelectedIndex].Name);

            var func = AstFactory.BuildFunctionCall(
             new Func<string, PanelSurfaceBoundaryCondition>(BoundaryConditionHelper.BoundaryConditionFromString),
             new List<AssociativeNode> { selection });

            // Assign the selected name to an actual enumeration value
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), func);

            // Return the enumeration value
            return new List<AssociativeNode> { assign };
        }
    }

}
