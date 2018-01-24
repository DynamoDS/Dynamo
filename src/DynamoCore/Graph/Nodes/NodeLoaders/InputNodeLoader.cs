using System.Xml;
using Dynamo.Graph.Nodes.CustomNodes;

namespace Dynamo.Graph.Nodes.NodeLoaders
{
    internal class InputNodeLoader : INodeLoader<Symbol>, INodeFactory<Symbol>
    {
        public InputNodeLoader()
        {
        }

        public Symbol CreateNodeFromXml(XmlElement elNode, SaveContext context, ProtoCore.Namespace.ElementResolver resolver)
        {
            var node = CreateNode();
            node.ElementResolver = resolver;
            node.RuntimeDeserialize(elNode, context);
            return node;
        }

        public Symbol CreateNode()
        {
            return new Symbol(); 
        }
    }
}
