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
                toNodes.Add(nodeAtt[/*NXLT*/"nickname"].Value.ToString());
            }
            nodeMapping.Add(oldName, toNodes);
        }

        internal void WriteToXmlFile(string dynFile)
        {
            Uri dynFileURI = new Uri(dynFile);

            string localPath = dynFileURI.LocalPath;
            string fileName = Path.GetFileNameWithoutExtension(localPath);
            fileName = /*NXLT*/"MigrationLog_" + fileName;
            string filePath = Path.Combine(Path.GetDirectoryName(localPath), fileName) + /*NXLT*/".xml";

            XmlDocument document = new XmlDocument();
            var rootElement = document.CreateElement(/*NXLT*/"MigrationReport");
            document.AppendChild(rootElement);

            var nodeMappingRoot = document.CreateElement(/*NXLT*/"NodeMappings");
            rootElement.AppendChild(nodeMappingRoot);

            foreach (var nodeMapping in this.nodeMapping)
            {
                var nodeElement = document.CreateElement(/*NXLT*/"Node");
                nodeElement.SetAttribute(/*NXLT*/"OldName", nodeMapping.Key);

                foreach (var newNode in nodeMapping.Value)
                {
                    var childElement = document.CreateElement(/*NXLT*/"MigratedNode");
                    childElement.InnerText = newNode;
                    nodeElement.AppendChild(childElement);
                }
                nodeMappingRoot.AppendChild(nodeElement);
            }

            document.Save(filePath);
        }
    }
}
