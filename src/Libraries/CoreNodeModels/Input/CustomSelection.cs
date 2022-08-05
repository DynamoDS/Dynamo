using Autodesk.DesignScript.Runtime;

using Dynamo.Graph.Nodes;

using ProtoCore.AST.AssociativeAST;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CoreNodeModelsWpf")]

namespace CoreNodeModels.Input
{
    /// <summary>
    /// This node allow the user to create a dropdown menu with an with an arbitrary number of customization items
    /// </summary>
    [NodeName("Custom Dropdown Menu")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("CustomSelectionNodeDescription", typeof(Properties.Resources))]
    [NodeSearchTags("CustomSelectionSearchTags", typeof(Properties.Resources))]
    [OutPortNames("value")]
    [OutPortTypes("var")]
    [OutPortDescriptions("Selected value")]
    [IsDesignScriptCompatible]
    public class CustomSelectionNodeModel : DSDropDownBase
    {
        /// <summary>
        /// Construct a new Custom Dropdown Menu node
        /// </summary>
        public CustomSelectionNodeModel() : base("Value")
        {
            // TODO: This isn't done in RevitDropDown. Necessary?
            ArgumentLacing = LacingStrategy.Disabled;

            Items.Add(new DynamoDropDownItem("One", "1"));
            Items.Add(new DynamoDropDownItem("Two", "2"));
            Items.Add(new DynamoDropDownItem("Three", "3"));
        }

        /// <summary>
        /// Build the AST for this node
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode associativeNode = AstFactory.BuildPrimitiveNodeFromObject(GetSelectedValue());

            return new List<AssociativeNode> { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), associativeNode) };
        }

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="ex"></param>
        [IsVisibleInDynamoLibrary(false)]
        internal void Log(Exception ex)
        {
            Log(ex.Message, Dynamo.Logging.WarningLevel.Error);
            Log(ex.StackTrace, Dynamo.Logging.WarningLevel.Error);
        }

        /// <summary>
        /// Return the selected item as an int, or a double, or a string
        /// </summary>
        /// <returns></returns>
        private object GetSelectedValue()
        {
            if (SelectedIndex == -1)
            {
                return null;
            }

            DynamoDropDownItem selectedItem = Items[SelectedIndex];

            if (selectedItem?.Item is string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }

                if (Items.All(item => item is null || ( item.Item is string v && ( string.IsNullOrEmpty(v) || int.TryParse(v, out _) ) )))
                {
                    int.TryParse(value, out int intValue);
                    return intValue;
                }

                if (Items.All(item => item is null || ( item.Item is string v && ( string.IsNullOrEmpty(v) || double.TryParse(v, out _) ) )))
                {
                    double.TryParse(value, out double doubleValue);
                    return doubleValue;
                }

                return value;
            }

            return selectedItem?.Item;
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            return SelectionState.Restore;
        }
    }
}