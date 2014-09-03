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

        protected override string GetResourceName(ResourceType resourceType, bool addInputs = false)
        {
            switch (resourceType)
            {
                case ResourceType.SmallIcon:
                {
                    if (!addInputs)
                        return FunctionDescriptor.QualifiedName;
                    return this.ShortenParameterType();
                }
                //TODO: try to load large icon, look how it works.
                case ResourceType.LargeIcon:
                {
                    if (!addInputs)
                        return FunctionDescriptor.QualifiedName;
                    return this.ShortenParameterType();
                }
            }

            throw new InvalidOperationException("Unhandled resourceType");
        }

        public override string ShortenParameterType()
        {
            string shortIconName = this.GetResourceName(ResourceType.SmallIcon) + ".";
            IEnumerable<Tuple<string, string>> inputParameters =
                this.FunctionDescriptor.InputParameters;
            if (inputParameters == null) return "";
            return this.GetFullIconName(shortIconName, inputParameters);
        }

        public string GetFullIconName(
            string shortIconName, IEnumerable<Tuple<string, string>> inputParameters)
        {
            string iconName = shortIconName + ".";
            List<Tuple<string, string>> listInputs = new List<Tuple<string, string>>();
            foreach (var parameter in inputParameters)
                listInputs.Add(parameter);

            for (int i = 0; i < listInputs.Count; i++)
            {
                string typeOfParameter = listInputs[i].Item2;

                // Check if there simbols like "[]..[]".
                // And remove it.
                // e.g. bool[]..[] -> boolN
                typeOfParameter = typeOfParameter.Replace("[]..[]", "N");

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
