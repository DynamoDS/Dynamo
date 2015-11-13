using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Engine;
using Dynamo.Models;
using System.Text;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;

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

            Name = functionDescriptor.UserFriendlyName;
            
            if (functionDescriptor.IsOverloaded)
            {
                var parameters = new StringBuilder();
                parameters.Append("(");
                parameters.Append(String.Join(", ", functionDescriptor.Parameters.Select(x => x.Name)));
                parameters.Append(")");

                Parameters = parameters.ToString();
            }
            
            FullCategoryName = functionDescriptor.Category;
            Description = functionDescriptor.Description;
            Assembly = functionDescriptor.Assembly;

            ElementType = ElementTypes.ZeroTouch;

            if (functionDescriptor.IsBuiltIn)
                ElementType |= ElementTypes.BuiltIn;

            if (functionDescriptor.IsPackageMember)
                ElementType |= ElementTypes.Packaged;

            inputParameters = new List<Tuple<string, string>>(functionDescriptor.InputParameters);
            outputParameters = new List<string>() { functionDescriptor.ReturnType.ToShortString() };

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
            string name = Graph.Nodes.Utilities.NormalizeAsResourceName(functionDescriptor.QualifiedName);

            if (string.IsNullOrEmpty(name))
                name = Graph.Nodes.Utilities.NormalizeAsResourceName(functionDescriptor.FunctionName);

            // Usual case.
            if (!functionDescriptor.IsOverloaded)
                return name;

            // Case for overloaded methods.
            if (name == functionDescriptor.QualifiedName)
            {
                return Graph.Nodes.Utilities.TypedParametersToString(functionDescriptor);
            }
            else
            {
                // Some nodes contain names with invalid symbols like %, <, >, etc. In this 
                // case the value of "FunctionDescriptor.Name" property should be used. For 
                // an example, "DynamoUnits.SUnit.%" to be renamed as "DynamoUnits.SUnit.mod".
                string shortName = Graph.Nodes.Utilities.NormalizeAsResourceName(functionDescriptor.FunctionName);
                return Graph.Nodes.Utilities.TypedParametersToString(functionDescriptor, name + shortName);
            }
        }
    }
}