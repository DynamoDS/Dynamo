using System;
using System.Collections.Generic;
using Dynamo.DSEngine;

namespace Dynamo.Search.SearchElements
{
    public class DSFunctionNodeSearchElement : NodeSearchElement, IEquatable<DSFunctionNodeSearchElement>
    {
        internal readonly FunctionDescriptor FunctionDescriptor;
        private string _displayString;

        /// <summary>
        /// The name that is used during node creation
        /// </summary>
        public override string CreationName { get { return FunctionDescriptor != null ? FunctionDescriptor.MangledName : this.Name; } }

        public DSFunctionNodeSearchElement(string displayString, FunctionDescriptor functionDescriptorItem) :
            base(displayString, functionDescriptorItem.Description, new List<string> { })
        {
            _displayString = displayString;
            FunctionDescriptor = functionDescriptorItem;
        }

        public override NodeSearchElement Copy()
        {
            return new DSFunctionNodeSearchElement(_displayString, FunctionDescriptor);
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
            return this.FunctionDescriptor == other.FunctionDescriptor;
        }
    }
}
