using System;
using System.Collections.Generic;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Search element for a Zero Touch node (DSFunction / DSVarArgFunction)
    /// </summary>
    public class ZeroTouchSearchElement : NodeSearchElement
    {
        private readonly FunctionDescriptor functionDescriptor;

        /// <summary>
        /// The name that is used during node creation
        /// </summary>
        public override string CreationName { get { return functionDescriptor != null ? functionDescriptor.MangledName : this.Name; } }

        public ZeroTouchSearchElement(FunctionDescriptor functionDescriptor)
        {
            this.functionDescriptor = functionDescriptor;

            var displayName = functionDescriptor.UserFriendlyName;
            if (functionDescriptor.IsOverloaded)
                displayName += "(" + string.Join(", ", functionDescriptor.Parameters) + ")";

            Name = displayName;
            FullCategoryName = functionDescriptor.Category;
            Description = functionDescriptor.Description;
            Assembly = functionDescriptor.Assembly;
            if (functionDescriptor.IsBuiltIn)
                ElementType = ElementTypeEnum.RegularNode;
            else
            {
                // Assembly, that is located in package directory, considered as part of package.
                if (Assembly.StartsWith(functionDescriptor.PathManager.PackagesDirectory))
                    ElementType = ElementTypeEnum.Package;
                else
                    ElementType = ElementTypeEnum.CustomDll;
            }

            inputParameters = new List<Tuple<string, string>>(functionDescriptor.InputParameters);
            outputParameters = new List<string>() { functionDescriptor.ReturnType };

            foreach (var tag in functionDescriptor.GetSearchTags())
                SearchKeywords.Add(tag);

            iconName = GetIconName();
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            if (functionDescriptor.IsVarArg)
                return new DSVarArgFunction(functionDescriptor);
            return new DSFunction(functionDescriptor);
        }

        private string GetIconName()
        {
            string name = Nodes.Utilities.NormalizeAsResourceName(functionDescriptor.QualifiedName);

            if (string.IsNullOrEmpty(name))
                name = Nodes.Utilities.NormalizeAsResourceName(functionDescriptor.FunctionName);

            // Usual case.
            if (!functionDescriptor.IsOverloaded)
                return name;

            // Case for overloaded methods.
            if (name == functionDescriptor.QualifiedName)
            {
                return Nodes.Utilities.TypedParametersToString(functionDescriptor);
            }
            else
            {
                // Some nodes contain names with invalid symbols like %, <, >, etc. In this 
                // case the value of "FunctionDescriptor.Name" property should be used. For 
                // an example, "DynamoUnits.SUnit.%" to be renamed as "DynamoUnits.SUnit.mod".
                string shortName = Nodes.Utilities.NormalizeAsResourceName(functionDescriptor.FunctionName);
                return Nodes.Utilities.TypedParametersToString(functionDescriptor, name + shortName);
            }
        }
    }
}