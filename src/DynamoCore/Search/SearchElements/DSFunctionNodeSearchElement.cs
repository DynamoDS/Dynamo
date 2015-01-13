using System;
using System.Linq;
using System.Collections.Generic;
using Dynamo.DSEngine;
using Dynamo.Library;
using Utils = Dynamo.Nodes.Utilities;

namespace Dynamo.Search.SearchElements
{
#if false
    public class DSFunctionNodeSearchElement : NodeSearchElement, IEquatable<DSFunctionNodeSearchElement>
    {
        internal readonly FunctionDescriptor FunctionDescriptor;
        private string _displayString;

        public DSFunctionNodeSearchElement(string displayString, FunctionDescriptor functionItem, SearchElementGroup group) :
            base(displayString, functionItem.Summary, new List<string> { }, group,
                    functionItem.DisplayName, functionItem.Assembly,
                    functionItem.InputParameters, new List<string>() {functionItem.ReturnType})
        {
            _displayString = displayString;
            FunctionDescriptor = functionItem;
        }

        public override NodeSearchElement Copy()
        {
            var copiedNode = new DSFunctionNodeSearchElement(_displayString, FunctionDescriptor, Group);
            copiedNode.ElementType = ElementType;

            return copiedNode;
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
                case ResourceType.LargeIcon:
                {
                    string name = Nodes.Utilities.NormalizeAsResourceName(FunctionDescriptor.QualifiedName);

                    if (string.IsNullOrEmpty(name)) 
                        name = Nodes.Utilities.NormalizeAsResourceName(FunctionDescriptor.Name); 

                    // Usual case.
                    if (!disambiguate)
                        return name;

                    // Case for overloaded methods.
                    if (name == FunctionDescriptor.QualifiedName)
                    {
                        return Utils.TypedParametersToString(FunctionDescriptor);
                    }
                    else
                    {
                        // Some nodes contain names with invalid symbols like %, <, >, etc. In this 
                        // case the value of "FunctionDescriptor.Name" property should be used. For 
                        // an example, "DynamoUnits.SUnit.%" to be renamed as "DynamoUnits.SUnit.mod".
                        string shortName = Nodes.Utilities.NormalizeAsResourceName(FunctionDescriptor.Name);
                        return Utils.TypedParametersToString(FunctionDescriptor, name + shortName);
                    }
                }
            }

            throw new InvalidOperationException("Unhandled resourceType");
        }

        protected override List<string> GenerateOutputParameters()
        {
            if (FunctionDescriptor.Type == FunctionType.Constructor)
                return new List<string>() { FunctionDescriptor.UnqualifedClassName };

            return base.GenerateOutputParameters();
        }

        protected override List<Tuple<string, string>> GenerateInputParameters()
        {
            string vartype = string.Empty;
            string varname = string.Empty;

            var className = FunctionDescriptor.ClassName;

            vartype = className.Split('.').Last();
            varname = vartype[0].ToString().ToLowerInvariant();

            List<Tuple<string, string>> inputParameters = new List<Tuple<string, string>>();
            inputParameters.Add(Tuple.Create(varname, vartype));

            return inputParameters;
        }
    }
#endif
}
