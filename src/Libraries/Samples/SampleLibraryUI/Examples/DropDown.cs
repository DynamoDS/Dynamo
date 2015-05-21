using System.Collections.Generic;
using DSCoreNodesUI;
using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace SamplesLibraryUI.Examples
{
    [NodeName("Drop Down Example")]
    [NodeDescription("An example drop down node.")]
    [IsDesignScriptCompatible]
    public class DropDownExample : DSDropDownBase
    {
        public DropDownExample() : base("item"){}

        public override void PopulateItems()
        {
            // The Items collection contains the elements
            // that appear in the list. For this example, we
            // clear the list before adding new items, but you
            // can also use the PopulateItems method to add items
            // to the list.

            Items.Clear();

            // Create a number of DynamoDropDownItem objects 
            // to store the items that we want to appear in our list.

            var newItems = new List<DynamoDropDownItem>()
            {
                new DynamoDropDownItem("Tywin", 0),
                new DynamoDropDownItem("Cersei", 1),
                new DynamoDropDownItem("Hodor",2)
            };

            Items.AddRange(newItems);

            // Set the selected index to something other
            // than -1, the default, so that your list
            // has a pre-selection.

            SelectedIndex = 0;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            // Build an AST node for the type of object contained in your Items collection.

            var intNode = AstFactory.BuildIntNode((int)Items[SelectedIndex].Item);
            var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), intNode);

            return new List<AssociativeNode> {assign};
        }
    }
}
