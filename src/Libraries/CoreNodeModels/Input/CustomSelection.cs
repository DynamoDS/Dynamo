using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using Dynamo.Graph;
using Dynamo.Graph.Nodes;

using Newtonsoft.Json;

using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.Input
{
    /// <summary>
    /// This node allows the user to create a dropdown selection list with an arbitrary number of custom items.
    /// </summary>
    [NodeName("Custom Selection")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("CustomSelectionNodeDescription", typeof(Properties.Resources))]
    [NodeSearchTags("CustomSelectionSearchTags", typeof(Properties.Resources))]
    [OutPortNames("value")]
    [OutPortTypes("var")]
    [OutPortDescriptions(typeof(Properties.Resources), "CustomSelectionOutputDescription")]
    [IsDesignScriptCompatible]
    public class CustomSelection : DSDropDownBase
    {
        private List<DynamoDropDownItem> serializedItems;
        private bool isVisibleDropDownTextBlock = false;

        /// <summary>
        /// This property will Collapse or make Visible the TextBlock for the ComboBox template "RefreshComboBox" (by default will be Collapsed)
        /// </summary>
        public bool IsVisibleDropDownTextBlock
        {
            get
            {
                return isVisibleDropDownTextBlock;
            }
            set
            {
                isVisibleDropDownTextBlock = value;
                RaisePropertyChanged(nameof(IsVisibleDropDownTextBlock));
            }
        }

        /// <summary>
        /// Copy of <see cref="DSDropDownBase.Items"/> to be serialized./>
        /// </summary>
        [JsonProperty]
        protected List<DynamoDropDownItem> SerializedItems
        {
            get => serializedItems;
            set
            {
                serializedItems = value;

                Items.Clear();

                foreach (DynamoDropDownItem item in serializedItems)
                {
                    Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Construct a new Custom Dropdown Menu node
        /// </summary>
        public CustomSelection() : base("Value")
        {
            ArgumentLacing = LacingStrategy.Disabled;

            Items.Add(new DynamoDropDownItem("One", "1"));
            Items.Add(new DynamoDropDownItem("Two", "2"));
            Items.Add(new DynamoDropDownItem("Three", "3"));

            SelectedIndex = 0;
        }

        [JsonConstructor]
        protected CustomSelection(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("Value", inPorts, outPorts)
        {
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

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), associativeNode) };
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

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            serializedItems = Items.ToList();
        }
    }
}
