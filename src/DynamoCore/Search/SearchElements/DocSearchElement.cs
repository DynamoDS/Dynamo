using System;
using Dynamo.Graph.Nodes;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Search element for documentations.
    /// </summary>
    internal class DocSearchElement : NodeSearchElement
    {
        /// <summary>
        /// Full name of the node documentation
        /// </summary>
        internal string DocName { get; set; }

        protected override NodeModel ConstructNewNodeModel()
        {
            return null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="docName"></param>
        public DocSearchElement(string nodeName, string docName)
        {
            Name = nodeName;
            DocName = docName;
            ElementType = ElementTypes.Doc;
        }
    }
}
