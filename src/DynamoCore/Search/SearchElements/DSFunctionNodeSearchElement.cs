using System;
using System.Collections.Generic;
using Dynamo.Core;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using Dynamo.DSEngine;

namespace Dynamo.Search.SearchElements
{
    public class DSFunctionNodeSearchElement : NodeSearchElement, IEquatable<DSFunctionNodeSearchElement>
    {
        private FunctionDescriptor _functionItem;
        private string _displayString;

        public DSFunctionNodeSearchElement(string displayString, FunctionDescriptor functionItem) :
            base(displayString, functionItem.Signature, new List<string> { })
        {
            _displayString = displayString;
            _functionItem = functionItem;
        }

        public override NodeSearchElement Copy()
        {
            return new DSFunctionNodeSearchElement(_displayString, _functionItem);
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.</summary>
        public override void Execute()
        {
            // create node
            var guid = Guid.NewGuid();
            DynamoSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynamoViewModel.CreateNodeCommand(guid, _functionItem.MangledName, 0, 0, true, true));

            // select node
            var placedNode = DynamoSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals(obj as DSFunctionNodeSearchElement);
        }

        /// <summary>
        /// Overriding equals, we need to override hashcode </summary>
        /// <returns> A unique hashcode for the object </returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode() + Name.GetHashCode() + Description.GetHashCode();
        }

        public bool Equals(DSFunctionNodeSearchElement other)
        {
            return _functionItem == other._functionItem;
        }
    }
}
