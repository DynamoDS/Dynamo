using System;
using Dynamo.Graph.Nodes;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Search element for documentations.
    /// </summary>
    internal class DocSearchElement : NodeSearchElement
    {
        protected override NodeModel ConstructNewNodeModel()
        {
            // Open node help Documentation
            return null;
        }

        public DocSearchElement(string nodeName)
        {
            Name = nodeName;
            ElementType = ElementTypes.Doc;
        }
    }
}
