using System;
using System.Linq;
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

        protected override string GetResourceName(ResourceType resourceType, bool disambiguate = false)
        {
            switch (resourceType)
            {
                case ResourceType.SmallIcon:
                {
                    if (!disambiguate)
                        return FunctionDescriptor.QualifiedName;

                    // Case for overloaded methods.
                    return TypedParametersToString(this.FunctionDescriptor);
                }
                case ResourceType.LargeIcon:
                {
                    if (!disambiguate)
                        return FunctionDescriptor.QualifiedName;

                    // Case for overloaded methods.
                    return TypedParametersToString(this.FunctionDescriptor);
                }
            }

            throw new InvalidOperationException("Unhandled resourceType");
        }

        internal static string TypedParametersToString(FunctionDescriptor descriptor)
        {
            string iconName = descriptor.QualifiedName + ".";
            List<Tuple<string, string>> listInputs = new List<Tuple<string, string>>();
            foreach (var parameter in descriptor.InputParameters)
                listInputs.Add(parameter);

            for (int i = 0; i < listInputs.Count; i++)
            {
                string typeOfParameter = listInputs[i].Item2;

                // Check if there simbols like "[]".
                // And remove them, according how much we found.
                // e.g. bool[][] -> bool2
                int squareBrackets = typeOfParameter.Count(x => x == '[');
                if (squareBrackets > 0)
                {
                    // Remove square brackets.
                    typeOfParameter =
                        typeOfParameter.Remove(typeOfParameter.Length - squareBrackets*2);
                    // Add number of them.
                    typeOfParameter = String.Concat(typeOfParameter, squareBrackets.ToString());
                }
                if (i != 0)
                    iconName += "-" + typeOfParameter;
                else
                    iconName += typeOfParameter;
            }
            return iconName;
        }
    }
}
