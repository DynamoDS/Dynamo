using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;
using Dynamo.DSEngine;

namespace Dynamo.Search.SearchElements
{
    public class DSFunctionNodeSearchElement : NodeSearchElement, IEquatable<DSFunctionNodeSearchElement>
    {
        private FunctionDescriptor _functionItem;
        private string _displayString;

        public DSFunctionNodeSearchElement(string displayString, FunctionDescriptor functionItem) :
            base(displayString, functionItem.Description, new List<string> { })
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
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.CreateNodeCommand(guid, this._functionItem.MangledName, 0, 0, true, true));

            // select node
            var placedNode = dynSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
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

            return this.Equals(obj as DSFunctionNodeSearchElement);
        }

        /// <summary>
        /// Overriding equals, we need to override hashcode </summary>
        /// <returns> A unique hashcode for the object </returns>
        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + this.Name.GetHashCode() + this.Description.GetHashCode();
        }

        public bool Equals(DSFunctionNodeSearchElement other)
        {
            return this._functionItem == other._functionItem;
        }
    }
}
