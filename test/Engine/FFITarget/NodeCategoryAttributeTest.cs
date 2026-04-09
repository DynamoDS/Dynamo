using System;
using Dynamo.Graph.Nodes;

namespace FFITarget
{
    /// <summary>
    /// Test class for NodeCategoryAttribute type-forwarded to DynamoServices.
    /// CreateFromAttribute is a static method (not a constructor) but is forced into the "Create" category via the attribute.
    /// </summary>
    public static class NodeCategoryAttributeTest
    {
        [NodeCategory("Create")]
        public static int CreateFromAttribute(int value)
        {
            return value;
        }
    }
}
