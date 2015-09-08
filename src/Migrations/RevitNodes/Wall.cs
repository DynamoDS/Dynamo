using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Migration;

namespace Dynamo.Nodes
{
    public class WallByCurve : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll", 
                "Wall.ByCurveAndHeight", 
                "Wall.ByCurveAndHeight@Curve,double,Level,WallType");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.Input);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.Input);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.Input);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.Input);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.Input);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.Input);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.Input);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.Input);

            data.ReconnectToPort(connector0, newInPort0);
            data.ReconnectToPort(connector1, newInPort2);
            data.ReconnectToPort(connector2, newInPort3);
            data.ReconnectToPort(connector3, newInPort1);

            return migratedData;
        }
    }

    public class SelectWallType : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            migrationData.AppendNode(MigrationManager.CloneAndChangeName(
                data.MigratedNodes.ElementAt(0), "DSRevitNodesUI.WallTypes", "Wall Types"));

            return migrationData;
        }
    }
}
