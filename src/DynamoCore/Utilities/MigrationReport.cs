using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Dynamo.Utilities
{

    /// <summary>
    /// Class responsible for creation of an XML in following format that records 
    /// node mapping information - which old node has been converted to which to new node(s) 
    /// <MigrationReport>
    ///     <NodeMappings>
    ///         <Node OldName="OldNodeName1">
    ///           <MigratedNode>NewNodeName1</MigratedNode>
    ///           <MigratedNode>NewNodeName2</MigratedNode>
    ///         </Node>        
    ///         <Node OldName="OldNodeName2">
    ///             ...
    ///         </Node>
    ///         ...
    ///     </NodeMappings>
    /// </MigrationReport>
    /// </summary>
    class MigrationReport
    {
        private Dictionary<string, List<string>> nodeMapping = new Dictionary<string, List<string>>();
        internal void AddMigrationDataToNodeMap(string oldName, IEnumerable<XmlElement> xmlElements)
        {
            List<string> toNodes = new List<string>();
            foreach (var node in xmlElements)
            {
                var nodeAtt = node.Attributes;
                toNodes.Add(nodeAtt["nickname"].Value.ToString());
            }
            nodeMapping.Add(oldName, toNodes);
        }

        internal void WriteToXmlFile(string dynFile)
        {
            Uri dynFileURI = new Uri(dynFile);

            string localPath = dynFileURI.LocalPath;
            string fileName = Path.GetFileNameWithoutExtension(localPath);
            fileName = "MigrationLog_" + fileName;
            string filePath = Path.Combine(Path.GetDirectoryName(localPath), fileName) + ".xml";

            XmlDocument document = new XmlDocument();
            var rootElement = document.CreateElement("MigrationReport");
            document.AppendChild(rootElement);

            var nodeMappingRoot = document.CreateElement("NodeMappings");
            rootElement.AppendChild(nodeMappingRoot);

            foreach (var nodeMapping in this.nodeMapping)
            {
                var nodeElement = document.CreateElement("Node");
                nodeElement.SetAttribute("OldName", nodeMapping.Key);

                foreach (var newNode in nodeMapping.Value)
                {
                    var childElement = document.CreateElement("MigratedNode");
                    childElement.InnerText = newNode;
                    nodeElement.AppendChild(childElement);
                }
                nodeMappingRoot.AppendChild(nodeElement);
            }

            document.Save(filePath);
        }
    }
}
