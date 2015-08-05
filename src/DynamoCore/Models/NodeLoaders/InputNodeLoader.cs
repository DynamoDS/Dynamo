using Dynamo.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dynamo.Models.NodeLoaders
{
    public class InputNodeLoader : INodeLoader<Symbol>, INodeFactory<Symbol>
    {
        public InputNodeLoader()
        {
        }

        public Symbol CreateNodeFromXml(XmlElement elNode, SaveContext context, ProtoCore.Namespace.ElementResolver resolver)
        {
            var node = CreateNode();
            node.ElementResolver = resolver;
            node.Deserialize(elNode, context);
            return node;
        }

        public Symbol CreateNode()
        {
            return new Symbol(); 
        }
    }
}
