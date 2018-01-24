using System.Xml;
using Dynamo.Engine;
using ProtoCore.Namespace;

namespace Dynamo.Graph.Nodes.NodeLoaders
{
    /// <summary>
    ///     Xml Loader for CodeBlock nodes.
    /// </summary>
    internal class CodeBlockNodeLoader : INodeLoader<CodeBlockNodeModel>, INodeFactory<CodeBlockNodeModel>
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
            node.RuntimeDeserialize(elNode, context);
            return node;
        }

        public CodeBlockNodeModel CreateNode()
        {
            return new CodeBlockNodeModel(libraryServices);
        }
    }
}