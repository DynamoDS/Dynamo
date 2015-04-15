using System;
using System.Linq;
using System.Xml;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Models.NodeLoaders
{
    /// <summary>
    ///     Xml Loader for Custom Nodes.
    /// </summary>
    public class CustomNodeLoader : INodeLoader<Function>
    {
        private readonly ICustomNodeSource customNodeManager;
        private readonly bool isTestMode;

        public CustomNodeLoader(ICustomNodeSource customNodeManager, bool isTestMode = false)
        {
            this.customNodeManager = customNodeManager;
            this.isTestMode = isTestMode;
        }
        
        public Function CreateNodeFromXml(XmlElement nodeElement, SaveContext context)
        {
            XmlNode idNode =
                nodeElement.ChildNodes.Cast<XmlNode>()
                    .LastOrDefault(subNode => subNode.Name.Equals("ID"));

            if (idNode == null || idNode.Attributes == null) 
                return null;

            string id = idNode.Attributes[0].Value;

            string nickname = nodeElement.Attributes["nickname"].Value;

            Guid funcId;
            if (!Guid.TryParse(id, out funcId))
                funcId = GuidUtility.Create(GuidUtility.UrlNamespace, nickname);

            var node = customNodeManager.CreateCustomNodeInstance(funcId, nickname, isTestMode);
            node.Deserialize(nodeElement, context);
            return node;
        }

        public Function CreateProxyNode(Guid funcId, string nickname, Guid nodeId, int inputNum, int outputNum)
        {
            var node = customNodeManager.CreateCustomNodeInstance(funcId, nickname, isTestMode);
            // create its definition and add inputs and outputs
            node.LoadNode(nodeId, inputNum, outputNum);
            return node;
        }
    }
}