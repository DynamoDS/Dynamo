﻿using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Dynamo.DSEngine;

namespace Dynamo.Search.SearchElements
{
    public class DSFunctionNodeSearchElement : NodeSearchElement, IEquatable<DSFunctionNodeSearchElement>
    {
        internal readonly FunctionDescriptor FunctionDescriptor;
        private string _displayString;

        public DSFunctionNodeSearchElement(string displayString, FunctionDescriptor functionItem, SearchElementGroup group) :
            base(displayString, functionItem.Summary, new List<string> { }, group,
                    functionItem.DisplayName, functionItem.Assembly,
                    functionItem.InputParameters, functionItem.ReturnType)
        {
            _displayString = displayString;
            FunctionDescriptor = functionItem;
        }

        public override NodeSearchElement Copy()
        {
            return new DSFunctionNodeSearchElement(_displayString, FunctionDescriptor, Group);
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

        protected override string GetResourceName(ResourceType resourceType)
        {
            switch (resourceType)
            {
                case ResourceType.SmallIcon:
                {
                    var name = Dynamo.Nodes.Utilities.NormalizeAsResourceName(FunctionDescriptor.QualifiedName);

                    // This case for operators. Operators should use FunctionDescriptor.Name property.
                    if (string.IsNullOrEmpty(name))
                        name = Dynamo.Nodes.Utilities.NormalizeAsResourceName(FunctionDescriptor.Name);
                    return name; 
                }
                case ResourceType.LargeIcon:
                {
                    var name = Dynamo.Nodes.Utilities.NormalizeAsResourceName(FunctionDescriptor.QualifiedName);

                    // This case for operators. Operators should use FunctionDescriptor.Name property.
                    if (string.IsNullOrEmpty(name))
                        name = Dynamo.Nodes.Utilities.NormalizeAsResourceName(FunctionDescriptor.Name);
                    return name;
                }
            }

            throw new InvalidOperationException("Unhandled resourceType");
        }
    }
}
