using System.Linq;
using System.Xml;
using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    public class ArcStartMiddleEnd : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Arc.ByThreePoints",
                "Arc.ByThreePoints@Point,Point,Point");
        }
    }

    public class ArcCenter : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            // This migration assumes that the first input of the old node is
            // always an XYZ and never a Transform.

            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "ProtoGeometry.dll",
                "Arc.ByCenterPointRadiusAngle", "Arc.ByCenterPointRadiusAngle@Point,double,double,double,Vector");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create new nodes
            XmlElement zAxisNode = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 0, "ProtoGeometry.dll", "Vector.ZAxis", "Vector.ZAxis");
            migrationData.AppendNode(zAxisNode);
            string zAxisNodeId = MigrationManager.GetGuidFromXmlElement(zAxisNode);

            XmlElement toDegreeNodeStart = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 1, "DSCoreNodes.dll", 
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(toDegreeNodeStart);
            string toDegreeNodeStartId = MigrationManager.GetGuidFromXmlElement(toDegreeNodeStart);

            XmlElement toDegreeNodeEnd = MigrationManager.CreateFunctionNode(
                data.Document, oldNode, 2, "DSCoreNodes.dll",
                "Math.RadiansToDegrees", "Math.RadiansToDegrees@double");
            migrationData.AppendNode(toDegreeNodeEnd);
            string toDegreeNodeEndId = MigrationManager.GetGuidFromXmlElement(toDegreeNodeEnd);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.INPUT);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId toDegreeNodeStartPort = new PortId(toDegreeNodeStartId, 0, PortType.INPUT);
            PortId toDegreeNodeEndPort = new PortId(toDegreeNodeEndId, 0, PortType.INPUT);

            // Update connectors
            data.ReconnectToPort(connector2, toDegreeNodeStartPort);
            data.ReconnectToPort(connector3, toDegreeNodeEndPort);
            data.CreateConnector(toDegreeNodeStart, 0, newNode, 2);
            data.CreateConnector(toDegreeNodeEnd, 0, newNode, 3);
            data.CreateConnector(zAxisNode, 0, newNode, 4);

            return migrationData;
        }
    }

    public class BestFitArc : MigrationNode
    {
        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data, "ProtoGeometry.dll", "Arc.ByBestFitThroughPoints", "Arc.ByBestFitThroughPoints@Point[]");
        }
    }
}
