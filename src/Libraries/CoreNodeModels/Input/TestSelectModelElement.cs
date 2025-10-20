using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels;
using CoreNodeModels.Properties;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;


namespace CoreNodeModels.Input
{
    /// <summary>
    /// Test node similar to select model element node for Dynamo sandbox.
    /// This node follows the same pattern as the Revit SelectElement node using SelectionBase.
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
    public class TestSelectModelElement : SelectionBase<string, string>
    {
        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        //public override string NodeType
        //{
        //    get
        //    {
        //        return "TestSelectModelElementNode";
        //    }
        //}

        /// <summary>
        /// Selection helper for this node. In a real implementation, this would
        /// interact with the host application's selection system.
        /// </summary>
        public override IModelSelectionHelper<string> SelectionHelper
        {
            get
            {
                return new TestModelSelectionHelper();
            }
        }

        /// <summary>
        /// Default constructor following the SelectionBase pattern.
        /// </summary>
        public TestSelectModelElement() 
            : base(SelectionType.One, SelectionObjectType.None, "Select a model element", "Test Element")
        {
        }

        /// <summary>
        /// JSON constructor for deserialization.
        /// </summary>
        [JsonConstructor]
        private TestSelectModelElement(
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix,
            IEnumerable<string> selectionIdentifier,
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts) 
            : base(selectionType, selectionObjectType, message, prefix, selectionIdentifier, inPorts, outPorts)
        {
        }

        /// <summary>
        /// Extracts selection results from the selection.
        /// </summary>
        protected override IEnumerable<string> ExtractSelectionResults(string selections)
        {
            if (string.IsNullOrEmpty(selections))
                return new List<string>();
            
            return new List<string> { selections };
        }

        /// <summary>
        /// Returns an object in the model from a string identifier.
        /// For testing purposes, this just returns the identifier itself.
        /// </summary>
        protected override string GetModelObjectFromIdentifier(string id)
        {
            return id;
        }

        /// <summary>
        /// Returns an object's unique identifier.
        /// For testing purposes, this just returns the object itself.
        /// </summary>
        protected override string GetIdentifierFromModelObject(string modelObject)
        {
            return modelObject;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildStringNode(SelectionResults.FirstOrDefault() ?? "");
            yield return AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);
        }
    }

    /// <summary>
    /// Test implementation of IModelSelectionHelper for sandbox testing.
    /// In a real implementation, this would interact with the host application.
    /// </summary>
    public class TestModelSelectionHelper : IModelSelectionHelper<string>
    {
        public event Action<ILogMessage> MessageLogged;

        public IEnumerable<string> RequestSelectionOfType(string message, SelectionType selectionType, SelectionObjectType objectType)
        {
            // For testing purposes, generate a mock element ID
            var mockElementId = $"Element_{Guid.NewGuid().ToString("N")[..8]}";
            return new List<string> { mockElementId };
        }
    }
}
