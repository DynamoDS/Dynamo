using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels;
using DSCore;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeometryUI
{
    [IsVisibleInDynamoLibrary(true)]
    [NodeName("PanelSurfaceBoundaryCondition")]
    [NodeCategory("Geometry.PanelSurface")]
    [NodeDescription("PanelSurfaceBoundaryConditionDropDownDesc", typeof(Properties.Resources))]
    [OutPortNames("PanelSurfaceBoundaryCondition")]
    [OutPortTypes(nameof(PanelSurfaceBoundaryCondition))]
    [OutPortDescriptions("PanelSurface BoundaryCondition enum value")]
    [IsDesignScriptCompatible]
    public class PanelSurfaceBoundaryConditionDropDown : DSDropDownBase
    {
        public PanelSurfaceBoundaryConditionDropDown(): base("PanelSurfaceBoundaryCondition") {
            SelectedIndex = (int)PanelSurfaceBoundaryCondition.Keep;
        }

        [JsonConstructor]
        protected PanelSurfaceBoundaryConditionDropDown(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(">", inPorts, outPorts) { }

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

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();
            foreach (var constant in Enum.GetValues(typeof(PanelSurfaceBoundaryCondition)))
            {
                Items.Add(new DynamoDropDownItem(constant.ToString(), constant));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        /// <summary>
        /// Builds the output AST.
        /// </summary>
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
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
