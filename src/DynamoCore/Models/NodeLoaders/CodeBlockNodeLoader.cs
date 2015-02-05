using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Nodes;

namespace Dynamo.Models.NodeLoaders
{
    /// <summary>
    ///     Xml Loader for CodeBlock nodes.
    /// </summary>
    public class CodeBlockNodeLoader : INodeLoader<CodeBlockNodeModel>, INodeFactory<CodeBlockNodeModel>
    {
        private readonly LibraryServices libraryServices;

        public CodeBlockNodeLoader(LibraryServices manager)
        {
            libraryServices = manager;
        }

        public CodeBlockNodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context)
        {
            var node = CreateNode();
            node.Deserialize(elNode, context);
            return node;
        }

        public CodeBlockNodeModel CreateNode()
        {
            return new CodeBlockNodeModel(libraryServices);
        }
    }
}