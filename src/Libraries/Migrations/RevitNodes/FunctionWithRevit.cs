using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    public class FunctionWithRevit
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement newNode = CloneAndChangeType(
                oldNode, "Dynamo.Nodes.Function");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }

        private static XmlElement CloneAndChangeType(XmlElement element, string type)
        {
            XmlDocument document = element.OwnerDocument;
            XmlElement cloned = document.CreateElement(type);

            foreach (XmlAttribute attribute in element.Attributes)
                cloned.SetAttribute(attribute.Name, attribute.Value);

            cloned.SetAttribute("type", type);
            return cloned;
        }

    }
}
