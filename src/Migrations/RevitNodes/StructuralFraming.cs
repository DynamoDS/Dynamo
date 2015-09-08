using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    public class StructuralFramingSelector: MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement newNode = MigrationManager.CloneAndChangeName(
                oldNode, "DSRevitNodesUI.StructuralFramingTypes", "Structural Framing Types");
            migrationData.AppendNode(newNode);

            foreach (XmlElement subNode in oldNode.ChildNodes)
                newNode.AppendChild(subNode.Clone());

            return migrationData;
        }
    }

    public class StructuralFraming : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            // Create DSFunction node
            XmlElement newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "RevitNodes.dll",
                "StructuralFraming.ByCurveLevelUpVectorAndType",
                "StructuralFraming.ByCurveLevelUpVectorAndType@Curve,Level,Vector,StructuralType,FamilySymbol");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement one = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 0, "1");
            migrationData.AppendNode(one);
            string oneId = MigrationManager.GetGuidFromXmlElement(one);

            XmlElement level = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "RevitNodes.dll",
                "Level.ByElevation", "Level.ByElevation@double");
            migrationData.AppendNode(level);
            string levelId = MigrationManager.GetGuidFromXmlElement(level);

            // Assume that structural framing created by 0.6.3 is always Beam
            XmlElement beam = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, "RevitNodes.dll",
                "StructuralType.Beam", "StructuralType.Beam");
            migrationData.AppendNode(beam);
            string beamId = MigrationManager.GetGuidFromXmlElement(beam);

            // Update connectors
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);
            PortId newInPort0 = new PortId(newNodeId, 0, PortType.Input);
            PortId newInPort4 = new PortId(newNodeId, 4, PortType.Input);

            data.CreateConnector(one, 0, level, 0);
            data.CreateConnector(level, 0, newNode, 1);
            data.CreateConnector(beam, 0, newNode, 3);
            data.ReconnectToPort(connector0, newInPort4);
            data.ReconnectToPort(connector1, newInPort0);

            return migrationData;
        }
    }
}
