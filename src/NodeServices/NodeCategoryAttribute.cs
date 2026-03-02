using System;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    ///     The NodeCategoryAttribute attribute allows the node implementer
    ///     to define in which category node will appear.
    ///     Moved from DynamoCore to DynamoServices so libraries that reference only DynamoServices
    ///     (e.g. LibG) can use it to control Create/Actions/Query grouping for Zero Touch nodes.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NodeCategoryAttribute : Attribute
    {
        /// <summary>
        /// Creates NodeCategoryAttribute.
        /// </summary>
        /// <param name="category">Full name of the category. E.g. Core.List.Create, or "Create", "Actions", "Query" for grouping.</param>
        public NodeCategoryAttribute(string category)
        {
            ElementCategory = category;
        }

        /// <summary>
        /// Full name of the category. E.g. Core.List.Create
        /// </summary>
        public string ElementCategory { get; set; }
    }
}
