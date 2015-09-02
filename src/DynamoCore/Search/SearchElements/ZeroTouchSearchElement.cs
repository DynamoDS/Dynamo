using System;
using System.Collections.Generic;
using System.Linq;
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
            UserFriendlyName = functionDescriptor.UserFriendlyName;
            FullCategoryName = functionDescriptor.Category;
            Description = functionDescriptor.Description;
            Assembly = functionDescriptor.Assembly;

            ElementType = ElementTypes.ZeroTouch;

            if (functionDescriptor.IsBuiltIn)
                ElementType |= ElementTypes.BuiltIn;

            // Assembly, that is located in package directory, considered as part of package.
            var packageDirectories = functionDescriptor.PathManager.PackagesDirectories;
            if (packageDirectories.Any(directory => Assembly.StartsWith(directory)))
                ElementType |= ElementTypes.Packaged;

            inputParameters = new List<Tuple<string, string>>(functionDescriptor.InputParameters);
            outputParameters = new List<string>() { functionDescriptor.ReturnType };

            foreach (var tag in functionDescriptor.GetSearchTags())
                SearchKeywords.Add(tag);

            var weights = functionDescriptor.GetSearchTagWeights();
            foreach (var weight in weights)
            {
                // Search tag weight can't be more then 1.
                if (weight <= 1)
                    keywordWeights.Add(weight);
            }

            int weightsCount = weights.Count();
            // If there weren't added weights for search tags, then add default value - 0.5
            if (weightsCount != SearchKeywords.Count)
            {
                int numberOfLackingWeights = SearchKeywords.Count - weightsCount;

                // Number of lacking weights should be more than 0.
                // It can be less then 0 only if there was some mistake in xml file.
                if (numberOfLackingWeights > 0)
                {
                    for (int i = 0; i < numberOfLackingWeights; i++)
                    {
                        keywordWeights.Add(0.5);
                    }
                }

            }

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