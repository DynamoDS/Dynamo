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
        private readonly DynamoModel dynamoModel;

        public CodeBlockNodeLoader(LibraryServices manager, DynamoModel model)
        {
            libraryServices = manager;
            dynamoModel = model;
        }

        public CodeBlockNodeModel CreateNodeFromXml(XmlElement elNode, SaveContext context, ElementResolver resolver)
        {
            var node = CreateNode();
            node.Deserialize(elNode, context);
            node.ProcessCodeDirect(resolver);
            return node;
        }

        public CodeBlockNodeModel CreateNode()
        {
            return new CodeBlockNodeModel(libraryServices);
        }
    }
}