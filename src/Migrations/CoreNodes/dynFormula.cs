using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class Formula : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(oldNode, "DSCoreNodesUI.Formula", "Formula");

            XmlElement newChild = data.Document.CreateElement("FormulaText");
            if (oldNode.FirstChild != null)
                newChild.InnerText = oldNode.FirstChild.InnerText;
            newNode.AppendChild(newChild);

            migrationData.AppendNode(newNode);
            return migrationData;
        }
    }
}
