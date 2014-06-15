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

        /// <summary>
        /// The name that is used during node creation
        /// </summary>
        public override string CreatingName { get { return this._functionItem != null ? this._functionItem.MangledName : this.Name; } }

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
