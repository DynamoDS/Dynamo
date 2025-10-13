using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.Input
{
    /// <summary>
    /// Test node similar to select model element node for Dynamo sandbox.
    /// This node is not visible in the Dynamo library by default.
    /// </summary>
    [NodeName("Test Select Model Element")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("TestSelectModelElementDescription", typeof(Resources))]
    [NodeSearchTags("TestSelectModelElementSearchTags", typeof(Resources))]
    [SupressImportIntoVM]
    [InPortTypes("UI Input")]
    [OutPortTypes("string")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    [AlsoKnownAs("CoreNodeModels.Input.TestSelectModelElement")]
    public class TestSelectModelElement : String
    {
        private bool _canSelect = true;
        private string _selectedElementId = "";

        /// <summary>
        /// The Element ID which is selected.
        /// </summary>
        [JsonProperty("SelectedElementId", Order = 11)]
        public string SelectedElementId
        {
            get { return _selectedElementId; }
            set
            {
                if (_selectedElementId != value)
                {
                    _selectedElementId = value;
                    Value = value; // Update the base string value
                    RaisePropertyChanged("SelectedElementId");
                    RaisePropertyChanged("Text");
                }
            }
        }

        /// <summary>
        /// Whether or not the Select button is enabled in the UI.
        /// </summary>
        [JsonProperty("CanSelect", Order = 12)]
        public bool CanSelect
        {
            get { return _canSelect; }
            set
            {
                if (_canSelect != value)
                {
                    _canSelect = value;
                    RaisePropertyChanged("CanSelect");
                }
            }
        }

        /// <summary>
        /// Display text for the node.
        /// </summary>
        public string Text
        {
            get
            {
                return string.IsNullOrEmpty(SelectedElementId) 
                    ? "No element selected" 
                    : $"Selected: {SelectedElementId}";
            }
        }

        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "TestSelectModelElementNode";
            }
        }

        [JsonConstructor]
        private TestSelectModelElement(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            Value = "";
            SelectedElementId = "";
            ShouldDisplayPreviewCore = false;
        }

        public TestSelectModelElement() : base()
        {
            Value = "";
            SelectedElementId = "";
            ShouldDisplayPreviewCore = false;
            
            // Set the output port tooltip
            if (OutPorts.Count > 0)
            {
                OutPorts[0].ToolTip = "Selected element ID as string";
            }
        }

        /// <summary>
        /// Simulates element selection for testing purposes.
        /// In a real implementation, this would interact with the host application.
        /// </summary>
        public void SelectElement()
        {
            try
            {
                CanSelect = false;

                // Simulate selection - in a real implementation this would:
                // 1. Show a selection prompt to the user
                // 2. Wait for user to select an element in the host application
                // 3. Return the selected element's ID
                
                // For testing purposes, generate a mock element ID
                var mockElementId = $"Element_{Guid.NewGuid().ToString("N")[..8]}";
                SelectedElementId = mockElementId;

                CanSelect = true;
            }
            catch (Exception e)
            {
                CanSelect = true;
                // In a real implementation, you would log the error
                System.Diagnostics.Debug.WriteLine($"Selection error: {e.Message}");
            }
        }

        /// <summary>
        /// Test method to verify the node is working correctly.
        /// This can be called programmatically for testing.
        /// </summary>
        public void TestSelection()
        {
            SelectElement();
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            SelectedElementId = "";
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == "SelectedElementId")
            {
                SelectedElementId = value;
                return true;
            }

            if (name == "CanSelect" && bool.TryParse(value, out bool canSelect))
            {
                CanSelect = canSelect;
                return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            if (!string.IsNullOrEmpty(SelectedElementId))
            {
                var xmlDocument = element.OwnerDocument;
                var subNode = xmlDocument.CreateElement("SelectedElementId");
                subNode.InnerText = SelectedElementId;
                element.AppendChild(subNode);
            }

            var canSelectNode = element.OwnerDocument.CreateElement("CanSelect");
            canSelectNode.InnerText = CanSelect.ToString();
            element.AppendChild(canSelectNode);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            foreach (XmlNode subNode in nodeElement.ChildNodes.Cast<XmlNode>())
            {
                if (subNode.Name.Equals("SelectedElementId"))
                {
                    SelectedElementId = subNode.InnerText;
                }
                else if (subNode.Name.Equals("CanSelect") && bool.TryParse(subNode.InnerText, out bool canSelect))
                {
                    CanSelect = canSelect;
                }
            }
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes, CompilationContext context)
        {
            if (context == CompilationContext.NodeToCode)
            {
                var rhs = AstFactory.BuildStringNode(Value.Replace(@"\", @"\\"));
                yield return AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);
            }
            else
            {
                var rhs = AstFactory.BuildStringNode(Value);
                yield return AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);
            }
        }
    }
}
