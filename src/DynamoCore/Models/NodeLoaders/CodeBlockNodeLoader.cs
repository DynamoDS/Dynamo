using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Nodes;
using ProtoCore.Namespace;

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

        public CodeBlockNodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context, ElementResolver resolver)
        {
            var node = CreateNode();
            node.ElementResolver = resolver;
            node.Deserialize(elNode, context);
            return node;
        }

        public CodeBlockNodeModel CreateNode()
        {
            return new CodeBlockNodeModel(libraryServices);
        }
    }
}