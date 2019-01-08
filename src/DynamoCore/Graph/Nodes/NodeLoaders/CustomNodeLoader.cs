using System;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Utilities;
using ProtoCore.Namespace;

namespace Dynamo.Graph.Nodes.NodeLoaders
{
    /// <summary>
    ///     Xml Loader for Custom Nodes.
    /// </summary>
    internal class CustomNodeLoader : INodeLoader<Function>
    {
        private readonly ICustomNodeSource customNodeManager;
        private readonly bool isTestMode;

        public CustomNodeLoader(ICustomNodeSource customNodeManager, bool isTestMode = false)
        {
            this.customNodeManager = customNodeManager;
            this.isTestMode = isTestMode;
        }
        
        public Function CreateNodeFromXml(XmlElement nodeElement, SaveContext context, ElementResolver resolver)
        {
            XmlNode idNode =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .LastOrDefault(subNode => subNode.Name.Equals("ID"));

            if (idNode == null || idNode.Attributes == null) 
                return null;

            string id = idNode.Attributes[0].Value;

            string name = nodeElement.Attributes["nickname"].Value;

            Guid funcId;
            if (!Guid.TryParse(id, out funcId))
                funcId = GuidUtility.Create(GuidUtility.UrlNamespace, name);

            var node = customNodeManager.CreateCustomNodeInstance(funcId, name, isTestMode);
            node.Deserialize(nodeElement, context);
            return node;
        }

        public Function CreateProxyNode(Guid funcId, string name, Guid nodeId, int inputNum, int outputNum)
        {
            var node = customNodeManager.CreateCustomNodeInstance(funcId, name, isTestMode);
            // create its definition and add inputs and outputs
            node.LoadNode(nodeId, inputNum, outputNum);
            return node;
        }
    }
}